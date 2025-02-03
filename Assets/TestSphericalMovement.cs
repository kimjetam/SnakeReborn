using UnityEngine;

public class TestSphericalMovement : MonoBehaviour
{
    public Transform center; // The center of the circle
    public float radius = 2f;
    public float duration = 2f; // Time to complete one quadrant
    private float timeElapsed = 0f;

    private Vector3 startPos;
    private Vector3 endPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = center.position + new Vector3(radius, 0, 0); // Start at (radius, 0)
        endPos = center.position + new Vector3(0, 0, -radius);
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime / duration;
        transform.position = Vector3.Slerp(startPos - center.position, endPos - center.position, timeElapsed) + center.position;
    }
}
