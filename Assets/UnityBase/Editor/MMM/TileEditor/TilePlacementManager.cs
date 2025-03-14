using System;
using System.Collections.Generic;
using System.Linq;
//using __Funflare.Scripts.Extensions;
//using DG.DemiEditor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace __Funflare.Scripts.Editor
{
    public static class TilePlacementManager
    {
        private static GameObject selectedPrefab;
        private static GameObject draggedObject;
        private static int tile = 10;
        private static int cell = 1;
        private static bool isMouseInsideScene;
        private static bool _isWindowVisible;
        private static bool _isDragging;
        private static float _placementRotation;

        public static bool IsFreePlacement => _isFreePlacement;
        
        private static Vector3[] _lines;
        private static int _storeTileIndex;
        private static List<Vector3> _brushPlacementHistory = new List<Vector3>();

        //private static MatActivateController _placingActivator;

        private static Vector3 _offetToLowerVisual;

        private static TileEditorConfig _tileEditorConfig;
        
        private static bool _isFreePlacement;

        [InitializeOnLoadMethod]
        private static void RegisterSceneViewEvent()
        {
            //SceneView.duringSceneGui += OnSceneGUI;
            
            //_tileEditorConfig = AssetDatabase.LoadAssetAtPath<TileEditorConfig>("Assets/__Funflare/Editor/TileEditorConfig.asset");
        }

        public static void UpdateSelectedPrefab(GameObject prefab, int storeTileIndex)
        {
            if (draggedObject != null)
            {
                Object.DestroyImmediate(draggedObject);
            }

            _storeTileIndex = storeTileIndex;
            selectedPrefab = prefab;
            draggedObject = InstantiatePrefab(selectedPrefab);
            draggedObject.SetActive(false);

            SetObjectPicking(draggedObject, false);
        }

        private static string FindUniqueIdentifierForBaseName(string baseName)
        {
            for (int i = 1; i <= 512; i++)
            {
                var nextName = string.Concat(baseName, "_", i.ToString());
                if (GameObject.Find(nextName) == null)
                {
                    return nextName;
                }
            }

            return baseName + "_FIXME!";
        }

        private static Vector3 CalculateOffsetToLowerVisual(GameObject gameObject)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>();

            var lowerY = float.MaxValue;
            var lowerIndex = -1;
            
            
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].transform.position.y < lowerY)
                {
                    lowerIndex = i;
                    lowerY = renderers[i].transform.position.y;
                }
            }

            if (lowerIndex < 0)
                return Vector3.zero;


            return new Vector3(0, gameObject.transform.position.y - lowerY, 0);
        }

        private static GameObject InstantiatePrefab(GameObject pf)
        {
            /*var result = PrefabUtility.InstantiatePrefab(pf) as GameObject;
            result.name = FindUniqueIdentifierForBaseName(result.name);

            if (_tileEditorConfig.TryGet(pf, out var objectConfig) && !objectConfig.disableAutoVisualOffset 
                || _tileEditorConfig.TryGet(pf, out var _) == false)
            {
                _offetToLowerVisual = CalculateOffsetToLowerVisual(result);
            }
            else
            {
                _offetToLowerVisual = Vector3.zero;
            }
            
            
            ValidateStoreTileHierarchy(result);

            if (pf.TryGetComponent<MatActivateController>(out var _))
            {
                _placementRotation = 0;
            }
            
            result.transform.rotation = Quaternion.AngleAxis(_placementRotation, Vector3.up);

            if (_placingActivator != null)
            {
                Object.DestroyImmediate(_placingActivator.gameObject);
            }

            if (result.TryGetComponent<MasterController>(out var mc) && mc is not MatActivateController)
            {
                _placingActivator = PrefabUtility.InstantiatePrefab(_tileEditorConfig.pfMatActivateController) as MatActivateController;
                _placingActivator.name = string.Concat(_placingActivator.name, result.name);
                _placingActivator.objectToActivateList = new GameObject[]
                {
                    result
                };

                ValidateStoreTileHierarchy(_placingActivator.gameObject);
            }


            return result;*/
            return null;
        }

        private static void ValidateStoreTileHierarchy(GameObject gameObject)
        {
            /*var sceneRoot = GameObject.Find("BigMartRoot");
            if (sceneRoot == null)
            {
                sceneRoot = new GameObject("BigMartRoot");
            }


            var storeTileRoot = sceneRoot.transform.Find($"store_tile_{_storeTileIndex}");
            
            StoreTileController storeTileController = null;
            
            if (storeTileRoot == null)
            {
                var storeTile = new GameObject($"store_tile_{_storeTileIndex}");
                storeTile.transform.SetParent(sceneRoot.transform);
                storeTileRoot = storeTile.transform;
            }
            if (_storeTileIndex != 0)
            {
                if (storeTileRoot.TryGetComponent<StoreTileController>(out var stc))
                {
                    storeTileController = stc;
                }
                else
                {
                    storeTileRoot.gameObject.AddComponent<StoreTileController>();
                    storeTileController = storeTileRoot.gameObject.GetComponent<StoreTileController>();
                    storeTileController.modelTransformList = Array.Empty<Transform>();
                }
            }

            if (_tileEditorConfig.TryGet(selectedPrefab, out var config))
            {
                var category = storeTileRoot.Find(config.placementCategory);
                if (category == null)
                {
                    var categoryGameObject = new GameObject(config.placementCategory);
                    categoryGameObject.transform.SetParent(storeTileRoot.transform);
                    category = categoryGameObject.transform;
                }

                l_ValidateStoreTile(category);

                gameObject.transform.SetParent(category);
                return;
            }

            if (gameObject.TryGetComponent<MasterController>(out var _))
            {
                var stationsRoot = storeTileRoot.Find("stations");
                if (stationsRoot == null)
                {
                    var stationRootGameObject = new GameObject($"stations");
                    stationRootGameObject.transform.SetParent(storeTileRoot.transform);
                    stationsRoot = stationRootGameObject.transform;
                }

                l_ValidateStoreTile(stationsRoot);

                gameObject.transform.SetParent(stationsRoot);
            }
            else
            {
                var other = storeTileRoot.Find("other");
                if (other == null)
                {
                    var otherRootGameObject = new GameObject($"other");
                    otherRootGameObject.transform.SetParent(storeTileRoot.transform);
                    other = otherRootGameObject.transform;
                }

                l_ValidateStoreTile(other);

                gameObject.transform.SetParent(other);
            }

            void l_ValidateStoreTile(Transform source)
            {
                if (_storeTileIndex != 0 && source != null && storeTileController != null &&
                    !storeTileController.modelTransformList.Contains(source))
                {
                    DeEditorUtils.Array.ExpandAndAdd(
                        ref storeTileController.modelTransformList,
                        source
                    );
                }
            }*/
        }

        public static void InitializeGrid(int tileSize, int cellSize)
        {
            tile = tileSize;
            cell = cellSize;
            _lines = new Vector3[(tile + 1) * 4];
            SceneView.RepaintAll();
        }

        public static void ClearPrefabs()
        {
            /*foreach (var shelfController in Object.FindObjectsOfType<ShelfController>(true))
            {
                Object.DestroyImmediate(shelfController.gameObject);
            }*/
        }

        public static void SetWindowVisibility(bool isVisible) => _isWindowVisible = isVisible;

        private static void OnSceneGUI(SceneView sceneView)
        {
            /*if (!_isWindowVisible || tile < 2 || cell <= 0) return;
            
            DrawGrid();

            if (selectedPrefab == null || draggedObject == null) return;

            Event e = Event.current;
            Vector3? mouseGridPosition = GetMouseGridPosition();


            if (e.isKey && e.keyCode == KeyCode.Escape)
            {
                SetObjectPicking(draggedObject, true);
                draggedObject.SetActive(false);
                Object.DestroyImmediate(draggedObject);
                selectedPrefab = null;
                draggedObject = null;

                if (_placingActivator != null)
                {
                    Object.DestroyImmediate(_placingActivator.gameObject);
                    _placingActivator = null;
                }

                return;
            }

            if (e.isKey && e.type == EventType.KeyDown && e.keyCode == KeyCode.C)
            {
                _isFreePlacement = !_isFreePlacement;
            }

            if (e.isKey && e.type == EventType.KeyDown && e.keyCode == KeyCode.R && !draggedObject.TryGetComponent<MatActivateController>(out var _))
            {
                _placementRotation += 90f;
                draggedObject.transform.rotation = Quaternion.AngleAxis(_placementRotation, Vector3.up);
            }

            if (e.isKey && e.type == EventType.KeyDown && e.keyCode == KeyCode.T && !draggedObject.TryGetComponent<MatActivateController>(out var _))
            {
                _placementRotation += 45f;
                draggedObject.transform.rotation = Quaternion.AngleAxis(_placementRotation, Vector3.up);
            }

            isMouseInsideScene = mouseGridPosition.HasValue && IsPositionInsideGrid(mouseGridPosition.Value);

            if (isMouseInsideScene)
            {
                draggedObject.SetActive(true);
                UpdateDraggedObjectPosition(mouseGridPosition);
            }
            else
            {
                draggedObject.SetActive(false);
            }
            
            Handles.Label(draggedObject.transform.position + Vector3.up, _storeTileIndex.ToString(), new GUIStyle()
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState()
                {
                    textColor = Color.red
                }
            });

            if ((e.type == EventType.MouseDown && e.button == 0) || (e.isKey && e.keyCode == KeyCode.E))
            {
                var canPlace = true;
                if (e.isKey && e.keyCode == KeyCode.E)
                {
                    if (HasObjectNearby(draggedObject))
                    {
                        canPlace = false;
                    }
                }

                if (canPlace)
                {
                    if (isMouseInsideScene)
                    {
                        Undo.RegisterCreatedObjectUndo(draggedObject, "obj init");

                        if (_placingActivator != null)
                        {
                            Undo.RegisterCreatedObjectUndo(_placingActivator.gameObject, "obj init activator");
                        }

                        _placingActivator = null; //So it wont get deleted when next is initialized

                        draggedObject = InstantiatePrefab(selectedPrefab);
                        draggedObject.SetActive(false);
                        SetObjectPicking(draggedObject, false);
                    }
                    else
                    {
                        Object.DestroyImmediate(draggedObject);
                    }
                }
            }

            SceneView.RepaintAll();*/
        }

        private static void ClearCurrentDragged()
        {
            /*if (draggedObject != null)
            {
                Object.DestroyImmediate(draggedObject);
                draggedObject = null;
            }

            if (_placingActivator != null)
            {
                Object.DestroyImmediate(_placingActivator.gameObject);
                _placingActivator = null;
            }*/
        }

        private static bool HasObjectNearby(GameObject gameObject)
        {
            var pos = gameObject.transform.position;
            var storeTileRoot = GameObject.Find($"store_tile_{_storeTileIndex}");
            if (storeTileRoot == null)
            {
                return false;
            }

            var categoryName = "other";
            if (_tileEditorConfig.TryGet(selectedPrefab, out var objectConfig))
            {
                categoryName = objectConfig.placementCategory;
            }

            var otherRoot = storeTileRoot.transform.Find(categoryName);
            if (otherRoot == null)
            {
                return false;
            }

            var otherRootChildCount = otherRoot.childCount;

            for (int i = 0; i < otherRootChildCount; i++)
            {
                if (otherRoot.GetChild(i).gameObject.Equals(gameObject))
                    continue;

                var d = (pos - otherRoot.GetChild(i).transform.position).sqrMagnitude;

                if (d < cell * cell)
                {
                    return true;
                }
            }

            return false;
        }

        private static void DrawGrid()
        {
            if (_lines == null || _lines.Length < 1) return;

            Handles.color = new Color(1f, 0.98f, 0.44f, 0.44f);

            var halfSize = tile * cell / 2f;

            var index = 0;

            for (float i = -halfSize; i <= halfSize; i += cell)
            {
                var lineStartX = new Vector3(-halfSize, 0, i);
                var lineEndX = new Vector3(halfSize, 0, i);

                var lineStartZ = new Vector3(i, 0, -halfSize);
                var lineEndZ = new Vector3(i, 0, halfSize);

                _lines[index++] = lineStartX;
                _lines[index++] = lineEndX;
                _lines[index++] = lineStartZ;
                _lines[index++] = lineEndZ;
            }

            Handles.DrawLines(_lines);
        }

        private static bool IsMouseOverCell(Vector3 cellCenter)
        {
            if (!isMouseInsideScene) return false;

            Vector3? mouseGridPosition = GetMouseGridPosition();

            if (!mouseGridPosition.HasValue) return false;

            return Vector3.Distance(mouseGridPosition.Value, cellCenter) < cell * 0.5f;
        }

        private static Vector3? GetMouseGridPosition()
        {
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            Plane plane = new Plane(Vector3.up, Vector3.zero);

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 worldPosition = ray.GetPoint(enter);
                return SnapToGrid(worldPosition);
            }

            return null;
        }

        private static Vector3? GetMouseRayPosition()
        {
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            Plane plane = new Plane(Vector3.up, Vector3.zero);

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 worldPosition = ray.GetPoint(enter);
                return worldPosition;
            }

            return null;
        }

        private static Vector3 SnapToGrid(Vector3 position)
        {
            float x = Mathf.Floor(position.x / cell) * cell;
            float z = Mathf.Floor(position.z / cell) * cell;
            return new Vector3(x, 0, z);
        }

        private static void UpdateDraggedObjectPosition(Vector3? mouseGridPosition)
        {
            /*if (draggedObject == null || !mouseGridPosition.HasValue)
                return;

            if (_isFreePlacement)
            {
                mouseGridPosition = GetMouseRayPosition();
            }

            draggedObject.transform.position = mouseGridPosition.Value + _offetToLowerVisual;

            if (_placingActivator != null)
            {
                _placingActivator.transform.position = mouseGridPosition.Value + Vector3.back * 10;
            }*/
        }

        private static Vector2Int GetGridIndex(Vector3 position)
        {
            int x = Mathf.FloorToInt((position.x + (tile * cell / 2f)) / cell);
            int y = Mathf.FloorToInt((position.z + (tile * cell / 2f)) / cell);

            return new Vector2Int(x, y);
        }

        private static bool IsPositionInsideGrid(Vector3 position)
        {
            Vector2Int gridIndex = GetGridIndex(position);
            return gridIndex.x >= 0 && gridIndex.x < tile && gridIndex.y >= 0 && gridIndex.y < tile;
        }

        private static void SetObjectPicking(GameObject gameObject, bool picking)
        {
            if (picking)
            {
                SceneVisibilityManager.instance.EnableAllPicking();
                /* SceneVisibilityManager.instance.EnablePicking(gameObject, true);
                 for (int i = 0; i < gameObject.transform.childCount; i++)
                 {
                     SceneVisibilityManager.instance.EnablePicking(gameObject.transform.GetChild(i).gameObject, true);
                 }*/
            }
            else
            {
                SceneVisibilityManager.instance.DisableAllPicking();
                /*SceneVisibilityManager.instance.DisablePicking(gameObject, true);
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    SceneVisibilityManager.instance.DisablePicking(gameObject.transform.GetChild(i).gameObject, true);
                }*/
            }
        }
    }
}