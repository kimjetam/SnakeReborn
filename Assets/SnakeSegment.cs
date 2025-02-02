using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SnakeSegment : MonoBehaviour
{
    public GameObject nextSegment;

    private Vector3 moveDirection = Vector3.forward; // Default forward direction
    private Vector3 rotationDirection = Vector3.forward; // Default forward direction
    private float rotationSpeed = 5f;
    private bool isMoving = false; // To prevent mid-movement turning
    private bool isTurning = false; // To prevent mid-movement turning
    GameObject sphere;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = transform.position; // Set position
        sphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        
    }

    private void Update()
    {
        if (!isMoving)
        {
            rotationDirection = moveDirection;
            // Move forward on the grid
            StartCoroutine(Move());
        }
    }

    private IEnumerator Move()
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = nextSegment.transform.position;

        var moveSpeed = GetComponentInParent<SnakeController>().moveSpeed;
        var gridSize = GetComponentInParent<SnakeController>().gridSize / 2; ;

        float elapsedTime = 0f;
        while (elapsedTime < gridSize / moveSpeed)
        {
            float t = (elapsedTime * moveSpeed) / gridSize;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            sphere.transform.position = transform.position;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition; // Snap to exact position
        sphere.transform.position = transform.position;

        isMoving = false;
        isTurning = false;
        yield return null;
    }
}
