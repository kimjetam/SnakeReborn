using System;
using UnityEngine;

public class HexGridLayout : MonoBehaviour
{
    [Header("Grid settings")]
    public Vector2Int gridSize; // Size of the grid in terms of hexagons (width, height)

    [Header("Tile settings")]
    public float outerSize = 1f; // Outer radius of the hexagon
    public float innerSize = 0f; // Inner radius of the hexagon
    public float height = 1f; // Height of the hexagon
    public bool isFlatTop = false; // Whether the hexagon is flat-topped or pointy-topped
    public Material hexMaterial; // Material for the hexagon mesh

    public float spacingFactor = 0.95f; // Factor to control spacing between hexagons

    private void OnEnable()
    {
        LayoutGrid();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            LayoutGrid();
        }
    }

    private void LayoutGrid()
    {
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                GameObject hexTile = new GameObject($"HexTile_{x}_{y}", typeof(HexRenderer));
                hexTile.transform.position = GetPositionForHexCoordinate(new Vector2Int(x, y));

                HexRenderer hexRenderer = hexTile.GetComponent<HexRenderer>();
                hexRenderer.outerSize = outerSize * spacingFactor;
                hexRenderer.innerSize = innerSize;
                hexRenderer.height = height;
                hexRenderer.isFlatTop = isFlatTop;
                hexRenderer.spacingFactor = spacingFactor;
                hexRenderer.SetMaterial(hexMaterial);
                hexRenderer.DrawMesh();
                hexTile.gameObject.transform.SetParent(transform, true);
            }
        }
    }

    private Vector3 GetPositionForHexCoordinate(Vector2Int vector2Int)
    {
        int column = vector2Int.x;
        int row = vector2Int.y;
        float width;
        float height;
        float xPosition = 0;
        float yPosition = 0;
        bool shouldOffset;
        float horizontalDistance;
        float verticalDistance;
        float offset;
        float size = outerSize;

        if (!isFlatTop)
        {
            shouldOffset = row % 2 == 0;
            width = Mathf.Sqrt(3) * size;
            height = 2 * size;

            horizontalDistance = width;
            verticalDistance = height * (3f / 4f);

            offset = shouldOffset ? width / 2f : 0f;

            xPosition = column * horizontalDistance + offset;
            yPosition = row * verticalDistance;
        }
        else
        {
            shouldOffset = column % 2 == 0;
            height = Mathf.Sqrt(3) * size;
            width = 2 * size;

            verticalDistance = height;
            horizontalDistance = width * (3f / 4f);

            offset = shouldOffset ? height / 2f : 0f;

            xPosition = column * horizontalDistance;
            yPosition = row * verticalDistance - offset;
        }

        return new Vector3(xPosition, 0f, -yPosition);
    }
}
