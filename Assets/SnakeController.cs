using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    private bool isMoving = false;
    private bool isTurning = false;
    public bool isPaused = false;

    public float moveSpeed = 5f;
    public float gridSize = 1f;

    public GameObject head;
    public List<GameObject> snakeSegments;

    private SnakeSegment headSegment;
    private LineRenderer lineRenderer;
    private List<Vector3> lineRendererPoints = new List<Vector3>();
    public bool showPath = false;

    void Start()
    {
        headSegment = head.GetComponent<SnakeSegment>();
        headSegment.moveDirection = Vector3.forward;
        headSegment.upcommingMoveDirection = Vector3.forward;

        SetupVisuals();

        if (showPath)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = lineRenderer.endWidth = 0.1f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.red;
        }
    }

    void Update()
    {
        HandleInput();
        RotateHead();
    }

    void FixedUpdate()
    {
        if (!isMoving) StartCoroutine(Move());
    }

    private void SetupVisuals()
    {
        CreateSegmentVisual(head);
        foreach (var segment in snakeSegments) CreateSegmentVisual(segment);
    }

    private void CreateSegmentVisual(GameObject segment)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = Vector3.one * 0.5f;
        sphere.transform.SetParent(segment.transform, false);
    }

    private void HandleInput()
    {
        if (isTurning) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) RotateSnake(-90f);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) RotateSnake(90f);
        else if (Input.GetKeyDown(KeyCode.UpArrow)) moveSpeed++;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && moveSpeed > 1) moveSpeed--;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0 : 1;
        }
    }

    private void RotateSnake(float angle)
    {
        headSegment.upcommingMoveDirection = Quaternion.Euler(0, angle, 0) * headSegment.moveDirection;
        isTurning = true;
    }

    private void RotateHead()
    {
        if (headSegment.moveDirection == Vector3.zero) return;
        var targetRotation = Quaternion.LookRotation(headSegment.moveDirection);
        head.transform.rotation = Quaternion.Slerp(head.transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    private IEnumerator Move()
    {
        isMoving = true;

        if (IsOnGrid(head.transform.position))
            headSegment.moveDirection = headSegment.upcommingMoveDirection;

        headSegment.startPosition = RoundToHalf(head.transform.position);
        headSegment.targetPosition = RoundToHalf(headSegment.startPosition + headSegment.moveDirection * gridSize);

        UpdateSegments();

        float elapsedTime = 0f;
        float moveDuration = gridSize / moveSpeed;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            head.transform.position = Vector3.Lerp(headSegment.startPosition, headSegment.targetPosition, t);
            MoveSegments(t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        SnapToGrid();
        isMoving = false;
        isTurning = false;
    }

    private void UpdateSegments()
    {
        for (int i = 0; i < snakeSegments.Count; i++)
        {
            var segment = snakeSegments[i].GetComponent<SnakeSegment>();
            var prevSegment = i == 0 ? headSegment : snakeSegments[i - 1].GetComponent<SnakeSegment>();

            segment.startPosition = RoundToHalf(segment.transform.position);
            segment.moveDirection = prevSegment.moveDirection;
            segment.targetPosition = RoundToHalf(prevSegment.startPosition);

            if (!ArePointsCollinear(prevSegment.targetPosition, segment.targetPosition, segment.startPosition) || segment.halfTurnDone)
            {
                SetupTurning(segment, prevSegment);
            }
        }
    }

    private void SetupTurning(SnakeSegment segment, SnakeSegment prevSegment)
    {
        segment.isTurning = true;

        if (!segment.halfTurnDone)
        {
            segment.turnCenterPosition = RoundToHalf(segment.startPosition + (prevSegment.targetPosition - segment.targetPosition));
            segment.turnStartPosition = segment.startPosition;

            var midPositionDirection = (segment.startPosition - segment.turnCenterPosition) + (prevSegment.targetPosition - segment.turnCenterPosition);
            segment.turnTargetPosition = segment.turnCenterPosition + midPositionDirection.normalized * gridSize;
        }
        else
        {
            segment.turnStartPosition = segment.turnTargetPosition;
            segment.turnTargetPosition = segment.targetPosition;
        }
    }

    private void MoveSegments(float t)
    {
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

            if (showPath && i == 0)
            {
                lineRendererPoints.Add(segment.transform.position);
                lineRenderer.positionCount = lineRendererPoints.Count;
                lineRenderer.SetPosition(lineRendererPoints.Count - 1, segment.transform.position);
            }
        }
    }

    private void SnapToGrid()
    {
        head.transform.position = headSegment.targetPosition;

        for (int i = 0; i < snakeSegments.Count; i++)
        {
            var segment = snakeSegments[i].GetComponent<SnakeSegment>();
            if (segment.isTurning)
            {
                if (segment.halfTurnDone)
                {
                    segment.isTurning = false;
                    segment.halfTurnDone = false;
                    segment.transform.position = RoundToHalf(segment.turnTargetPosition);
                }
                else
                {
                    segment.halfTurnDone = true;
                    segment.transform.position = segment.turnTargetPosition;
                }
            }
            else
            {
                segment.transform.position = RoundToHalf(segment.targetPosition);
            }
        }
    }

    private bool IsOnGrid(Vector3 position)
    {
        return Mathf.Round(position.x * 2) % 2 == 0 && Mathf.Round(position.z * 2) % 2 == 0;
    }

    private bool ArePointsCollinear(Vector3 A, Vector3 B, Vector3 C, float epsilon = 1e-6f)
    {
        return Vector3.Cross(B - A, C - A).sqrMagnitude < epsilon;
    }

    private Vector3 RoundToHalf(Vector3 original)
    {
        return new Vector3(
            Mathf.Round(original.x * 2) / 2,
            Mathf.Round(original.y * 2) / 2,
            Mathf.Round(original.z * 2) / 2
        );
    }
}