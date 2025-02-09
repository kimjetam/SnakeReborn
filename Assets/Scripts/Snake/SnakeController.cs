using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    private bool isMoving = false;
    private bool isTurning = false;
    private LineRenderer lineRenderer;
    private List<Vector3> lineRendererPoints = new List<Vector3>();
    private SnakeSegment headMovingPart;

    public bool isPaused = false;

    public float moveSpeed = 5f;
    public float gridSize = 1f;
    public float snakeWidthRadius = 0.15f;
    public int initialSnakeLength = 5;
    [HideInInspector]
    public GameObject headNeck;
    [HideInInspector]
    public GameObject headTip;
    [HideInInspector]
    public GameObject headMiddle;
    [HideInInspector]
    public List<GameObject> snakeSegments;
    [HideInInspector]
    public GameObject tail;
    [HideInInspector]
    public GameObject eye1;
    [HideInInspector]
    public GameObject eye2;
    private Material eyeMaterial;
    
    public bool showDebugPath = false;
    public bool showDebugSegments = false;
    public bool showDebugMeshVerticles = false;

    private void OnEnable()
    {
        var snakeInput = GetComponent<SnakeInput>();
        snakeInput.OnSnakeTurn += HandleSnakeTurn; // Subscribe to the event
        snakeInput.OnSnakeSpeedIncrement += HandleMoveSpeedIncrement;
        snakeInput.OnFreezeTime += HandleTimeFreeze;
    }

    private void OnDisable()
    {
        var snakeInput = GetComponent<SnakeInput>();
        snakeInput.OnSnakeTurn -= HandleSnakeTurn; // Unsubscribe from the event
        snakeInput.OnSnakeSpeedIncrement -= HandleMoveSpeedIncrement;
        snakeInput.OnFreezeTime -= HandleTimeFreeze;
    }


    void Start()
    {
        InitSnakeHeadAndTail();
        InitSnakeBody();

        if (showDebugSegments )
        {
            SetupDebugVisuals();
        }

        if (showDebugPath)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = lineRenderer.endWidth = 0.1f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.red;
        }
    }

    void InitSnakeHeadAndTail()
    {
        headNeck = new GameObject("HeadNeck");
        headNeck.transform.position = Vector3.zero;
        headNeck.AddComponent<SnakeSegment>();
        headNeck.AddComponent<MeshSegment>();
        headNeck.transform.SetParent(gameObject.transform, false);
        headMovingPart = headNeck.GetComponent<SnakeSegment>();


        headTip = new GameObject("headTip");
        headTip.transform.position = Vector3.zero;
        headTip.AddComponent<SnakeSegment>();
        headTip.AddComponent<MeshSegment>();
        headTip.transform.SetParent(gameObject.transform, false);
        var headTipMeshSegment = headTip.GetComponent<MeshSegment>();
        headTipMeshSegment.radiusX = 0.01f;
        headTipMeshSegment.radiusY = 0.01f;
        headTipMeshSegment.overrideRadiusValues = true;


        headMiddle = new GameObject("headMiddle");
        headMiddle.transform.position = Vector3.zero;
        headMiddle.AddComponent<SnakeSegment>();
        headMiddle.AddComponent<MeshSegment>();
        headMiddle.transform.SetParent(gameObject.transform, false);
        var headMiddleMeshSegment = headMiddle.GetComponent<MeshSegment>();
        headMiddleMeshSegment.radiusX = 0.3f;
        headMiddleMeshSegment.radiusY = 0.21f;
        headMiddleMeshSegment.overrideRadiusValues = true;

        tail = new GameObject("Tail");
        tail.transform.position = Vector3.zero;
        tail.AddComponent<SnakeSegment>();
        tail.AddComponent<MeshSegment>();
        tail.transform.SetParent(gameObject.transform, false);
        var tailMeshSegment = tail.GetComponent<MeshSegment>();
        tailMeshSegment.radiusX = 0.01f;
        tailMeshSegment.radiusY = 0.01f;
        tailMeshSegment.overrideRadiusValues = true;

        eyeMaterial = Resources.Load<Material>("Materials/yellow");
        eye1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye1.gameObject.name = "eye1";
        eye1.transform.localScale = Vector3.one * 0.1f;
        eye1.transform.SetParent(gameObject.transform, false);

        eye2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye2.gameObject.name = "eye2";
        eye2.transform.localScale = Vector3.one * 0.1f;
        eye2.transform.SetParent(gameObject.transform, false);

        Renderer rend = eye1.GetComponent<Renderer>();
        if (rend != null && eyeMaterial != null)
        {
            rend.material = eyeMaterial;
        }

        rend = eye2.GetComponent<Renderer>();
        if (rend != null && eyeMaterial != null)
        {
            rend.material = eyeMaterial;
        }
    }

    void InitSnakeBody()
    {
        snakeSegments = new List<GameObject>();
        for (int i = 0; i < initialSnakeLength; i++)
        {
            var segment = new GameObject($"Segment_{i}");
            segment.transform.position = Vector3.forward * (gridSize + (gridSize  * i)) * -1;
            segment.AddComponent<SnakeSegment>();
            segment.AddComponent<MeshSegment>();
            segment.transform.SetParent(transform, false);
            snakeSegments.Add(segment);
        }
    }

    void Update()
    {
        RotateHead();

        if (!isMoving) StartCoroutine(Move());
    }

    void FixedUpdate()
    {
        
    }

    private void SetupDebugVisuals()
    {
        CreateSegmentVisual(headNeck);
        foreach (var segment in snakeSegments) CreateSegmentVisual(segment);
    }

    private void CreateSegmentVisual(GameObject segment)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = Vector3.one * 0.5f;
        sphere.transform.SetParent(segment.transform, false);
    }

    private void HandleMoveSpeedIncrement(short increment)
    {
        var newMoveSpeed = moveSpeed + increment;

        if (newMoveSpeed >= 2 && newMoveSpeed <= 6) 
        {
            moveSpeed = newMoveSpeed;
        }
    }

    private void HandleTimeFreeze()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
    }

    private void HandleSnakeTurn(SnakeMovementType movementType)
    {
        if(isTurning) { return; }

        var angle = movementType switch
        {
            SnakeMovementType.TurnLeft => -90f,
            SnakeMovementType.TurnRight => 90f,
            _ => throw new InvalidOperationException($"{movementType} is not a valid turn type")
        };
        headMovingPart.upcommingMoveDirection = Quaternion.Euler(0, angle, 0) * headMovingPart.moveDirection;
        isTurning = true;
    }

    private void RotateHead()
    {
        if (headMovingPart.moveDirection == Vector3.zero) return;
        var targetRotation = Quaternion.LookRotation(headMovingPart.moveDirection);
        headTip.transform.rotation = headMiddle.transform.rotation =  headMovingPart.transform.rotation = Quaternion.Slerp(headMovingPart.transform.rotation, targetRotation, Time.deltaTime * moveSpeed * 2.5f);
    }

    private IEnumerator Move()
    {
        isMoving = true;

        if (IsOnGrid(headMovingPart.transform.position))
            headMovingPart.moveDirection = headMovingPart.upcommingMoveDirection;

        headMovingPart.startPosition = RoundToHalf(headMovingPart.transform.position);
        headMovingPart.targetPosition = RoundToHalf(headMovingPart.startPosition + headMovingPart.moveDirection * gridSize);

        UpdateSegments();

        float elapsedTime = 0f;
        float moveDuration = gridSize / moveSpeed;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            var newPosition = Vector3.Lerp(headMovingPart.startPosition, headMovingPart.targetPosition, t);
            var direction = (newPosition - headMovingPart.transform.position).normalized;
            headMovingPart.transform.position = newPosition;
            headTip.transform.position = newPosition + headTip.transform.forward * 0.9f;
            headMiddle.transform.position = newPosition + headMiddle.transform.forward * 0.35f;
            eye1.transform.position = headMiddle.transform.position + headTip.transform.forward * 0.1f - headTip.transform.right * 0.2f + headTip.transform.up * 0.5f;
            eye2.transform.position = headMiddle.transform.position + headTip.transform.forward * 0.1f + headTip.transform.right * 0.2f + headTip.transform.up * 0.5f;

            //cameraMovement.FixedUpdate2();

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
            var prevSegment = i == 0 ? headMovingPart : snakeSegments[i - 1].GetComponent<SnakeSegment>();

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

            var newPosition = segment.isTurning 
                ? segment.turnCenterPosition + Vector3.Slerp(segment.turnStartPosition - segment.turnCenterPosition, segment.turnTargetPosition - segment.turnCenterPosition, t)
                : Vector3.Lerp(segment.startPosition, segment.targetPosition, t);

            Vector3 rotationDirection = (newPosition - segment.transform.position).normalized;
            if (rotationDirection != Vector3.zero)
            {
                segment.transform.rotation = Quaternion.LookRotation(rotationDirection);
            }
            segment.transform.position = newPosition;

            if(i == snakeSegments.Count - 1)
            {
                tail.transform.position = segment.transform.position - segment.transform.forward;
                tail.transform.rotation = segment.transform.rotation;
            }

            if (showDebugPath && i == 0)
            {
                lineRendererPoints.Add(segment.transform.position);
                lineRenderer.positionCount = lineRendererPoints.Count;
                lineRenderer.SetPosition(lineRendererPoints.Count - 1, segment.transform.position);
            }
        }
    }

    private void SnapToGrid()
    {
        headMovingPart.transform.position = RoundToHalf(headMovingPart.targetPosition);
        headTip.transform.position = headMovingPart.targetPosition + headMovingPart.moveDirection * 0.9f;
        headMiddle.transform.position = headMovingPart.targetPosition + headMovingPart.moveDirection * 0.35f;

        //cameraMovement.FixedUpdate2();

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

            if (i == snakeSegments.Count - 1)
            {
                tail.transform.position = segment.transform.position - segment.transform.forward;
                tail.transform.rotation = segment.transform.rotation;
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