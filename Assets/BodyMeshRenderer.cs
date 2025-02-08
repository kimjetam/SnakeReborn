using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BodyMeshRenderer : MonoBehaviour
{
    private List<MeshSegment> meshSegments;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    public Material materialA;
    public Material materialB;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    void UpdateBodyMeshSegments()
    {
        var snakeController = GetComponentInParent<SnakeController>();

        var headTipMeshSegment = snakeController.headTip.GetComponent<MeshSegment>();
        var headMiddleMeshSegment = snakeController.headMiddle.GetComponent<MeshSegment>();
        var headNeckMeshSegment = snakeController.headNeck.GetComponent<MeshSegment>();
        var tailMeshSegment = snakeController.tail.GetComponent<MeshSegment>();
        var bodyMeshSegments = snakeController.snakeSegments.Select(x => x.GetComponent<MeshSegment>());

        meshSegments = new List<MeshSegment> {
            headTipMeshSegment, headMiddleMeshSegment, headNeckMeshSegment
        }.Concat(bodyMeshSegments).Concat(new List<MeshSegment> { tailMeshSegment }).ToList();
    }

    void Update()
    {
        UpdateBodyMeshSegments();
        GenerateSnakeMesh();
    }

    void GenerateSnakeMesh()
    {
        if (meshSegments == null || meshSegments.Count < 2)
            return;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        int segmentCount = meshSegments.Count - 1;
        List<int>[] submeshTriangles = new List<int>[segmentCount];

        for (int i = 0; i < segmentCount; i++)
            submeshTriangles[i] = new List<int>();

        for (int i = 0; i < meshSegments.Count; i++)
        {
            var segment = meshSegments[i];

            // Calculate segment direction for smooth normals
            Vector3 forward = Vector3.zero;
            if (i > 0 && i < meshSegments.Count - 1)
                forward = (meshSegments[i + 1].transform.position - meshSegments[i - 1].transform.position).normalized;
            else if (i == 0)
                forward = (meshSegments[i + 1].transform.position - meshSegments[i].transform.position).normalized;
            else if (i == meshSegments.Count - 1)
                forward = (meshSegments[i].transform.position - meshSegments[i - 1].transform.position).normalized;

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;

            // Store vertices
            vertices.Add(segment.vertex1); // Right
            vertices.Add(segment.vertex2); // Left
            vertices.Add(segment.vertex3); // Up
            vertices.Add(segment.vertex4); // Down

            // Smooth normals
            normals.Add(right);
            normals.Add(-right);
            normals.Add(up);
            normals.Add(-up);

            // UV Mapping
            float uvY = i / (float)(meshSegments.Count - 1);
            uvs.Add(new Vector2(0, uvY)); // Right
            uvs.Add(new Vector2(1, uvY)); // Left
            uvs.Add(new Vector2(0, uvY + 0.1f)); // Up
            uvs.Add(new Vector2(1, uvY + 0.1f)); // Down
        }

        // Generate triangles for each segment
        for (int i = 0; i < segmentCount; i++)
        {
            int baseIndex = i * 4;

            // Right Face
            submeshTriangles[i].Add(baseIndex + 0);
            submeshTriangles[i].Add(baseIndex + 4);
            submeshTriangles[i].Add(baseIndex + 6);

            submeshTriangles[i].Add(baseIndex + 0);
            submeshTriangles[i].Add(baseIndex + 6);
            submeshTriangles[i].Add(baseIndex + 2);

            // Left Face
            submeshTriangles[i].Add(baseIndex + 1);
            submeshTriangles[i].Add(baseIndex + 5);
            submeshTriangles[i].Add(baseIndex + 7);

            submeshTriangles[i].Add(baseIndex + 1);
            submeshTriangles[i].Add(baseIndex + 7);
            submeshTriangles[i].Add(baseIndex + 3);

            // Top Face
            submeshTriangles[i].Add(baseIndex + 2);
            submeshTriangles[i].Add(baseIndex + 6);
            submeshTriangles[i].Add(baseIndex + 5);

            submeshTriangles[i].Add(baseIndex + 2);
            submeshTriangles[i].Add(baseIndex + 5);
            submeshTriangles[i].Add(baseIndex + 1);

            // Bottom Face
            submeshTriangles[i].Add(baseIndex + 3);
            submeshTriangles[i].Add(baseIndex + 7);
            submeshTriangles[i].Add(baseIndex + 4);

            submeshTriangles[i].Add(baseIndex + 3);
            submeshTriangles[i].Add(baseIndex + 4);
            submeshTriangles[i].Add(baseIndex + 0);
        }

        // Assign Materials Alternating
        Material[] materials = new Material[segmentCount];
        for (int i = 0; i < segmentCount; i++)
            materials[i] = i < 2 || (i % 2 == 1) ? materialA : materialB;

        // Apply to Mesh
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.subMeshCount = segmentCount;
        for (int i = 0; i < segmentCount; i++)
            mesh.SetTriangles(submeshTriangles[i], i);

        meshRenderer.materials = materials;
    }
}
