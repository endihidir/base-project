using UnityEngine;

namespace UnityBase.GridSystem
{
    public class HexWorldGridTest : MonoBehaviour
    {
        [SerializeField] private int _gridWidth;
        [SerializeField] private int _gridHeight;
        [SerializeField] private int _gridDepth;
        [SerializeField] private Vector3 _cellSize;
        [SerializeField] private Vector3 _cellOffset;
        [SerializeField] private Vector3 _gridOffset;
        [SerializeField] private bool _drawGizmos;
        [SerializeField] private Color _gizmosColor = Color.yellow;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private bool _isPointyTopped = true;
        [SerializeField][Range(0, 10)] private int _activeDepth = 0;

        private PathNode _startNode;
        private Vector3 _previousRot;

        private HexWorldGrid<PathNode> _grid;
        private Camera _cam;
        private Mesh _mesh;

        private void Awake()
        {
            _mesh = new Mesh();
            _meshFilter.mesh = _mesh;
            _previousRot = transform.eulerAngles;
        }

        private void Start()
        {
            _cam = Camera.main;

            _grid = new HexWorldGrid<PathNode>(
                transform,
                _gridWidth,
                _gridHeight,
                _gridDepth,
                _cellSize,
                _gridOffset,
                _cellOffset,
                _drawGizmos,
                _gizmosColor,
                _isPointyTopped
            );

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

                var path = _grid.FindPathWithJobs(_startNode.GridPos, gridPos);

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

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || _grid is not { DrawGizmos: true }) return;

            _grid.DrawGrid();
        }
    }
}