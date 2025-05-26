using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private SnakeSegment _playerSegment;
    private float _moveSpeed;
    private float _gridHalfSize;

    private bool _isMovementCoroutineInProgress = false;
    private bool _isTurning = false;
    private bool _isFrozen = false;
    private bool _isHalfStep = false;
    private GridType _gridType;

    public event Action OnSnakeMovementStarted; // Event for head movement
    public event Action<Vector3, float> OnSnakeMovementUpdated; // Event for head movement
    public event Action OnSnakeMovementCompleted; // Event for head movement

    public void Initialize(SnakeSegment playerSegment, float gridHalfSize, float moveSpeed, GridType gridType)
    {
        _playerSegment = playerSegment;
        _gridHalfSize = gridHalfSize;
        _moveSpeed = moveSpeed;
        _gridType = gridType;
    }

    private void OnEnable()
    {
        var snakeInput = GetComponent<SnakeInput>();
        if (snakeInput != null)
        {
            snakeInput.OnSnakeTurn += HandleSnakeTurn;
            snakeInput.OnSnakeSpeedIncrement += HandleMoveSpeedIncrement;
            snakeInput.OnFreezeTime += HandleTimeFreezeSwitch;
        }
    }

    private void OnDisable()
    {
        var snakeInput = GetComponent<SnakeInput>();
        if (snakeInput != null)
        {
            snakeInput.OnSnakeTurn -= HandleSnakeTurn;
            snakeInput.OnSnakeSpeedIncrement -= HandleMoveSpeedIncrement;
            snakeInput.OnFreezeTime -= HandleTimeFreezeSwitch;
        }
    }

    private void Update()
    {
        RotateHead();

        if (!_isMovementCoroutineInProgress && !_isFrozen)
        {
            StartCoroutine(Move());
        }
    }

    private void HandleSnakeTurn(SnakeMovementType movementType)
    {
        if (_isTurning) return;

        var absoluteAngle = (byte) _gridType;
        var angle = movementType switch
        {
            SnakeMovementType.TurnLeft => absoluteAngle * -1,
            SnakeMovementType.TurnRight => absoluteAngle,
            _ => throw new InvalidOperationException($"{movementType} is not a valid turn type")
        }; ;

        _playerSegment.upcommingMoveDirection = Quaternion.Euler(0, angle, 0) * _playerSegment.moveDirection;
        _isTurning = true;
    }

    private void HandleTimeFreezeSwitch()
    {
        _isFrozen = !_isFrozen;
        //Time.timeScale = _isFrozen ? 0 : 1;
    }

    private void HandleMoveSpeedIncrement(short increment)
    {
        var newMoveSpeed = _moveSpeed + increment;

        if (newMoveSpeed >= 2 && newMoveSpeed <= 6)
        {
            _moveSpeed = newMoveSpeed;
            gameObject.GetComponent<SnakeManager>().moveSpeed = newMoveSpeed;
        }
    }

    private IEnumerator Move()
    {
        _isMovementCoroutineInProgress = true;

        if (Vector3.Distance(_playerSegment.transform.position, _playerSegment.targetPosition) < 0.001f  && !_isHalfStep)
        {
            _playerSegment.moveDirection = _playerSegment.upcommingMoveDirection;
        }

        _playerSegment.startPosition = _playerSegment.transform.position;
        _playerSegment.targetPosition = _playerSegment.startPosition + _playerSegment.moveDirection * _gridHalfSize;

        OnSnakeMovementStarted?.Invoke();

        float elapsedTime = 0f;
        float moveDuration = _gridHalfSize / _moveSpeed;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            var newPosition = Vector3.Lerp(_playerSegment.startPosition, _playerSegment.targetPosition, t);
            _playerSegment.transform.position = newPosition;

            OnSnakeMovementUpdated?.Invoke(_playerSegment.transform.position, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _playerSegment.transform.position = _playerSegment.targetPosition;
        OnSnakeMovementCompleted?.Invoke();

        _isHalfStep = !_isHalfStep; // Toggle half step for next movement
        _isTurning = false;
        _isMovementCoroutineInProgress = false;
    }

    private void RotateHead()
    {
        if (_playerSegment.moveDirection == Vector3.zero) return;

        var targetRotation = Quaternion.LookRotation(_playerSegment.moveDirection);
        float rotationSpeed = Mathf.Clamp(_moveSpeed * 2.5f, 3f, 10f);
        _playerSegment.transform.rotation = Quaternion.Slerp(
            _playerSegment.transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }
}