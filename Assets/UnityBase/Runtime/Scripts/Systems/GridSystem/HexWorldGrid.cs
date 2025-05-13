using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UnityBase.GridSystem
{
    public class HexWorldGrid<T> : WorldGrid<T> where T : struct, IPathNodeData
    {
        private readonly bool _isPointyTopped;

        private static readonly Vector3Int[] _hexOffsetsEven =
        {
            new (+1, 0, 0), new (0, +1, 0), new (-1, +1, 0),
            new (-1, 0, 0), new (-1, -1, 0), new (0, -1, 0)
        };

        private static readonly Vector3Int[] _hexOffsetsOdd =
        {
            new (+1, 0, 0), new (+1, +1, 0), new (0, +1, 0),
            new (-1, 0, 0), new (0, -1, 0), new (+1, -1, 0)
        };

        public HexWorldGrid(Transform coreTransform, int width, int height, int depth, Vector3 cellSize, Vector3 offset, Vector3 cellOffset, bool drawGizmos, Color gizmosColor, bool isPointyTopped = false) : base(coreTransform, width, height, depth, cellSize, offset, cellOffset, drawGizmos, gizmosColor)
        {
            _isPointyTopped = isPointyTopped;
        }
   
        public override Vector3 GridToWorld3(Vector3Int gridPos)
        {
            var localPos = CalculateLocalPosition(gridPos);
            var centeredPos = localPos - CalculateGridCenterOffset();
            return Transform.TransformPoint(centeredPos + GridOffset);
        }

        private Vector3 CalculateGridCenterOffset()
        {
            var centerGridPos = new Vector3Int(Width / 2, Height / 2, Depth / 2);
            return CalculateLocalPosition(centerGridPos);
        }

        private Vector3 CalculateLocalPosition(Vector3Int gridPos)
        {
            if (!_isPointyTopped)
            {
                var xSpacing = (CellSize.x + CellOffset.x) * Mathf.Sqrt(3f) / 2f;
                var ySpacing = (CellSize.y + CellOffset.y);

                var x = gridPos.x * xSpacing;
                var z = (gridPos.y + 0.5f * (gridPos.x & 1)) * ySpacing;
                var y = (gridPos.z + 0.5f) * (CellSize.z + CellOffset.z);

                return new Vector3(x, y, z);
            }
            else
            {
                var xSpacing = (CellSize.x + CellOffset.x);
                var ySpacing = (CellSize.y + CellOffset.y) * Mathf.Sqrt(3f) / 2f;

                var x = (gridPos.x + 0.5f * (gridPos.y & 1)) * xSpacing;
                var z = gridPos.y * ySpacing;
                var y = (gridPos.z + 0.5f) * (CellSize.z + CellOffset.z);

                return new Vector3(x, y, z);
            }
        }

        public override Vector3Int WorldToGrid3(Vector3 position, bool clamp = true)
        {
            var localPos = Transform.InverseTransformPoint(position) - GridOffset + CalculateGridCenterOffset();
            var xSpacing = (CellSize.x + CellOffset.x);
            var ySpacing = (CellSize.y + CellOffset.y);
            var zStep = (CellSize.z + CellOffset.z);

            int x, y, z;

            if (!_isPointyTopped)
            {
                xSpacing *= Mathf.Sqrt(3f) / 2f;
                var q = localPos.x / xSpacing;
                var isOdd = Mathf.RoundToInt(q) % 2 != 0;
                var r = localPos.z / ySpacing - (isOdd ? 0.5f : 0f);
        
                var rounded = RoundAxial(new Vector2(q, r));
                x = Mathf.RoundToInt(rounded.x);
                y = Mathf.RoundToInt(rounded.y);
            }
            else
            {
                ySpacing *= Mathf.Sqrt(3f) / 2f;
                var r = localPos.z / ySpacing;
                var isOdd = Mathf.RoundToInt(r) % 2 != 0;
                var q = localPos.x / xSpacing - (isOdd ? 0.5f : 0f);
        
                var rounded = RoundAxial(new Vector2(q, r));
                x = Mathf.RoundToInt(rounded.x);
                y = Mathf.RoundToInt(rounded.y);
            }

            z = Mathf.FloorToInt(localPos.y / zStep);

            if (clamp)
            {
                x = Mathf.Clamp(x, 0, Width - 1);
                y = Mathf.Clamp(y, 0, Height - 1);
                z = Mathf.Clamp(z, 0, Depth - 1);
            }

            return new Vector3Int(x, y, z);
        }

        private Vector2 RoundAxial(Vector2 axial)
        {
            var cube = new Vector3(axial.x, -axial.x - axial.y, axial.y);
            var roundedCube = new Vector3(
                Mathf.Round(cube.x),
                Mathf.Round(cube.y),
                Mathf.Round(cube.z)
            );

            var dx = Mathf.Abs(roundedCube.x - cube.x);
            var dy = Mathf.Abs(roundedCube.y - cube.y);
            var dz = Mathf.Abs(roundedCube.z - cube.z);

            if (dx > dy && dx > dz)
                roundedCube.x = -roundedCube.y - roundedCube.z;
            else if (dy > dz)
                roundedCube.y = -roundedCube.x - roundedCube.z;
            else
                roundedCube.z = -roundedCube.x - roundedCube.y;

            return new Vector2(roundedCube.x, roundedCube.z);
        }

        protected override IEnumerable<Vector3Int> GetFilteredOffsets(Vector3Int gridPos, bool includeDepth, bool includeDiagonal)
        {
            if (!includeDepth)
            {
                return !_isPointyTopped
                    ? (gridPos.x & 1) == 0 ? _hexOffsetsEven : _hexOffsetsOdd
                    : (gridPos.y & 1) == 0 ? _hexOffsetsEven : _hexOffsetsOdd;
            }
            
            return base.GetFilteredOffsets(gridPos, includeDepth, includeDiagonal);
        }
        
        public override void DrawGrid()
        {
            if (!DrawGizmos) return;

            Gizmos.color = _gizmosColor;

            int total = Width * Height * Depth;

            for (int i = 0; i < total; i++)
            {
                int x = i % Width;
                int y = (i / Width) % Height;
                int z = i / (Width * Height);

                var pos = new Vector3Int(x, y, z);
                DrawHighlightedCell(pos, _gizmosColor);
            }
        }

        public override void DrawHighlightedCell(Vector3Int gridPos, Color highlightColor)
        {
            var center = GridToWorld3(gridPos);
            var radius = (CellSize.x + CellOffset.x) / Mathf.Sqrt(3f);
            var halfHeight = (CellSize.z + CellOffset.z) * 0.5f;

            var cornersTop = new Vector3[6];
            var cornersBottom = new Vector3[6];

            for (int i = 0; i < 6; i++)
            {
                var angle = _isPointyTopped ? Mathf.Deg2Rad * (60 * i - 30) : Mathf.Deg2Rad * (60 * i);
                var cos = Mathf.Cos(angle);
                var sin = Mathf.Sin(angle);

                var offset = (radius * cos * Transform.right) + (radius * sin * Transform.forward);
                cornersTop[i] = center + offset + (Transform.up * halfHeight);
                cornersBottom[i] = center + offset - (Transform.up * halfHeight);
            }

            Gizmos.color = highlightColor;
            
            for (int i = 0; i < 6; i++)
                Gizmos.DrawLine(cornersTop[i], cornersTop[(i + 1) % 6]);
            
            for (int i = 0; i < 6; i++)
                Gizmos.DrawLine(cornersBottom[i], cornersBottom[(i + 1) % 6]);
            
            for (int i = 0; i < 6; i++)
                Gizmos.DrawLine(cornersTop[i], cornersBottom[i]);
        }

        public override void RebuildMeshVisual(Mesh mesh)
        {
            var cellCount = Width * Height * Depth;

            MeshUtils.CreateEmptyMeshArraysHex3D(cellCount, out var vertices, out var uv, out var triangles);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        var node = GetFirst(pos);
                        if (node.Equals(default(T))) continue;
                        var size = node.IsWalkable ? Vector3.zero : CellSize;
                        var worldPos = GridToWorld3(pos);
                        int index = CalculateIndex(pos);
                        MeshUtils.AddToMeshArraysHex3D(vertices, uv, triangles, index, worldPos, (size.x + CellOffset.x) / Mathf.Sqrt(3f),size.y, Transform, !_isPointyTopped);
                    }
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
        }

        public override List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos, bool allowDiagonalCornerCutting = false)
        {
            var totalSize = Width * Height * Depth;
            var pathNodeArray = new T[totalSize];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        pathNodeArray[CalculateIndex(pos)] = GetFirst(pos);
                    }
                }
            }

            var pathFinder = new FindPathHex<T>
            {
                GridSize = new int3(Width, Height, Depth),
                PathNodeArray = pathNodeArray,
                StartPos = startPos,
                EndPos = endPos,
                AllowDiagonalCornerCutting = allowDiagonalCornerCutting,
                IsPointyTopped = _isPointyTopped
            };

            return pathFinder.Execute();
        }

        public override NativeList<Vector3Int> FindPathWithJobs(Vector3Int startPos, Vector3Int endPos, bool allowDiagonalCornerCutting = false)
        {
            var totalSize = Width * Height * Depth;
            var pathNodeArray = new NativeArray<T>(totalSize, Allocator.TempJob);
            var result = new NativeList<Vector3Int>(Allocator.TempJob);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        pathNodeArray[CalculateIndex(new Vector3Int(x,y,z))] = GetFirst(pos);
                    }
                }
            }

            var job = new FindPathHexJob<T>
            {
                pathNodeArray = pathNodeArray,
                gridSize = new int3(Width, Height, Depth),
                startPos = startPos,
                endPos = endPos,
                calculatedPathList = result,
                allowDiagonalCornerCutting = allowDiagonalCornerCutting,
                isPointyTopped = _isPointyTopped
            };

            job.Schedule().Complete();
            pathNodeArray.Dispose();
            return result;
        }
    }
    
    public class HexWorldGridBuilder<T> : WorldGridBuilder<T> where T : struct, IPathNodeData
    {
        private bool _isPointyTopped = true;

        public HexWorldGridBuilder<T> WithIsPointyTopped(bool isPointyTopped)
        {
            _isPointyTopped = isPointyTopped;
            return this;
        }

        public override IWorldGrid<T> Build()
        {
            if (_transform == null || !_width.HasValue || !_height.HasValue || !_depth.HasValue || !_cellSize.HasValue)
            {
                Debug.LogError("HexWorldGridBuilder: Missing required parameters.");
                return default;
            }

            return new HexWorldGrid<T>(
                _transform,
                _width.Value,
                _height.Value,
                _depth.Value,
                _cellSize.Value,
                _offset,
                _cellOffset,
                _drawGizmos,
                _gizmosColor,
                _isPointyTopped
            );
        }
    }
}