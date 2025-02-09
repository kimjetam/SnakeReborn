using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnakeMovementController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool isMovementCoroutineInProgress = false;
    private SnakeSegment playerSegment;
    private List<SnakeSegment> bodySegments;
    private float moveSpeed;
    void Start()
    {
        //var snakeController = GetComponent<SnakeController>();
        //playerSegment = snakeController.headMiddle.GetComponent<SnakeSegment>();
        //bodySegments = snakeController.snakeSegments.Select(x => x.GetComponent<SnakeSegment>()).ToList();
    }

    // Update is called once per frame
    void Update()
    {
        //if (!isMovementCoroutineInProgress) StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        isMovementCoroutineInProgress = true;

        isMovementCoroutineInProgress = false;
        yield return null;
    }

    private void RotateHead()
    {
        if (playerSegment.moveDirection == Vector3.zero) return;
        var targetRotation = Quaternion.LookRotation(playerSegment.moveDirection);
        playerSegment.transform.rotation = Quaternion.Slerp(playerSegment.transform.rotation, targetRotation, Time.deltaTime * moveSpeed * 2.5f);
    }
}
