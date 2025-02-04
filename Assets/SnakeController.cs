using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    private bool isMoving = false; // To prevent mid-movement turning
    private bool isTurning = false; // To prevent mid-movement turning
    public bool isPaused = false;

    public float moveSpeed = 5f; // Speed of movement
    public float gridSize = 1f; // Distance between grid points

    public GameObject head;
    public List<GameObject> snakeSegments;

    LineRenderer lineRenderer;
    private List<Vector3> lineRendererPoints = new List<Vector3>();
    public bool showPath = false;

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

        if(showPath)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.red;
        }
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

        headSegment.startPosition = RoundToHalf(head.transform.position);
        headSegment.targetPosition = RoundToHalf(headSegment.startPosition + headSegment.moveDirection * gridSize);



        for (int i = 0; i < snakeSegments.Count; i++)
        {
            var segment = snakeSegments[i].GetComponent<SnakeSegment>();
            segment.startPosition = RoundToHalf(segment.transform.position);

            var prevSegment = i == 0 ? headSegment : snakeSegments[i - 1].GetComponent<SnakeSegment>();
            segment.moveDirection = prevSegment.moveDirection;
            segment.targetPosition = RoundToHalf(prevSegment.startPosition);

            if (!ArePointsCollinear(prevSegment.targetPosition, segment.targetPosition, segment.startPosition) || segment.turnStep != 0)
            {
                segment.isTurning = true;
                if (segment.turnStep == 0)
                {
                    segment.turnCenterPosition = RoundToHalf(segment.startPosition + (prevSegment.targetPosition - segment.targetPosition));
                    segment.turnStartPosition = segment.startPosition;
                    var midPositionDirection = (segment.startPosition - segment.turnCenterPosition) + (prevSegment.targetPosition - segment.turnCenterPosition);
                    midPositionDirection.Normalize();
                    segment.turnTargetPosition = segment.turnCenterPosition + midPositionDirection * gridSize;
                }
                else
                {
                    segment.turnStartPosition = segment.turnTargetPosition;
                    segment.turnTargetPosition = segment.targetPosition;
                }
            }
        }

        float elapsedTime = 0f;
        while (elapsedTime < gridSize / moveSpeed)
        {
            float t = elapsedTime * moveSpeed / gridSize;
            head.transform.position = Vector3.Lerp(headSegment.startPosition, headSegment.targetPosition, t);

            for (int i = 0; i < snakeSegments.Count; i++)
            {
                var segment = snakeSegments[i].GetComponent<SnakeSegment>();

                if (segment.isTurning)
                {
                    segment.transform.position = segment.turnCenterPosition + Vector3.Slerp(segment.turnStartPosition - segment.turnCenterPosition, segment.turnTargetPosition - segment.turnCenterPosition, t);
                }
                else
                {
                    segment.transform.position = Vector3.Lerp(segment.startPosition, segment.targetPosition, t);
                }

                if(showPath && i == 0)
                {
                    lineRendererPoints.Add(segment.transform.position);
                    lineRenderer.positionCount = lineRendererPoints.Count;
                    lineRenderer.SetPosition(lineRendererPoints.Count - 1, segment.transform.position);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        head.transform.position = headSegment.targetPosition; // Snap to exact position
        for (int i = 0; i < snakeSegments.Count; i++)
        {
            var segment = snakeSegments[i].GetComponent<SnakeSegment>();
            if (segment.isTurning)
            {
                segment.turnStep++;
                if(segment.turnStep > 1)
                {
                    segment.isTurning = false;
                    segment.turnStep = 0;
                    segment.transform.position = RoundToHalf(segment.turnTargetPosition);
                }
                else
                {
                    segment.transform.position = segment.turnTargetPosition;
                }
            }
            else
            {
                segment.transform.position = RoundToHalf(segment.targetPosition);
            }
        }

        isMoving = false;
        isTurning = false;
    }

    bool ArePointsCollinear(Vector3 A, Vector3 B, Vector3 C, float epsilon = 1e-6f)
    {
        Vector3 AB = B - A;
        Vector3 AC = C - A;

        return Vector3.Cross(AB, AC).sqrMagnitude < epsilon;
    }

    Vector3 RoundToHalf(Vector3 original)
    {
        return new Vector3(
            Mathf.Round(original.x * 2) / 2,
            Mathf.Round(original.y * 2) / 2,
            Mathf.Round(original.z * 2) / 2
        );
    }
}
