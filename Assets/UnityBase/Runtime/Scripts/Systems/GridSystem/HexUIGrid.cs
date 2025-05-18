using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityBase.Extensions;
using UnityEngine;

namespace UnityBase.GridSystem
{
    public class HexUIGrid<T> : UIGrid<T> where T : struct, IGridNodeData
    {
        private const float SQRT3 = 1.73205f; // Mathf.Sqrt(3f)
        private const float HEX_INNER_RADIUS_FACTOR = 0.866025f; // Mathf.Sqrt(3f) / 2f
        
        private readonly bool _isPointyTopped;

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

        public HexUIGrid(Camera cam, int width, int height, float screenSidePaddingRatio, float cellSpacingRatio, Vector3 originOffset, bool drawGizmos, Color gizmosColor, bool isPointyTopped = false)
            : base(cam, width, height, screenSidePaddingRatio, cellSpacingRatio, originOffset, drawGizmos, gizmosColor)
        {
            _isPointyTopped = isPointyTopped;
        }

        public override Vector3 GridToWorld(Vector3Int pos)
        {
            if (!IsInRange(pos)) return Vector3.zero;

            var spacing = GetScreenWidth() * (_cellSpacingRatio / 100f);
            var borderOffset = GetScreenWidth() * (_screenSidePaddingRatio / 100f);

            var w = (CellSize * HEX_INNER_RADIUS_FACTOR) + spacing;
            var h = HEX_INNER_RADIUS_FACTOR * w;

            var x = _isPointyTopped ? w * (pos.x + 0.5f * (pos.y & 1)) : h * pos.x;
            
            var y = _isPointyTopped ? h * pos.y : w * (pos.y + 0.5f * (pos.x & 1));

            var totalAvailableWidth = GetScreenWidth() - borderOffset;
            var totalGridWidth = _isPointyTopped ? Width * w + (w * 0.5f) : Width * h + (h * 0.5f);

            var startX = GetLeftX() + (totalAvailableWidth - totalGridWidth) / 2f + borderOffset / 2f;
            var startY = GetTopY();

            return new Vector3(startX + x + w / 2f, startY - y - h / 2f, 0f);
        }
        
        public override Vector3Int WorldToGrid(Vector3 worldPos, bool clamp = true)
        {
            var spacing = GetScreenWidth() * (_cellSpacingRatio / 100f);
            var borderOffset = GetScreenWidth() * (_screenSidePaddingRatio / 100f);
      
            var totalAvailableWidth = GetScreenWidth() - borderOffset;
            var w = (CellSize * HEX_INNER_RADIUS_FACTOR) + spacing;
            var h = HEX_INNER_RADIUS_FACTOR * w;
            
            var totalGridWidth = _isPointyTopped ? Width * w + (w * 0.5f) : Width * h + (h * 0.5f);
            var startX = GetLeftX() + (totalAvailableWidth - totalGridWidth) / 2f + borderOffset / 2f;
            var startY = GetTopY();
      
            var normalizedX = worldPos.x - startX - w / 2f;
            var normalizedY = startY - worldPos.y - h / 2f;
            
            int x, y;
            
            if (_isPointyTopped)
            {
                var q = normalizedX / w;
                var r = normalizedY / h;
                
                var qOffset = 0.5f * (Mathf.RoundToInt(r) & 1);
                var axialCoord = new Vector2(q - qOffset, r);
                
                var rounded = RoundAxial(axialCoord);
                x = Mathf.RoundToInt(rounded.x);
                y = Mathf.RoundToInt(rounded.y);
            }
            else
            {
                var q = normalizedX / h;
                var r = normalizedY / w;
                
                var rOffset = 0.5f * (Mathf.RoundToInt(q) & 1);
                var axialCoord = new Vector2(q, r - rOffset);
                
                var rounded = RoundAxial(axialCoord);
                x = Mathf.RoundToInt(rounded.x);
                y = Mathf.RoundToInt(rounded.y);
            }
            
            if (clamp)
            {
                x = Mathf.Clamp(x, 0, Width - 1);
                y = Mathf.Clamp(y, 0, Height - 1);
            }
            
            var estimatedPos = GridToWorld(new Vector3Int(x, y, 0));
            var radius = (CellSize / SQRT3) * 0.8f;

            if (Vector3.Distance(worldPos, estimatedPos) > radius)
            {
                return new Vector3Int(-1, -1, -1); 
            }

            return new Vector3Int(x, y, 0);
        }

        private Vector2 RoundAxial(Vector2 axial)
        {
            var cube = new Vector3(axial.x, -axial.x - axial.y, axial.y);
            var roundedCube = new Vector3(Mathf.Round(cube.x), Mathf.Round(cube.y), Mathf.Round(cube.z));

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


        public override bool TryGetNeighbor(Vector3Int pos, Direction direction, out T neighbour)
        {
            neighbour = default;

            var offset = (pos.y & 1) == 0 ? _hexOffsetsEven[(int)direction] : _hexOffsetsOdd[(int)direction];
            var neighborPos = pos + offset;

            if (IsInRange(neighborPos))
            {
                neighbour = GetGridObject(neighborPos);
                return true;
            }

            return false;
        }

        public override void DrawGrid()
        {
            if (!DrawGizmos) return;

            Gizmos.color = GizmosColor;
            var size = CellSize;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var center = GridToWorld(new Vector3Int(x, y));
                    DrawHexCell(center, size);
                }
            }
        }

        private void DrawHexCell(Vector3 center, float size)
        {
            var corners = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                var angle = _isPointyTopped ? Mathf.Deg2Rad * (60 * i - 30) : Mathf.Deg2Rad * (60 * i);
                corners[i] = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * size / 2f;
            }

            for (int i = 0; i < 6; i++)
            {
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 6]);
            }
        }

        public override void RebuildMeshVisual(Mesh mesh)
        {
            var cellCount = Width * Height;
            MeshUtils.CreateEmptyMeshArraysHex2D(cellCount, out var vertices, out var uvs, out var triangles);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var pos = new Vector3Int(x, y);
                    var node = GetGridObject(pos);
                    if (node.IsWalkable) continue;
                    var worldPos = GridToWorld(pos);
                    var radius = CellSize * 0.5f;
                    var index = GridPositionToIndex(pos);
                    MeshUtils.AddToMeshArraysHex2D(vertices, uvs, triangles, index, worldPos, radius, _isPointyTopped);
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
        }

        public override List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos, bool allowDiagonal = false)
        {
            var totalSize = Width * Height;
            var pathNodeArray = new T[totalSize];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var pos = new Vector3Int(x, y);
                    pathNodeArray[GridPositionToIndex(pos)] = GetGridObject(pos);
                }
            }

            var pathFinder = new FindPathHex<T>
            {
                GridSize = new int3(Width, Height, 1),
                PathNodeArray = pathNodeArray,
                StartPos = startPos,
                EndPos = endPos,
                AllowDiagonalCornerCutting = true, //allowDiagonal, // Here needs to be always true for hex!!
                IsPointyTopped = _isPointyTopped
            };

            return pathFinder.Execute();
        }

        public override NativeList<Vector3Int> FindPathWithJobs(Vector3Int startPos, Vector3Int endPos, bool allowDiagonal = false)
        {
            var totalSize = Width * Height;
            var pathNodeArray = new NativeArray<T>(totalSize, Allocator.TempJob);
            var result = new NativeList<Vector3Int>(Allocator.TempJob);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var pos = new Vector3Int(x, y);
                    pathNodeArray[GridPositionToIndex(pos)] = GetGridObject(pos);
                }
            }

            var job = new FindPathHexJob<T>
            {
                PathNodeArray = pathNodeArray,
                GridSize = new int3(Width, Height, 1),
                StartPos = startPos,
                EndPos = endPos,
                CalculatedPathList = result,
                AllowDiagonalCornerCutting = true, //allowDiagonal, // Here needs to be always true for hex!!
                IsPointyTopped = _isPointyTopped
            };

            job.Schedule().Complete();
            pathNodeArray.Dispose();
            return result;
        }
    }
}