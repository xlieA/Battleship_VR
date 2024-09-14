
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject cellPrefab;
    public int width = 10;
    public int height = 10;
    public float spacing = 1.1f;

    public int xOffset = -5;
    public int yOffset = 0;
    public int zOffset = 14;

    void Start()
    {
        GenerateGrid();
    }

    public Vector3 GetGridStartingPoint()
    {
        return new Vector3(xOffset, yOffset, zOffset);
    }

    public int GetGridWidth()
    {
        return width;
    }

    public int GetGridHeight()
    {
        return height;
    }

    void GenerateGrid()
    {
        for (int x = xOffset; x < width + xOffset; x++)
        {
            for (int y = yOffset; y < height + yOffset; y++)
            {
                GameObject cell = Instantiate(cellPrefab, new Vector3(x, y, zOffset), Quaternion.identity);
                cell.transform.parent = transform;
                cell.name = $"Cell_{x}_{y}";
            }
        }
    }
}