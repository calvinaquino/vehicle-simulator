using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireRenderer : MonoBehaviour {
    //private Mesh mesh;
    //private MeshFilter meshFilter;
    //private Vector3 outDir;

    void Start() {
        //mesh = new Mesh();
        //outDir = Vector3.right;

        //gameObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
        //meshFilter = gameObject.AddComponent<MeshFilter>();
        //meshFilter.sharedMesh = mesh;

        Vector3[] vertices = new Vector3[0];
        Vector2[] uv = new Vector2[0];
        int[] triangles = new int[0];

        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        gameObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    void Update() {
        
    }
}
