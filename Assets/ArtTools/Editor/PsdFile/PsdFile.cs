/////////////////////////////////////////////////////////////////////////////////
//
// Photoshop PSD FileType Plugin for Paint.NET
//
// This software is provided under the MIT License:
//   Copyright (c) 2006-2007 Frank Blumenberg
//   Copyright (c) 2010-2020 Tao Yue
//
// Portions of this file are provided under the BSD 3-clause License:
//   Copyright (c) 2006, Jonas Beckeman
//
// See LICENSE.txt for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace PhotoshopFile
{
  public enum PsdColorMode
  {
    Bitmap = 0,
    Grayscale = 1,
    Indexed = 2,
    RGB = 3,
    CMYK = 4,
    Multichannel = 7,
    Duotone = 8,
    Lab = 9
  };

  public enum PsdFileVersion : short
  {
    Psd = 1,
    PsbLargeDocument = 2
  }

  public class PsdFile
  {
    public readonly string Name;
    
    #region Constructors

    public PsdFile(PsdFileVersion version = PsdFileVersion.Psd)
    {
      Version = version;

      BaseLayer = new Layer(this);
      ImageResources = new ImageResources();
      Layers = new List<Layer>();
      AdditionalInfo = new List<LayerInfo>();
    }

    public PsdFile(string filename, LoadContext loadContext)
      : this()
    {
      Name = Path.GetFileNameWithoutExtension(filename);
      //using (var stream = new FileStream(filename, FileMode.Open))
      using (var stream = new MemoryStream(File.ReadAllBytes(filename)))
      {
        Load(stream, loadContext);
      }
    }

    public PsdFile(Stream stream, LoadContext loadContext)
      : this()
    {
      Load(stream, loadContext);
    }

    #endregion

    #region Load and save

    internal LoadContext LoadContext { get; private set; }

    private void Load(Stream stream, LoadContext loadContext)
    {
      LoadContext = loadContext;
      var reader = new PsdBinaryReader(stream, loadContext.Encoding);

      LoadHeader(reader);
      LoadColorModeData(reader);
      LoadImageResources(reader);
      LoadLayerAndMaskInfo(reader);

      LoadImage(reader);
      DecompressImages();
      BuildLayerTree();
    }

    #endregion

    #region Header

    /// <summary>
    /// Photoshop file format version.
    /// </summary>
    public PsdFileVersion Version { get; private set; }

    public bool IsLargeDocument =>
      (Version == PsdFileVersion.PsbLargeDocument);

    private Int16 channelCount;
    /// <summary>
    /// The number of channels in the image, including any alpha channels.
    /// </summary>
    public Int16 ChannelCount
    {
      get => channelCount;
      set
      {
        if (value < 1 || value > 56)
        {
          throw new ArgumentException("Number of channels must be from 1 to 56.");
        }
        channelCount = value;
      }
    }

    private void CheckDimension(int dimension)
    {
      if (dimension < 1)
      {
        throw new ArgumentException("Image dimension must be at least 1.");
      }
      if ((Version == PsdFileVersion.Psd) && (dimension > 30000))
      {
        throw new ArgumentException("PSD image dimension cannot exceed 30000.");
      }
      if ((Version == PsdFileVersion.PsbLargeDocument) && (dimension > 300000))
      {
        throw new ArgumentException("PSB image dimension cannot exceed 300000.");
      }
    }

    /// <summary>
    /// The height of the image in pixels.
    /// </summary>
    public int RowCount
    {
      get => (int)this.BaseLayer.Rect.height;
      set
      {
        CheckDimension(value);
        BaseLayer.Rect = new Rect(0, 0, BaseLayer.Rect.width, value);
      }
    }


    /// <summary>
    /// The width of the image in pixels. 
    /// </summary>
    public int ColumnCount
    {
      get => (int)this.BaseLayer.Rect.width;
      set
      {
        CheckDimension(value);
        BaseLayer.Rect = new Rect(0, 0, value, BaseLayer.Rect.height);
      }
    }

    private int bitDepth;
    /// <summary>
    /// The number of bits per channel. Supported values are 1, 8, 16, and 32.
    /// </summary>
    public int BitDepth
    {
      get => bitDepth;
      set
      {
        switch (value)
        {
          case 1:
          case 8:
          case 16:
          case 32:
            bitDepth = value;
            break;
          default:
            throw new NotImplementedException("Invalid bit depth.");
        }
      }
    }

    /// <summary>
    /// The color mode of the file.
    /// </summary>
    public PsdColorMode ColorMode { get; set; }

    ///////////////////////////////////////////////////////////////////////////

    private void LoadHeader(PsdBinaryReader reader)
    {
      Util.DebugMessage(reader.BaseStream, "Load, Begin, File header");

      var signature = reader.ReadAsciiChars(4);
      if (signature != "8BPS")
      {
        throw new PsdInvalidException("The given stream is not a valid PSD file");
      }

      Version = (PsdFileVersion)reader.ReadInt16();
      Util.DebugMessage(reader.BaseStream, $"Load, Info, Version {(int)Version}");
      if ((Version != PsdFileVersion.Psd)
        && (Version != PsdFileVersion.PsbLargeDocument))
      {
        throw new PsdInvalidException("The PSD file has an unknown version");
      }

      //6 bytes reserved
      reader.BaseStream.Position += 6;

      this.ChannelCount = reader.ReadInt16();
      this.RowCount = reader.ReadInt32();
      this.ColumnCount = reader.ReadInt32();
      BitDepth = reader.ReadInt16();
      ColorMode = (PsdColorMode)reader.ReadInt16();

      Util.DebugMessage(reader.BaseStream, "Load, End, File header");
    }

    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region ColorModeData

    /// <summary>
    /// If ColorMode is ColorModes.Indexed, the following 768 bytes will contain 
    /// a 256-color palette. If the ColorMode is ColorModes.Duotone, the data 
    /// following presumably consists of screen parameters and other related information. 
    /// Unfortunately, it is intentionally not documented by Adobe, and non-Photoshop 
    /// readers are advised to treat duotone images as gray-scale images.
    /// </summary>
    public byte[] ColorModeData = new byte[0];

    private void LoadColorModeData(PsdBinaryReader reader)
    {
      Util.DebugMessage(reader.BaseStream, "Load, Begin, ColorModeData");

      var paletteLength = reader.ReadUInt32();
      if (paletteLength > 0)
      {
        ColorModeData = reader.ReadBytes((int)paletteLength);
      }

      Util.DebugMessage(reader.BaseStream, "Load, End, ColorModeData");
    }

    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region ImageResources

    /// <summary>
    /// The Image resource blocks for the file
    /// </summary>
    public ImageResources ImageResources { get; set; }

    public ResolutionInfo Resolution
    {
      get => (ResolutionInfo)ImageResources.Get(ResourceID.ResolutionInfo);
      set => ImageResources.Set(value);
    }


    ///////////////////////////////////////////////////////////////////////////

    private void LoadImageResources(PsdBinaryReader reader)
    {
      Util.DebugMessage(reader.BaseStream, "Load, Begin, ImageResources");

      var imageResourcesLength = reader.ReadUInt32();
      if (imageResourcesLength <= 0)
      {
        return;
      }

      var startPosition = reader.BaseStream.Position;
      var endPosition = startPosition + imageResourcesLength;
      while (reader.BaseStream.Position < endPosition)
      {
        var imageResource = ImageResourceFactory.CreateImageResource(reader);
        ImageResources.Add(imageResource);
      }

      Util.DebugMessage(reader.BaseStream, "Load, End, ImageResources");

      //-----------------------------------------------------------------------
      // make sure we are not on a wrong offset, so set the stream position 
      // manually
      reader.BaseStream.Position = startPosition + imageResourcesLength;
    }

    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region LayerAndMaskInfo

    public List<Layer> Layers { get; private set; }
    
    public List<Layer> LayerTree { get; private set; }

    public List<LayerInfo> AdditionalInfo { get; private set; }

    public bool AbsoluteAlpha { get; set; }

    ///////////////////////////////////////////////////////////////////////////

    private void LoadLayerAndMaskInfo(PsdBinaryReader reader)
    {
      Util.DebugMessage(reader.BaseStream, "Load, Begin, Layer and mask info");

      var layersAndMaskLength = IsLargeDocument
        ? reader.ReadInt64()
        : reader.ReadUInt32();
      if (layersAndMaskLength <= 0)
      {
        return;
      }

      var startPosition = reader.BaseStream.Position;
      var endPosition = startPosition + layersAndMaskLength;

      LoadLayers(reader, true);
      LoadGlobalLayerMask(reader, endPosition);
      LayerInfoFactory.LoadAll(reader, this, AdditionalInfo, endPosition, true);
      
      var names = new HashSet<string>();
      foreach (var layer in Layers)
      {
        if (names.Contains(layer.Name))
        {
          for (var i = 0; i < 100; i++)
          {
            var newName = layer.Name + i;
            if (!names.Contains(newName))
            {
              layer.Name = newName;
              break;
            }
          }
        }

        names.Add(layer.Name);
      }

      foreach (var layerInfo in AdditionalInfo)
      {
        switch (layerInfo.Key)
        {
          case "LMsk":
            GlobalLayerMaskData = ((RawLayerInfo)layerInfo).Data;
            break;
        }
      }

      Util.DebugMessage(reader.BaseStream, "Load, End, Layer and mask info");
    }

    /// <summary>
    /// Load Layers Info section, including image data.
    /// </summary>
    /// <param name="reader">PSD reader.</param>
    /// <param name="hasHeader">Whether the Layers Info section has a length header.</param>
    internal void LoadLayers(PsdBinaryReader reader, bool hasHeader)
    {
      Util.DebugMessage(reader.BaseStream, "Load, Begin, Layers Info section");

      long sectionLength = 0;
      if (hasHeader)
      {
        sectionLength = IsLargeDocument
          ? reader.ReadInt64()
          : reader.ReadUInt32();

        if (sectionLength <= 0)
        {
          // The callback may take action when there are 0 layers, so it must
          // be called even though the Layers Info section is empty.
          LoadContext.OnLoadLayersHeader(this);
          Util.DebugMessage(reader.BaseStream, "Load, End, Layers Info section");
          return;
        }
      }

      var startPosition = reader.BaseStream.Position;
      var numLayers = reader.ReadInt16();

      // If numLayers < 0, then number of layers is absolute value,
      // and the first alpha channel contains the transparency data for
      // the merged result.
      if (numLayers < 0)
      {
        AbsoluteAlpha = true;
        numLayers = Math.Abs(numLayers);
      }

      for (int i = 0; i < numLayers; i++)
      {
        var layer = new Layer(reader, this);
        Layers.Add(layer);
      }

      // Header is complete just before loading pixel data
      LoadContext.OnLoadLayersHeader(this);

      //-----------------------------------------------------------------------

      // Load image data for all channels.
      foreach (var layer in Layers)
      {
        Util.DebugMessage(reader.BaseStream,
          $"Load, Begin, Layer image, {layer.Name}");
        foreach (var channel in layer.Channels)
        {
          channel.LoadPixelData(reader);
        }
        Util.DebugMessage(reader.BaseStream,
          $"Load, End, Layer image, {layer.Name}");
      }

      // Length is set to 0 when called on higher bitdepth layers.
      if (sectionLength > 0)
      {
        // Layers Info section is documented to be even-padded, but Photoshop
        // actually pads to 4 bytes.
        var endPosition = startPosition + sectionLength;
        var positionOffset = reader.BaseStream.Position - endPosition;
        Debug.Assert(positionOffset > -4,
          "LoadLayers did not read the full length of the Layers Info section.");
        Debug.Assert(positionOffset <= 0,
          "LoadLayers read past the end of the Layers Info section.");

        if (reader.BaseStream.Position < endPosition)
        {
          reader.BaseStream.Position = endPosition;
        }
      }

      Util.DebugMessage(reader.BaseStream, "Load, End, Layers");
    }

    ///////////////////////////////////////////////////////////////////////////

    private byte[] GlobalLayerMaskData = new byte[0];

    private void LoadGlobalLayerMask(PsdBinaryReader reader, long endPosition)
    {
      Util.DebugMessage(reader.BaseStream, "Load, Begin, GlobalLayerMask");

      if (endPosition - reader.BaseStream.Position >= 4)
      {
        var maskLength = reader.ReadUInt32();

        if (maskLength > 0)
        {
          GlobalLayerMaskData = reader.ReadBytes((int)maskLength);
        }
      }

      Util.DebugMessage(reader.BaseStream, "Load, End, GlobalLayerMask");
    }

    ///////////////////////////////////////////////////////////////////////////

    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region Composite image

    /// <summary>
    /// Represents the composite image.
    /// </summary>
    public Layer BaseLayer { get; set; }

    public ImageCompression ImageCompression { get; set; }

    private void LoadImage(PsdBinaryReader reader)
    {
      Util.DebugMessage(reader.BaseStream, "Load, Begin, Composite image");

      ImageCompression = (ImageCompression)reader.ReadInt16();

      // Create channels
      for (Int16 i = 0; i < ChannelCount; i++)
      {
        Util.DebugMessage(reader.BaseStream, "Load, Begin, Channel image data");

        var channel = new Channel(i, this.BaseLayer);
        channel.ImageCompression = ImageCompression;
        channel.Length = this.RowCount
          * Util.BytesPerRow(BaseLayer.Rect.size, BitDepth);

        // The composite image stores all RLE headers up-front, rather than
        // with each channel.
        if (ImageCompression == ImageCompression.Rle)
        {
          channel.RleRowLengths = new RleRowLengths(reader, RowCount, IsLargeDocument);
          channel.Length = channel.RleRowLengths.Total;
        }

        BaseLayer.Channels.Add(channel);
        Util.DebugMessage(reader.BaseStream, "Load, End, Channel image data");
      }

      foreach (var channel in this.BaseLayer.Channels)
      {
        Util.DebugMessage(reader.BaseStream, "Load, Begin, Channel image data");
        Util.CheckByteArrayLength(channel.Length);
        channel.ImageDataRaw = reader.ReadBytes((int)channel.Length);
        Util.DebugMessage(reader.BaseStream, "Load, End, Channel image data");
      }

      // If there is exactly one more channel than we need, then it is the
      // alpha channel.
      if ((ColorMode != PsdColorMode.Multichannel)
        && (ChannelCount == ColorMode.MinChannelCount() + 1))
      {
        var alphaChannel = BaseLayer.Channels.Last();
        alphaChannel.ID = -1;
      }

      Util.DebugMessage(reader.BaseStream, "Load, End, Composite image");
    }

    ///////////////////////////////////////////////////////////////////////////
    
    /// <summary>
    /// Decompress the document image data and all the layers' image data, in parallel.
    /// </summary>
    private void DecompressImages()
    {
      var layersAndComposite = Layers.Concat(new[] { BaseLayer });
      var channels = layersAndComposite.SelectMany(x => x.Channels);
      Parallel.ForEach(channels, channel =>
      {
        channel.DecodeImageData();
      });

      foreach (var layer in Layers)
      {
        foreach (var channel in layer.Channels)
        {
          if (channel.ID == -2)
          {
            layer.Masks.LayerMask.ImageData = channel.ImageData;
          }
          else if (channel.ID == -3)
          {
            layer.Masks.UserMask.ImageData = channel.ImageData;
          }
        }
      }
    }
    
    /// <summary>
    /// Constructs a tree collection based on the PSD layer groups from the raw list of layers.
    /// </summary>
    /// <returns>The layers reorganized into a tree structure based on the layer groups.</returns>
    private void BuildLayerTree()
    {
      LayerTree = new List<Layer>();
      Layer currentGroupLayer = null;
      var previousLayers = new Stack<Layer>();
      var allFolders = new List<Layer>();

      // PSD layers are stored backwards (with End Groups before Start Groups), so we must reverse them
      for (var i = Layers.Count - 1; i >= 0; i--)
      {
        var layer = Layers[i];
        
        if (layer.IsEndGroup())
        {
          if (previousLayers.Count > 0)
          {
            currentGroupLayer = previousLayers.Pop();
          }
          else if (currentGroupLayer != null)
          {
            LayerTree.Add(currentGroupLayer);
            currentGroupLayer = null;
          }
        }
        else if (layer.IsFolder)
        {
          // push the current layer
          if (currentGroupLayer != null)
          {
            currentGroupLayer.Children.Add(layer);
            layer.Parent = currentGroupLayer;
          }
          else
          {
            LayerTree.Add(layer);
          }

          previousLayers.Push(currentGroupLayer);
          allFolders.Add(layer);
          currentGroupLayer = layer;
        }
        else if (layer.Rect.width != 0 && layer.Rect.height != 0)
        {
          // It must be a text layer or image layer
          if (currentGroupLayer != null)
          {
            currentGroupLayer.Children.Add(layer);
            layer.Parent = currentGroupLayer;
          }
          else
          {
            LayerTree.Add(layer);
          }
        }
      }

      // if there are any dangling layers, add them to the tree
      if (LayerTree.Count == 0 && currentGroupLayer != null && currentGroupLayer.Children.Count > 0)
      {
        LayerTree.Add(currentGroupLayer);
      }

      for (var i = allFolders.Count - 1; i >=0; i--)
      {
        RevisitSizeOfLayer(allFolders[i]);
      }
    }
    
    private static void RevisitSizeOfLayer(Layer layer)
    {
      if (layer == null || !layer.IsFolder) return;

      var maxWidth = layer.Rect.width;
      var maxHeight = layer.Rect.height;
      var minY = float.MaxValue;
      var minX = float.MaxValue;

      for (var i = 0; i < layer.Children.Count; i++)
      {
        var children = layer.Children[i];
        if (children.Rect.height> maxHeight) maxHeight = children.Rect.height;
        if (children.Rect.width> maxWidth) maxWidth = children.Rect.width;
        if (children.Rect.y < minY) minY = children.Rect.y;
        if (children.Rect.x < minX) minX = children.Rect.x;
      }
			
      for (var i = 0; i < layer.Children.Count; i++)
      {
        var children = layer.Children[i];
        children.Rect = new Rect(children.Rect.x - minX, children.Rect.y - minY, children.Rect.width, children.Rect.height);
      }

      layer.Rect = new Rect(minX, minY, maxWidth, maxHeight);
    }

    #endregion
  }


  /// <summary>
  /// The possible Compression methods.
  /// </summary>
  public enum ImageCompression
  {
    /// <summary>
    /// Raw data
    /// </summary>
    Raw = 0,
    /// <summary>
    /// RLE compressed
    /// </summary>
    Rle = 1,
    /// <summary>
    /// ZIP without prediction.
    /// </summary>
    Zip = 2,
    /// <summary>
    /// ZIP with prediction.
    /// </summary>
    ZipPrediction = 3
  }

}
