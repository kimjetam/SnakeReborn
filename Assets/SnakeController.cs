using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GridBrushBase;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using Unity.Hierarchy;

public class SnakeController : MonoBehaviour
{
    private bool isMoving = false; // To prevent mid-movement turning
    private bool isTurning = false; // To prevent mid-movement turning
    public bool isPaused = false;

    public float moveSpeed = 5f; // Speed of movement
    public float gridSize = 1f; // Distance between grid points

    public GameObject head;
    public List<GameObject> snakeSegments;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        sphere.transform.SetParent(head.transform, false);

        for (int i = 0; i < snakeSegments.Count; i++)
        {
            var segmentSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            segmentSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            segmentSphere.transform.SetParent(snakeSegments[i].transform, false);
        }

        head.GetComponent<SnakeSegment>().moveDirection = Vector3.forward;
        head.GetComponent<SnakeSegment>().upcommingMoveDirection = Vector3.forward;
        
    }

    // Update is called once per frame

    void Update()
    {
        HandleInput();
        var headSegment = head.GetComponent<SnakeSegment>();
        if (headSegment.moveDirection != Vector3.zero)
        {
            //create the rotation we need to be in to look at the target
            var lookRotation = Quaternion.LookRotation(headSegment.moveDirection);

            // Smoothly rotate towards the target point.
            head.transform.rotation = Quaternion.Slerp(head.transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

       
    }

    private void FixedUpdate()
    {
        if (!isMoving)
        {
            StartCoroutine(Move());
        }
    }

    private void HandleInput()
    {
        var headSegment = head.GetComponent<SnakeSegment>();
        if (!isTurning && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            var rotationAngle = -90f;
            headSegment.upcommingMoveDirection = Quaternion.Euler(0, rotationAngle, 0) * headSegment.moveDirection;
            isTurning = true;
        }
        else if (!isTurning && Input.GetKeyDown(KeyCode.RightArrow))
        {
            var rotationAngle = 90f;
            headSegment.upcommingMoveDirection = Quaternion.Euler(0, rotationAngle, 0) * headSegment.moveDirection;
            isTurning = true;
        }
        else if (!isTurning && Input.GetKeyDown(KeyCode.UpArrow))
        {
            GetComponentInParent<SnakeController>().moveSpeed++;
        }
        else if (!isTurning && Input.GetKeyDown(KeyCode.DownArrow))
        {
            var moveSpeed = GetComponentInParent<SnakeController>().moveSpeed;
            if (moveSpeed > 1)
            {
                GetComponentInParent<SnakeController>().moveSpeed--;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0 : 1;
        }
    }

    private IEnumerator Move()
    {
        isMoving = true;
        var headSegment = head.GetComponent<SnakeSegment>();

        if (Mathf.Round(head.transform.position.x * 2) % 2 == 0 &&
        Mathf.Round(head.transform.position.z * 2) % 2 == 0)
        {
            headSegment.moveDirection = headSegment.upcommingMoveDirection;
        }

        headSegment.startPosition = head.transform.position;
        headSegment.targetPosition = headSegment.startPosition + headSegment.moveDirection * gridSize;

        for(int i = 0; i < snakeSegments.Count; i++)
        {
            var segment = snakeSegments[i].GetComponent<SnakeSegment>();
            segment.startPosition = segment.transform.position;
            if(i == 0)
            {
                segment.targetPosition = headSegment.startPosition;
                segment.moveDirection = headSegment.moveDirection;
            }
            else
            {
                var prevSegment = snakeSegments[i - 1].GetComponent<SnakeSegment>();
                segment.targetPosition = prevSegment.transform.position;
                segment.moveDirection = prevSegment.moveDirection;
            }
        }

        float elapsedTime = 0f;
        while (elapsedTime < gridSize / moveSpeed)
        {
            float t = elapsedTime * moveSpeed / gridSize;
            head.transform.position = Vector3.Lerp(headSegment.startPosition, headSegment.targetPosition, t);
                
            for(int i = 0; i < snakeSegments.Count; i++)
            {
                var segment = snakeSegments[i].GetComponent<SnakeSegment>();
                segment.transform.position = Vector3.Lerp(segment.startPosition, segment.targetPosition, t);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        head.transform.position = headSegment.targetPosition; // Snap to exact position
        for (int i = 0; i < snakeSegments.Count; i++)
        {
            var segment = snakeSegments[i].GetComponent<SnakeSegment>();
            segment.transform.position = segment.targetPosition;
        }

        isMoving = false;
        isTurning = false;
    }
}
