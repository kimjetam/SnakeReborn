using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class HexRenderer : MonoBehaviour
{
    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private List<Face> _faces;

    public float innerSize = 0.5f; // Inner radius of the hexagon
    public float outerSize = 1f; // Outer radius of the hexagon
    public float height = 1f; // Height of the hexagon
    public bool isFlatTop = false; // Whether the hexagon is flat-topped or pointy-topped
    public float spacingFactor = 0.95f; // Factor to control spacing between hexagons

    public Material hexMaterial;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _mesh = new Mesh();
        _mesh.name = "HexMesh";
        _meshFilter.mesh = _mesh;
        _meshRenderer.material = hexMaterial;
    }

    private void OnEnable()
    {
        //DrawMesh();
        
    }

    public void DrawMesh()
    {
        DrawFaces();
        CombineFaces();

    }

    public void OnValidate()
    {
        if (Application.isPlaying)
        {
            DrawMesh();
        }
    }

    public void SetMaterial(Material material)
    {
        hexMaterial = material;
        _meshRenderer.material = hexMaterial;
    }

    public void DrawFaces()
    {
        _faces = new List<Face>();

        //Top faces
        for(int point = 0; point < 6; point++)
        {
            _faces.Add(CreateFace(innerSize, outerSize, height / 2f, height / 2f, point));
        }

        //Bottom faces
        //for (int point = 0; point < 6; point++)
        //{
        //    _faces.Add(CreateFace(innerSize, outerSize, -height / 2f, -height / 2f, point, true));
        //}

        ////Outer faces
        //for (int point = 0; point < 6; point++)
        //{
        //    _faces.Add(CreateFace(outerSize, outerSize, height / 2f, -height / 2f, point, true));
        //}

        ////Inner faces
        //for (int point = 0; point < 6; point++)
        //{
        //    _faces.Add(CreateFace(innerSize, innerSize, height / 2f, -height / 2f, point, false));
        //}
    }

    public void CombineFaces()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < _faces.Count; i++)
        {
            // Add triangles for the current face
            vertices.AddRange(_faces[i].vertices);
            uvs.AddRange(_faces[i].uvs);

            // Offset the triangle indices
            int offset = 4 * i;

            foreach (int triangle in _faces[i].triangles)
            {
                triangles.Add(triangle + offset);
            }
        }

        _mesh.vertices = vertices.ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.uv = uvs.ToArray();
        _mesh.RecalculateNormals();
    }

    private Face CreateFace(float innerRad, float outerRad, float heightA, float heightB, int point, bool reverse = false)
    {
        Vector3 pointA = GetPoint(innerRad, heightB, point); 
        Vector3 pointB = GetPoint(innerRad, heightB, point < 5 ? point + 1 : 0);
        Vector3 pointC = GetPoint(outerRad, heightA, point < 5 ? point + 1 : 0);
        Vector3 pointD = GetPoint(outerRad, heightA, point);

        List<Vector3> vertices = new List<Vector3>
        {
            pointA,
            pointB,
            pointC,
            pointD
        };

        List<int> triangles = new List<int>
        {
            0, 1, 2, 2, 3, 0
        };

        List<Vector2> uvs = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        if(reverse)
        {
            triangles.Reverse();
        }

        return new Face(vertices, triangles, uvs);
    }

    protected Vector3 GetPoint(float size, float height, int index)
    {
        float angle_deg = isFlatTop ?  60 * index : 60 * index -30;
        float angle_rad = Mathf.PI / 180f * angle_deg;

        return new Vector3(size * Mathf.Cos(angle_rad) * spacingFactor, height, size * Mathf.Sin(angle_rad) * spacingFactor);
    }


}

public struct Face
{
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector2> uvs;

    public Face(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.uvs = uvs;
    }
}
