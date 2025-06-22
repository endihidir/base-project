using UnityEngine;

public static class MeshExtensions 
{
    
    private static readonly Vector3 Vector3zero = Vector3.zero;
    private static readonly Vector3 Vector3one = Vector3.one;
    private static readonly Vector3 Vector3yDown = new Vector3(0,-1);

    
    private static Quaternion[] cachedQuaternionEulerArr;
    private static void CacheQuaternionEuler() 
    {
        if (cachedQuaternionEulerArr != null) return;
        cachedQuaternionEulerArr = new Quaternion[360];
        for (int i=0; i<360; i++) {
            cachedQuaternionEulerArr[i] = Quaternion.Euler(0,0,i);
        }
    }
    private static Quaternion GetQuaternionEuler(float rotFloat) 
    {
        int rot = Mathf.RoundToInt(rotFloat);
        rot = rot % 360;
        if (rot < 0) rot += 360;
        if (cachedQuaternionEulerArr == null) CacheQuaternionEuler();
        return cachedQuaternionEulerArr[rot];
    }


    public static Mesh CreateEmptyMesh() 
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[0];
        mesh.uv = new Vector2[0];
        mesh.triangles = new int[0];
        return mesh;
    }

    public static void CreateEmptyMeshArrays(int quadCount, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles) 
    {
		vertices = new Vector3[4 * quadCount];
		uvs = new Vector2[4 * quadCount];
		triangles = new int[6 * quadCount];
    }
    
    public static void CreateEmptyMeshArrays3D(int cubeCount, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles)
    {
	    vertices = new Vector3[8 * cubeCount];
	    uvs = new Vector2[8 * cubeCount];
	    triangles = new int[36 * cubeCount];
    }
    
    public static void CreateEmptyMeshArraysHex2D(int hexCount, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles)
    {
	    vertices = new Vector3[7 * hexCount]; 
	    uvs = new Vector2[7 * hexCount];
	    triangles = new int[18 * hexCount];
    }
    
    public static void CreateEmptyMeshArraysHex3D(int hexCount, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles)
    {
	    vertices = new Vector3[14 * hexCount];
	    uvs = new Vector2[14 * hexCount];
	    triangles = new int[72 * hexCount];
    }
        
    public static Mesh CreateMesh(Vector3 pos, float rot, Vector3 baseSize, Vector2 uv00, Vector2 uv11) 
    {
        return AddToMesh(null, pos, rot, baseSize, uv00, uv11);
    }

    public static Mesh AddToMesh(Mesh mesh, Vector3 pos, float rot, Vector3 baseSize, Vector2 uv00, Vector2 uv11) 
    {
        if (mesh == null) 
        {
            mesh = CreateEmptyMesh();
        }
        
		Vector3[] vertices = new Vector3[4 + mesh.vertices.Length];
		Vector2[] uvs = new Vector2[4 + mesh.uv.Length];
		int[] triangles = new int[6 + mesh.triangles.Length];
            
        mesh.vertices.CopyTo(vertices, 0);
        mesh.uv.CopyTo(uvs, 0);
        mesh.triangles.CopyTo(triangles, 0);

        int index = vertices.Length / 4 - 1;
		//Relocate vertices
		int vIndex = index*4;
		int vIndex0 = vIndex;
		int vIndex1 = vIndex+1;
		int vIndex2 = vIndex+2;
		int vIndex3 = vIndex+3;

        baseSize *= .5f;

        bool skewed = baseSize.x != baseSize.y;
        if (skewed) 
        {
			vertices[vIndex0] = pos+GetQuaternionEuler(rot)*new Vector3(-baseSize.x,  baseSize.y);
			vertices[vIndex1] = pos+GetQuaternionEuler(rot)*new Vector3(-baseSize.x, -baseSize.y);
			vertices[vIndex2] = pos+GetQuaternionEuler(rot)*new Vector3( baseSize.x, -baseSize.y);
			vertices[vIndex3] = pos+GetQuaternionEuler(rot)*baseSize;
		} 
        else 
        {
			vertices[vIndex0] = pos+GetQuaternionEuler(rot-270)*baseSize;
			vertices[vIndex1] = pos+GetQuaternionEuler(rot-180)*baseSize;
			vertices[vIndex2] = pos+GetQuaternionEuler(rot- 90)*baseSize;
			vertices[vIndex3] = pos+GetQuaternionEuler(rot-  0)*baseSize;
		}
		
		//Relocate UVs
		uvs[vIndex0] = new Vector2(uv00.x, uv11.y);
		uvs[vIndex1] = new Vector2(uv00.x, uv00.y);
		uvs[vIndex2] = new Vector2(uv11.x, uv00.y);
		uvs[vIndex3] = new Vector2(uv11.x, uv11.y);
		
		//Create triangles
		int tIndex = index*6;
		
		triangles[tIndex+0] = vIndex0;
		triangles[tIndex+1] = vIndex3;
		triangles[tIndex+2] = vIndex1;
		
		triangles[tIndex+3] = vIndex1;
		triangles[tIndex+4] = vIndex3;
		triangles[tIndex+5] = vIndex2;
            
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;

        //mesh.bounds = bounds;

        return mesh;
    }

    public static void AddToMeshArrays(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 pos, float rot, Vector3 baseSize, Vector2 uv00, Vector2 uv11) 
    {
		//Relocate vertices
		int vIndex = index * 4;
		int vIndex0 = vIndex;
		int vIndex1 = vIndex + 1;
		int vIndex2 = vIndex + 2;
		int vIndex3 = vIndex + 3;

        baseSize *= 0.5f;

        bool skewed = baseSize.x != baseSize.y;
        
        if (skewed) 
        {
			vertices[vIndex0] = pos + GetQuaternionEuler(rot) * new Vector3(-baseSize.x,  baseSize.y);
			vertices[vIndex1] = pos + GetQuaternionEuler(rot) * new Vector3(-baseSize.x, -baseSize.y);
			vertices[vIndex2] = pos + GetQuaternionEuler(rot) * new Vector3( baseSize.x, -baseSize.y);
			vertices[vIndex3] = pos + GetQuaternionEuler(rot) * baseSize;
		} 
        else 
        {
			vertices[vIndex0] = pos + GetQuaternionEuler(rot - 270) * baseSize;
			vertices[vIndex1] = pos + GetQuaternionEuler(rot - 180) * baseSize;
			vertices[vIndex2] = pos + GetQuaternionEuler(rot - 90) * baseSize;
			vertices[vIndex3] = pos + GetQuaternionEuler(rot -  0) * baseSize;
		}
		
		//Relocate UVs
		uvs[vIndex0] = new Vector2(uv00.x, uv11.y);
		uvs[vIndex1] = new Vector2(uv00.x, uv00.y);
		uvs[vIndex2] = new Vector2(uv11.x, uv00.y);
		uvs[vIndex3] = new Vector2(uv11.x, uv11.y);
		
		//Create triangles
		int tIndex = index * 6;
		
		triangles[tIndex+0] = vIndex0;
		triangles[tIndex+1] = vIndex3;
		triangles[tIndex+2] = vIndex1;
		
		triangles[tIndex+3] = vIndex1;
		triangles[tIndex+4] = vIndex3;
		triangles[tIndex+5] = vIndex2;
    }
    
    public static void AddToMeshArrays2(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 pos, Vector3 baseSize, Vector2 uv00, Vector2 uv11, Transform transform)
    {
	    int vIndex = index * 4;

	    Vector3 forward = transform.forward;
	    Vector3 right = transform.right;

	    baseSize *= 0.5f;
	    
	    vertices[vIndex + 0] = pos + (-right - forward) * baseSize.x;
	    vertices[vIndex + 1] = pos + (-right + forward) * baseSize.x;
	    vertices[vIndex + 2] = pos + (right + forward) * baseSize.x;
	    vertices[vIndex + 3] = pos + (right - forward) * baseSize.x;
	    
	    uvs[vIndex + 0] = new Vector2(uv00.x, uv11.y);
	    uvs[vIndex + 1] = new Vector2(uv00.x, uv00.y);
	    uvs[vIndex + 2] = new Vector2(uv11.x, uv00.y);
	    uvs[vIndex + 3] = new Vector2(uv11.x, uv11.y);
	    
	    int tIndex = index * 6;
	    triangles[tIndex + 0] = vIndex + 0;
	    triangles[tIndex + 1] = vIndex + 1;
	    triangles[tIndex + 2] = vIndex + 2;

	    triangles[tIndex + 3] = vIndex + 0;
	    triangles[tIndex + 4] = vIndex + 2;
	    triangles[tIndex + 5] = vIndex + 3;
    }
    
    public static void AddToMeshArrays3D(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 pos, Vector3 size, Vector2 uv00, Vector2 uv11, Transform transform)
    {
	    int vIndex = index * 8;
	    int tIndex = index * 36;

	    Vector3[] corners = new Vector3[8];

	    Vector3 halfSize = size * 0.5f;

	    corners[0] = pos + transform.TransformDirection(new Vector3(-halfSize.x, -halfSize.z, -halfSize.y));
	    corners[1] = pos + transform.TransformDirection(new Vector3(-halfSize.x, -halfSize.z,  halfSize.y));
	    corners[2] = pos + transform.TransformDirection(new Vector3(-halfSize.x,  halfSize.z, -halfSize.y));
	    corners[3] = pos + transform.TransformDirection(new Vector3(-halfSize.x,  halfSize.z,  halfSize.y));
	    corners[4] = pos + transform.TransformDirection(new Vector3( halfSize.x, -halfSize.z, -halfSize.y));
	    corners[5] = pos + transform.TransformDirection(new Vector3( halfSize.x, -halfSize.z,  halfSize.y));
	    corners[6] = pos + transform.TransformDirection(new Vector3( halfSize.x,  halfSize.z, -halfSize.y));
	    corners[7] = pos + transform.TransformDirection(new Vector3( halfSize.x,  halfSize.z,  halfSize.y));
	    
	    // Assign vertices
	    for (int i = 0; i < 8; i++)
		    vertices[vIndex + i] = corners[i];

	    // Dummy UVs (adjust as needed)
	    for (int i = 0; i < 8; i++)
		    uvs[vIndex + i] = uv00;

	    // Triangles
	    int[] cubeTris = {
		    0, 2, 1, 2, 3, 1, // -X
		    4, 5, 6, 6, 5, 7, // +X
		    0, 1, 5, 0, 5, 4, // -Y
		    2, 6, 3, 3, 6, 7, // +Y
		    0, 4, 2, 2, 4, 6, // -Z
		    1, 3, 5, 5, 3, 7  // +Z
	    };

	    for (int i = 0; i < 36; i++)
		    triangles[tIndex + i] = vIndex + cubeTris[i];
    }
    
    public static void AddToMeshArraysHex2D(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 center, float radius, bool isPointyTopped)
    {
	    int vIndex = index * 7;
	    int tIndex = index * 18;
	    
	    vertices[vIndex + 6] = center;
	    uvs[vIndex + 6] = Vector2.zero;

	    for (int i = 0; i < 6; i++)
	    {
		    float angleDeg = isPointyTopped ? 60 * i - 30 : 60 * i;
		    float angleRad = Mathf.Deg2Rad * angleDeg;

		    float x = Mathf.Cos(angleRad) * radius;
		    float y = Mathf.Sin(angleRad) * radius;

		    vertices[vIndex + i] = center + new Vector3(x, y, 0f);
		    uvs[vIndex + i] = Vector2.zero;
		    
		    int next = (i + 1) % 6;
		    triangles[tIndex + i * 3 + 0] = vIndex + i;
		    triangles[tIndex + i * 3 + 1] = vIndex + next;
		    triangles[tIndex + i * 3 + 2] = vIndex + 6;
	    }
    }
    
    public static void AddToMeshArraysHex3D(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 center, float radius, float height, Transform transform, bool isPointyTopped)
    {
	    int vIndex = index * 14;
	    int tIndex = index * 72;

	    Vector3 up = transform.up * (height / 2f);
	    Vector3 down = -up;

	    for (int i = 0; i < 6; i++)
	    {
		    float angleDeg = isPointyTopped ? 60 * i : 60 * i - 30;
		    float angleRad = Mathf.Deg2Rad * angleDeg;

		    float x = Mathf.Cos(angleRad) * radius;
		    float z = Mathf.Sin(angleRad) * radius;

		    Vector3 offset = transform.right * x + transform.forward * z;

		    vertices[vIndex + i] = center + offset + up;
		    vertices[vIndex + i + 6] = center + offset + down;

		    uvs[vIndex + i] = Vector2.zero;
		    uvs[vIndex + i + 6] = Vector2.zero;
	    }

	    vertices[vIndex + 12] = center + up;
	    vertices[vIndex + 13] = center + down;
	    uvs[vIndex + 12] = Vector2.zero;
	    uvs[vIndex + 13] = Vector2.zero;

	    for (int i = 0; i < 6; i++)
	    {
		    int next = (i + 1) % 6;
		    triangles[tIndex + i * 3 + 0] = vIndex + i;
		    triangles[tIndex + i * 3 + 1] = vIndex + next;
		    triangles[tIndex + i * 3 + 2] = vIndex + 12;
	    }

	    for (int i = 0; i < 6; i++)
	    {
		    int next = (i + 1) % 6;
		    triangles[tIndex + 18 + i * 3 + 0] = vIndex + next + 6;
		    triangles[tIndex + 18 + i * 3 + 1] = vIndex + i + 6;
		    triangles[tIndex + 18 + i * 3 + 2] = vIndex + 13;
	    }

	    for (int i = 0; i < 6; i++)
	    {
		    int next = (i + 1) % 6;

		    triangles[tIndex + 36 + i * 6 + 0] = vIndex + i;
		    triangles[tIndex + 36 + i * 6 + 1] = vIndex + i + 6;
		    triangles[tIndex + 36 + i * 6 + 2] = vIndex + next;

		    triangles[tIndex + 36 + i * 6 + 3] = vIndex + next;
		    triangles[tIndex + 36 + i * 6 + 4] = vIndex + i + 6;
		    triangles[tIndex + 36 + i * 6 + 5] = vIndex + next + 6;
	    }
    }

}
