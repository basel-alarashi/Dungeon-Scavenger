using UnityEngine;
using UnityEditor;
using System.IO;
using DungeonScavenger.Inventory;

namespace DungeonScavenger.Editor
{
    /// <summary>
    /// Generates procedural placeholder icons for inventory items.
    /// Creates colored squares that can be used as item icons.
    /// </summary>
    public class ProceduralIconGenerator : EditorWindow
    {
        private Color iconColor = Color.white;
        private string iconName = "NewIcon";
        private int iconSize = 128;
        private string outputPath = "Assets/UI/Icons/";
        
        [MenuItem("Tools/Dungeon Scavenger/Icon Generator")]
        public static void ShowWindow()
        {
            GetWindow<ProceduralIconGenerator>("Icon Generator");
        }
        
        [MenuItem("Tools/Dungeon Scavenger/Generate Icons for All Items")]
        public static void GenerateIconsForAllItems()
        {
            // Find all ItemData assets
            string[] guids = AssetDatabase.FindAssets("t:ItemData");
            
            int generated = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                
                if (itemData != null && itemData.icon == null)
                {
                    // Generate icon
                    Sprite icon = GenerateColoredSprite(
                        itemData.itemColor,
                        itemData.itemName,
                        128
                    );
                    
                    if (icon != null)
                    {
                        itemData.icon = icon;
                        EditorUtility.SetDirty(itemData);
                        generated++;
                    }
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[IconGenerator] Generated {generated} icons for ItemData assets.");
        }
        
        [MenuItem("Tools/Dungeon Scavenger/Fix Missing Icons")]
        public static void FixMissingIcons()
        {
            string[] guids = AssetDatabase.FindAssets("t:ItemData");
            int fixedCount = 0;
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                
                if (itemData != null && itemData.icon == null)
                {
                    // Try to find existing icon by name
                    string expectedIconName = $"Icon_{itemData.itemName.Replace(" ", "")}";
                    string[] iconGuids = AssetDatabase.FindAssets(expectedIconName);
                    
                    if (iconGuids.Length > 0)
                    {
                        string iconPath = AssetDatabase.GUIDToAssetPath(iconGuids[0]);
                        Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                        
                        if (icon != null)
                        {
                            itemData.icon = icon;
                            EditorUtility.SetDirty(itemData);
                            fixedCount++;
                            Debug.Log($"[IconGenerator] Fixed icon for {itemData.itemName}");
                        }
                    }
                }
            }
            
            if (fixedCount > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
            Debug.Log($"[IconGenerator] Fixed {fixedCount} missing icons.");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Procedural Icon Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            iconName = EditorGUILayout.TextField("Icon Name", iconName);
            iconColor = EditorGUILayout.ColorField("Icon Color", iconColor);
            iconSize = EditorGUILayout.IntSlider("Icon Size", iconSize, 32, 512);
            
            GUILayout.Space(10);
            
            // Preview
            GUILayout.Label("Preview:", EditorStyles.boldLabel);
            Rect previewRect = GUILayoutUtility.GetRect(64, 64);
            EditorGUI.DrawRect(previewRect, iconColor);
            
            GUILayout.Space(10);
            
            outputPath = EditorGUILayout.TextField("Output Path", outputPath);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Generate Single Icon", GUILayout.Height(30)))
            {
                GenerateSingleIcon();
            }
            
            if (GUILayout.Button("Generate Icons for All Items", GUILayout.Height(30)))
            {
                GenerateIconsForAllItems();
            }
            
            if (GUILayout.Button("Fix Missing Icons", GUILayout.Height(30)))
            {
                FixMissingIcons();
            }
        }
        
        private void GenerateSingleIcon()
        {
            // Ensure directory exists
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            
            Sprite icon = GenerateColoredSprite(iconColor, iconName, iconSize);
            
            if (icon != null)
            {
                Selection.activeObject = icon;
                EditorGUIUtility.PingObject(icon);
                Debug.Log($"[IconGenerator] Generated icon: {iconName}");
            }
        }
        
        /// <summary>
        /// Generates a colored square sprite.
        /// </summary>
        public static Sprite GenerateColoredSprite(Color color, string spriteName, int size = 128)
        {
            string path = $"Assets/UI/Icons/{spriteName}.png";
            
            // Check if icon already exists
            if (AssetDatabase.LoadAssetAtPath<Sprite>(path) != null)
            {
                Debug.Log($"[IconGenerator] Icon already exists: {spriteName}");
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            
            // Create the texture
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            
            // Fill with color
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            
            // Add a subtle border
            int borderWidth = Mathf.Max(1, size / 16);
            Color borderColor = Color.Lerp(color, Color.black, 0.3f);
            
            // Top and bottom borders
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < borderWidth; y++)
                {
                    texture.SetPixel(x, y, borderColor);
                    texture.SetPixel(x, size - 1 - y, borderColor);
                }
            }
            
            // Left and right borders
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < borderWidth; x++)
                {
                    texture.SetPixel(x, y, borderColor);
                    texture.SetPixel(size - 1 - x, y, borderColor);
                }
            }
            
            texture.Apply();
            
            // Save as PNG
            byte[] pngData = texture.EncodeToPNG();
            if (pngData != null)
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllBytes(path, pngData);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                
                // Configure import settings
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 100;
                    importer.mipmapEnabled = false;
                    importer.SaveAndReimport();
                }
                
                // Clean up temporary texture
                Object.DestroyImmediate(texture);
                
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            
            Object.DestroyImmediate(texture);
            return null;
        }
    }
}