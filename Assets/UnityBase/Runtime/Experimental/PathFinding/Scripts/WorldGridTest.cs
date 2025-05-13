using UnityEngine;

namespace UnityBase.GridSystem
{
    public class WorldGridTest : MonoBehaviour
    {
        #region VARIABLES

        [SerializeField] private bool _useJobSystem;
        [SerializeField] protected int _gridWidth;
        [SerializeField] protected int _gridHeight;
        [SerializeField] protected int _gridDepth;
        [SerializeField] protected Vector3 _cellSize;
        [SerializeField] protected Vector3 _cellOffset;
        [SerializeField] protected Vector3 _gridOffset;
        [SerializeField] protected bool _drawGizmos;
        [SerializeField] protected Color _gizmosColor = Color.yellow;
        [SerializeField] protected MeshFilter _meshFilter;
        [SerializeField][Range(0, 10)] protected int _activeDepth = 0;
        [SerializeField] private bool _allowDiagonalCornerCutting;
        
        private PathNode _startNode;
        private Vector3 _previousRot;

        protected IWorldGrid<PathNode> _grid;

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

            _grid.Initialize(pos => new PathNode
            {
                GridPos = pos,
                IsWalkable = true,
                GCost = int.MaxValue,
                HCost = 0,
                FCost = 0,
                CameFromNodeIndex = -1
            });

            _startNode = _grid.GetFirst(new Vector3Int(0, 0, _activeDepth));
        }

        protected virtual void Init()
        {
            _grid = new WorldGridBuilder<PathNode>().WithTransform(transform)
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
                
                if (!_grid.TryGetNodeFromScreenRay(ray, _activeDepth, out var gridPos)) return;
                
                var endNode = _grid.GetFirst(gridPos);
                
                if (!endNode.IsWalkable || endNode.GridPos == _startNode.GridPos) return;

                if (_useJobSystem)
                {
                    var path = _grid.FindPathWithJobs(_startNode.GridPos, gridPos, _allowDiagonalCornerCutting);
                    
                    if (path.Length > 0)
                    {
                        _startNode = new PathNode()
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
                        _startNode = new PathNode()
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
                
                if (!_grid.TryGetNodeFromScreenRay(ray, _activeDepth, out var gridPos)) return;

                var pathNode = _grid.GetFirst(gridPos);
                
                pathNode.IsWalkable = !pathNode.IsWalkable;
                
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
            
            _grid.DrawGrid();
        }
    }
}