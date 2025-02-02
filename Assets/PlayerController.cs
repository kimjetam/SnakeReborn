using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private Vector3 moveDirection = Vector3.forward; // Default forward direction
    private Vector3 rotationDirection = Vector3.forward; // Default forward direction
    private float rotationSpeed = 5f;
    private bool isMoving = false; // To prevent mid-movement turning
    private bool isTurning = false; // To prevent mid-movement turning
    public bool isPaused = false;
    private SnakeController _snakeController = null;

    private void Start()
    {
        var snakeController = GetComponentInParent<SnakeController>();
        if (snakeController != null)
        {
            _snakeController = snakeController;
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
        Vector3 targetPosition = startPosition + moveDirection * gridSize;

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
        yield return null;
    }
}
