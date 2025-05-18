using UnityBase.Extensions;
using UnityEngine;

namespace UnityBase.GridSystem
{
    public class UIGridTest : MonoBehaviour
    {
        [Header("Pathfinding Settings")]
        [SerializeField] protected bool _useJobSystem = false;
        [SerializeField] protected bool _allowDiagonalCornerCutting = false;
        
        [Header("Grid Settings")]
        [SerializeField] protected int _width;
        [SerializeField] protected int _height;
        [SerializeField] protected float _screenSidePaddingRatio = 10f;
        [SerializeField] protected float _cellSpacingRatio = 1f;
        [SerializeField] protected Vector3 _originOffset = Vector3.zero;

        [Header("Visuals")]
        [SerializeField] protected bool _drawGizmos = true;
        [SerializeField] protected Color _gizmosColor = Color.green;
        [SerializeField] protected MeshFilter _meshFilter;

        protected IUIGrid<GridNode> _grid;
        protected GridNode _startNode;
        private Camera _cam;
        protected Mesh _mesh;

        private void Awake()
        {
            _mesh = new Mesh();
            _meshFilter.mesh = _mesh;
        }

        private void Start()
        {
            _cam = Camera.main;
            Init();
            _startNode = _grid.GetGridObject(new Vector3Int(0, 0, 0));
        }

        protected virtual void Init()
        {
            _grid = new UIGrid<GridNode>(Camera.main, _width, _height, _screenSidePaddingRatio, _cellSpacingRatio, _originOffset, _drawGizmos, _gizmosColor);
        }
        
        private void Update()
        {
            if (_grid == null) return;
            
            _grid.Update(_width, _height, _screenSidePaddingRatio, _cellSpacingRatio, _originOffset, _drawGizmos, _gizmosColor);
            
            if (Input.GetMouseButtonDown(0))
            {
                if (!_grid.TryGetGridObjectFromMousePosition(out var endNode)) return;

                if (!endNode.IsWalkable || endNode.GridPos == _startNode.GridPos) return;

                if (_useJobSystem)
                {
                    var path = _grid.FindPathWithJobs(_startNode.GridPos, endNode.GridPos, _allowDiagonalCornerCutting);
                    
                    if (path.Length > 0)
                    {
                        _startNode = new GridNode
                        {
                            GridPos = endNode.GridPos,
                            IsWalkable = endNode.IsWalkable,
                            GCost = int.MaxValue,
                            HCost = 0,
                            FCost = 0,
                            CameFromNodeIndex = -1
                        };
                        
                        _grid.DebugDrawPath(path, 2f, Color.green);
                    }
                    
                    path.Dispose();
                }
                else
                {
                    var path = _grid.FindPath(_startNode.GridPos, endNode.GridPos, _allowDiagonalCornerCutting);
                    
                    if (path.Count > 0)
                    {
                        _startNode = new GridNode
                        {
                            GridPos = endNode.GridPos,
                            IsWalkable = endNode.IsWalkable,
                            GCost = int.MaxValue,
                            HCost = 0,
                            FCost = 0,
                            CameFromNodeIndex = -1
                        };
                        
                        _grid.DebugDrawPath(path, 2f, Color.green);
                    }
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                var worldPosition = _cam.ScreenToWorldPoint(Input.mousePosition).With(z: _cam.nearClipPlane);
                var gridPos = _grid.WorldToGrid(worldPosition, false);
                if (!_grid.IsInRange(gridPos)) return;
                    
                var node = _grid.GetGridObject(gridPos);
                node.IsWalkable = !node.IsWalkable;
                _grid.SetGridObject(gridPos, node);
                _grid.RebuildMeshVisual(_mesh);
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                if (_grid is not { DrawGizmos: true })
                {
                    Init();                   
                    _startNode = _grid.GetGridObject(new Vector3Int(0, 0, 0));
                    return;
                }
                
                _grid.Update(_width, _height, _screenSidePaddingRatio, _cellSpacingRatio, _originOffset, _drawGizmos, _gizmosColor);
            }
            
            _grid.DrawGrid();
        }
    }
}