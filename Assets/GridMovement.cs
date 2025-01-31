using System.Collections;
using UnityEngine;

public class GridMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement
    public float gridSize = 1f; // Distance between grid points
    private Vector3 moveDirection = Vector3.forward; // Default forward direction
    private bool isMoving = false; // To prevent mid-movement turning
    private bool keyPressed = false; // To prevent mid-movement turning
    public bool isPaused = false;

    void Update()
    {
        if (!keyPressed && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Debug.Log("Left Arrow Pressed");
            moveDirection = Quaternion.Euler(0, -90, 0) * moveDirection;
            keyPressed = true;
        }
        else if (!keyPressed && Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log("Right Arrow Pressed");
            moveDirection = Quaternion.Euler(0, 90, 0) * moveDirection;
            keyPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0 : 1;
            Debug.Log(moveDirection);
        }

        if(!isMoving)
        {
            // Move forward on the grid
            StartCoroutine(Move());
        }
    }

    private IEnumerator Move()
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + moveDirection * gridSize;

        float elapsedTime = 0f;
        while (elapsedTime < gridSize / moveSpeed)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, (elapsedTime * moveSpeed) / gridSize);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition; // Snap to exact position
        isMoving = false;
        keyPressed = false;
        yield return null;
    }
}
