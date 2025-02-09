using UnityEngine;

public class MeshSegment : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [HideInInspector]
    public Vector3 vertex1;
    [HideInInspector]
    public Vector3 vertex2;
    [HideInInspector]
    public Vector3 vertex3;
    [HideInInspector]
    public Vector3 vertex4;

    public float radiusX = 0.15f;
    public float radiusY = 0.15f;
    public bool overrideRadiusValues = false;

    private bool showDebugVertices = false;

    void Start()
    {
        if(!overrideRadiusValues)
        {
            radiusX = radiusY = gameObject.GetComponentInParent<SnakeManager>().snakeWidthRadius;
        }
        showDebugVertices = gameObject.GetComponentInParent<SnakeManager>().showDebugMeshVerticles;
    }

    private void OnDrawGizmos()
    {
        if (showDebugVertices)
        {
            Gizmos.color = UnityEngine.Color.blue;
            Gizmos.DrawSphere(vertex1, 0.02f);
            Gizmos.DrawSphere(vertex2, 0.02f);
            Gizmos.DrawSphere(vertex3, 0.02f);
            Gizmos.DrawSphere(vertex4, 0.02f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        var computedVertices = ComputeSegmentMeshVertices(gameObject.transform.position, gameObject.transform.rotation, radiusX, radiusY);
        vertex1 = computedVertices[0];
        vertex2 = computedVertices[1];
        vertex3 = computedVertices[2];
        vertex4 = computedVertices[3];
    }

    private static Vector3[] ComputeSegmentMeshVertices(Vector3 centerPosition, Quaternion rotation, float radiusX, float radiusY)
    {
        var right = new Vector3(radiusX, 0, 0);
        var left = new Vector3(-radiusX, 0, 0);
        var up = new Vector3(0, radiusY, 0);
        var down = new Vector3(0, -radiusY, 0);

        right = rotation * right;
        left = rotation * left;
        up = rotation * up;
        down = rotation * down;

        // Add the rotated offsets to the center position
        return new[]
        {
            centerPosition + right,
            centerPosition + left,
            centerPosition + up,
            centerPosition + down
        };
    }
}
