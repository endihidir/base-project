using UnityEngine;

namespace UnityBase.GridSystem
{
    public class UIGridTest : MonoBehaviour
    {
        [Header("Pathfinding Settings")]
        [SerializeField] private bool _useJobSystem = false;
        [SerializeField] private bool _allowDiagonalCornerCutting = false;
        
        [Header("Grid Settings")]
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private float _screenSidePaddingRatio = 10f;
        [SerializeField] private float _cellSpacingRatio = 1f;
        [SerializeField] private Vector3 _originOffset = Vector3.zero;

        [Header("Visuals")]
        [SerializeField] private bool _drawGizmos = true;
        [SerializeField] private Color _gizmosColor = Color.green;
        [SerializeField] private MeshFilter _meshFilter;

        private IUIGrid<GridNode> _grid;
        private GridNode _startNode;
        private Camera _cam;
        private Mesh _mesh;

        private void Awake()
        {
            _mesh = new Mesh();
            _meshFilter.mesh = _mesh;
        }

        private void Start()
        {
            _cam = Camera.main;
            _grid = new UIGrid<GridNode>(_cam, _width, _height, _screenSidePaddingRatio, _cellSpacingRatio, _originOffset, _drawGizmos, _gizmosColor);
            _startNode = _grid.GetGridObject(new Vector3Int(0, 0, 0));
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
                var mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
                var gridPos = _grid.WorldToGrid(mousePos);

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
                    _grid = new UIGrid<GridNode>(Camera.main, _width, _height, _screenSidePaddingRatio, _cellSpacingRatio, _originOffset, _drawGizmos, _gizmosColor);
                    _startNode = _grid.GetGridObject(new Vector3Int(0, 0, 0));
                    return;
                }
                
                _grid.Update(_width, _height, _screenSidePaddingRatio, _cellSpacingRatio, _originOffset, _drawGizmos, _gizmosColor);
            }
            
            _grid.DrawGrid();
        }
    }
}