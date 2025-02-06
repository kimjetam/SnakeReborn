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
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        meshRenderer = GetComponent<MeshRenderer>();

        // Assign a default material with texture
        meshRenderer.material = defaultMaterial;
        //meshRenderer.material.mainTexture = Resources.Load<Texture2D>("default_snake_texture");
    }

    void UpdateBodyMeshSegments()
    {
        var snakeController = GetComponentInParent<SnakeController>();
        var headMeshSegment = snakeController.head.GetComponent<MeshSegment>();
        var bodyMeshSegments = snakeController.snakeSegments.Select(x => x.GetComponent<MeshSegment>());

        meshSegments = new List<MeshSegment> {
            headMeshSegment
        }.Concat(bodyMeshSegments).ToList();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdateBodyMeshSegments();
        GenerateSnakeMesh();
    }

    void GenerateSnakeMesh()
    {
        if (meshSegments == null || meshSegments.Count < 2)
            return;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>(); List<Vector2> uvs = new List<Vector2>(); // UV coordinates

        float uvStep = 1.0f / (meshSegments.Count - 1); // Step for texture mapping

        for (int i = 0; i < meshSegments.Count - 1; i++)
        {
            var current = meshSegments[i];
            var next = meshSegments[i + 1];

            int baseIndex = vertices.Count;

            // Add current segment's vertices
            vertices.Add(current.vertex1); // Right
            vertices.Add(current.vertex2); // Left
            vertices.Add(current.vertex3); // Up
            vertices.Add(current.vertex4); // Down

            // Add next segment's vertices
            vertices.Add(next.vertex1); // Right
            vertices.Add(next.vertex2); // Left
            vertices.Add(next.vertex3); // Up
            vertices.Add(next.vertex4); // Down

            // UV Mapping - Stretch along the length of the snake
            float uvX = i * uvStep;
            float uvXNext = (i + 1) * uvStep;

            uvs.Add(new Vector2(uvX, 1)); // Current Right
            uvs.Add(new Vector2(uvX, 0)); // Current Left
            uvs.Add(new Vector2(uvX, 1)); // Current Up
            uvs.Add(new Vector2(uvX, 0)); // Current Down

            uvs.Add(new Vector2(uvXNext, 1)); // Next Right
            uvs.Add(new Vector2(uvXNext, 0)); // Next Left
            uvs.Add(new Vector2(uvXNext, 1)); // Next Up
            uvs.Add(new Vector2(uvXNext, 0)); // Next Down

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
        mesh.uv = uvs.ToArray(); // Assign UV mapping
        mesh.RecalculateNormals();
    }
}
