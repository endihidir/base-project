using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using Cinemachine.Editor;

namespace __Funflare.Scripts.Editor
{
   public class TileEditorWindow : EditorWindow
    {
        private readonly string[] folderPaths =
        {
            "Assets/_Prefabs/Shelves",
            "Assets/_Prefabs/Factories",
            "Assets/__Funflare/Prefabs/Tiles/Buildings",
            "Assets/__Funflare/Prefabs/Tiles/Walls",
            "Assets/__Funflare/Prefabs/Tiles/Decorations",
            "Assets/__Funflare/Prefabs/Tiles/Util",
        };

        private string[] filterNames;
        private string[] FilterNames
        {
            get
            {
                if (filterNames == null || filterNames.Length != folderPaths.Length)
                {
                    filterNames = new string[folderPaths.Length];
                }
                return filterNames;
            }
        }

        private Dictionary<string, GameObject[]> prefabDictionary = new();
        private int selectedTab = 0;
        private string selectedPrefabName;
        private GameObject selectedPrefab;
        private Vector2 scrollPosition;
       // private int gridSize = 100;
        //private int cellSize = 10;
        //private bool showGrid = true;
        private int storeTileIndex;

        [MenuItem("Tools/Tile Editor Window")]
        public static void ShowWindow()
        {
            GetWindow<TileEditorWindow>("Tile Editor Window");
        }

       // private void OnEnable() => LoadPrefabsFromFolders();
        private void OnBecameVisible() => TilePlacementManager.SetWindowVisibility(true);
        private void OnBecameInvisible() => TilePlacementManager.SetWindowVisibility(false);

        /*private void OnGUI()
        {
            GUILayout.Label("Tile Editor", EditorStyles.boldLabel);
            GUILayout.Label($"(C) Free Placement Toggle");
            GUILayout.Label($"(R) Rotate 90");
            GUILayout.Label($"(T) Rotate 45");
            GUILayout.Label($"(ESC) Cancel Placement");
         
            GUILayout.Space(10);
            
            showGrid = EditorGUILayout.ToggleLeft("Show Grid ",showGrid);
            gridSize = EditorGUILayout.IntField("Grid Size", gridSize);
            cellSize = EditorGUILayout.IntField("Cell Size", cellSize);
            
            TilePlacementManager.InitializeGrid(gridSize, cellSize);
            
            GUILayout.BeginHorizontal();
            
            //float buttonY = 95;
            //float buttonY = 0;
            
            /*if (GUILayout.Button( "Clear Prefabs"))
            {
                TilePlacementManager.ClearPrefabs();
            }#1#
            
            //buttonY += 40; 
            
            if (GUILayout.Button( "Refresh Assets"))
            {
                LoadPrefabsFromFolders();
            }
            
            //buttonY += 40; 
            
            if (GUILayout.Button(  "Select Config"))
            {
                var obj = AssetDatabase.LoadAssetAtPath<TileEditorConfig>("Assets/__Funflare/Editor/TileEditorConfig.asset");
                Selection.SetActiveObjectWithContext(obj, Selection.activeContext);
            }
            
            if (selectedPrefab != null && GUILayout.Button(  "Select Prefab"))
            {
                
                Selection.SetActiveObjectWithContext(selectedPrefab, Selection.activeContext);
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            GUILayout.Label("Search : ", EditorStyles.label);
            
            var previousName = FilterNames[selectedTab];
            
            FilterNames[selectedTab] = GUILayout.TextField(FilterNames[selectedTab]);
            
            TilePlacementManager.SetWindowVisibility(showGrid);
            
            GUILayout.Space(10);
            
            var previousTab = selectedTab;
            selectedTab = GUILayout.Toolbar(selectedTab, GetTabNames());

            if (folderPaths.Length > selectedTab && prefabDictionary.ContainsKey(folderPaths[selectedTab]))
            {
                DisplayStoreTileIndex();   
                DisplayPrefabsForTab(folderPaths[selectedTab]);
            }
            
            if (previousTab != selectedTab || (!string.IsNullOrEmpty(previousName) && !previousName.Equals(FilterNames[selectedTab])))
            {
                LoadPrefabsFromFolders();
            }
        }*/

        private string[] GetTabNames()
        {
            List<string> tabNames = new List<string>();
            foreach (var path in folderPaths)
            {
                string folderName = new DirectoryInfo(path).Name;
                tabNames.Add(folderName);
            }
            return tabNames.ToArray();
        }

        private void LoadPrefabsFromFolders()
        {
            prefabDictionary.Clear();
            
            AssetDatabase.StartAssetEditing();

            foreach (var path in folderPaths)
            {
                if (!Directory.Exists(path))
                {
                    Debug.LogWarning($"Folder path not found: {path}");
                    continue;
                }

                string[] prefabPaths = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
                var prefabs = new List<GameObject>();

                foreach (var prefabPath in prefabPaths)
                {
                    string assetPath = prefabPath.Replace("\\", "/").Replace(Application.dataPath, "Assets");
                    
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                    if (!prefab) continue;
                    
                    if (!string.IsNullOrEmpty(FilterNames[selectedTab]) && !prefab.name.Contains(FilterNames[selectedTab], StringComparison.OrdinalIgnoreCase)) continue;

                    /*if (prefab.TryGetComponent(out IUsable usable))
                    {
                        if (usable.OutOfUse) continue;
                            
                        prefabs.Add(prefab);
                    }
                    else
                    {
                        prefabs.Add(prefab);
                    }*/
                }

                prefabDictionary[path] = prefabs.ToArray();
            }
            
            AssetDatabase.StopAssetEditing();
        }

        private void DisplayStoreTileIndex()
        {
            GUILayout.BeginHorizontal(GUILayout.Width(position.width));
            
            storeTileIndex = EditorGUILayout.IntField("Store Tile Index", storeTileIndex);

            GUILayout.EndHorizontal();
        }

        private void DisplayPrefabsForTab(string folderPath)
        {
            scrollPosition = scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height - 160));

            if (prefabDictionary.TryGetValue(folderPath, out GameObject[] prefabs))
            {
                int columns = Mathf.Min(4, Mathf.Max(1, (int)(position.width / 190)));
                int rows = Mathf.CeilToInt((float)prefabs.Length / columns);

                for (int row = 0; row < rows; row++)
                {
                    GUILayout.BeginHorizontal(GUILayout.Width(position.width));
                    
                    for (int col = 0; col < columns; col++)
                    {
                        int index = row * columns + col;
                        if (index >= prefabs.Length)
                            break;

                        GameObject prefab = prefabs[index];
                        
                        
                        float columnWidth = position.width / columns;
                        GUILayout.Space(columnWidth / 3);
                        GUILayout.BeginVertical(GUILayout.Width(columnWidth / 1.75f), GUILayout.Height(120));

                        Rect previewRect = GUILayoutUtility.GetRect(100, 100, GUILayout.ExpandWidth(false));
                        
                        Texture previewTexture = AssetPreview.GetAssetPreview(prefab);
                        
                        if (GUI.Button(previewRect, GUIContent.none))
                        {
                            selectedPrefabName = prefab.name;
                            selectedPrefab = prefab;
                            TilePlacementManager.UpdateSelectedPrefab(prefab,storeTileIndex);
                        }


                        if (selectedPrefabName == prefab.name)
                        {
                            GUI.color = Color.green;
                        }
                        else
                        {
                            GUI.color = Color.white;
                        }
                        GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit);

                        GUILayout.Label(prefab.name.Length > 12 ? prefab.name.Substring(0, 12) + "..." : prefab.name, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
                        
                        GUILayout.Space(20);
                        GUILayout.EndVertical();
                    }
                    

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();
        }
    }
}
