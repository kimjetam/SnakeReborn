using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement
    public float gridSize = 1f; // Distance between grid points
    public Queue<GameObject> path;
    public List<GameObject> snakeSegments;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(path == null) path = new Queue<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
