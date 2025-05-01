using UnityEngine;
using UnityBase.GridSystem;

namespace UnityBase.PathFinding
{
    public class PathFindingTest : MonoBehaviour
    {
        #region VARIABLES

        [SerializeField] private int _gridWidth;
        [SerializeField] private int _gridHeight;
        [SerializeField] private int _gridDepth;
        [SerializeField] private float _gridCellSize;
        [SerializeField] private float _gridCellDepth;
        [SerializeField] private Vector3 _gridOffset;
        [SerializeField] private bool _drawGizmos;
        [SerializeField] private Color _gizmosColor = Color.yellow;
        [SerializeField] private MeshFilter _meshFilter;
        
        private IWorldGrid<PathNode> _grid;

        private PathNode _startNode;
        private PathFinding _pathFinding;

        private Vector3 _previousRot;

        #endregion

        #region COMPONENTS

        private Camera _cam;
        private Mesh _mesh;

        #endregion

        private void Awake()
        {
            _mesh = new Mesh();
            _meshFilter.mesh = _mesh;
            _previousRot = transform.eulerAngles;
        }

        private void Start()
        {
            _cam = Camera.main;
            
            _grid = new WorldGrid<PathNode>(transform, _gridWidth, _gridHeight, _gridDepth, _gridCellSize, _gridCellDepth, _gridOffset, _drawGizmos, _gizmosColor);

            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    for (int z = 0; z < _grid.Depth; z++)
                    {
                        var pos = new Vector3Int(x, y, z);

                        var nodeData = new PathNode
                        {
                            gridPos = pos,
                            isWalkable = true,
                            gCost = int.MaxValue,
                            hCost = 0,
                            fCost = 0,
                            cameFromNodeIndex = -1
                        };

                        _grid.SetFirst(pos, nodeData);
                    }
                }
            }
            
            _pathFinding = new PathFinding(_grid);
            _startNode = _grid.GetFirst(new Vector3Int(0,0,0));
        }
        
        private void Update()
        {
            if (_grid == null) return;
            
            _grid.Update(_gridWidth, _gridHeight, _gridDepth, _gridCellSize, _gridCellDepth, _gridOffset, _drawGizmos, _gizmosColor);
            
            if (Input.GetMouseButtonDown(0))
            {

                var ray = _cam.ScreenPointToRay(Input.mousePosition);
                var plane = new Plane(transform.up, transform.position);

                if (!plane.Raycast(ray, out var enter)) return;
                
                var point = ray.GetPoint(enter);
                
                if (!_grid.IsInRange2(point)) return;
                
                var gridPos = _grid.WorldToGrid3(point);
                    
                var endNode = _grid.GetFirst(gridPos);
                if (!endNode.isWalkable || endNode.gridPos == _startNode.gridPos) return;

                var path = _pathFinding.FindPathJobs(_startNode.gridPos, endNode.gridPos);

                if (path.Length > 0)
                {
                    _startNode = endNode;

                    for (int i = 0; i < path.Length - 1; i++)
                    {
                        var startPos = _grid.GridToWorld(path[i]);
                        var endPos = _grid.GridToWorld(path[i + 1]);

                        Debug.DrawLine(startPos, endPos, Color.green, 3f);
                    }
                }

                path.Dispose();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                var ray = _cam.ScreenPointToRay(Input.mousePosition);
                var plane = new Plane(transform.up, transform.position);
                
                if (!plane.Raycast(ray, out var enter)) return;

                var point = ray.GetPoint(enter);

                if (!_grid.IsInRange2(point)) return;
                
                var gridPos = _grid.WorldToGrid3(point);
                
                var pathNode = _grid.GetFirst(gridPos);

                pathNode.isWalkable = !pathNode.isWalkable;
                _grid.SetFirst(gridPos, pathNode);
                
                UpdateVisual();
            }
            
            var dist = transform.eulerAngles - _previousRot;
            
            if (dist.magnitude > 1f)
            {
                _previousRot = transform.eulerAngles;
                UpdateVisual();
            }
        }

        private void UpdateVisual()
        {
            int width = _grid.Width;
            int height = _grid.Height;
            int depth = _grid.Depth;

            int cellCount = width * height * depth;
            MeshUtils.CreateEmptyMeshArrays(cellCount, out var vertices, out var uv, out var triangles);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        var grid = _grid.GetFirst(pos);

                        var quadSize = grid.isWalkable ? Vector3.zero : new Vector3(1, 1, 0) * _grid.CellSize;
                        var worldPos = _grid.GridToWorld(pos);

                        int index = x + y * width + z * width * height;

                        MeshUtils.AddToMeshArrays2(vertices, uv, triangles, index, worldPos, quadSize, Vector2.zero, Vector2.zero, transform);
                    }
                }
            }

            _mesh.vertices = vertices;
            _mesh.uv = uv;
            _mesh.triangles = triangles;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || _grid is not { DrawGizmos: true }) return;
            
            _grid.DrawGrid();
        }
    }
}