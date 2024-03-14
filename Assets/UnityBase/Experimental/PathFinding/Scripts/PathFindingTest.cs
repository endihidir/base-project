using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityBase.GridSystem;

namespace UnityBase.PathFinding
{
    public class PathFindingTest : MonoBehaviour, IGridEntity
    {
        #region VARIABLES

        [SerializeField] private Vector2Int _gridSize;

        [SerializeField] private float _cellSize;

        [ReadOnly] [SerializeField] private Vector3 _centerPos;

        private Grid<PathNode> _grid;

        private PathNode _startNode;

        private PathFinding _pathFinding;

        #endregion

        #region COMPONENTS

        private Camera _cam;

        private Mesh _mesh;

        #endregion

        #region PROPERTIES

        public bool Is2DGrid => true;
        public int Width => _gridSize.x;
        public int Height => _gridSize.y;
        public float NodeSize => _cellSize;
        public float3 Padding => float3.zero;
        public float3 OriginPos => _centerPos;

        #endregion

        private void Awake()
        {
            _mesh = new Mesh();

            GetComponent<MeshFilter>().mesh = _mesh;
        }

        private void Start()
        {
            _cam = Camera.main;

            _centerPos = new Vector3(-Width * _cellSize, -Height * _cellSize) * 0.5f;

            _pathFinding = new PathFinding(this);

            _grid = _pathFinding.GetGrid();

            _startNode = _grid.GetGridObject(_centerPos);
        }

        private void Update()
        {
            var mouseWorldPos = _cam.ScreenToWorldPoint(Input.mousePosition);

            mouseWorldPos.z = 0f;

            if (Input.GetMouseButtonDown(0))
            {
                if (_grid == null) return;

                _grid.Get(mouseWorldPos, out var x, out var y, out var z);

                if (!_grid.IsInRange(x, y)) return;

                var endNode = _grid.GetGridObject(mouseWorldPos);

                if (!endNode.isWalkable || endNode.index == _startNode.index) return;

                var path = _pathFinding.FindPath(_startNode.Pos, endNode.Pos);

                if (path.Length > 0)
                {
                    _startNode = endNode;

                    var startX = _centerPos.x / _cellSize;
                    var startY = _centerPos.y / _cellSize;
                    
                    for (int i = 0; i < path.Length - 1; i++)
                    {
                        var startPos = new Vector3(startX + path[i].x, startY + path[i].y) * _cellSize + Vector3.one * (_cellSize * 0.5f);

                        var endPos = new Vector3(startX + path[i + 1].x, startY + path[i + 1].y) * _cellSize + Vector3.one * (_cellSize * 0.5f);

                        Debug.DrawLine(startPos, endPos, Color.green, 3f);
                    }
                }

                path.Dispose();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                if (_grid == null) return;

                var pathNode = _grid.GetGridObject(mouseWorldPos);

                pathNode.SetWalkable(!pathNode.isWalkable);

                _grid.SetGridObject(mouseWorldPos, pathNode);

                UpdateVisual();
            }
        }

        private void UpdateVisual()
        {
            MeshUtils.CreateEmptyMeshArrays(Width * Height, out var vertices, out var uv, out var triangles);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var grid = _grid.GetGridObject(x, y);

                    var quadSize = grid.isWalkable ? float3.zero : new float3(1, 1, 0) * _cellSize;

                    var pos = _grid.GetWorldPosition(x, y, 0) + quadSize * 0.5f;

                    MeshUtils.AddToMeshArrays(vertices, uv, triangles, grid.index, pos, 0f, quadSize, Vector2.zero, Vector2.zero);
                }
            }

            _mesh.vertices = vertices;
            _mesh.uv = uv;
            _mesh.triangles = triangles;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            if (_grid == null) return;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Gizmos.DrawLine(_grid.GetWorldPosition(x, y, 0), _grid.GetWorldPosition(x, y + 1, 0));
                    Gizmos.DrawLine(_grid.GetWorldPosition(x, y, 0), _grid.GetWorldPosition(x + 1, y, 0));
                }

                Gizmos.DrawLine(_grid.GetWorldPosition(0, Height, 0), _grid.GetWorldPosition(Width, Height, 0));
                Gizmos.DrawLine(_grid.GetWorldPosition(Width, 0, 0), _grid.GetWorldPosition(Width, Height, 0));
            }
        }
    }
}