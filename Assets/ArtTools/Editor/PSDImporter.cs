namespace Tools
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using PhotoshopFile;
	using TMPro;
	using UnityEditor;
	using UnityEditorInternal;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;

	/// <summary>
	/// Handles all of the importing for a PSD file (exporting textures, creating prefabs, etc).
	/// </summary>
	public static class PSDImporter
	{
		/// <summary>
		/// The current file path to use to save layers as .png files
		/// </summary>
		private static string currentPath;

		/// <summary>
		/// The <see cref="GameObject"/> representing the root PSD layer.  It contains all of the other layers as children GameObjects.
		/// </summary>
		private static GameObject rootPsdGameObject;

		/// <summary>
		/// The <see cref="GameObject"/> representing the current group (folder) we are processing.
		/// </summary>
		private static GameObject currentGroupGameObject;

		/// <summary>
		/// The current depth (Z axis position) that sprites will be placed on.  It is initialized to the MaximumDepth ("back" depth) and it is automatically
		/// decremented as the PSD file is processed, back to front.
		/// </summary>
		private static float currentDepth;

		/// <summary>
		/// The amount that the depth decrements for each layer.  This is automatically calculated from the number of layers in the PSD file and the MaximumDepth.
		/// </summary>
		private static float depthStep;

		/// <summary>
		/// Initializes static members of the <see cref="PsdImporter"/> class.
		/// </summary>
		static PSDImporter()
		{
			MaximumDepth = 10;
			PixelsToUnits = 100;
		}

		/// <summary>
		/// Gets or sets the maximum depth.  This is where along the Z axis the back will be, with the front being at 0.
		/// </summary>
		public static float MaximumDepth { get; set; }

		/// <summary>
		/// Gets or sets the number of pixels per Unity unit value.  Defaults to 100 (which matches Unity's Sprite default).
		/// </summary>
		public static float PixelsToUnits { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to use the Unity 4.6+ UI system or not.
		/// </summary>
		public static bool UseUnityUI { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the import process should create <see cref="GameObject"/>s in the scene.
		/// </summary>
		private static bool LayoutInScene { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the import process should create a prefab in the project's assets.
		/// </summary>
		private static bool CreatePrefab { get; set; }

		/// <summary>
		/// Gets or sets the size (in pixels) of the entire PSD canvas.
		/// </summary>
		private static Vector2 CanvasSize { get; set; }

		/// <summary>
		/// Gets or sets the name of the current 
		/// </summary>
		private static string PsdName { get; set; }

		/// <summary>
		/// Gets or sets the Unity 4.6+ UI canvas.
		/// </summary>
		private static GameObject Canvas { get; set; }

		/// <summary>
		/// Gets or sets the current <see cref="PsdFile"/> that is being imported.
		/// </summary>
		////private static PsdFile CurrentPsdFile { get; set; }

		/// <summary>
		/// Exports each of the art layers in the PSD file as separate textures (.png files) in the project's assets.
		/// </summary>
		/// <param name="assetPath">The path of to the .psd file relative to the project.</param>
		public static void ExportLayersAsTextures(string assetPath)
		{
			LayoutInScene = false;
			CreatePrefab = false;
			Import(assetPath);
		}

		/// <summary>
		/// Lays out sprites in the current scene to match the PSD's layout.  Each layer is exported as Sprite-type textures in the project's assets.
		/// </summary>
		/// <param name="assetPath">The path of to the .psd file relative to the project.</param>
		public static void LayoutInCurrentScene(string assetPath)
		{
			LayoutInScene = true;
			CreatePrefab = false;
			Import(assetPath);
		}

		/// <summary>
		/// Generates a prefab consisting of sprites laid out to match the PSD's layout. Each layer is exported as Sprite-type textures in the project's assets.
		/// </summary>
		/// <param name="assetPath">The path of to the .psd file relative to the project.</param>
		public static void GeneratePrefab(string assetPath)
		{
			LayoutInScene = false;
			CreatePrefab = true;
			Import(assetPath);
		}

		/// <summary>
		/// Imports a Photoshop document (.psd) file at the given path.
		/// </summary>
		/// <param name="asset">The path of to the .psd file relative to the project.</param>
		private static void Import(string asset)
		{
			currentDepth = MaximumDepth;
			string fullPath = Path.Combine(GetFullProjectPath(), asset.Replace('\\', '/'));

			PsdFile psd = new PsdFile(fullPath, new LoadContext());
			CanvasSize = new Vector2(psd.BaseLayer.Rect.width, psd.BaseLayer.Rect.height);

			// Set the depth step based on the layer count.  If there are no layers, default to 0.1f.
			depthStep = psd.Layers.Count != 0 ? MaximumDepth / psd.Layers.Count : 0.1f;

			int lastSlash = asset.LastIndexOf('/');
			string assetPathWithoutFilename = asset.Remove(lastSlash + 1, asset.Length - (lastSlash + 1));
			PsdName = asset.Replace(assetPathWithoutFilename, string.Empty).Replace(".psd", string.Empty);

			currentPath = GetFullProjectPath() + "Assets";
			currentPath = Path.Combine(currentPath, PsdName);
			Directory.CreateDirectory(currentPath);

			if (LayoutInScene || CreatePrefab)
			{
				if (UseUnityUI)
				{
					CreateUIEventSystem();
					CreateUICanvas();
					rootPsdGameObject = Canvas;
				}
				else
				{
					rootPsdGameObject = new GameObject(PsdName);
				}

				currentGroupGameObject = rootPsdGameObject;
			}

			List<Layer> tree = BuildLayerTree(psd.Layers);
			ExportTree(tree);

			if (CreatePrefab)
			{
				UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(asset.Replace(".psd", ".prefab"));
				PrefabUtility.ReplacePrefab(rootPsdGameObject, prefab);

				if (!LayoutInScene)
				{
					// if we are not flagged to layout in the scene, delete the GameObject used to generate the prefab
					UnityEngine.Object.DestroyImmediate(rootPsdGameObject);
				}
			}

			AssetDatabase.Refresh();
		}

		/// <summary>
		/// Constructs a tree collection based on the PSD layer groups from the raw list of layers.
		/// </summary>
		/// <param name="flatLayers">The flat list of all layers.</param>
		/// <returns>The layers reorganized into a tree structure based on the layer groups.</returns>
		private static List<Layer> BuildLayerTree(List<Layer> flatLayers)
		{
			// There is no tree to create if there are no layers
			if (flatLayers == null)
			{
				return null;
			}

			// PSD layers are stored backwards (with End Groups before Start Groups), so we must reverse them
			flatLayers.Reverse();

			List<Layer> tree = new List<Layer>();
			Layer currentGroupLayer = null;
			Stack<Layer> previousLayers = new Stack<Layer>();

			foreach (Layer layer in flatLayers)
			{
				if (IsEndGroup(layer))
				{
					if (previousLayers.Count > 0)
					{
						Layer previousLayer = previousLayers.Pop();
						previousLayer.Children.Add(currentGroupLayer);
						currentGroupLayer = previousLayer;
					}
					else if (currentGroupLayer != null)
					{
						tree.Add(currentGroupLayer);
						currentGroupLayer = null;
					}
				}
				else if (IsStartGroup(layer))
				{
					// push the current layer
					if (currentGroupLayer != null)
					{
						previousLayers.Push(currentGroupLayer);
					}

					currentGroupLayer = layer;
				}
				else if (layer.Rect.width != 0 && layer.Rect.height != 0)
				{
					// It must be a text layer or image layer
					if (currentGroupLayer != null)
					{
						currentGroupLayer.Children.Add(layer);
					}
					else
					{
						tree.Add(layer);
					}
				}
			}

			// if there are any dangling layers, add them to the tree
			if (tree.Count == 0 && currentGroupLayer != null && currentGroupLayer.Children.Count > 0)
			{
				tree.Add(currentGroupLayer);
			}

			return tree;
		}

		/// <summary>
		/// Fixes any layer names that would cause problems.
		/// </summary>
		/// <param name="name">The name of the layer</param>
		/// <returns>The fixed layer name</returns>
		private static string MakeNameSafe(string name)
		{
			// replace all special characters with an underscore
			Regex pattern = new Regex("[/:&.<>,$¢;+]");
			string newName = pattern.Replace(name, "_");

			if (name != newName)
			{
				Debug.Log(string.Format("Layer name \"{0}\" was changed to \"{1}\"", name, newName));
			}

			return newName;
		}

		/// <summary>
		/// Returns true if the given <see cref="Layer"/> is marking the start of a layer group.
		/// </summary>
		/// <param name="layer">The <see cref="Layer"/> to check if it's the start of a group</param>
		/// <returns>True if the layer starts a group, otherwise false.</returns>
		private static bool IsStartGroup(Layer layer)
		{
			return layer.IsPixelDataIrrelevant;
		}

		/// <summary>
		/// Returns true if the given <see cref="Layer"/> is marking the end of a layer group.
		/// </summary>
		/// <param name="layer">The <see cref="Layer"/> to check if it's the end of a group.</param>
		/// <returns>True if the layer ends a group, otherwise false.</returns>
		private static bool IsEndGroup(Layer layer)
		{
			return layer.Name.Contains("</Layer set>") ||
				layer.Name.Contains("</Layer group>") ||
				(layer.Name == " copy" && layer.Rect.height == 0);
		}

		/// <summary>
		/// Gets full path to the current Unity project. In the form "C:/Project/".
		/// </summary>
		/// <returns>The full path to the current Unity project.</returns>
		private static string GetFullProjectPath()
		{
			string projectDirectory = Application.dataPath;

			// remove the Assets folder from the end since each imported asset has it already in its local path
			if (projectDirectory.EndsWith("Assets"))
			{
				projectDirectory = projectDirectory.Remove(projectDirectory.Length - "Assets".Length);
			}

			return projectDirectory;
		}

		/// <summary>
		/// Gets the relative path of a full path to an asset.
		/// </summary>
		/// <param name="fullPath">The full path to the asset.</param>
		/// <returns>The relative path to the asset.</returns>
		private static string GetRelativePath(string fullPath)
		{
			return fullPath.Replace(GetFullProjectPath(), string.Empty);
		}

		#region Layer Exporting Methods

		/// <summary>
		/// Processes and saves the layer tree.
		/// </summary>
		/// <param name="tree">The layer tree to export.</param>
		private static void ExportTree(List<Layer> tree)
		{
			// we must go through the tree in reverse order since Unity draws from back to front, but PSDs are stored front to back
			for (int i = tree.Count - 1; i >= 0; i--)
			{
				ExportLayer(tree[i]);
			}
		}

		/// <summary>
		/// Exports a single layer from the tree.
		/// </summary>
		/// <param name="layer">The layer to export.</param>
		private static void ExportLayer(Layer layer)
		{
			layer.Name = MakeNameSafe(layer.Name);
			if (layer.Children.Count > 0 || layer.Rect.width == 0)
			{
				ExportFolderLayer(layer);
			}
			else
			{
				ExportArtLayer(layer);
			}
		}

		/// <summary>
		/// Exports a <see cref="Layer"/> that is a folder containing child layers.
		/// </summary>
		/// <param name="layer">The layer that is a folder.</param>
		private static void ExportFolderLayer(Layer layer)
		{
			if (layer.Name.ContainsIgnoreCase("|Button"))
			{
				layer.Name = layer.Name.ReplaceIgnoreCase("|Button", string.Empty);

				if (UseUnityUI)
				{
					CreateUIButton(layer);
				}
			}
			else
			{
				// it is a "normal" folder layer that contains children layers
				string oldPath = currentPath;
				GameObject oldGroupObject = currentGroupGameObject;

				currentPath = Path.Combine(currentPath, layer.Name);
				Directory.CreateDirectory(currentPath);

				if (LayoutInScene || CreatePrefab)
				{
					currentGroupGameObject = new GameObject(layer.Name);
					currentGroupGameObject.transform.parent = oldGroupObject.transform;
				}

				ExportTree(layer.Children);

				currentPath = oldPath;
				currentGroupGameObject = oldGroupObject;
			}
		}

		/// <summary>
		/// Checks if the string contains the given string, while ignoring any casing.
		/// </summary>
		/// <param name="source">The source string to check.</param>
		/// <param name="toCheck">The string to search for in the source string.</param>
		/// <returns>True if the string contains the search string, otherwise false.</returns>
		private static bool ContainsIgnoreCase(this string source, string toCheck)
		{
			return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		/// <summary>
		/// Replaces any instance of the given string in this string with the given string.
		/// </summary>
		/// <param name="str">The string to replace sections in.</param>
		/// <param name="oldValue">The string to search for.</param>
		/// <param name="newValue">The string to replace the search string with.</param>
		/// <returns>The replaced string.</returns>
		private static string ReplaceIgnoreCase(this string str, string oldValue, string newValue)
		{
			StringBuilder sb = new StringBuilder();

			int previousIndex = 0;
			int index = str.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
			while (index != -1)
			{
				sb.Append(str.Substring(previousIndex, index - previousIndex));
				sb.Append(newValue);
				index += oldValue.Length;

				previousIndex = index;
				index = str.IndexOf(oldValue, index, StringComparison.OrdinalIgnoreCase);
			}

			sb.Append(str.Substring(previousIndex));

			return sb.ToString();
		}

		/// <summary>
		/// Exports an art layer as an image file and sprite.  It can also generate text meshes from text layers.
		/// </summary>
		/// <param name="layer">The art layer to export.</param>
		private static void ExportArtLayer(Layer layer)
		{
			if (!layer.IsTextLayer)
			{
				if (LayoutInScene || CreatePrefab)
				{
					// create a sprite from the layer to lay it out in the scene
					if (!UseUnityUI)
					{
						CreateSpriteGameObject(layer);
					}
					else
					{
						CreateUIImage(layer);
					}
				}
				else
				{
					// it is not being laid out in the scene, so simply save out the .png file
					CreatePNG(layer);
				}
			}
			else
			{
				// it is a text layer
				if (LayoutInScene || CreatePrefab)
				{
					// create text mesh
					if (!UseUnityUI)
					{
						CreateTextGameObject(layer);
					}
					else
					{
						CreateUIText(layer);
					}
				}
			}
		}

		/// <summary>
		/// Saves the given <see cref="Layer"/> as a PNG on the hard drive.
		/// </summary>
		/// <param name="layer">The <see cref="Layer"/> to save as a PNG.</param>
		/// <returns>The filepath to the created PNG file.</returns>
		private static string CreatePNG(Layer layer)
		{
			string file = string.Empty;

			if (layer.Children.Count == 0 && layer.Rect.width > 0)
			{
				// decode the layer into a texture
				Texture2D texture = ImageDecoder.DecodeImage(layer);

				file = Path.Combine(currentPath, layer.Name + ".png");

				File.WriteAllBytes(file, texture.EncodeToPNG());
			}

			return file;
		}

		/// <summary>
		/// Creates a <see cref="Sprite"/> from the given <see cref="Layer"/>.
		/// </summary>
		/// <param name="layer">The <see cref="Layer"/> to use to create a <see cref="Sprite"/>.</param>
		/// <returns>The created <see cref="Sprite"/> object.</returns>
		private static Sprite CreateSprite(Layer layer)
		{
			return CreateSprite(layer, PsdName);
		}

		/// <summary>
		/// Creates a <see cref="Sprite"/> from the given <see cref="Layer"/>.
		/// </summary>
		/// <param name="layer">The <see cref="Layer"/> to use to create a <see cref="Sprite"/>.</param>
		/// <param name="packingTag">The tag used for Unity's atlas packer.</param>
		/// <returns>The created <see cref="Sprite"/> object.</returns>
		private static Sprite CreateSprite(Layer layer, string packingTag)
		{
			Sprite sprite = null;

			if (layer.Children.Count == 0 && layer.Rect.width > 0)
			{
				string file = CreatePNG(layer);
				sprite = ImportSprite(GetRelativePath(file), packingTag);
			}

			return sprite;
		}

		/// <summary>
		/// Imports the <see cref="Sprite"/> at the given path, relative to the Unity project. For example "Assets/Textures/texture.png".
		/// </summary>
		/// <param name="relativePathToSprite">The path to the sprite, relative to the Unity project "Assets/Textures/texture.png".</param>
		/// <param name="packingTag">The tag to use for Unity's atlas packing.</param>
		/// <returns>The imported image as a <see cref="Sprite"/> object.</returns>
		private static Sprite ImportSprite(string relativePathToSprite, string packingTag)
		{
			AssetDatabase.ImportAsset(relativePathToSprite, ImportAssetOptions.ForceUpdate);

			// change the importer to make the texture a sprite
			TextureImporter textureImporter = AssetImporter.GetAtPath(relativePathToSprite) as TextureImporter;
			if (textureImporter != null)
			{
				textureImporter.textureType = TextureImporterType.Sprite;
				textureImporter.mipmapEnabled = false;
				textureImporter.spriteImportMode = SpriteImportMode.Single;
				textureImporter.spritePivot = new Vector2(0.5f, 0.5f);
				textureImporter.maxTextureSize = 2048;
				textureImporter.spritePixelsPerUnit = PixelsToUnits;
				textureImporter.spritePackingTag = packingTag;
			}

			AssetDatabase.ImportAsset(relativePathToSprite, ImportAssetOptions.ForceUpdate);

			Sprite sprite = (Sprite)AssetDatabase.LoadAssetAtPath(relativePathToSprite, typeof(Sprite));
			return sprite;
		}

		/// <summary>
		/// Creates a <see cref="GameObject"/> with a <see cref="TextMesh"/> from the given <see cref="Layer"/>.
		/// </summary>
		/// <param name="layer">The <see cref="Layer"/> to create a <see cref="TextMesh"/> from.</param>
		private static void CreateTextGameObject(Layer layer)
		{
			Color color = layer.FillColor;

			float x = layer.Rect.x / PixelsToUnits;
			float y = layer.Rect.y / PixelsToUnits;
			y = (CanvasSize.y / PixelsToUnits) - y;
			float width = layer.Rect.width / PixelsToUnits;
			float height = layer.Rect.height / PixelsToUnits;

			GameObject gameObject = new GameObject(layer.Name);
			gameObject.transform.position = new Vector3(x + (width / 2), y - (height / 2), currentDepth);
			gameObject.transform.parent = currentGroupGameObject.transform;

			currentDepth -= depthStep;

			Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/fonts/walibi/walibi-Black.ttf");

			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.material = font.material;

			TextMesh textMesh = gameObject.AddComponent<TextMesh>();
			textMesh.text = layer.Text;
			textMesh.font = font;
			textMesh.fontSize = 0;
			textMesh.characterSize = layer.FontSize / PixelsToUnits;
			textMesh.color = color;
			textMesh.anchor = TextAnchor.MiddleCenter;

		}

		/// <summary>
		/// Creates a <see cref="GameObject"/> with a sprite from the given <see cref="Layer"/>
		/// </summary>
		/// <param name="layer">The <see cref="Layer"/> to create the sprite from.</param>
		/// <returns>The <see cref="SpriteRenderer"/> component attached to the new sprite <see cref="GameObject"/>.</returns>
		private static SpriteRenderer CreateSpriteGameObject(Layer layer)
		{
			float x = layer.Rect.x / PixelsToUnits;
			float y = layer.Rect.y / PixelsToUnits;
			y = (CanvasSize.y / PixelsToUnits) - y;
			float width = layer.Rect.width / PixelsToUnits;
			float height = layer.Rect.height / PixelsToUnits;

			GameObject gameObject = new GameObject(layer.Name);
			gameObject.transform.position = new Vector3(x + (width / 2), y - (height / 2), currentDepth);
			gameObject.transform.parent = currentGroupGameObject.transform;

			currentDepth -= depthStep;

			SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = CreateSprite(layer);
			return spriteRenderer;
		}
		#endregion

		#region Unity UI
		/// <summary>
		/// Creates the Unity UI event system game object that handles all input.
		/// </summary>
		private static void CreateUIEventSystem()
		{
			if (!GameObject.Find("EventSystem"))
			{
				GameObject gameObject = new GameObject("EventSystem");
				gameObject.AddComponent<EventSystem>();
				gameObject.AddComponent<StandaloneInputModule>();
				gameObject.AddComponent<TouchInputModule>();
			}
		}

		/// <summary>
		/// Creates a Unity UI <see cref="Canvas"/>.
		/// </summary>
		private static void CreateUICanvas()
		{
			Canvas = new GameObject(PsdName);

			Canvas canvas = Canvas.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.WorldSpace;

			RectTransform transform = Canvas.GetComponent<RectTransform>();
			Vector2 scaledCanvasSize = new Vector2(CanvasSize.x / PixelsToUnits, CanvasSize.y / PixelsToUnits);
			transform.sizeDelta = scaledCanvasSize;

			CanvasScaler scaler = Canvas.AddComponent<CanvasScaler>();
			scaler.dynamicPixelsPerUnit = PixelsToUnits;
			scaler.referencePixelsPerUnit = PixelsToUnits;

			Canvas.AddComponent<GraphicRaycaster>();
		}

		/// <summary>
		/// Creates a Unity UI <see cref="UnityEngine.UI.Image"/> <see cref="GameObject"/> with a <see cref="Sprite"/> from a PSD <see cref="Layer"/>.
		/// </summary>
		/// <param name="layer">The <see cref="Layer"/> to use to create the UI Image.</param>
		/// <returns>The newly constructed Image object.</returns>
		private static Image CreateUIImage(Layer layer)
		{
			float x = layer.Rect.x / PixelsToUnits;
			float y = layer.Rect.y / PixelsToUnits;

			// Photoshop increase Y while going down. Unity increases Y while going up.  So, we need to reverse the Y position.
			y = (CanvasSize.y / PixelsToUnits) - y;

			// Photoshop uses the upper left corner as the pivot (0,0).  Unity defaults to use the center as (0,0), so we must offset the positions.
			x = x - ((CanvasSize.x / 2) / PixelsToUnits);
			y = y - ((CanvasSize.y / 2) / PixelsToUnits);

			float width = layer.Rect.width / PixelsToUnits;
			float height = layer.Rect.height / PixelsToUnits;

			GameObject gameObject = new GameObject(layer.Name);
			gameObject.transform.position = new Vector3(x + (width / 2), y - (height / 2), currentDepth);
			gameObject.transform.parent = currentGroupGameObject.transform;

			// if the current group object actually has a position (not a normal Photoshop folder layer), then offset the position accordingly
			gameObject.transform.position = new Vector3(gameObject.transform.position.x + currentGroupGameObject.transform.position.x, gameObject.transform.position.y + currentGroupGameObject.transform.position.y, gameObject.transform.position.z);

			currentDepth -= depthStep;

			Image image = gameObject.AddComponent<Image>();
			image.sprite = CreateSprite(layer);

			RectTransform transform = gameObject.GetComponent<RectTransform>();
			transform.sizeDelta = new Vector2(width, height);

			return image;
		}

		/// <summary>
		/// Creates a Unity UI <see cref="UnityEngine.UI.Text"/> <see cref="GameObject"/> with the text from a PSD <see cref="Layer"/>.
		/// </summary>
		/// <param name="layer">The <see cref="Layer"/> used to create the <see cref="UnityEngine.UI.Text"/> from.</param>
		private static void CreateUIText(Layer layer)
		{
			Color color = layer.FillColor;

			float x = layer.Rect.x / PixelsToUnits;
			float y = layer.Rect.y / PixelsToUnits;

			// Photoshop increase Y while going down. Unity increases Y while going up.  So, we need to reverse the Y position.
			y = (CanvasSize.y / PixelsToUnits) - y;

			// Photoshop uses the upper left corner as the pivot (0,0).  Unity defaults to use the center as (0,0), so we must offset the positions.
			x = x - ((CanvasSize.x / 2) / PixelsToUnits);
			y = y - ((CanvasSize.y / 2) / PixelsToUnits);

			float width = layer.Rect.width / PixelsToUnits;
			float height = layer.Rect.height / PixelsToUnits;

			GameObject gameObject = new GameObject(layer.Name);
			gameObject.transform.position = new Vector3(x + (width / 2), y - (height / 2), currentDepth);
			gameObject.transform.parent = currentGroupGameObject.transform;

			currentDepth -= depthStep;


			TMP_Text textUI = gameObject.GetComponent<TMP_Text>();

			if (textUI == null)
			{
				textUI = gameObject.AddComponent<TextMeshProUGUI>();
			}

			TMP_FontAsset fontUI = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/fonts/Walibi-Black SDF.asset");
			fontUI.material.color = layer.FillColor;
			textUI.font = fontUI;



			textUI.enableWordWrapping = false;
			textUI.rectTransform.sizeDelta = new Vector2(width, height);
			textUI.raycastTarget = false;//can not  click text by yanru 2016-06-16 19:27:41

			//Controls UI Text Anchors (set to Center middle)
			textUI.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			textUI.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			textUI.rectTransform.pivot = new Vector2(0.5f, 0.5f);

			//Controling the font size
			textUI.fontSize = layer.FontSize;
			Color32 _color = new Color(layer.FillColor.r / 255, layer.FillColor.g / 255, layer.FillColor.b / 255, layer.FillColor.a / 255);
			textUI.color = _color;
			textUI.alignment = TextAlignmentOptions.Center;

		}
		
		/// <summary>
		/// Creates a <see cref="UnityEngine.UI.Button"/> from the given <see cref="Layer"/>.
		/// </summary>
		/// <param name="layer">The Layer to create the Button from.</param>
		private static void CreateUIButton(Layer layer)
		{
			// create an empty Image object with a Button behavior attached
			Image image = CreateUIImage(layer);
			Button button = image.gameObject.AddComponent<Button>();

			// look through the children for a clip rect
			////Rectangle? clipRect = null;
			////foreach (Layer child in layer.Children)
			////{
			////    if (child.Name.ContainsIgnoreCase("|ClipRect"))
			////    {
			////        clipRect = child.Rect;
			////    }
			////}

			// look through the children for the sprite states
			foreach (Layer child in layer.Children)
			{
				if (child.Name.ContainsIgnoreCase("|Disabled"))
				{
					child.Name = child.Name.ReplaceIgnoreCase("|Disabled", string.Empty);
					button.transition = Selectable.Transition.SpriteSwap;

					SpriteState spriteState = button.spriteState;
					spriteState.disabledSprite = CreateSprite(child);
					button.spriteState = spriteState;
				}
				else if (child.Name.ContainsIgnoreCase("|Highlighted"))
				{
					child.Name = child.Name.ReplaceIgnoreCase("|Highlighted", string.Empty);
					button.transition = Selectable.Transition.SpriteSwap;

					SpriteState spriteState = button.spriteState;
					spriteState.highlightedSprite = CreateSprite(child);
					button.spriteState = spriteState;
				}
				else if (child.Name.ContainsIgnoreCase("|Pressed"))
				{
					child.Name = child.Name.ReplaceIgnoreCase("|Pressed", string.Empty);
					button.transition = Selectable.Transition.SpriteSwap;

					SpriteState spriteState = button.spriteState;
					spriteState.pressedSprite = CreateSprite(child);
					button.spriteState = spriteState;
				}
				else if (child.Name.ContainsIgnoreCase("|Default") ||
						 child.Name.ContainsIgnoreCase("|Enabled") ||
						 child.Name.ContainsIgnoreCase("|Normal") ||
						 child.Name.ContainsIgnoreCase("|Up"))
				{
					child.Name = child.Name.ReplaceIgnoreCase("|Default", string.Empty);
					child.Name = child.Name.ReplaceIgnoreCase("|Enabled", string.Empty);
					child.Name = child.Name.ReplaceIgnoreCase("|Normal", string.Empty);
					child.Name = child.Name.ReplaceIgnoreCase("|Up", string.Empty);

					image.sprite = CreateSprite(child);

					float x = child.Rect.x / PixelsToUnits;
					float y = child.Rect.y / PixelsToUnits;

					// Photoshop increase Y while going down. Unity increases Y while going up.  So, we need to reverse the Y position.
					y = (CanvasSize.y / PixelsToUnits) - y;

					// Photoshop uses the upper left corner as the pivot (0,0).  Unity defaults to use the center as (0,0), so we must offset the positions.
					x = x - ((CanvasSize.x / 2) / PixelsToUnits);
					y = y - ((CanvasSize.y / 2) / PixelsToUnits);

					float width = child.Rect.width / PixelsToUnits;
					float height = child.Rect.height / PixelsToUnits;

					image.gameObject.transform.position = new Vector3(x + (width / 2), y - (height / 2), currentDepth);

					RectTransform transform = image.gameObject.GetComponent<RectTransform>();
					transform.sizeDelta = new Vector2(width, height);

					button.targetGraphic = image;
				}
				else if (child.Name.ContainsIgnoreCase("|Text") && !child.IsTextLayer)
				{
					child.Name = child.Name.ReplaceIgnoreCase("|Text", string.Empty);

					GameObject oldGroupObject = currentGroupGameObject;
					currentGroupGameObject = button.gameObject;

					// If the "text" is a normal art layer, create an Image object from the "text"
					CreateUIImage(child);

					currentGroupGameObject = oldGroupObject;
				}

				if (child.IsTextLayer)
				{
					// TODO: Create a child text game object
				}
			}
		}
		#endregion
	}
}
/*
using PhotoshopFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Handles all of the importing for a PSD file (exporting textures, creating prefabs, etc).
public static class PSDImporter
{
	// The current file path to use to save layers as .png files
	private static string currentPath;

	// The root PSD layer.  It contains all of the other layers as children GameObjects.
	private static GameObject rootPsdGameObject;

	// The current group (folder) we are processing.
	/// </summary>
	private static GameObject currentGroupGameObject;

	// The current depth (Z axis position) that sprites will be placed on.  It is initialized to the MaximumDepth ("back" depth) and it is automatically
	// decremented as the PSD file is processed, back to front.
	private static float currentDepth;

	// The amount that the depth decrements for each layer.  This is automatically calculated from the number of layers in the PSD file and the MaximumDepth.
	private static float depthStep;

	private static TMP_FontAsset[] availableFonts = new TMP_FontAsset[20];

	// save the layer font name
	private static string font_name;

	/// Initializes static members of the class.
	static PSDImporter()
	{
		MaximumDepth = 10;
		PixelsToUnits = 100;
	}

	/// Gets or sets the maximum depth.  This is where along the Z axis the back will be, with the front being at 0.
	public static float MaximumDepth { get; set; }

	// Gets or sets the number of pixels per Unity unit value.  Defaults to 100 (which matches Unity's Sprite default).
	public static float PixelsToUnits { get; set; }

	// Gets or sets a value indicating whether to use the Unity 4.6+ UI system or not.
	public static bool UseUnityUI { get; set; }

	// Gets or sets a value indicating whether to include hidden layers in your design.
	public static bool includeHiddenLayers { get; set; }

	// The Default font
	public static TMP_FontAsset MainFontOverride { get; set; }

	/// Gets or sets a value indicating whether the import process should create in the scene.
	private static bool LayoutInScene { get; set; }

	// Gets or sets a value indicating whether the import process should create a prefab in the project's assets.
	private static bool CreatePrefab { get; set; }

	//Reverse the Arrangement
	public static bool reverseArrangement { get; set; }


	// Gets or sets the size (in pixels) of the entire PSD canvas.
	private static Vector2 Canvas_Size { get; set; }

	// Gets or sets the name of the current
	private static string PsdName { get; set; }

	// Gets or sets the Unity 4.6+ UI canvas.
	private static GameObject Canvas { get; set; }


	// Gets or sets the current PSD File that is being imported.
	private static PsdFile CurrentPsdFile { get; set; }

	private static string pathToPsdFile { get; set; }
	// Exports each of the art layers in the PSD file as separate textures (.png files) in the project's assets.
	[Obsolete]
	public static void ExportLayersAsTextures(string assetPath)
	{
		LayoutInScene = false;
		CreatePrefab = false;
		Import(assetPath);
	}

	// Lays out sprites in the current scene to match the PSD's layout.  Each layer is exported as Sprite-type textures in the project's assets.
	[Obsolete]
	public static void LayoutInCurrentScene(string assetPath)
	{
		LayoutInScene = true;
		CreatePrefab = false;
		Import(assetPath);
	}

	// Generates a prefab consisting of sprites laid out to match the PSD's layout. Each layer is exported as Sprite-type textures in the project's assets.
	[Obsolete]
	public static void GeneratePrefab(string assetPath)
	{
		LayoutInScene = false;
		CreatePrefab = true;
		Import(assetPath);
	}

	private static float ppi;


	// Imports a Photoshop document (.psd) file at the given path.
	[Obsolete]
	private static void Import(string asset)
	{
		currentDepth = MaximumDepth;
		pathToPsdFile = asset.Replace('\\', '/');
		string fullPath = Path.Combine(GetFullProjectPath(), asset.Replace('\\', '/'));
		PsdFile psd = new PsdFile(fullPath, new LoadContext());
		ppi = GetFileResolution(GetRelativePath(fullPath));
		Canvas_Size = new Vector2(psd.BaseLayer.Rect.width, psd.BaseLayer.Rect.height);
		// Set the depth step based on the layer count.  If there are no layers, default to 0.1f.
		depthStep = psd.Layers.Count != 0 ? MaximumDepth / psd.Layers.Count : 0.1f;

		int lastSlash = asset.LastIndexOf('/');
		string assetPathWithoutFilename = asset.Remove(lastSlash + 1, asset.Length - (lastSlash + 1));
		PsdName = asset.Replace(assetPathWithoutFilename, string.Empty).Replace(".psd", string.Empty);
		currentPath = assetPathWithoutFilename.Replace('\\', '/') + "Textures";
		GetOtherSameNameDirectories("Textures");
		Directory.CreateDirectory(currentPath);
		if (LayoutInScene || CreatePrefab)
		{
			if (UseUnityUI)
			{
				CreateUIEventSystem();
				CreateUICanvas();
				rootPsdGameObject = Canvas;
			}
			else
			{
				rootPsdGameObject = new GameObject(PsdName);
			}

			currentGroupGameObject = rootPsdGameObject;
		}

		List<Layer> tree = BuildLayerTree(psd.Layers);
		ExportTree(tree);

		if (CreatePrefab)
		{
			UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(asset.Replace(".psd", ".prefab"));

			FixStruct(rootPsdGameObject.transform);
			ReplaceImages(rootPsdGameObject.transform);

			PrefabUtility.ReplacePrefab(rootPsdGameObject, prefab);

			if (!LayoutInScene)
			{
				// if we are not flagged to layout in the scene, delete the GameObject used to generate the prefab
				UnityEngine.Object.DestroyImmediate(rootPsdGameObject);
			}
		}
		else
		{
			if (UseUnityUI)
			{
				FixStruct(rootPsdGameObject.transform);
			}
		}


		AssetDatabase.Refresh();
	}



	public static void ReplaceImages(Transform trm)
	{
		Image[] images = trm.GetComponentsInChildren<Image>();

		// Enable Readwrite
		foreach (Image image in images)
		{
			if (image.sprite != null)
			{
				MakeTextureReadable(image.sprite.texture, true);
			}
		}

		// Loop through top level
		foreach (Image imageX in images)
		{
			foreach (Image imageY in images)
			{
				if (imageX.sprite != null && imageY.sprite != null)
				{
					if (imageX != imageY && imageX.sprite != imageY.sprite)
					{
						if (CompareTextures.CompareTexture(imageX.sprite.texture, imageY.sprite.texture))
						{
							string asset = AssetDatabase.GetAssetPath(imageY.sprite.texture);
							AssetDatabase.DeleteAsset(asset);

							imageY.sprite = imageX.sprite;
							Debug.Log(imageY.sprite + " = " + imageX.sprite);
						}
					}
				}
			}
		}

		// Disable Readwrite
		foreach (Image image in images)
		{
			if (image.sprite != null)
			{
				MakeTextureReadable(image.sprite.texture, true);
			}
		}
	}
	public static string MakeTextureReadable(Texture2D assetPath, bool isReadable)
	{
		string asset = AssetDatabase.GetAssetPath(assetPath);
		TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(asset);
		textureImporter.isReadable = isReadable;
		AssetDatabase.ImportAsset(asset, ImportAssetOptions.ForceUpdate);
		return asset;
	}
	public static void FixStruct(Transform trm)
	{
		// Loop through top level
		foreach (RectTransform t in trm)
		{
			if (t.childCount > 0)
			{
				FixStruct(t);
			}

			RectTransform parent = t.parent.GetComponent<RectTransform>();

			// Vector3 averageChildPosition = Vector3.zero;
			Vector4 boundingScale = new Vector4(float.PositiveInfinity, float.PositiveInfinity, float.NegativeInfinity, float.NegativeInfinity);
			int childCount = parent.childCount;

			for (int i = 0; i < childCount; i++)
			{
				RectTransform child = parent.GetChild(i).GetComponent<RectTransform>();

				var minX = child.localPosition.x - child.sizeDelta.x / 2;
				var minY = child.localPosition.y - child.sizeDelta.y / 2;
				var maxX = child.localPosition.x + child.sizeDelta.x / 2;
				var maxY = child.localPosition.y + child.sizeDelta.y / 2;

				boundingScale[0] = Mathf.Min(boundingScale[0], minX);
				boundingScale[1] = Mathf.Min(boundingScale[1], minY);
				boundingScale[2] = Mathf.Max(boundingScale[2], maxX);
				boundingScale[3] = Mathf.Max(boundingScale[3], maxY);
			}

			Vector2 parentScale = new Vector2(boundingScale[2] - boundingScale[0], boundingScale[3] - boundingScale[1]);

			parent.sizeDelta = parentScale;
			Vector3 offset = new Vector3(boundingScale[0] + parentScale.x / 2, boundingScale[1] + parentScale.y / 2, 0);

			parent.localPosition += offset;
			//offset = averageChildPosition / childCount;

			// offset the children
			for (int i = 0; i < childCount; i++)
			{
				RectTransform child = parent.GetChild(i).GetComponent<RectTransform>();
				child.localPosition -= offset;
				child.SetParent(parent);
			}
		}
		rootPsdGameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
		rootPsdGameObject.GetComponent<RectTransform>().sizeDelta = Canvas_Size;
	}


	private static void CombineMatchingImages(List<Image> imgList)
	{
		foreach (Image imgX in imgList)
		{
			foreach (Image imgY in imgList)
			{

				if (imgX.sprite != null && imgY.sprite != null)
				{
					Debug.Log(CompareTextures.CompareTexture(imgX.sprite.texture, imgY.sprite.texture));
					if (CompareTextures.CompareTexture(imgX.sprite.texture, imgY.sprite.texture))
					{
						Debug.Log("REPLACE THAT THING!");
					}
				}
			}
		}
	}


	private static void GetOtherSameNameDirectories(string newFolderName)
	{
		string[] allDirectories = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);

		List<string> duplicateFolders = new List<string>();

		foreach (string directoryPath in allDirectories)
		{
			string folderName = Path.GetFileName(directoryPath);
			if (folderName == newFolderName)
			{
				duplicateFolders.Add(directoryPath);
			}
		}

		if (duplicateFolders.Count > 0)
		{
			string warningMessage = "The following directories with the name " + newFolderName + " already exist:\n\n";
			warningMessage = warningMessage.Replace('\\', '/');
			foreach (string duplicateFolder in duplicateFolders)
			{
				warningMessage += duplicateFolder + "\n";
			}

			Debug.LogWarning(warningMessage);
			//EditorUtility.DisplayDialog("Duplicate Folder Names Found" + newFolderName, warningMessage, "OK");
		}
	}

	// Constructs a tree collection based on the PSD layer groups from the raw list of layers.
	private static List<Layer> BuildLayerTree(List<Layer> flatLayers)
	{
		// There is no tree to create if there are no layers
		if (flatLayers == null)
		{
			return null;
		}

		// PSD layers are stored backwards (with End Groups before Start Groups), so we must reverse them
		flatLayers.Reverse();

		List<Layer> tree = new List<Layer>();
		Layer currentGroupLayer = null;
		Stack<Layer> previousLayers = new Stack<Layer>();

		foreach (Layer layer in flatLayers)
		{
			if (IsEndGroup(layer))
			{
				if (previousLayers.Count > 0)
				{
					Layer previousLayer = previousLayers.Pop();
					previousLayer.Children.Add(currentGroupLayer);
					currentGroupLayer = previousLayer;
				}
				else if (currentGroupLayer != null)
				{
					tree.Add(currentGroupLayer);
					currentGroupLayer = null;
				}
			}
			else if (IsStartGroup(layer))
			{
				// push the current layer
				if (currentGroupLayer != null)
				{
					previousLayers.Push(currentGroupLayer);
				}

				currentGroupLayer = layer;
			}
			else if (layer.Rect.width != 0 && layer.Rect.height != 0)
			{
				// It must be a text layer or image layer
				if (currentGroupLayer != null)
				{
					currentGroupLayer.Children.Add(layer);
				}
				else
				{
					tree.Add(layer);
				}
			}
		}

		// if there are any dangling layers, add them to the tree
		if (tree.Count == 0 && currentGroupLayer != null && currentGroupLayer.Children.Count > 0)
		{
			tree.Add(currentGroupLayer);
		}

		return tree;
	}

	// Fixes any layer names that would cause problems.
	private static string MakeNameSafe(string name)
	{
		char[] invalidChars = { '/', ':', '&', '.', '<', '>', ',', '$', '¢', ';', '+' };
		string newName = name;

		foreach (char invalidChar in invalidChars)
		{
			newName = newName.Replace(invalidChar.ToString(), "_");
		}

		if (name != newName)
		{
			Debug.Log(string.Format("Layer name \"{0}\" was changed to \"{1}\"", name, newName));
		}

		return newName;
	}


	// Returns true if the given <see cref="Layer"/> is marking the start of a layer group.
	private static bool IsStartGroup(Layer layer)
	{
		return layer.IsPixelDataIrrelevant;
	}

	// Returns true if the given <see cref="Layer"/> is marking the end of a layer group.
	private static bool IsEndGroup(Layer layer)
	{			
		return layer.Name.Contains("</Layer set>") ||
			layer.Name.Contains("</Layer group>") ||
			(layer.Name == " copy" && layer.Rect.height == 0);
	}

	// Gets full path to the current Unity project. In the form "C:/Project/".
	private static string GetFullProjectPath()
	{
		string projectDirectory = Application.dataPath;

		// remove the Assets folder from the end since each imported asset has it already in its local path
		if (projectDirectory.EndsWith("Assets"))
		{
			projectDirectory = projectDirectory.Remove(projectDirectory.Length - "Assets".Length);
		}

		return projectDirectory;
	}

	// Gets the relative path of a full path to an asset.
	private static string GetRelativePath(string fullPath)
	{
		return fullPath.Replace(GetFullProjectPath(), string.Empty);
	}

	#region Layer Exporting Methods

	// Processes and saves the layer tree.
	[Obsolete]
	private static void ExportTree(List<Layer> tree)
	{
		// we must go through the tree in reverse order since Unity draws from back to front, but PSDs are stored front to back
		for (int i = tree.Count - 1; i >= 0; i--)
		{
			ExportLayer(tree[i]);
		}
	}

	// Exports a single layer from the tree.
	[Obsolete]
	private static void ExportLayer(Layer layer)
	{
		layer.Name = MakeNameSafe(layer.Name);
		if (layer.Children.Count > 0)
		{
			ExportFolderLayer(layer);
		}
		else
		{
			ExportArtLayer(layer);
		}
	}

	// Exports a layer that is a folder containing child layers.
	private static void ExportFolderLayer(Layer layer)
	{
		// it is a "normal" folder layer that contains children layers
		string oldPath = currentPath;
		GameObject oldGroupObject = currentGroupGameObject;

		currentPath = Path.Combine(currentPath, layer.Name);
		currentPath = currentPath.Replace('\\', '/');
		Directory.CreateDirectory(currentPath);

		if (LayoutInScene || CreatePrefab)
		{
			currentGroupGameObject = new GameObject(layer.Name);
			//currentGroupGameObject.transform.parent = oldGroupObject.transform;
			currentGroupGameObject.transform.parent = oldGroupObject.transform;

			Image image = currentGroupGameObject.AddComponent<Image>();
			image.raycastTarget = false;
			image.preserveAspect = true;
			image.color = new Color(0, 0, 0, 0);
			//Controls UI Image Anchors
			RectTransform rectTransform = currentGroupGameObject.GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.anchoredPosition = Vector2.zero;
		}

		ExportTree(layer.Children);

		currentPath = oldPath;
		currentGroupGameObject = oldGroupObject; // Effects the game object
	}

	// Checks if the string contains the given string, while ignoring any casing.
	private static bool ContainsIgnoreCase(this string source, string toCheck)
	{
		return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
	}

	// Replaces any instance of the given string in this string with the given string.
	private static string ReplaceIgnoreCase(this string str, string oldValue, string newValue)
	{
		StringBuilder sb = new StringBuilder();

		int previousIndex = 0;
		int index = str.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
		while (index != -1)
		{
			sb.Append(str.Substring(previousIndex, index - previousIndex));
			sb.Append(newValue);
			index += oldValue.Length;

			previousIndex = index;
			index = str.IndexOf(oldValue, index, StringComparison.OrdinalIgnoreCase);
		}

		sb.Append(str.Substring(previousIndex));

		return sb.ToString();
	}


	// Exports an art layer as an image file and sprite.  It can also generate text meshes from text layers.
	[Obsolete]
	private static void ExportArtLayer(Layer layer)
	{
		if (includeHiddenLayers)
		{
			if (!layer.IsTextLayer)
			{
				if (LayoutInScene || CreatePrefab)
				{
					// create a sprite from the layer to lay it out in the scene
					if (!UseUnityUI)
					{
						CreateSpriteGameObject(layer);
					}
					else
					{
						CreateUIImage(layer);
					}
				}
				else
				{
					// it is not being laid out in the scene, so simply save out the .png file
					CreatePNG(layer);
				}
			}
			else
			{
				// it is a text layer
				if (LayoutInScene || CreatePrefab)
				{
					// create text mesh
					if (!UseUnityUI)
					{
						font_name = layer.FontName;
						CreateTextGameObject(layer);
					}
					else
					{
						font_name = layer.FontName;
						CreateUIText(layer);
					}
				}
			}
		}
		else
		{
			if (layer.Visible)
			{
				if (!layer.IsTextLayer)
				{
					if (LayoutInScene || CreatePrefab)
					{
						// create a sprite from the layer to lay it out in the scene
						if (!UseUnityUI)
						{
							CreateSpriteGameObject(layer);
						}
						else
						{
							CreateUIImage(layer);
						}
					}
					else
					{
						// it is not being laid out in the scene, so simply save out the .png file
						CreatePNG(layer);
					}
				}
				else
				{
					// it is a text layer
					if (LayoutInScene || CreatePrefab)
					{
						// create text mesh
						if (!UseUnityUI)
						{
							font_name = layer.FontName;
							CreateTextGameObject(layer);
						}
						else
						{
							font_name = layer.FontName;
							CreateUIText(layer);
						}
					}
				}
			}
		}
	}

	// Saves the given layer as a PNG on the hard drive.
	private static string CreatePNG(Layer layer)
	{
		string file = string.Empty;

		if (layer.Children.Count == 0 && layer.Rect.width > 0)
		{
			// decode the layer into a texture
			Texture2D texture = ImageDecoder.DecodeImage(layer);

			file = Path.Combine(currentPath, layer.Name + ".png");
			file = file.Replace('\\', '/');
			File.WriteAllBytes(file, texture.EncodeToPNG());
		}

		return file;
	}

	// Creates a sprite from the given layer
	[Obsolete]
	private static Sprite CreateSpriteFromLayer(Layer layer, string packingTag)
	{
		Sprite sprite = null;

		if (layer.Children.Count == 0 && layer.Rect.width > 0)
		{
			string file = CreatePNG(layer);
			sprite = ImportSprite(GetRelativePath(file), packingTag);
		}

		return sprite;
	}

	// Imports the sprite at the given path, relative to the Unity project. For example "Assets/Textures/texture.png".

	[Obsolete]
	private static Sprite ImportSprite(string relativePathToSprite, string packingTag)
	{
		AssetDatabase.ImportAsset(relativePathToSprite, ImportAssetOptions.ForceUpdate);

		// change the importer to make the texture a sprite
		TextureImporter textureImporter = AssetImporter.GetAtPath(relativePathToSprite) as TextureImporter;
		if (textureImporter != null)
		{
			textureImporter.textureType = TextureImporterType.Sprite;
			textureImporter.mipmapEnabled = false;
			textureImporter.spriteImportMode = SpriteImportMode.Single;
			textureImporter.spritePivot = new Vector2(0.5f, 0.5f);
			textureImporter.maxTextureSize = 2048;
			textureImporter.spritePixelsPerUnit = PixelsToUnits;
			textureImporter.spritePackingTag = packingTag;
		}

		AssetDatabase.ImportAsset(relativePathToSprite, ImportAssetOptions.ForceUpdate);

		Sprite sprite = (Sprite)AssetDatabase.LoadAssetAtPath(relativePathToSprite, typeof(Sprite));
		return sprite;
	}


	// Creates a game object with a TextMesh from the given layer
	private static void CreateTextGameObject(Layer layer)
	{
		float x = layer.Rect.x;
		float y = layer.Rect.y;
		y = (Canvas_Size.y) - y;
		float width = layer.Rect.width;
		float height = layer.Rect.height;

		GameObject gameObject = new GameObject(layer.Name);
		gameObject.transform.position = new Vector3(x + (width / 2), y - (height / 2), currentDepth);
		gameObject.transform.parent = currentGroupGameObject.transform;

		currentDepth -= depthStep;

		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		TextMesh textMesh = gameObject.AddComponent<TextMesh>();
		textMesh.text = layer.Text;
		if (font_name != null)
		{
			if (FindRegularFont() != null)
			{
				textMesh.font = FindRegularFont();
			}
		}
		else
		{
			//Use default
			// Get the font asset from the Resources folder.
			textMesh.font = Resources.Load<Font>("Resources/Fonts/WMSLionelClassic - Regular.ttf");

		}

		meshRenderer.material = textMesh.font.material;

		float fontSize = layer.FontSize / PixelsToUnits;
		float ceiling = Mathf.Ceil(fontSize);
		if (fontSize < ceiling)
		{
			// Unity UI Text doesn't support floating point font sizes, so we have to round to the next size and scale everything else
			float scaleFactor = (ceiling / fontSize);
			textMesh.fontSize = (int)(scaleFactor * fontSize);
		}
		else
		{
			textMesh.fontSize = (int)fontSize;
		}
		textMesh.fontSize = (int)layer.FontSize;
		textMesh.characterSize = layer.FontSize;
		Color32 _color = new Color(layer.FillColor.r / 255, layer.FillColor.g / 255, layer.FillColor.b / 255, layer.FillColor.a / 255);

		textMesh.color = _color;

		textMesh.anchor = TextAnchor.MiddleCenter;
	}

	// Creates a game object with a sprite from the given <see cref="Layer"/>
	[Obsolete]
	private static SpriteRenderer CreateSpriteGameObject(Layer layer)
	{
		float x = layer.Rect.x;
		float y = layer.Rect.y;
		y = (Canvas_Size.y) - y;
		float width = layer.Rect.width;
		float height = layer.Rect.height;

		GameObject gameObject = new GameObject(layer.Name);
		gameObject.transform.position = new Vector3(x + (width / 2), y - (height / 2), currentDepth);
		gameObject.transform.parent = currentGroupGameObject.transform;
		currentDepth -= depthStep;

		SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = CreateSpriteFromLayer(layer, PsdName);
		return spriteRenderer;
	}

	// Creates a Unity sprite animation from the given layer that is a group layer.  It grabs all of the children art
	// layers and uses them as the frames of the animation.
	[Obsolete]
	private static void CreateAnimation(Layer layer)
	{
		float fps = 30;

		string[] args = layer.Name.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

		foreach (string arg in args)
		{
			if (arg.ContainsIgnoreCase("FPS="))
			{
				layer.Name = layer.Name.Replace("|" + arg, string.Empty);

				string[] fpsArgs = arg.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
				if (!float.TryParse(fpsArgs[1], out fps))
				{
					Debug.LogError(string.Format("Unable to parse FPS: \"{0}\"", arg));
				}
			}
		}

		List<Sprite> frames = new List<Sprite>();

		Layer firstChild = layer.Children.First();
		SpriteRenderer spriteRenderer = CreateSpriteGameObject(firstChild);
		spriteRenderer.name = layer.Name;

		foreach (Layer child in layer.Children)
		{
			frames.Add(CreateSpriteFromLayer(child, layer.Name));
		}

		spriteRenderer.sprite = frames[0];

		Animator animator = spriteRenderer.gameObject.AddComponent<Animator>();
		RuntimeAnimatorController controller = CreateAnimatorController(layer.Name, frames, fps);
		animator.runtimeAnimatorController = controller;
	}

	private static RuntimeAnimatorController CreateAnimatorController(string name, IList<Sprite> sprites, float fps)
	{
		AnimatorController controller = new AnimatorController();
		AnimatorControllerLayer controllerLayer = controller.layers[0];
		controllerLayer.name = "Base Layer";
		AnimatorStateMachine stateMachine = controllerLayer.stateMachine;
		AnimatorState state = stateMachine.AddState(name);
		state.motion = CreateAnimationClip(name, sprites, fps);

		string controllerPath = GetRelativePath(currentPath) + "/" + name + ".controller";
		AssetDatabase.CreateAsset(controller, controllerPath);

		return controller;
	}

	private static AnimationClip CreateAnimationClip(string name, IList<Sprite> sprites, float fps)
	{
		AnimationClip clip = new AnimationClip();
		clip.name = name;
		clip.frameRate = fps;
		clip.wrapMode = WrapMode.Loop;

		ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[sprites.Count];

		for (int i = 0; i < sprites.Count; i++)
		{
			ObjectReferenceKeyframe kf = new ObjectReferenceKeyframe();
			kf.time = i / fps;
			kf.value = sprites[i];
			keyFrames[i] = kf;
		}

		EditorCurveBinding curveBinding = new EditorCurveBinding();
		curveBinding.type = typeof(SpriteRenderer);
		curveBinding.path = "";
		curveBinding.propertyName = "m_Sprite";

		AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);

		string clipPath = GetRelativePath(currentPath) + "/" + name + ".anim";
		AssetDatabase.CreateAsset(clip, clipPath);

		return clip;
	}


	#endregion Layer Exporting Methods

	#region Unity UI

	// Creates the Unity UI event system game object that handles all input.
	[Obsolete]
	private static void CreateUIEventSystem()
	{
		if (!GameObject.Find("EventSystem"))
		{
			GameObject gameObject = new GameObject("EventSystem");
			gameObject.AddComponent<EventSystem>();
			gameObject.AddComponent<StandaloneInputModule>();
			gameObject.AddComponent<TouchInputModule>();
		}
	}

	/// Creates a Unity UI Canvas
	private static void CreateUICanvas()
	{
		Canvas = new GameObject(PsdName);

		Canvas canvas = Canvas.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.WorldSpace;

		RectTransform transform = Canvas.GetComponent<RectTransform>();
		Vector2 scaledCanvasSize = new Vector2(Canvas_Size.x, Canvas_Size.y);
		transform.sizeDelta = scaledCanvasSize;
		transform.anchorMin = new Vector2(0f, 0f);
		transform.anchorMax = new Vector2(1f, 1f);
		transform.offsetMin = Vector2.zero;
		transform.offsetMax = Vector2.zero;
		transform.anchoredPosition = Vector3.zero;

		CanvasScaler scaler = Canvas.AddComponent<CanvasScaler>();
		scaler.dynamicPixelsPerUnit = PixelsToUnits;
		scaler.referencePixelsPerUnit = PixelsToUnits;

		Canvas.AddComponent<GraphicRaycaster>();

	}

	// Creates a Unity UI Image with an assigned sprite based on the PSD layer
	[Obsolete]
	private static Image CreateUIImage(Layer layer)
	{
		float x = layer.Rect.x;
		float y = layer.Rect.y;

		// Photoshop increase Y while going down. Unity increases Y while going up.  So, we need to reverse the Y position.
		y = (Canvas_Size.y) - y;

		// Photoshop uses the upper left corner as the pivot (0,0).  Unity defaults to use the center as (0,0), so we must offset the positions.
		x = x - ((Canvas_Size.x / 2));
		y = y - ((Canvas_Size.y / 2));

		float width = layer.Rect.width;
		float height = layer.Rect.height;

		GameObject gameObject = new GameObject(layer.Name);
		gameObject.transform.position = new Vector3(x + (width / 2), y - (height / 2), 0);
		gameObject.transform.parent = currentGroupGameObject.transform;

		// if the current group object actually has a position (not a normal Photoshop folder layer), then offset the position accordingly
		gameObject.transform.position = new Vector3(gameObject.transform.position.x + currentGroupGameObject.transform.position.x, gameObject.transform.position.y + currentGroupGameObject.transform.position.y, gameObject.transform.position.z);

		currentDepth -= depthStep;

		Image image = gameObject.AddComponent<Image>();
		image.raycastTarget = false;
		image.preserveAspect = true;
		image.sprite = CreateSpriteFromLayer(layer, PsdName);

		RectTransform transform = gameObject.GetComponent<RectTransform>();
		transform.sizeDelta = new Vector2(width, height);

		//Controls UI Image Anchors
		transform.anchorMin = new Vector2(0.5f, 0.5f);
		transform.anchorMax = new Vector2(0.5f, 0.5f);
		return image;
	}

	// Creates a Unity UI Text Mesh pro with the text from a PSD layer
	private static void CreateUIText(Layer layer)
	{
		Color color = layer.FillColor;

		float x = layer.Rect.x;
		float y = layer.Rect.y;

		// Photoshop increase Y while going down. Unity increases Y while going up.  So, we need to reverse the Y position.
		y = (Canvas_Size.y) - y;

		// Photoshop uses the upper left corner as the pivot (0,0).  Unity defaults to use the center as (0,0), so we must offset the positions.
		x = x - ((Canvas_Size.x / 2));
		y = y - ((Canvas_Size.y / 2));

		float width = layer.Rect.width;
		float height = layer.Rect.height;
		GameObject gameObject = new GameObject(layer.Name);
		gameObject.transform.position = new Vector3(x + (width / 2), y - (height / 2), 0);
		gameObject.transform.SetParent(currentGroupGameObject.transform, false);
		currentDepth -= depthStep;

		TMP_Text textUI = gameObject.GetComponent<TMP_Text>();

		if (textUI == null)
		{
			textUI = gameObject.AddComponent<TextMeshProUGUI>();
		}
		textUI.text = layer.Text;
		//Getting the correct font
		if (font_name == null)
		{
			//Use default
			// Get the font asset from the Resources folder.
			textUI.font = MainFontOverride;
		}
		else
		{
			if (FindFont() != null)
			{
				TMP_FontAsset fontUI = FindFont();
				fontUI.material.color = layer.FillColor;
				textUI.font = fontUI;
			}
			else
			{
				textUI.font = MainFontOverride;
			}
		}

		textUI.enableWordWrapping = false;
		textUI.rectTransform.sizeDelta = new Vector2(width, height);
		textUI.raycastTarget = false;//can not  click text by yanru 2016-06-16 19:27:41

		//Controls UI Text Anchors (set to Center middle)
		textUI.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		textUI.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		textUI.rectTransform.pivot = new Vector2(0.5f, 0.5f);

		//Controling the font size
		textUI.fontSize = layer.FontSize;
		Color32 _color = new Color(layer.FillColor.r / 255, layer.FillColor.g / 255, layer.FillColor.b / 255, layer.FillColor.a / 255);
		textUI.color = _color;
		textUI.alignment = TextAlignmentOptions.Center;
	}
	private static void CreateUIText(Layer layer, GameObject parent)
	{
		Color color = layer.FillColor;

		float x = layer.Rect.x;
		float y = layer.Rect.y;

		// Photoshop increase Y while going down. Unity increases Y while going up.  So, we need to reverse the Y position.
		y = (Canvas_Size.y) - y;

		// Photoshop uses the upper left corner as the pivot (0,0).  Unity defaults to use the center as (0,0), so we must offset the positions.
		x = x - ((Canvas_Size.x / 2));
		y = y - ((Canvas_Size.y / 2));

		float width = layer.Rect.width;
		float height = layer.Rect.height;
		GameObject gameObject = new GameObject(layer.Name);
		gameObject.transform.position = new Vector3(x + (width / 2), y - (height / 2), 0);
		gameObject.transform.SetParent(parent.transform, false);
		currentDepth -= depthStep;

		TMP_Text textUI = gameObject.GetComponent<TMP_Text>();

		if (textUI == null)
		{
			textUI = gameObject.AddComponent<TextMeshProUGUI>();
		}
		textUI.text = layer.Text;
		//Getting the correct font
		if (font_name == null)
		{
			//Use default
			// Get the font asset from the Resources folder.
			textUI.font = MainFontOverride;
		}
		else
		{
			if (FindFont() != null)
			{
				TMP_FontAsset fontUI = FindFont();
				fontUI.material.color = layer.FillColor;
				textUI.font = fontUI;
			}
			else
			{
				textUI.font = MainFontOverride;
			}
		}

		textUI.enableWordWrapping = false;
		textUI.rectTransform.sizeDelta = new Vector2(width, height);
		textUI.raycastTarget = false;//can not  click text by yanru 2016-06-16 19:27:41

		//Controls UI Text Anchors (set to Center middle)
		textUI.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		textUI.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		textUI.rectTransform.pivot = new Vector2(0.5f, 0.5f);

		//Controling the font size
		float fontSize = (layer.FontSize * 72) / ppi;
		textUI.fontSize = fontSize;
		Color32 _color = new Color(layer.FillColor.r / 255, layer.FillColor.g / 255, layer.FillColor.b / 255, layer.FillColor.a / 255);
		textUI.color = _color;
		textUI.alignment = TextAlignmentOptions.Center;
	}
	public static float GetFileResolution(string fileName)
	{
		Debug.Log(fileName);
		TextureImporter textureImporter = AssetImporter.GetAtPath((fileName)) as TextureImporter;
		return textureImporter.spritePixelsPerUnit;

	}

	private static Font FindRegularFont()
	{
		Font[] fonts = LoadFontsFromFolder();
		// Get the list of fonts in the folder.

		// Iterate through the list of fonts and find the font with the given name.
		foreach (var font in fonts)
		{
			// If the font name matches the given name, return the font object.
			if (font.name.ToUpper().Equals((font_name).ToUpper()))
			{
				// Now load the font via the Font class from the file that was just created
				return font;
			}
		}

		// If the font with the given name is not found, return null.
		return null;
	}

	private static Font[] LoadFontsFromFolder()
	{
		string[] fontGuids = AssetDatabase.FindAssets("t:Font");
		Font[] fonts = new Font[fontGuids.Length];

		for (int i = 0; i < fontGuids.Length; i++)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(fontGuids[i]);
			fonts[i] = AssetDatabase.LoadAssetAtPath<Font>(assetPath);
		}

		return fonts;
	}

	private static TMP_FontAsset FindFont()
	{
		TMP_FontAsset[] fonts = LoadTMPFontsFromFolder();

		// Iterate through the list of fonts and find the font with the given name.
		foreach (var font in fonts)
		{
			// If the font name matches the given name, return the font object.
			if (font.name.ToUpper().Equals((font_name + " SDF").ToUpper()))
			{
				// Now load the font via the Font class from the file that was just created
				return font;
			}
		}

		// If the font with the given name is not found, return null.
		return null;
	}

	private static TMP_FontAsset[] LoadTMPFontsFromFolder()
	{
		string[] fontGuids = AssetDatabase.FindAssets("t:TMP_FontAsset");
		TMP_FontAsset[] fonts = new TMP_FontAsset[fontGuids.Length];

		for (int i = 0; i < fontGuids.Length; i++)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(fontGuids[i]);
			fonts[i] = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
		}

		return fonts;
	}

	/// Creates a Button from the given layer
	[Obsolete]
	private static void CreateUIButton(Layer layer)
	{
		Image image = CreateUIImage(layer);
		Button button = image.gameObject.AddComponent<Button>();

		foreach (Layer child in layer.Children)
		{
			string childName = RemoveButtonStateSuffix(child.Name);

			if (child.Name.ContainsIgnoreCase("|Disabled"))
			{
				button.transition = Selectable.Transition.SpriteSwap;
				SpriteState spriteState = button.spriteState;
				spriteState.disabledSprite = CreateSpriteFromLayer(child, PsdName);
				button.spriteState = spriteState;
			}
			else if (child.Name.ContainsIgnoreCase("|Highlighted"))
			{
				button.transition = Selectable.Transition.SpriteSwap;
				SpriteState spriteState = button.spriteState;
				spriteState.highlightedSprite = CreateSpriteFromLayer(child, PsdName);
				button.spriteState = spriteState;
			}
			else if (child.Name.ContainsIgnoreCase("|Pressed"))
			{
				button.transition = Selectable.Transition.SpriteSwap;
				SpriteState spriteState = button.spriteState;
				spriteState.pressedSprite = CreateSpriteFromLayer(child, PsdName);
				button.spriteState = spriteState;
			}
			else if (IsDefaultButtonState(child.Name))
			{
				image.sprite = CreateSpriteFromLayer(child, PsdName);
				//SetButtonPosition(image, child.Rect);
				button.targetGraphic = image;
			}
			else if (child.Name.ContainsIgnoreCase("|Text") && !child.IsTextLayer)
			{
				childName = RemoveButtonStateSuffix(childName);
				CreateUIText(child, button.gameObject);
			}
			else if (child.IsTextLayer)
			{
				//Create a child text game object
				CreateUIText(child);
			}
		}
	}

	private static string RemoveButtonStateSuffix(string name)
	{
		string[] suffixes = { "|Default", "|Enabled", "|Normal", "|Up", "|Disabled", "|Highlighted", "|Pressed" };
		foreach (string suffix in suffixes)
		{
			name = name.ReplaceIgnoreCase(suffix, string.Empty);
		}
		return name;
	}

	private static bool IsDefaultButtonState(string name)
	{
		string[] defaultSuffixes = { "|Default", "|Enabled", "|Normal", "|Up" };
		foreach (string suffix in defaultSuffixes)
		{
			if (name.ContainsIgnoreCase(suffix))
				return true;
		}
		return false;
	}

	private static void SetButtonPosition(Image image, Rect rect)
	{
		float x = rect.x;
		float y = (Canvas_Size.y) - rect.y;

		x = x - (Canvas_Size.x / 2);
		y = y - (Canvas_Size.y / 2);

		float width = rect.width;
		float height = rect.height;

		image.transform.localPosition = new Vector3(x + (width / 2), y - (height / 2), currentDepth);

		RectTransform transform = image.GetComponent<RectTransform>();
		transform.sizeDelta = new Vector2(width, height);
	}


	#endregion Unity UI

	public enum AnchorPresets
	{
		TopLeft,
		TopCenter,
		TopRight,

		MiddleLeft,
		MiddleCenter,
		MiddleRight,

		BottomLeft,
		BottonCenter,
		BottomRight,
		BottomStretch,

		VertStretchLeft,
		VertStretchRight,
		VertStretchCenter,

		HorStretchTop,
		HorStretchMiddle,
		HorStretchBottom,

		StretchAll
	}

	public enum PivotPresets
	{
		TopLeft,
		TopCenter,
		TopRight,

		MiddleLeft,
		MiddleCenter,
		MiddleRight,

		BottomLeft,
		BottomCenter,
		BottomRight,
	}

	public static void SetAnchor(this RectTransform source, AnchorPresets allign, int offsetX = 0, int offsetY = 0)
	{
		source.anchoredPosition = new Vector3(offsetX, offsetY, 0);

		switch (allign)
		{
			case (AnchorPresets.TopLeft):
				{
					source.anchorMin = new Vector2(0, 1);
					source.anchorMax = new Vector2(0, 1);
					break;
				}
			case (AnchorPresets.TopCenter):
				{
					source.anchorMin = new Vector2(0.5f, 1);
					source.anchorMax = new Vector2(0.5f, 1);
					break;
				}
			case (AnchorPresets.TopRight):
				{
					source.anchorMin = new Vector2(1, 1);
					source.anchorMax = new Vector2(1, 1);
					break;
				}

			case (AnchorPresets.MiddleLeft):
				{
					source.anchorMin = new Vector2(0, 0.5f);
					source.anchorMax = new Vector2(0, 0.5f);
					break;
				}
			case (AnchorPresets.MiddleCenter):
				{
					source.anchorMin = new Vector2(0.5f, 0.5f);
					source.anchorMax = new Vector2(0.5f, 0.5f);
					break;
				}
			case (AnchorPresets.MiddleRight):
				{
					source.anchorMin = new Vector2(1, 0.5f);
					source.anchorMax = new Vector2(1, 0.5f);
					break;
				}

			case (AnchorPresets.BottomLeft):
				{
					source.anchorMin = new Vector2(0, 0);
					source.anchorMax = new Vector2(0, 0);
					break;
				}
			case (AnchorPresets.BottonCenter):
				{
					source.anchorMin = new Vector2(0.5f, 0);
					source.anchorMax = new Vector2(0.5f, 0);
					break;
				}
			case (AnchorPresets.BottomRight):
				{
					source.anchorMin = new Vector2(1, 0);
					source.anchorMax = new Vector2(1, 0);
					break;
				}

			case (AnchorPresets.HorStretchTop):
				{
					source.anchorMin = new Vector2(0, 1);
					source.anchorMax = new Vector2(1, 1);
					break;
				}
			case (AnchorPresets.HorStretchMiddle):
				{
					source.anchorMin = new Vector2(0, 0.5f);
					source.anchorMax = new Vector2(1, 0.5f);
					break;
				}
			case (AnchorPresets.HorStretchBottom):
				{
					source.anchorMin = new Vector2(0, 0);
					source.anchorMax = new Vector2(1, 0);
					break;
				}

			case (AnchorPresets.VertStretchLeft):
				{
					source.anchorMin = new Vector2(0, 0);
					source.anchorMax = new Vector2(0, 1);
					break;
				}
			case (AnchorPresets.VertStretchCenter):
				{
					source.anchorMin = new Vector2(0.5f, 0);
					source.anchorMax = new Vector2(0.5f, 1);
					break;
				}
			case (AnchorPresets.VertStretchRight):
				{
					source.anchorMin = new Vector2(1, 0);
					source.anchorMax = new Vector2(1, 1);
					break;
				}

			case (AnchorPresets.StretchAll):
				{
					source.anchorMin = new Vector2(0, 0);
					source.anchorMax = new Vector2(1, 1);
					break;
				}
		}
	}

	public static void SetPivot(this RectTransform source, PivotPresets preset)
	{
		switch (preset)
		{
			case (PivotPresets.TopLeft):
				{
					source.pivot = new Vector2(0, 1);
					break;
				}
			case (PivotPresets.TopCenter):
				{
					source.pivot = new Vector2(0.5f, 1);
					break;
				}
			case (PivotPresets.TopRight):
				{
					source.pivot = new Vector2(1, 1);
					break;
				}

			case (PivotPresets.MiddleLeft):
				{
					source.pivot = new Vector2(0, 0.5f);
					break;
				}
			case (PivotPresets.MiddleCenter):
				{
					source.pivot = new Vector2(0.5f, 0.5f);
					break;
				}
			case (PivotPresets.MiddleRight):
				{
					source.pivot = new Vector2(1, 0.5f);
					break;
				}

			case (PivotPresets.BottomLeft):
				{
					source.pivot = new Vector2(0, 0);
					break;
				}
			case (PivotPresets.BottomCenter):
				{
					source.pivot = new Vector2(0.5f, 0);
					break;
				}
			case (PivotPresets.BottomRight):
				{
					source.pivot = new Vector2(1, 0);
					break;
				}
		}
	}
}
}
*/
