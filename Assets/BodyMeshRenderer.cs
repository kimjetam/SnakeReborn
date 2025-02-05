using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BodyMeshRenderer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private List<MeshSegment> meshSegments;

    void Start()
    {
        var snakeController = GetComponentInParent<SnakeController>();
        var headMeshSegment = snakeController.head.GetComponent<MeshSegment>();
        var bodyMeshSegments = snakeController.snakeSegments.Select(x => x.GetComponent<MeshSegment>());
        meshSegments = new List<MeshSegment> { 
            headMeshSegment
        }.Concat(bodyMeshSegments).ToList();
        Debug.Log(meshSegments.Count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
