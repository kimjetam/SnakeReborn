using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnakeMovementController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool isMovementCoroutineInProgress = false;
    private bool isTurning = false;
    private bool isFrozen = false;
    private SnakeSegment playerSegment;
    private List<SnakeSegment> bodySegments;
    private float moveSpeed;
    private float gridHalfSize;


    void OnEnable()
    {
        var snakeInput = GetComponent<SnakeInput>();
        snakeInput.OnSnakeTurn += HandleSnakeTurn; // Subscribe to the event
        snakeInput.OnSnakeSpeedIncrement += HandleMoveSpeedIncrement;
        snakeInput.OnFreezeTime += HandleTimeFreezeSwitch;
    }

    void OnDisable()
    {
        var snakeInput = GetComponent<SnakeInput>();
        snakeInput.OnSnakeTurn -= HandleSnakeTurn; // Unsubscribe from the event
        snakeInput.OnSnakeSpeedIncrement -= HandleMoveSpeedIncrement;
        snakeInput.OnFreezeTime -= HandleTimeFreezeSwitch;
    }
    void Start()
    {
        var snakeManager = GetComponent<SnakeManager>();
        playerSegment = snakeManager.headNeck.GetComponent<SnakeSegment>();
        bodySegments = snakeManager.snakeSegments.Select(x => x.GetComponent<SnakeSegment>()).ToList();
        gridHalfSize = snakeManager.gridHalfSize;
        moveSpeed = snakeManager.moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        RotateHead();

        if (!isMovementCoroutineInProgress) StartCoroutine(Move());
    }

    private void HandleSnakeTurn(SnakeMovementType movementType)
    {
        if (isTurning) { return; }

        var angle = movementType switch
        {
            SnakeMovementType.TurnLeft => -90f,
            SnakeMovementType.TurnRight => 90f,
            _ => throw new InvalidOperationException($"{movementType} is not a valid turn type")
        };
        playerSegment.upcommingMoveDirection = Quaternion.Euler(0, angle, 0) * playerSegment.moveDirection;
        isTurning = true;
    }

    private void HandleTimeFreezeSwitch()
    {
        isFrozen = !isFrozen;
        Time.timeScale = isFrozen ? 0 : 1;
    }

    private void HandleMoveSpeedIncrement(short increment)
    {
        var newMoveSpeed = moveSpeed + increment;

        if (newMoveSpeed >= 2 && newMoveSpeed <= 6)
        {
            moveSpeed = newMoveSpeed;
        }
    }

    private IEnumerator Move()
    {
        isMovementCoroutineInProgress = true;

        if (IsOnGrid(playerSegment.transform.position))
            playerSegment.moveDirection = playerSegment.upcommingMoveDirection;

        playerSegment.startPosition = RoundToHalf(playerSegment.transform.position);
        playerSegment.targetPosition = RoundToHalf(playerSegment.startPosition + playerSegment.moveDirection * gridHalfSize);

        UpdateSegments();

        float elapsedTime = 0f;
        float moveDuration = gridHalfSize / moveSpeed;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            var newPosition = Vector3.Lerp(playerSegment.startPosition, playerSegment.targetPosition, t);
            playerSegment.transform.position = newPosition;

            MoveSegments(t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        SnapToGrid();
        isTurning = false;

        isMovementCoroutineInProgress = false;
        yield return null;
    }

    private void RotateHead()
    {
        if (playerSegment.moveDirection == Vector3.zero) return;
        var targetRotation = Quaternion.LookRotation(playerSegment.moveDirection);
        playerSegment.transform.rotation = Quaternion.Slerp(playerSegment.transform.rotation, targetRotation, Time.deltaTime * moveSpeed * 2.5f);
    }

    private void UpdateSegments()
    {
        for (int i = 0; i < bodySegments.Count; i++)
        {
            var segment = bodySegments[i];
            var prevSegment = i == 0 ? playerSegment : bodySegments[i - 1];

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
            segment.turnTargetPosition = segment.turnCenterPosition + midPositionDirection.normalized * gridHalfSize;
        }
        else
        {
            segment.turnStartPosition = segment.turnTargetPosition;
            segment.turnTargetPosition = segment.targetPosition;
        }
    }

    private void MoveSegments(float t)
    {
        for (int i = 0; i < bodySegments.Count; i++)
        {
            var segment = bodySegments[i];

            var newPosition = segment.isTurning
                ? segment.turnCenterPosition + Vector3.Slerp(segment.turnStartPosition - segment.turnCenterPosition, segment.turnTargetPosition - segment.turnCenterPosition, t)
                : Vector3.Lerp(segment.startPosition, segment.targetPosition, t);

            Vector3 rotationDirection = (newPosition - segment.transform.position).normalized;
            if (rotationDirection != Vector3.zero)
            {
                segment.transform.rotation = Quaternion.LookRotation(rotationDirection);
            }
            segment.transform.position = newPosition;
        }
    }

    private void SnapToGrid()
    {
        playerSegment.transform.position = RoundToHalf(playerSegment.targetPosition);

        for (int i = 0; i < bodySegments.Count; i++)
        {
            var segment = bodySegments[i];
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
