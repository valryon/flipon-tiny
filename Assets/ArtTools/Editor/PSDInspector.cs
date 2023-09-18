namespace Tools
{
    using System;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    
    [CustomEditor(typeof(TextureImporter))]
    public class PSDInspector : Editor
    {
        /// The native Unity editor used to render the TextureImporter's Inspector.
        private Editor nativeEditor;

        // The style used to draw the section header text.
        private GUIStyle guiStyle;

        //Display the regular 2D Texture Import Inspector
        private bool showTextureImportSettings;

        //Set the Unity UI to true at the start of the game

        private bool useUI;

        private void Awake()
        {
            useUI = true;
            showTextureImportSettings = false;
        }
        // Called by Unity when any Texture file is first clicked on and the Inspector is populated.
        public void OnEnable()
        {
            // use reflection to get the default Inspector
            Type type = Type.GetType("UnityEditor.TextureImporterInspector, UnityEditor");
            nativeEditor = CreateEditor(target, type);

            // set up the GUI style for the section headers
            guiStyle = new GUIStyle();
            guiStyle.richText = true;
            guiStyle.fontSize = 14;
            guiStyle.normal.textColor = Color.black;

            if (Application.HasProLicense())
            {
                guiStyle.normal.textColor = Color.white;
            }      
        }

        // Draws the Inspector GUI for the TextureImporter.
        // Normal Texture files should appear as they normally do, however PSD files will have additional items.
        [Obsolete]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		public override void OnInspectorGUI()
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
            if (nativeEditor != null)
            {
                // check if it is a PSD file selected
                string assetPath = ((TextureImporter)target).assetPath;

                if (assetPath.EndsWith(".psd"))
                {
                    GUILayout.Label("<b>PSD Layout Tool</b>", guiStyle, GUILayout.Height(23));
                    GUILayout.BeginHorizontal();
                    GUIContent maximumDepthLabel = new GUIContent("Maximum Depth", "The Z value of the far back plane. The PSD will be laid out from here to 0.");
                    PSDImporter.MaximumDepth = EditorGUILayout.FloatField(maximumDepthLabel, PSDImporter.MaximumDepth);
                    GUILayout.EndHorizontal();
                    
                    
                    GUILayout.BeginHorizontal();
                    GUIContent pixelsToUnitsLabel = new GUIContent("Pixels to Unity Units (PPU)", "The PPU value determines the scale of your sprites or textures.");
                    PSDImporter.PixelsToUnits = EditorGUILayout.FloatField(pixelsToUnitsLabel, PSDImporter.PixelsToUnits);
                    GUILayout.EndHorizontal();

                    GUIContent createUnityUILabel = new GUIContent("Use Unity UI", "Create Unity UI elements instead of regular GameObjects.");
                    useUI = EditorGUILayout.Toggle(createUnityUILabel, useUI);
                    PSDImporter.UseUnityUI = useUI;
                   
                    EditorGUILayout.Space(10f);
                    
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Display the standard Unity 2D Texture Import Settings");
                    showTextureImportSettings = EditorGUILayout.Toggle(showTextureImportSettings);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(20f);

                    if (showTextureImportSettings)
                    {
                        GUIStyle guiStyle = new GUIStyle(GUI.skin.label);
                        guiStyle.richText = true;

                        //Adding the regular unity 2D texture import setting
                        GUILayout.Label("<b>Unity Texture Importer Settings</b>", guiStyle, GUILayout.Height(23));

                        // Draw the default Inspector for 2D Texture import
                        nativeEditor.OnInspectorGUI();
                    }
                    if (GUILayout.Button("Export Layers as Textures"))
                    {
                        PSDImporter.ExportLayersAsTextures(assetPath);
                    }

                    if (GUILayout.Button("Layout in Current Scene"))
                    {
                        PSDImporter.LayoutInCurrentScene(assetPath);
                    }

                    if (GUILayout.Button("Generate Prefab"))
                    {
                        PSDImporter.GeneratePrefab(assetPath);
                    }
                }
                else
                {
                    // It is a "normal" Texture, not a PSD
                    nativeEditor.OnInspectorGUI();
                }
            }
        }
    }
}