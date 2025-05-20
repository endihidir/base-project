using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UnityBase.GridSystem
{
    public class HexWorldGrid<T> : WorldGrid<T> where T : struct, IGridNodeData
    {
        private const float SQRT3 = 1.73205f; // Mathf.Sqrt(3f)
        private const float HEX_INNER_RADIUS_FACTOR = 0.866025f; // Mathf.Sqrt(3f) / 2f
        
        private readonly bool _isPointyTopped;
        
        private static readonly Dictionary<Direction2D, int> _pointyToppedDirections = new()
        {
            { Direction2D.Right, 0 },
            { Direction2D.RightUp, 1 },
            { Direction2D.LeftUp, 2 },
            { Direction2D.Left, 3 },
            { Direction2D.LeftDown, 4 },
            { Direction2D.RightDown, 5 }
        };

        private static readonly Dictionary<Direction2D, int> _flatToppedDirections = new()
        {
            { Direction2D.Up, 0 },
            { Direction2D.RightUp, 1 },
            { Direction2D.LeftUp, 2 },
            { Direction2D.Down, 3 },
            { Direction2D.LeftDown, 4 },
            { Direction2D.RightDown, 5 }
        };

        private static readonly Vector3Int[] _hexOffsetsEven =
        {
            new(+1, 0, 0), new(0, +1, 0), new(-1, +1, 0),
            new(-1, 0, 0), new(-1, -1, 0), new(0, -1, 0)
        };

        private static readonly Vector3Int[] _hexOffsetsOdd =
        {
            new(+1, 0, 0), new(+1, +1, 0), new(0, +1, 0),
            new(-1, 0, 0), new(0, -1, 0), new(+1, -1, 0)
        };
        
        private static readonly Vector3Int[] _flatHexOffsetsEven =
        {
            new(0, +1, 0), new(+1, 0, 0), new(-1, 0, 0),
            new(0, -1, 0), new(-1, -1, 0), new(+1, -1, 0)
        };

        private static readonly Vector3Int[] _flatHexOffsetsOdd =
        {
            new(0, +1, 0), new(+1, +1, 0), new(-1, +1, 0),
            new(0, -1, 0), new(-1, 0, 0), new(+1, 0, 0)
        };

        public HexWorldGrid(Transform coreTransform, int width, int height, int depth, Vector3 cellSize, Vector3 offset, Vector3 cellOffset, bool drawGizmos, Color gizmosColor, bool isPointyTopped = false) : base(coreTransform, width, height, depth, cellSize, offset, cellOffset, drawGizmos, gizmosColor)
        {
            _isPointyTopped = isPointyTopped;
        }
   
        private (float xSpacing, float ySpacing, float zSpacing) GetSpacings()
        {
            var x = CellSize.x + CellOffset.x;
            var y = CellSize.y + CellOffset.y;
            var z = CellSize.z + CellOffset.z;

            return !_isPointyTopped ? (x * HEX_INNER_RADIUS_FACTOR, y, z) : (x, y * HEX_INNER_RADIUS_FACTOR, z);
        }

        private float GetAverageOffsetForHeight() => 0.25f * ((Width - 1f) / Width);
        private float GetAverageOffsetForWidth() => 0.5f * ((Height - 1f) / Height);

        private Vector3 CalculateGridCenterOffset()
        {
            var centerX = (Width - 1) / 2f;
            var centerY = (Height - 1) / 2f;
            
            var (xSpacing, ySpacing, zSpacing) = GetSpacings();

            if (!_isPointyTopped)
            {
                var offsetY = GetAverageOffsetForHeight();
                return new Vector3(centerX * xSpacing, zSpacing * 0.5f, (centerY + offsetY) * ySpacing);
            }

            var offsetX = GetAverageOffsetForWidth();
            return new Vector3((centerX + offsetX) * xSpacing, zSpacing * 0.5f, centerY * ySpacing);
        }

        private Vector3 CalculateLocalPosition(Vector3 gridPos)
        {
            var (xSpacing, ySpacing, zSpacing) = GetSpacings();

            if (!_isPointyTopped)
            {
                var offset = 0.5f * ((int)gridPos.x & 1);
                return new Vector3(gridPos.x * xSpacing, (gridPos.z + 0.5f) * zSpacing, (gridPos.y + offset) * ySpacing);
            }
            else
            {
                var offset = 0.5f * ((int)gridPos.y & 1);
                return new Vector3((gridPos.x + offset) * xSpacing, (gridPos.z + 0.5f) * zSpacing, gridPos.y * ySpacing);
            }
        }

        private Vector3 HexGridOffset() => new (GridOffset.x, GridOffset.z, GridOffset.y);

        public override Vector3 GridToWorld(Vector3Int gridPos)
        {
            var localPos = CalculateLocalPosition(gridPos);
            var centeredPos = localPos - CalculateGridCenterOffset();
            return Transform.TransformPoint(centeredPos + HexGridOffset());
        }
        public override Vector3Int WorldToGrid(Vector3 position, bool clamp = true)
        {
            var localPos = Transform.InverseTransformPoint(position) - HexGridOffset() + CalculateGridCenterOffset();
            var xSpacing = (CellSize.x + CellOffset.x);
            var ySpacing = (CellSize.y + CellOffset.y);
            var zStep = (CellSize.z + CellOffset.z);

            int x, y, z;

            if (!_isPointyTopped)
            {
                xSpacing *= HEX_INNER_RADIUS_FACTOR;
                var q = localPos.x / xSpacing;
                var isOdd = Mathf.RoundToInt(q) % 2 != 0;
                var r = localPos.z / ySpacing - (isOdd ? 0.5f : 0f);
        
                var rounded = RoundAxial(new Vector2(q, r));
                x = Mathf.RoundToInt(rounded.x);
                y = Mathf.RoundToInt(rounded.y);
            }
            else
            {
                ySpacing *= HEX_INNER_RADIUS_FACTOR;
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
            
            var estimatedPos = GridToWorld(new Vector3Int(x, y, z));
            var radius = CellSize.x / SQRT3;
        
            return Vector3.Distance(position, estimatedPos) > radius 
                ? new Vector3Int(-1, -1, -1) 
                : new Vector3Int(x, y, z);
        }
        
        private Vector2 RoundAxial(Vector2 axial)
        {
            var x = axial.x;
            var z = axial.y;
            var y = -x - z;
            
            var rx = Mathf.Round(x);
            var ry = Mathf.Round(y);
            var rz = Mathf.Round(z);
            
            var dx = Mathf.Abs(rx - x);
            var dy = Mathf.Abs(ry - y);
            var dz = Mathf.Abs(rz - z);
            
            if (dx > dy && dx > dz)
                rx = -ry - rz;
            else if (dy > dz)
                ry = -rx - rz;
            else
                rz = -rx - ry;
            
            return new Vector2(rx, rz);
        }
        
        public override bool TryGetNeighbor(Vector3Int pos, Direction2D direction2D, out T neighbor, DepthDirection depthDirection = default)
        {
            neighbor = default;
            
            var directionMap = _isPointyTopped ? _pointyToppedDirections : _flatToppedDirections;

            if (!directionMap.TryGetValue(direction2D, out var dirIndex))
                return false;

            var offset = _isPointyTopped
                ? ((pos.y & 1) == 0 ? _hexOffsetsEven[dirIndex] : _hexOffsetsOdd[dirIndex])
                : ((pos.x & 1) == 0 ? _flatHexOffsetsEven[dirIndex] : _flatHexOffsetsOdd[dirIndex]);
            
            var neighborGridPos = pos + offset;
            
            if (IsInRange(neighborGridPos))
            {
                var candidate = GetGridObject(neighborGridPos);
                
                if (!candidate.Equals(default(T)))
                {
                    neighbor = candidate;
                    return true;
                }
            }

            return false;
        }
        
        public override void DrawGrid()
        {
            if (!DrawGizmos) return;

            Gizmos.color = GizmosColor;

            int total = Width * Height * Depth;

            for (int i = 0; i < total; i++)
            {
                var x = i % Width;
                var y = (i / Width) % Height;
                var z = i / (Width * Height);

                var pos = new Vector3Int(x, y, z);
                DrawHighlightedCell(pos, GizmosColor);
            }
        }

        public override void DrawHighlightedCell(Vector3Int gridPos, Color highlightColor)
        {
            var center = GridToWorld(gridPos);
            var radius = CellSize.x / SQRT3;
            var halfHeight = CellSize.z * 0.5f;

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
                        var node = GetGridObject(pos);
                        if (node.Equals(default(T))) continue;
                        var size = node.IsWalkable ? Vector3.zero : CellSize;
                        var worldPos = GridToWorld(pos);
                        int index = GridPositionToIndex(pos);
                        MeshUtils.AddToMeshArraysHex3D(vertices, uv, triangles, index, worldPos, size.x / SQRT3, size.y, Transform, !_isPointyTopped);
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
                        pathNodeArray[GridPositionToIndex(pos)] = GetGridObject(pos);
                    }
                }
            }

            var pathFinder = new FindPathHex<T>
            {
                GridSize = new int3(Width, Height, Depth),
                PathNodeArray = pathNodeArray,
                StartPos = startPos,
                EndPos = endPos,
                AllowDiagonalCornerCutting = true, //allowDiagonalCornerCutting, // Here needs to be always true for hex!!
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
                        pathNodeArray[GridPositionToIndex(new Vector3Int(x,y,z))] = GetGridObject(pos);
                    }
                }
            }

            var job = new FindPathHexJob<T>
            {
                PathNodeArray = pathNodeArray,
                GridSize = new int3(Width, Height, Depth),
                StartPos = startPos,
                EndPos = endPos,
                CalculatedPathList = result,
                AllowDiagonalCornerCutting = true, //allowDiagonalCornerCutting, // Here needs to be always true for hex!!
                IsPointyTopped = _isPointyTopped
            };

            job.Schedule().Complete();
            pathNodeArray.Dispose();
            return result;
        }
    }
}