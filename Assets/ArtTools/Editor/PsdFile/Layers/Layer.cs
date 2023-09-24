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
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PhotoshopFile.Enums;
using UnityEngine;

namespace PhotoshopFile
{

	[DebuggerDisplay("Name = {Name}")]
	public class Layer
	{
		internal PsdFile PsdFile { get; private set; }

		/// <summary>
		/// The rectangle containing the contents of the layer.
		/// </summary>
		public Rect Rect { get; set; }

		/// <summary>
		/// A list of the children Layer that belong to this Layer.
		/// </summary>
		public readonly List<Layer> Children = new List<Layer>();

		/// <summary>
		/// Folder layer holding this layer. Null for root layers.
		/// </summary>
		public Layer Parent;

		/// <summary>
		/// Image channels.
		/// </summary>
		public ChannelList Channels { get; private set; }

		/// <summary>
		/// All channels used.
		/// 0 = red, 1 = green, etc.
		/// –1 = transparency mask
		/// –2 = user supplied layer mask
		/// </summary>
		public readonly Channel Channel0, Channel1, Channel2, Channel3, ChannelN1, ChannelN2;

		/// <summary>
		/// Returns alpha channel if it exists, otherwise null.
		/// </summary>
		public Channel AlphaChannel => Channels.SingleOrDefault(x => x.ID == -1);

		private string blendModeKey;
		/// <summary>
		/// Photoshop blend mode key for the layer
		/// </summary>
		public string BlendModeKey
		{
			get => blendModeKey;
			set
			{
				if (value.Length != 4)
				{
					throw new ArgumentException(
					  $"{nameof(BlendModeKey)} must be 4 characters in length.");
				}
				blendModeKey = value;
			}
		}

		/// <summary>
		/// 0 = transparent ... 255 = opaque
		/// </summary>
		public byte Opacity { get; set; }

		/// <summary>
		/// false = base, true = non-base
		/// </summary>
		public bool Clipping { get; set; }

		private static int protectTransBit = BitVector32.CreateMask();
		private static int visibleBit = BitVector32.CreateMask(protectTransBit);
		BitVector32 flags = new BitVector32();


		// The bit flag representing the layer being obsolete.
		private static readonly int ObsBit = BitVector32.CreateMask(visibleBit);

		// The bit flag representing the layer being version 5+.
		private static readonly int VersionFiveOrHigherBit = BitVector32.CreateMask(ObsBit);

		// The bit flag representing the layer's pixel data being irrelevant (a group layer, for example).
		private static readonly int PixelDataIrrelevantBit = BitVector32.CreateMask(VersionFiveOrHigherBit);
		// Gets a value indicating whether this layer's pixel data is irrelevant.  This is often the case with group layers.
		public bool IsPixelDataIrrelevant
		{
			get
			{
				return flags[PixelDataIrrelevantBit];
			}
		}
		public readonly bool IsFolder;
		public readonly bool IsTextLayer;

		/// <summary>
		/// Returns true if the given <see cref="Layer"/> is marking the end of a layer group.
		/// </summary>
		/// <param name="layer">The <see cref="Layer"/> to check if it's the end of a group.</param>
		/// <returns>True if the layer ends a group, otherwise false.</returns>
		public bool IsEndGroup()
		{
			return (Name.Contains("</Layer set>") || Name.Contains("</Layer group>") || (Name == " copy" && Rect.height == 0));
		}

		/// <summary>
		/// If true, the layer is visible.
		/// </summary>
		public bool Visible
		{
			get => !flags[visibleBit];
			set => flags[visibleBit] = !value;
		}

		/// <summary>
		/// Protect the transparency
		/// </summary>
		public bool ProtectTrans
		{
			get => flags[protectTransBit];
			set => flags[protectTransBit] = value;
		}

		/// <summary>
		/// The descriptive layer name
		/// </summary>
		public string Name { get; set; }

		public BlendingRanges BlendingRangesData { get; set; }

		public MaskInfo Masks { get; set; }

		public List<LayerInfo> AdditionalInfo { get; set; }

		/// <summary>
		/// Gets the actual text string, if this is a text layer.
		/// </summary>
		public string Text { get; private set; }

		/// <summary>
		/// Gets the actual text string, if this is a text layer.
		/// </summary>
		public float TextScale { get; private set; }

		/// <summary>
		/// Gets the point size of the font, if this is a text layer.
		/// </summary>
		public float FontSize { get; private set; }

		/// <summary>
		/// Gets the name of the font used, if this is a text layer.
		/// </summary>
		public string FontName { get; private set; }

		/// <summary>
		/// Gets the justification of the text, if this is a text layer.
		/// </summary>
		public TextJustification Justification { get; private set; }

		/// <summary>
		/// Gets the Fill Color of the text, if this is a text layer.
		/// </summary>
		public Color FillColor { get; private set; }

		/// <summary>
		/// Gets the style of warp done on the text, if it is a text layer.
		/// Can be warpNone, warpTwist, etc.
		/// </summary>
		public string WarpStyle { get; private set; }

		///////////////////////////////////////////////////////////////////////////

		public Layer(PsdFile psdFile)
		{
			PsdFile = psdFile;
			Rect = Rect.zero;
			Channels = new ChannelList();
			BlendModeKey = PsdBlendMode.Normal;
			AdditionalInfo = new List<LayerInfo>();
		}

		internal Layer(PsdBinaryReader reader, PsdFile psdFile)
		  : this(psdFile)
		{
			Util.DebugMessage(reader.BaseStream, "Load, Begin, Layer");

			Rect = reader.ReadRectangle();

			// Channel headers
			int numberOfChannels = reader.ReadUInt16();
			for (int channel = 0; channel < numberOfChannels; channel++)
			{
				var ch = new Channel(reader, this);
				Channels.Add(ch);

				switch (ch.ID)
				{
					case 0: Channel0 = ch; break;
					case 1: Channel1 = ch; break;
					case 2: Channel2 = ch; break;
					case 3: Channel3 = ch; break;
					case -1: ChannelN1 = ch; break;
					case -2: ChannelN2 = ch; break;
				}
			}

			// Layer blending
			var signature = reader.ReadAsciiChars(4);
			if (signature != "8BIM")
			{
				throw (new PsdInvalidException("Invalid signature in layer header."));
			}
			BlendModeKey = reader.ReadAsciiChars(4);
			Opacity = reader.ReadByte();
			Clipping = reader.ReadBoolean();

			var flagsByte = reader.ReadByte();
			flags = new BitVector32(flagsByte);
			reader.ReadByte(); // Padding

			// Variable-length data
			var extraDataSize = reader.ReadUInt32();
			var extraDataStartPosition = reader.BaseStream.Position;
			long extraDataEndPosition = extraDataStartPosition + extraDataSize;

			Masks = new MaskInfo(reader, this);
			BlendingRangesData = new BlendingRanges(reader, this);
			Name = reader.ReadPascalString(4);
			LayerInfoFactory.LoadAll(reader, PsdFile, AdditionalInfo,
			  extraDataEndPosition, false);

			foreach (var adjustmentInfo in AdditionalInfo)
			{
				switch (adjustmentInfo.Key)
				{
					case "luni":
						Name = ((LayerUnicodeName)adjustmentInfo).Name;
						break;
					case "lsct":
					case "lsdk":
						var sectionInfo = (LayerSectionInfo)adjustmentInfo;
						IsFolder = sectionInfo.SectionType == LayerSectionType.OpenFolder || sectionInfo.SectionType == LayerSectionType.ClosedFolder;
						break;
					case "TySh":
						IsTextLayer = true;
						ReadTextLayer(new PsdBinaryReader(new MemoryStream(((RawLayerInfo)adjustmentInfo).Data), reader));
						break;
				}
			}

			Util.DebugMessage(reader.BaseStream, $"Load, End, Layer, {Name}");

			PsdFile.LoadContext.OnLoadLayerHeader(this);
		}

		/// <summary>
		/// Reads the text information for the layer.
		/// </summary>
		/// <param name="dataReader">The reader to use to read the text data.</param>
		private void ReadTextLayer(PsdBinaryReader dataReader)
		{
			// read the text layer's text string
			dataReader.ReadBytes(26); // 2 for version, 8 for xx, xy, yx
			TextScale = (float)dataReader.ReadDouble(); // yy
			dataReader.Seek("/Text");
			dataReader.ReadBytes(4);
			Text = dataReader.ReadString();

			// read the text justification
			dataReader.Seek("/Justification ");
			int justification = dataReader.ReadByte() - 48;
			Justification = TextJustification.Left;
			if (justification == 1)
			{
				Justification = TextJustification.Right;
			}
			else if (justification == 2)
			{
				Justification = TextJustification.Center;
			}

			// read the font size
			dataReader.Seek("/FontSize ");
			FontSize = dataReader.ReadFloat();

			dataReader.Seek("/FontCaps ");
			var caps = dataReader.ReadByte() - 48;
			if (caps == 1) Text = Text.ToLower();
			else if (caps == 2) Text = Text.ToUpper();

			// read the font fill color
			dataReader.Seek("/FillColor");
			dataReader.Seek("/Values [ ");
			float alpha = dataReader.ReadFloat();
			dataReader.ReadByte();
			float red = dataReader.ReadFloat();
			dataReader.ReadByte();
			float green = dataReader.ReadFloat();
			dataReader.ReadByte();
			float blue = dataReader.ReadFloat();
			FillColor = new Color(red, green, blue, alpha);

			// read the font name
			dataReader.Seek("/FontSet ");
			dataReader.Seek("/Name");
			dataReader.ReadBytes(4);
			FontName = dataReader.ReadString();

			// read the warp style
			dataReader.Seek("warpStyle");
			dataReader.Seek("warpStyle");
			dataReader.ReadBytes(3);
			int num13 = dataReader.ReadByte();
			WarpStyle = dataReader.ReadAsciiChars(num13);
		}

		///////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Create ImageData for any missing channels.
		/// </summary>
		public void CreateMissingChannels()
		{
			var channelCount = this.PsdFile.ColorMode.MinChannelCount();
			for (short id = 0; id < channelCount; id++)
			{
				if (!this.Channels.ContainsId(id))
				{
					var size = (int)this.Rect.height * (int)this.Rect.width;

					var ch = new Channel(id, this);
					ch.ImageData = new byte[size];

					if (size > 0)
					{
						unsafe
						{
							fixed (byte* ptr = &ch.ImageData[0])
							{
								Util.Fill(ptr, ptr + size, (byte)255);
							}
						}
					}

					this.Channels.Add(ch);
				}
			}
		}
	}
}
