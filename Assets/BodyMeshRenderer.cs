using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BodyMeshRenderer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private List<MeshSegment> meshSegments;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    public Material defaultMaterial;

    void Start()
    {
        var snakeController = GetComponentInParent<SnakeController>();
        var headMeshSegment = snakeController.head.GetComponent<MeshSegment>();
        var bodyMeshSegments = snakeController.snakeSegments.Select(x => x.GetComponent<MeshSegment>());
        meshSegments = new List<MeshSegment> { 
            headMeshSegment
        }.Concat(bodyMeshSegments).ToList();

        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        meshRenderer = GetComponent<MeshRenderer>();

        // Assign a default material with texture
        meshRenderer.material = defaultMaterial;
        //meshRenderer.material.mainTexture = Resources.Load<Texture2D>("default_snake_texture");
    }

    // Update is called once per frame
    void Update()
    {
        GenerateSnakeMesh();
    }

    void GenerateSnakeMesh()
    {
        if (meshSegments == null || meshSegments.Count < 2)
            return;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < meshSegments.Count - 1; i++)
        {
            MeshSegment current = meshSegments[i];
            MeshSegment next = meshSegments[i + 1];

            int baseIndex = vertices.Count;

            // Add current segment's vertices
            vertices.Add(current.vertex1.position); // Right
            vertices.Add(current.vertex2.position); // Left
            vertices.Add(current.vertex3.position); // Up
            vertices.Add(current.vertex4.position); // Down

            // Add next segment's vertices
            vertices.Add(next.vertex1.position); // Right
            vertices.Add(next.vertex2.position); // Left
            vertices.Add(next.vertex3.position); // Up
            vertices.Add(next.vertex4.position); // Down

            // Define four quads (8 triangles)

            // **1. Right Face (vertex1 - vertex3)**
            triangles.Add(baseIndex + 0); // Current Right
            triangles.Add(baseIndex + 4); // Next Right
            triangles.Add(baseIndex + 6); // Next Up

            triangles.Add(baseIndex + 0); // Current Right
            triangles.Add(baseIndex + 6); // Next Up
            triangles.Add(baseIndex + 2); // Current Up

            // **2. Left Face (vertex2 - vertex4)**
            triangles.Add(baseIndex + 1); // Current Left
            triangles.Add(baseIndex + 5); // Current Down
            triangles.Add(baseIndex + 7); // Next Down

            triangles.Add(baseIndex + 1); // Current Left
            triangles.Add(baseIndex + 7); // Next Down
            triangles.Add(baseIndex + 3); // Next Left

            // **3. Top Face (vertex3 - vertex2)**
            triangles.Add(baseIndex + 2); // Current Up
            triangles.Add(baseIndex + 6); // Next Up
            triangles.Add(baseIndex + 5); // Next Left

            triangles.Add(baseIndex + 2); // Current Up
            triangles.Add(baseIndex + 5); // Next Left
            triangles.Add(baseIndex + 1); // Current Left

            // **4. Bottom Face (vertex4 - vertex1)**
            triangles.Add(baseIndex + 3); // Current Down
            triangles.Add(baseIndex + 7); // Current Right
            triangles.Add(baseIndex + 4); // Next Right

            triangles.Add(baseIndex + 3); // Current Down
            triangles.Add(baseIndex + 4); // Next Right
            triangles.Add(baseIndex + 0); // Next Down

        }

        // Apply to mesh
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
}
