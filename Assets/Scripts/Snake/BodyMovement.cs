using System.Collections.Generic;
using UnityEngine;

public class BodyMovement : MonoBehaviour
{
    private List<SnakeSegment> _bodySegments;
    private float _gridHalfSize;
    private SnakeSegment _playerSegment;

    public void Initialize(List<SnakeSegment> bodySegments, float gridHalfSize, SnakeSegment playerSegment)
    {
        _bodySegments = bodySegments;
        _gridHalfSize = gridHalfSize;
        _playerSegment = playerSegment;
    }

    private void OnEnable()
    {
        var snakeMovementController = GetComponent<SnakeMovementController>();
        if(snakeMovementController != null )
        {
            snakeMovementController.OnSnakeMovementUpdated += MoveSegments;
            snakeMovementController.OnSnakeMovementStarted += InitBodyMovement;
            snakeMovementController.OnSnakeMovementCompleted += SnapBodySegmentsToGrid;
        }
    }

    private void OnDisable()
    {
        var snakeMovementController = GetComponent<SnakeMovementController>();
        if( snakeMovementController != null)
        {
            snakeMovementController.OnSnakeMovementUpdated -= MoveSegments;
            snakeMovementController.OnSnakeMovementStarted -= InitBodyMovement;
            snakeMovementController.OnSnakeMovementCompleted -= SnapBodySegmentsToGrid;
        }
    }

    private void InitBodyMovement()
    {
        for (int i = 0; i < _bodySegments.Count; i++)
        {
            var segment = _bodySegments[i];
            var prevSegment = i == 0 ? _playerSegment : _bodySegments[i - 1];

            segment.startPosition = RoundToHalf(segment.transform.position);
            segment.moveDirection = prevSegment.moveDirection;
            segment.targetPosition = RoundToHalf(prevSegment.startPosition);

            if (!ArePointsCollinear(prevSegment.targetPosition, segment.targetPosition, segment.startPosition) || segment.halfTurnDone)
            {
                SetupTurning(segment, prevSegment);
            }
        }
    }

    private void MoveSegments(Vector3 headPosition, float step)
    {
        for (int i = 0; i < _bodySegments.Count; i++)
        {
            var segment = _bodySegments[i];
            var newPosition = CalculateNewPosition(segment, step);
            UpdateSegmentPositionAndRotation(segment, newPosition);
        }
    }

    private Vector3 CalculateNewPosition(SnakeSegment segment, float step)
    {
        return segment.isTurning
            ? segment.turnCenterPosition + Vector3.Slerp(segment.turnStartPosition - segment.turnCenterPosition, segment.turnTargetPosition - segment.turnCenterPosition, step)
            : Vector3.Lerp(segment.startPosition, segment.targetPosition, step);
    }

    private void UpdateSegmentPositionAndRotation(SnakeSegment segment, Vector3 newPosition)
    {
        Vector3 rotationDirection = (newPosition - segment.transform.position).normalized;
        if (rotationDirection != Vector3.zero)
        {
            segment.transform.rotation = Quaternion.LookRotation(rotationDirection);
        }
        segment.transform.position = newPosition;
    }

    private void SetupTurning(SnakeSegment segment, SnakeSegment prevSegment)
    {
        segment.isTurning = true;

        if (!segment.halfTurnDone)
        {
            segment.turnCenterPosition = RoundToHalf(segment.startPosition + (prevSegment.targetPosition - segment.targetPosition));
            segment.turnStartPosition = segment.startPosition;

            var midPositionDirection = (segment.startPosition - segment.turnCenterPosition) + (prevSegment.targetPosition - segment.turnCenterPosition);
            segment.turnTargetPosition = segment.turnCenterPosition + midPositionDirection.normalized * _gridHalfSize;
        }
        else
        {
            segment.turnStartPosition = segment.turnTargetPosition;
            segment.turnTargetPosition = segment.targetPosition;
        }
    }

    private void SnapBodySegmentsToGrid()
    {
        for (int i = 0; i < _bodySegments.Count; i++)
        {
            var segment = _bodySegments[i];
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
