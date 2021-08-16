using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolver : MonoBehaviour
{
    [SerializeField] private int _groupCount = 4;

    private MeshFilter _meshFilter;
    private Material _material;








    #region Unity_Lifecycle
    //private void Awake() { }
    private void OnEnable() { InitializeDissolver(); GenerateMesh(); }
    //private void Start() { }
    //private void FixedUpdate() { }
    //private void Update() { }
    //private void LateUpdate() { }
    //private void OnDrawGizmos() { }
    //private void OnDisable() { }
    //private void OnDestroy() { }
    #endregion

    private void InitializeDissolver()
    {
        _meshFilter = GetComponent<MeshFilter>();
        GenerateMesh();
        _material = GetComponent<Renderer>()?.sharedMaterial;
    }




    private void GenerateMesh()
    {
        if (!_meshFilter) { return; }

        var mesh = _meshFilter.sharedMesh;

        var uv = mesh.uv;
        var vertices = mesh.vertices;
        var triangles = mesh.triangles;

        var trianglesLength = triangles.Length;

        var newTriangles = new int[trianglesLength];
        var newVertices = new Vector3[trianglesLength];
        var newUV = new Vector2[trianglesLength];
        var newUV2 = new Vector2[trianglesLength];
        var newUV3 = new Vector2[trianglesLength];

        var trianglesCount = trianglesLength / 3;
        var triangleCenters = new Vector3[trianglesCount];

        var groupId = new float[trianglesCount];

        for (int i = 0; i < trianglesCount; i++)
        {
            var v0 = vertices[triangles[i * 3]];
            var v1 = vertices[triangles[i * 3] + 1];
            var v2 = vertices[triangles[i * 3] + 2];

            triangleCenters[i] = (v0 + v1 + v2) / 3;
            groupId[i] = (float)Random.Range(0, _groupCount) / _groupCount;
        }

        for (int i = 0; i < trianglesLength; i++)
        {
            var vi = triangles[i];
            newTriangles[i] = i;
            newVertices[i] = vertices[vi];
            newUV[i] = uv[vi];

            var tri = i / 3;
            var center = triangleCenters[tri];
            newUV2[i] = new Vector2(center.x, center.y);
            newUV3[i] = new Vector2(center.z, groupId[tri]);
        }

        var newMesh = new Mesh();
        newMesh.name = "New Mesh";
        newMesh.vertices = newVertices;
        newMesh.uv = newUV;
        newMesh.uv2 = newUV2;
        newMesh.uv3 = newUV3;
        newMesh.triangles = newTriangles;
        _meshFilter.mesh = newMesh;
    }
}