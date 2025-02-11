using System.Collections.Generic;
using UnityEngine;

public class BodyMovement : MonoBehaviour
{
    private List<SnakeSegment> _bodySegments;
    private float _gridHalfSize;
    private SnakeSegment _playerSegment;

    // Cache frequently used variables
    private Vector3 _newPosition;
    private Vector3 _rotationDirection;

    public void Initialize(List<SnakeSegment> bodySegments, float gridHalfSize, SnakeSegment playerSegment)
    {
        _bodySegments = bodySegments;
        _gridHalfSize = gridHalfSize;
        _playerSegment = playerSegment;
    }

    private void OnEnable()
    {
        var playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.OnSnakeMovementUpdated += MoveSegments;
            playerMovement.OnSnakeMovementStarted += InitBodyMovement;
            playerMovement.OnSnakeMovementCompleted += SnapBodySegmentsToGrid;
        }
    }

    private void OnDisable()
    {
        var playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.OnSnakeMovementUpdated -= MoveSegments;
            playerMovement.OnSnakeMovementStarted -= InitBodyMovement;
            playerMovement.OnSnakeMovementCompleted -= SnapBodySegmentsToGrid;
        }
    }

    private void InitBodyMovement()
    {
        for (int i = 0; i < _bodySegments.Count; i++)
        {
            var segment = _bodySegments[i];
            var prevSegment = i == 0 ? _playerSegment : _bodySegments[i - 1];

            segment.startPosition = VectorHelper.RoundToHalf(segment.transform.position);
            segment.moveDirection = prevSegment.moveDirection;
            segment.targetPosition = VectorHelper.RoundToHalf(prevSegment.startPosition);

            if (!VectorHelper.ArePointsCollinear(prevSegment.targetPosition, segment.targetPosition, segment.startPosition) || segment.halfTurnDone)
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
            _newPosition = CalculateNewPosition(segment, step);
            UpdateSegmentPositionAndRotation(segment, _newPosition);
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
        _rotationDirection = (newPosition - segment.transform.position).normalized;
        if (_rotationDirection != Vector3.zero)
        {
            segment.transform.rotation = Quaternion.LookRotation(_rotationDirection);
        }
        segment.transform.position = newPosition;
    }

    private void SetupTurning(SnakeSegment segment, SnakeSegment prevSegment)
    {
        segment.isTurning = true;

        if (!segment.halfTurnDone)
        {
            segment.turnCenterPosition = VectorHelper.RoundToHalf(segment.startPosition + (prevSegment.targetPosition - segment.targetPosition));
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
                    segment.transform.position = VectorHelper.RoundToHalf(segment.turnTargetPosition);
                }
                else
                {
                    segment.halfTurnDone = true;
                    segment.transform.position = segment.turnTargetPosition;
                }
            }
            else
            {
                segment.transform.position = VectorHelper.RoundToHalf(segment.targetPosition);
            }
        }
    }
}