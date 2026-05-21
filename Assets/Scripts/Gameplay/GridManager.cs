using UnityEngine;

public class GridManager : SingletonMB<GridManager>
{
    [Header("Grid Settings")]
    public float gridSize = 1f;
    public float moveDuration = 0.2f;
    public LayerMask obstacleLayer;

    [Header("Gizmos Settings")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.2f);


    private void OnDrawGizmos()
    {
        if (!showGrid) return;

        Gizmos.color = gridColor;

        Vector3 origin = transform.position;
        float halfWidth = (gridWidth * gridSize) / 2f;
        float halfHeight = (gridHeight * gridSize) / 2f;

        for (int x = 0; x <= gridWidth; x++)
        {
            float posX = origin.x - halfWidth + (x * gridSize);
            Gizmos.DrawLine(new Vector3(posX, origin.y - halfHeight, 0),
                            new Vector3(posX, origin.y + halfHeight, 0));
        }

        for (int y = 0; y <= gridHeight; y++)
        {
            float posY = origin.y - halfHeight + (y * gridSize);
            Gizmos.DrawLine(new Vector3(origin.x - halfWidth, posY, 0),
                            new Vector3(origin.x + halfWidth, posY, 0));
        }
    }
}