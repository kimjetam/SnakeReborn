using System.Drawing;
using UnityEngine;

public class MeshSegment : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform vertex1;
    public Transform vertex2;
    public Transform vertex3;
    public Transform vertex4;

    private float size = 0.15f;
    private bool showDebugVertices = false;

    void Start()
    {
        size = gameObject.GetComponentInParent<SnakeController>().snakeWidthRadius;

        CreateVertex(ref vertex1, "Vertex1", new Vector3(size, 0, 0)); // right
        CreateVertex(ref vertex2, "Vertex2", new Vector3(-size, 0, 0)); // left
        CreateVertex(ref vertex3, "Vertex3", new Vector3(0, size, 0)); // up
        CreateVertex(ref vertex4, "Vertex4", new Vector3(0, -size, 0)); // down

        showDebugVertices = gameObject.GetComponentInParent<SnakeController>().showDebugMeshVerticles;

        if (showDebugVertices)
        {
            CreateDebugSphere(vertex1);
            CreateDebugSphere(vertex2);
            CreateDebugSphere(vertex3);
            CreateDebugSphere(vertex4);
        }
    }

    void CreateVertex(ref Transform vertex, string name, Vector3 offset)
    {
        var obj = new GameObject(name);
        obj.transform.position = transform.position + offset;
        obj.transform.SetParent(transform);
        vertex = obj.transform;
    }

    void CreateDebugSphere(Transform vertex)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = vertex.position;
        sphere.transform.localScale = Vector3.one * 0.05f;
        sphere.transform.SetParent(vertex);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
