using System;
using UnityEngine;

public class SnakeInput : MonoBehaviour
{
    public event Action<SnakeMovementType> OnSnakeTurn;
    public event Action<short> OnSnakeSpeedIncrement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) OnSnakeTurn?.Invoke(SnakeMovementType.TurnLeft);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) OnSnakeTurn?.Invoke(SnakeMovementType.TurnRight);
        else if (Input.GetKeyDown(KeyCode.UpArrow)) OnSnakeSpeedIncrement?.Invoke(1);
        else if (Input.GetKeyDown(KeyCode.DownArrow)) OnSnakeSpeedIncrement?.Invoke(-1);
    }
}
