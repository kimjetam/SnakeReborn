using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool _isMovementCoroutineInProgress = false;
    private bool _isTurning = false;
    private bool _isFrozen = false;
    private SnakeSegment _playerSegment;
    private float _moveSpeed;
    private float _gridHalfSize;

    public event Action OnSnakeMovementStarted; // Event for head movement
    public event Action<Vector3, float> OnSnakeMovementUpdated; // Event for head movement
    public event Action OnSnakeMovementCompleted; // Event for head movement

    public void Initialize(SnakeSegment playerSegment, float gridHalfSize, float moveSpeed)
    {
        _playerSegment = playerSegment;
        _gridHalfSize = gridHalfSize;
        _moveSpeed = moveSpeed;
    }


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

    // Update is called once per frame
    void Update()
    {
        RotateHead();

        if (!_isMovementCoroutineInProgress) StartCoroutine(Move());
    }

    private void HandleSnakeTurn(SnakeMovementType movementType)
    {
        if (_isTurning) { return; }

        var angle = movementType switch
        {
            SnakeMovementType.TurnLeft => -90f,
            SnakeMovementType.TurnRight => 90f,
            _ => throw new InvalidOperationException($"{movementType} is not a valid turn type")
        };
        _playerSegment.upcommingMoveDirection = Quaternion.Euler(0, angle, 0) * _playerSegment.moveDirection;
        _isTurning = true;
    }

    private void HandleTimeFreezeSwitch()
    {
        _isFrozen = !_isFrozen;
        Time.timeScale = _isFrozen ? 0 : 1;
    }

    private void HandleMoveSpeedIncrement(short increment)
    {
        var newMoveSpeed = _moveSpeed + increment;

        if (newMoveSpeed >= 2 && newMoveSpeed <= 6)
        {
            _moveSpeed = newMoveSpeed;
        }
    }

    private IEnumerator Move()
    {
        _isMovementCoroutineInProgress = true;

        if (IsOnGrid(_playerSegment.transform.position))
            _playerSegment.moveDirection = _playerSegment.upcommingMoveDirection;

        _playerSegment.startPosition = RoundToHalf(_playerSegment.transform.position);
        _playerSegment.targetPosition = RoundToHalf(_playerSegment.startPosition + _playerSegment.moveDirection * _gridHalfSize);

        OnSnakeMovementStarted?.Invoke();

        float elapsedTime = 0f;
        float moveDuration = _gridHalfSize / _moveSpeed;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            var newPosition = Vector3.Lerp(_playerSegment.startPosition, _playerSegment.targetPosition, t);
            _playerSegment.transform.position = newPosition;

            OnSnakeMovementUpdated?.Invoke(_playerSegment.transform.position, t); // Notify listeners

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _playerSegment.transform.position = _playerSegment.targetPosition;
        OnSnakeMovementCompleted?.Invoke();
        _isTurning = false;

        _isMovementCoroutineInProgress = false;
        yield return null;
    }

    private void RotateHead()
    {
        if (_playerSegment.moveDirection == Vector3.zero) return;
        var targetRotation = Quaternion.LookRotation(_playerSegment.moveDirection);
        _playerSegment.transform.rotation = Quaternion.Slerp(_playerSegment.transform.rotation, targetRotation, Time.deltaTime * _moveSpeed * 2.5f);
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
