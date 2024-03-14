using System.Linq;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

public class CubicMeshGenerator : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Transform[] _quadPoints;
    [SerializeField] private float _wallWidth;
    [SerializeField] private float _wallHeight;
    [ReadOnly] [SerializeField] private Vector3[] vertices;
    [ReadOnly] [SerializeField] private int[] triangles;

    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        var transforms = GetComponentsInChildren<Transform>().Where(x=> !x.GetComponent<CubicMeshGenerator>()).ToArray();
        _quadPoints = transforms;
    }
#endif

    private void OnDrawGizmos()
    {
        if(_quadPoints == null || _quadPoints.Length < 1) return;

        int pointsLength = _quadPoints.Length;
        
        int pointsOneMinusLenght = pointsLength - 1;

        vertices = new Vector3[pointsLength * 4];

        int trianglesLenght = 12 + (6 * (pointsLength - 1) * 4);
        
        triangles = new int[trianglesLenght];

        for (int i = 0; i < pointsLength; i++)
        {
            // Points
            vertices[i] = _quadPoints[i].transform.localPosition;
            vertices[i + pointsLength] = _quadPoints[i].transform.localPosition + _quadPoints[i].transform.forward * _wallWidth;
            vertices[i + pointsLength * 2] = _quadPoints[i].transform.localPosition + Vector3.up * _wallHeight;
            vertices[i + pointsLength * 3] = _quadPoints[i].transform.localPosition + _quadPoints[i].transform.forward * _wallWidth + Vector3.up * _wallHeight;
        }

        for (int i = 0; i < pointsOneMinusLenght; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                // Bottom
                triangles[(i * 2 + j) * 3] = i + j;
                triangles[(i * 2 + j) * 3 + 1] = i + pointsLength;
                triangles[(i * 2 + j)  * 3 + 2] = (i + 1) + (j * pointsLength);

                // Top
                triangles[pointsOneMinusLenght * 6 + (i * 2 + j) * 3] = (i + j) + (pointsLength * 2); 
                triangles[pointsOneMinusLenght * 6 + (i * 2 + j) * 3 + 1] = (i + (pointsLength * 2 + 1)) + (j * pointsLength);
                triangles[pointsOneMinusLenght * 6 + (i * 2 + j) * 3 + 2] = (i + (pointsLength * 3));
                    
                //Inside
                triangles[pointsOneMinusLenght * 12 + (i * 2 + j) * 3] = i + j; 
                triangles[pointsOneMinusLenght * 12 + (i * 2 + j) * 3 + 1] = i + (pointsLength * 2 + 1);
                triangles[pointsOneMinusLenght * 12 + (i * 2 + j) * 3 + 2] = (i + (pointsLength * 2)) - (j * (pointsLength * 2));
                    
                //Outside
                triangles[pointsOneMinusLenght * 18 + (i * 2 + j) * 3] = i + pointsLength; 
                triangles[pointsOneMinusLenght * 18 + (i * 2 + j) * 3 + 1] = (i + (pointsLength * 3 + 1)) - j;
                triangles[pointsOneMinusLenght * 18 + (i * 2 + j) * 3 + 2] = (i + pointsLength + 1) + (j * pointsLength * 2);
            }
        }

        for (int i = 0; i < 2; i++)
        {
            // First Cap
            triangles[(pointsOneMinusLenght * 24) + i * 3] = 0; 
            triangles[(pointsOneMinusLenght * 24) + (i * 3) + 1] = (pointsLength * 3) - (i * pointsLength);
            triangles[(pointsOneMinusLenght * 24) + (i * 3) + 2] = (i + pointsLength) + (i * (pointsLength - 1) * 2 + i);

            // Last Cap
            triangles[pointsOneMinusLenght * 24 + (i + 2) * 3] = pointsLength + pointsOneMinusLenght;
            triangles[pointsOneMinusLenght * 24 + ((i + 2) * 3) + 1] = (i + (pointsLength * 3) - 1) + (i * pointsOneMinusLenght);
            triangles[pointsOneMinusLenght * 24 + ((i + 2) * 3) + 2] = (i + pointsLength) + (i * (pointsLength - 1) * 2 + i) - 1;
        }

        if (_mesh)
        {
            Gizmos.DrawMesh(_mesh);
        }

    }

    [Button]
    private void GenerateMesh()
    {
        _mesh = new Mesh();
        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(triangles, 0);
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        
        if(!_meshFilter)
            _meshFilter = gameObject.AddComponent<MeshFilter>();
        
        if(!_meshCollider)
            _meshCollider = gameObject.AddComponent<MeshCollider>();
        
        _meshFilter.mesh = _mesh;
        _meshCollider.sharedMesh = _mesh;
    }

    [Button]
    private void ClearMesh()
    {
        DestroyImmediate(_meshFilter);
        DestroyImmediate(_meshCollider);
        _mesh = null;
    }

    [Button]
    public void SaveMesh(string meshName)
    {
#if UNITY_EDITOR
        
        AssetDatabase.CreateAsset(_mesh,  "Assets/_AssetsMain/Models/Meshes"+ meshName + ".asset");
        AssetDatabase.SaveAssets();
#endif
    }
}