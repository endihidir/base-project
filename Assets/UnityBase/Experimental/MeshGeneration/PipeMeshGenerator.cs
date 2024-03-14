using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class PipeMeshGenerator : MonoBehaviour
{
    [SerializeField] private List<Transform> _pipePoints = new List<Transform>();
    [SerializeField] private Collider _hitCollider;    
    [SerializeField] private float _radius;
    [SerializeField] private int _circleSegmentCount;
    [SerializeField] private float _pointGenerationDistance;
    [SerializeField] private bool _useLineSmoothness;
    [SerializeField] private int _pathSmoothnessIteration = 10;
    [SerializeField] private int _smoothedLastPointsCount = 10;

    [Sirenix.OdinInspector.ReadOnly] [SerializeField] private MeshFilter _meshFilter;
    [Sirenix.OdinInspector.ReadOnly] [SerializeField] private MeshRenderer _meshRenderer;
    [Sirenix.OdinInspector.ReadOnly] [SerializeField] private MeshCollider _meshCollider;
    
    
    [Sirenix.OdinInspector.ReadOnly] [SerializeField] private Vector3[] _vertices;
    [Sirenix.OdinInspector.ReadOnly] [SerializeField] private int[] _triangles;
    [Sirenix.OdinInspector.ReadOnly] [SerializeField] private Mesh _generatedMesh;
    [Sirenix.OdinInspector.ReadOnly] [SerializeField] private Transform _lastPointT;
    [Sirenix.OdinInspector.ReadOnly] [SerializeField] private bool _isDrag;

    private Camera _cam;

#if UNITY_EDITOR
    private void OnValidate()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshCollider = GetComponent<MeshCollider>();
    }
#endif

    private void Awake()
    {
        _cam = Camera.main;
        _generatedMesh = new Mesh();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isDrag = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            _isDrag = false;
        }

        if (_isDrag)
        {
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            
            if (_hitCollider.Raycast(ray, out var hit, Mathf.Infinity))
            {
                if(!_lastPointT) GenerateNewPoint(hit);
                
                var dist = (_lastPointT.position - hit.point).magnitude;
                
                if (dist > _pointGenerationDistance)
                {
                    GenerateNewPoint(hit);
                    
                    if (_useLineSmoothness)
                    {
                        SmoothPath();
                    }
                    
                    CalculateMesh();
                }
            }
        }
    }

    private void CalculateMesh()
    {
        var pointsLength = _pipePoints.Count;

        if(pointsLength < 2) return;
        
        var isAnyPointsNull = _pipePoints.Any(x => !x);
        
        if(isAnyPointsNull) return;
        
        var oneMinusPointsLenght = pointsLength - 1;
        var verticesCount = _circleSegmentCount * pointsLength;
        var trisCount = oneMinusPointsLenght * (_circleSegmentCount * 6);
        
        var dataArray = Mesh.AllocateWritableMeshData(1);
        var data = dataArray[0];

        var positionAttribute = new VertexAttributeDescriptor(VertexAttribute.Position);
        var normalAttribute = new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1);
        data.SetVertexBufferParams(verticesCount, positionAttribute, normalAttribute);
        data.SetIndexBufferParams(trisCount, IndexFormat.UInt32);

        var vertices = data.GetVertexData<Vector3>();
        var tris = data.GetIndexData<int>();

        for (int i = 0; i < pointsLength; i++)
        {
            for (int j = 0; j < _circleSegmentCount; j++)
            {
                float angle = j * Mathf.PI * 2 / _circleSegmentCount;

                float x = Mathf.Cos(angle) * _radius;

                float y = Mathf.Sin(angle) * _radius;

                Vector3 pos = _pipePoints[i].localPosition + _pipePoints[i].TransformDirection(new Vector3(x, y, 0f));

                int index = (i * _circleSegmentCount) + j;

                vertices[index] = pos;
            }
        }

        var combinedSegmentsLenght = _circleSegmentCount * oneMinusPointsLenght;
        
        for (int i = 0; i < combinedSegmentsLenght; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                var firstStepReducer = (i + 1) % _circleSegmentCount == 0 ? _circleSegmentCount * j : 0;
                tris[(i * 2 + j) * 3] = i + j - firstStepReducer;
                
                var secondStepReducer = (i + 1) % _circleSegmentCount == 0 ? _circleSegmentCount : 0;
                tris[(i * 2 + j) * 3 + 1] = (i + 1) + (j * _circleSegmentCount) - secondStepReducer;
                
                tris[(i * 2 + j) * 3 + 2] = _circleSegmentCount + i;
            }
        }
        
        _generatedMesh.Clear();

        data.subMeshCount = 1;
        data.SetSubMesh(0, new SubMeshDescriptor(0, trisCount));
        
        Mesh.ApplyAndDisposeWritableMeshData(dataArray, _generatedMesh);

        _generatedMesh.RecalculateBounds();
        _generatedMesh.RecalculateNormals();

        _meshFilter.mesh = _generatedMesh;
        _meshCollider.sharedMesh = _generatedMesh;
    }


    private void GenerateNewPoint(RaycastHit hit)
    {
        var go = new GameObject("P_" + (_pipePoints.Count + 1)).transform;
        go.position = hit.point;
        go.SetParent(transform);
        if (_lastPointT)
        {
            var dir = go.position - _lastPointT.position;
            go.rotation = Quaternion.LookRotation(dir.normalized, hit.normal);
            _lastPointT.rotation = Quaternion.LookRotation(dir.normalized, hit.normal);
        }
        _lastPointT = go;
        _pipePoints.Add(go);
    }
    
    
    private void OnDrawGizmos()
    {
        if(Application.isPlaying) return;
        
        if (_pipePoints == null)
            return;

        var isAnyPointsNull = _pipePoints.Any(x => !x);
        
        if(isAnyPointsNull) return;
        
        if (_pipePoints.Count < 2)
        {
            _triangles = Array.Empty<int>();
            _vertices = Array.Empty<Vector3>();
            return;
        }
        
        var pointsLength = _pipePoints.Count;

        var oneMinusPointsLenght = pointsLength - 1;
        
        _vertices = new Vector3[_circleSegmentCount * pointsLength];

        int triangleCount = oneMinusPointsLenght * _circleSegmentCount * 6;
        
        _triangles = new int[triangleCount];
        
        for (int i = 0; i < pointsLength; i++)
        {
            for (int j = 0; j < _circleSegmentCount; j++)
            {
                float angle = j * Mathf.PI * 2 / _circleSegmentCount;

                float x = Mathf.Cos(angle) * _radius;

                float y = Mathf.Sin(angle) * _radius;

                Vector3 pos = _pipePoints[i].localPosition + _pipePoints[i].TransformDirection(new Vector3(x, y, 0f));

                int index = (i * _circleSegmentCount) + j;

                _vertices[index] = pos;
                
                Gizmos.DrawSphere(pos, 0.2f);
            }
        }
        
        for (int i = 0; i < _circleSegmentCount * oneMinusPointsLenght; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                var firstStepReducer = (i + 1) % _circleSegmentCount == 0 ? _circleSegmentCount * j : 0;
                _triangles[(i * 2 + j) * 3] = i + j - firstStepReducer;
                
                var secondStepReducer = (i + 1) % _circleSegmentCount == 0 ? _circleSegmentCount : 0;
                _triangles[(i * 2 + j) * 3 + 1] = (i + 1) + (j * _circleSegmentCount) - secondStepReducer;
                
                _triangles[(i * 2 + j)  * 3 + 2] = _circleSegmentCount + i;
            }
        }

        var mesh = new Mesh();
        
        mesh.SetVertices(_vertices);
        
        mesh.SetTriangles(_triangles, 0);
        
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        
        Gizmos.DrawMesh(mesh);
    }
    
    private void SmoothPath()
    {
        var pathPoints = new NativeArray<Vector3>(_pipePoints.Count, Allocator.TempJob);

        for (int i = 0; i < pathPoints.Length; i++)
        {
            pathPoints[i] = _pipePoints[i].position;
        }

        var smoothedPoints = new NativeArray<Vector3>(pathPoints.Length, Allocator.TempJob);
            
        smoothedPoints[0] = pathPoints[0];
        smoothedPoints[pathPoints.Length - 1] = pathPoints[^1];

        var smoothingJob = new PathSmoothingJob
        {
            pts = pathPoints,
            smoothedPoints = smoothedPoints
        };

        for (var i = 0; i < _pathSmoothnessIteration; i++)
        {
            smoothingJob.Schedule(pathPoints.Length, 32).Complete();
            pathPoints.CopyFrom(smoothedPoints);
        }

        if (pathPoints.Length > _pipePoints.Count)
        {
            var dist = pathPoints.Length - _pipePoints.Count;

            for (int i = _pipePoints.Count - 1; i < dist; i++)
            {
                var ray = new Ray(_cam.transform.position, (pathPoints[i] - _cam.transform.position).normalized);

                if (_hitCollider.Raycast(ray, out var hit, Mathf.Infinity))
                {
                    GenerateNewPoint(hit);
                }
            }
        }
        
        if (_pipePoints.Count > _smoothedLastPointsCount)
        {
            for (int i = _pipePoints.Count - _smoothedLastPointsCount; i < _pipePoints.Count; i++)
            {
                _pipePoints[i].position = pathPoints[i];
            }
        }
        
        pathPoints.Dispose();
        smoothedPoints.Dispose();
    }


    [BurstCompile]
    private struct PathSmoothingJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Vector3> pts;

        [WriteOnly]
        public NativeArray<Vector3> smoothedPoints;

        public void Execute(int index)
        {
            if (index > 0 && index < pts.Length - 1)
            {
                smoothedPoints[index] = (pts[index - 1] + pts[index] + pts[index + 1]) / 3f;
            }
        }
    }
}