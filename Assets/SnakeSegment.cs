using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public class SnakeSegment : MonoBehaviour
{
    public Vector3 moveDirection;
    public Vector3 upcommingMoveDirection;
    public Vector3 startPosition;
    public Vector3 targetPosition;
    public bool isTurning = false;
    public bool halfTurnDone = false;
    public Vector3 turnCenterPosition;
    public Vector3 turnStartPosition;
    public Vector3 turnTargetPosition;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void Update()
    {

    }
}
