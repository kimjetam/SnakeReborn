using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnakeManager : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private List<Vector3> lineRendererPoints = new List<Vector3>();
    private SnakeSegment headMovingPart;

    public float moveSpeed = 5f;
    public float gridHalfSize = 0.5f;
    public float snakeWidthRadius = 0.15f;
    public int initialSnakeLength = 5;
    [HideInInspector]
    public GameObject headNeck;
    [HideInInspector]
    public GameObject headTip;
    [HideInInspector]
    public GameObject headMiddle;
    [HideInInspector]
    public List<GameObject> snakeSegments;
    [HideInInspector]
    public GameObject tail;
    [HideInInspector]
    public GameObject eye1;
    [HideInInspector]
    public GameObject eye2;
    private Material eyeMaterial;
    
    public bool showDebugPath = false;
    public bool showDebugSegments = false;
    public bool showDebugMeshVerticles = false;

    private void Awake()
    {
        InitSnakeHeadAndTail();
        InitSnakeBody();

        var playerMovement = GetComponent<PlayerMovement>();
        playerMovement.Initialize(headNeck.GetComponent<SnakeSegment>(), gridHalfSize, moveSpeed);

        var bodyMovement = GetComponent<BodyMovement>();
        bodyMovement.Initialize(snakeSegments.Select(x => x.GetComponent<SnakeSegment>()).ToList(), gridHalfSize, headNeck.GetComponent<SnakeSegment>());

        if (showDebugSegments)
        {
            SetupDebugVisuals();
        }

        if (showDebugPath)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = lineRenderer.endWidth = 0.1f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.red;
        }
    }

    void Start()
    {
        
    }

    void InitSnakeHeadAndTail()
    {
        headNeck = new GameObject("HeadNeck");
        headNeck.transform.position = Vector3.zero;
        headNeck.AddComponent<SnakeSegment>();
        headNeck.AddComponent<MeshSegment>();
        headNeck.transform.SetParent(gameObject.transform, false);
        headMovingPart = headNeck.GetComponent<SnakeSegment>();


        headTip = new GameObject("headTip");
        headTip.transform.position = Vector3.zero;
        headTip.AddComponent<SnakeSegment>();
        headTip.AddComponent<MeshSegment>();
        headTip.transform.SetParent(gameObject.transform, false);
        var headTipMeshSegment = headTip.GetComponent<MeshSegment>();
        headTipMeshSegment.radiusX = 0.01f;
        headTipMeshSegment.radiusY = 0.01f;
        headTipMeshSegment.overrideRadiusValues = true;


        headMiddle = new GameObject("headMiddle");
        headMiddle.transform.position = Vector3.zero;
        headMiddle.AddComponent<SnakeSegment>();
        headMiddle.AddComponent<MeshSegment>();
        headMiddle.transform.SetParent(gameObject.transform, false);
        var headMiddleMeshSegment = headMiddle.GetComponent<MeshSegment>();
        headMiddleMeshSegment.radiusX = 0.35f;
        headMiddleMeshSegment.radiusY = 0.25f;
        headMiddleMeshSegment.overrideRadiusValues = true;

        tail = new GameObject("Tail");
        tail.transform.position = Vector3.zero;
        tail.AddComponent<SnakeSegment>();
        tail.AddComponent<MeshSegment>();
        tail.transform.SetParent(gameObject.transform, false);
        var tailMeshSegment = tail.GetComponent<MeshSegment>();
        tailMeshSegment.radiusX = 0.01f;
        tailMeshSegment.radiusY = 0.01f;
        tailMeshSegment.overrideRadiusValues = true;

        eyeMaterial = Resources.Load<Material>("Materials/yellow");
        eye1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye1.gameObject.name = "eye1";
        eye1.transform.localScale = Vector3.one * 0.1f;
        eye1.transform.SetParent(gameObject.transform, false);

        eye2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye2.gameObject.name = "eye2";
        eye2.transform.localScale = Vector3.one * 0.1f;
        eye2.transform.SetParent(gameObject.transform, false);

        Renderer rend = eye1.GetComponent<Renderer>();
        if (rend != null && eyeMaterial != null)
        {
            rend.material = eyeMaterial;
        }

        rend = eye2.GetComponent<Renderer>();
        if (rend != null && eyeMaterial != null)
        {
            rend.material = eyeMaterial;
        }
    }

    void InitSnakeBody()
    {
        snakeSegments = new List<GameObject>();
        for (int i = 0; i < initialSnakeLength; i++)
        {
            var segment = new GameObject($"Segment_{i}");
            segment.transform.position = Vector3.forward * (gridHalfSize + (gridHalfSize  * i)) * -1;
            segment.AddComponent<SnakeSegment>();
            segment.AddComponent<MeshSegment>();
            segment.transform.SetParent(transform, false);
            snakeSegments.Add(segment);
        }
    }

    void Update()
    {
        headTip.transform.position = headMovingPart.transform.position + headTip.transform.forward * 0.9f;
        headMiddle.transform.position = headMovingPart.transform.position + headMiddle.transform.forward * 0.35f;
        eye1.transform.position = headMiddle.transform.position + headTip.transform.forward * 0.1f - headTip.transform.right * 0.25f + headTip.transform.up * 0.1f;
        eye2.transform.position = headMiddle.transform.position + headTip.transform.forward * 0.1f + headTip.transform.right * 0.25f + headTip.transform.up * 0.1f;
        var lastSegment = snakeSegments.Last();
        tail.transform.position = lastSegment.transform.position - lastSegment.transform.forward;
        tail.transform.rotation = lastSegment.transform.rotation;

        headTip.transform.rotation = headMiddle.transform.rotation = headNeck.transform.rotation;

        if (showDebugPath)
        {
            lineRendererPoints.Add(lastSegment.transform.position);
            lineRenderer.positionCount = lineRendererPoints.Count;
            lineRenderer.SetPosition(lineRendererPoints.Count - 1, lastSegment.transform.position);
        }

    }

    private void SetupDebugVisuals()
    {
        CreateSegmentVisual(headNeck);
        foreach (var segment in snakeSegments) CreateSegmentVisual(segment);
    }

    private void CreateSegmentVisual(GameObject segment)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = Vector3.one * 0.5f;
        sphere.transform.SetParent(segment.transform, false);
    }
}