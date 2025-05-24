using UnityEngine;

namespace UnityBase.GridSystem
{
    public class WorldGridTest : MonoBehaviour
    {
        #region VARIABLES

        [SerializeField] private bool _useJobSystem;
        [SerializeField] protected MeshFilter _meshFilter;
        
        [SerializeField] protected int _gridWidth;
        [SerializeField] protected int _gridHeight;
        [SerializeField] protected int _gridDepth;
        [SerializeField] protected Vector3 _cellSize;
        [SerializeField] protected Vector3 _cellOffset;
        [SerializeField] protected Vector3 _gridOffset;
        [SerializeField][Range(0, 10)] protected int _activeDepth = 0;
        
        [SerializeField] protected bool _drawGizmos;
        [SerializeField] protected Color _gizmosColor = Color.yellow;
        
        [SerializeField] private Direction2D[] _direction2Ds;
        [SerializeField] private DepthDirection[] _depthDirections;
        [SerializeField] private bool _drawNeighbours;

        [SerializeField] private bool _allowDiagonalCornerCutting;
        [SerializeField] private bool _clamp;
        
        private GridNode _startNode;
        private Vector3 _previousRot;

        protected IWorldGrid<GridNode> _grid;
        
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
            
            Init();

            _startNode = _grid.GetGridObject(new Vector3Int(0, 0, _activeDepth));
        }

        protected virtual void Init()
        {
            _grid = new WorldGridBuilder<GridNode>().WithTransform(transform)
                .WithSize(_gridWidth, _gridHeight, _gridDepth)
                .WithCellSize(_cellSize)
                .WithOffset(_gridOffset)
                .WithGizmos(_drawGizmos, _gizmosColor)
                .Build();
        }

        private void Update()
        {
            if (_grid == null) return;
            
            _grid.Update(_gridWidth, _gridHeight, _gridDepth, _cellSize, _gridOffset, _cellOffset, _drawGizmos, _gizmosColor);
            
            if (Input.GetMouseButtonDown(0))
            {
                var ray = _cam.ScreenPointToRay(Input.mousePosition);
                
                if (!_grid.TryGetNodeFromScreenRay(ray, _activeDepth, out var gridPos, _clamp)) return;
                
                var endNode = _grid.GetGridObject(gridPos);
                
                if (!endNode.IsWalkable || endNode.GridPos == _startNode.GridPos) return;

                if (_useJobSystem)
                {
                    var path = _grid.FindPathWithJobs(_startNode.GridPos, gridPos, _allowDiagonalCornerCutting);
                    
                    if (path.Length > 0)
                    {
                        _startNode = new GridNode()
                        {
                            GridPos = gridPos,
                            IsWalkable = endNode.IsWalkable,
                            GCost = int.MaxValue,
                            HCost = 0,
                            FCost = 0,
                            CameFromNodeIndex = -1
                        };

                        _grid.DebugDrawPath(path, 3f, Color.green);
                    }
                    
                    path.Dispose();
                }
                else
                {
                    var path = _grid.FindPath(_startNode.GridPos, gridPos, _allowDiagonalCornerCutting);
                    
                    if (path.Count > 0)
                    {
                        _startNode = new GridNode()
                        {
                            GridPos = gridPos,
                            IsWalkable = endNode.IsWalkable,
                            GCost = int.MaxValue,
                            HCost = 0,
                            FCost = 0,
                            CameFromNodeIndex = -1
                        };

                        _grid.DebugDrawPath(path, 3f, Color.green);
                    }
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                var ray = _cam.ScreenPointToRay(Input.mousePosition);
                
                if (!_grid.TryGetNodeFromScreenRay(ray, _activeDepth, out var gridPos, _clamp)) return;

                var pathNode = _grid.GetGridObject(gridPos);
                
                pathNode.IsWalkable = !pathNode.IsWalkable;
                
                _grid.SetGridObject(gridPos, pathNode);

                UpdateVisual();
            }

            var dist = transform.eulerAngles - _previousRot;
            
            if (dist.magnitude > 1f)
            {
                _previousRot = transform.eulerAngles;
                UpdateVisual();
            }
        }

        private void UpdateVisual() => _grid.RebuildMeshVisual(_mesh);

        protected virtual void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                if (_grid is not { DrawGizmos: true })
                {
                    Init();
                    return;
                }
                
                _grid.Update(_gridWidth, _gridHeight, _gridDepth, _cellSize, _gridOffset, _cellOffset, _drawGizmos, _gizmosColor);
            }
            
            NeighbourTest();
            
            _grid.DrawGrid();
        }

        private void NeighbourTest()
        {
            if (!_drawNeighbours || !Application.isPlaying) return;
            
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
                
            if (!_grid.TryGetNodeFromScreenRay(ray, _activeDepth, out var gridPos, _clamp)) return;
                
            for (int i = 0; i < _direction2Ds.Length; i++)
            {
                for (int j = 0; j < _depthDirections.Length; j++)
                {
                    if (_grid.TryGetNeighbor(gridPos, _direction2Ds[i], out var node, _depthDirections[j]))
                    {
                        var worldPos = _grid.GridToWorld(node.GridPos);
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(worldPos, Vector3.one);
                    }
                }
            }
        }
    }
}