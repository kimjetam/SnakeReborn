using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.forward; // Default forward direction
    private Vector3 rotationDirection = Vector3.forward; // Default forward direction
    private float rotationSpeed = 5f;
    private bool isMoving = false; // To prevent mid-movement turning
    private bool isTurning = false; // To prevent mid-movement turning
    public bool isPaused = false;
    private SnakeController _snakeController = null;
    public Vector3 targetPosition;

    private Vector3[] sphereOffsets = new Vector3[4]; // Store relative positions
    private GameObject[] smallSpheres;

    private void Start()
    {
        var snakeController = GetComponentInParent<SnakeController>();
        if (snakeController != null)
        {
            _snakeController = snakeController;
        }

        smallSpheres = new GameObject[4];

        // Define the offsets (adjust based on the size of your main object)
        sphereOffsets[0] = new Vector3(0.25f, 0, 0);   // Right
        sphereOffsets[1] = new Vector3(-0.25f, 0, 0);  // Left
        sphereOffsets[2] = new Vector3(0, 0.25f, 0);   // Up
        sphereOffsets[3] = new Vector3(0, -0.25f, 0);  // Down

        for (int i = 0; i < 4; i++)
        {
            // Create sphere dynamically
            smallSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            // Position it relative to the main object
            smallSpheres[i].transform.position = transform.position + sphereOffsets[i];

            // Make it a child of the main object
            smallSpheres[i].transform.SetParent(transform);

            // Scale down the spheres to be small
            smallSpheres[i].transform.localScale = Vector3.one * 0.05f;
        }
    }


    void Update()
    {
        if (!isTurning && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            var rotationAngle = -90f;
            moveDirection = Quaternion.Euler(0, rotationAngle, 0) * moveDirection;
            isTurning = true;
        }
        else if (!isTurning && Input.GetKeyDown(KeyCode.RightArrow))
        {
            var rotationAngle = 90f;
            moveDirection = Quaternion.Euler(0, rotationAngle, 0) * moveDirection;
            isTurning = true;
        }
        else if (!isTurning && Input.GetKeyDown(KeyCode.UpArrow))
        {
            GetComponentInParent<SnakeController>().moveSpeed++;
        }
        else if (!isTurning && Input.GetKeyDown(KeyCode.DownArrow))
        {
            var moveSpeed = GetComponentInParent<SnakeController>().moveSpeed;
            if(moveSpeed > 1)
            {
                GetComponentInParent<SnakeController>().moveSpeed--;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0 : 1;
        }

        if (rotationDirection != Vector3.zero)
        {
            //create the rotation we need to be in to look at the target
            var lookRotation = Quaternion.LookRotation(rotationDirection);

            // Smoothly rotate towards the target point.
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        if (!isMoving)
        {
            rotationDirection = moveDirection;
            // Move forward on the grid
            StartCoroutine(Move());
        }
    }

    private void FixedUpdate()
    {
        
    }

    private IEnumerator Move()
    {
        isMoving = true;

        var moveSpeed = GetComponentInParent<SnakeController>().moveSpeed;
        var gridSize = GetComponentInParent<SnakeController>().gridSize;

        Vector3 startPosition = transform.position;
        targetPosition = startPosition + moveDirection * gridSize;

        float elapsedTime = 0f;
        while (elapsedTime < gridSize / moveSpeed)
        {
            float t = elapsedTime * moveSpeed / gridSize;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition; // Snap to exact position

        if (_snakeController != null)
        {

        }

        isMoving = false;
        isTurning = false;
    }
}
