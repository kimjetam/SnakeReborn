using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public class SnakeSegment : MonoBehaviour
{
    public GameObject nextSegment;

    private Vector3 moveDirection = Vector3.forward; // Default forward direction
    private Vector3 rotationDirection = Vector3.forward; // Default forward direction
    private float rotationSpeed = 5f;
    private bool isMoving = false; // To prevent mid-movement turning
    private bool isTurning = false; // To prevent mid-movement turning
    public Vector3 targetPosition;

    private Vector3[] sphereOffsets = new Vector3[4]; // Store relative positions
    private GameObject[] smallSpheres;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

    void RotateSmallSpheres()
    {
        float rotationSpeed = 5f; // Adjust speed as needed
        foreach (GameObject sphere in smallSpheres)
        {
            sphere.transform.RotateAround(transform.position, Vector3.right, rotationSpeed * Time.deltaTime);
        }
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


            //Vector3? nextMoveDirection = null;
            //if (nextSegment.GetComponent<PlayerController>() != null)
            //{
            //    nextMoveDirection = nextSegment.GetComponent<PlayerController>().moveDirection;
            //}
            //else
            //{
            //    nextMoveDirection = nextSegment.GetComponent<SnakeSegment>().moveDirection;
            //}

            //if (nextMoveDirection != null && moveDirection != nextMoveDirection)
            //{
            //    Debug.Log($"Turn detected in {gameObject.name}!");
            //}
            //else
            //{
            //    StartCoroutine(Move());
            //}
        }

        //RotateSmallSpheres();

        var targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private IEnumerator Move()
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        targetPosition = nextSegment.transform.position;

        var moveSpeed = GetComponentInParent<SnakeController>().moveSpeed;
        var gridSize = GetComponentInParent<SnakeController>().gridSize / 2;
        

        var direction = (targetPosition - startPosition);
        direction.Normalize();
        moveDirection = direction;

        

        float elapsedTime = 0f;
        while (elapsedTime < gridSize / moveSpeed)
        {
            float t = (elapsedTime * moveSpeed) / gridSize;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        

        transform.position = targetPosition; // Snap to exact position

        isMoving = false;
        isTurning = false;
    }
}
