using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public class SnakeSegment : MonoBehaviour
{
    [HideInInspector]
    public Vector3 moveDirection;
    [HideInInspector]
    public Vector3 upcommingMoveDirection;
    [HideInInspector]
    public Vector3 startPosition;
    [HideInInspector]
    public Vector3 targetPosition;
    [HideInInspector]
    public bool isTurning = false;
    [HideInInspector]
    public bool halfTurnDone = false;
    [HideInInspector]
    public Vector3 turnCenterPosition;
    [HideInInspector]
    public Vector3 turnStartPosition;
    [HideInInspector]
    public Vector3 turnTargetPosition;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void Update()
    {

    }
}
