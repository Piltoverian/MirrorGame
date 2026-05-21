using System.Collections;
using UnityEngine;

public class PushableMirror : MonoBehaviour
{
    [Header("Mirror Size Settings")]
    [SerializeField] private float gridCellsX = 1f;     // Scale X Direction
    [SerializeField] private float mirrorThickness = 0.2f;  // Scale Y direction

    private float mirrorSign = 22.5f;
    private bool isMoving = false;
    private LightRendererPipeLine pipeline;

    private void Start()
    {
        pipeline = FindAnyObjectByType<LightRendererPipeLine>();
        SnapToGrid();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateVisualScale();

        if (!Application.isPlaying) 
        {
            SnapToGridEditorDelay(); 
        }
    }
#endif

    private void UpdateVisualScale()
    {
        float stepSize = 1f;

        if (Application.isPlaying)
        {
            if (GridManager.Instance != null) stepSize = GridManager.Instance.gridSize;
        }
        else
        {
            GridManager manager = FindAnyObjectByType<GridManager>();
            if (manager != null) stepSize = manager.gridSize;
        }

        transform.localScale = new Vector3(gridCellsX * stepSize, mirrorThickness * stepSize, transform.localScale.z);
    }

    public void Push(Vector3 direction)
    {
        if (isMoving || GridManager.Instance == null) return;

        Vector3 snapDir = GetSnapDirection(direction);
        if (snapDir == Vector3.zero) return;

        float stepSize = GridManager.Instance.gridSize;
        Vector3 targetPos = transform.position + snapDir * stepSize;

        Vector2 boxSize = new Vector2(gridCellsX * stepSize * 0.95f, mirrorThickness * stepSize * 0.95f);

        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in allColliders)
        {
            col.enabled = false;
        }

        Collider2D hit = Physics2D.OverlapBox(targetPos, boxSize, transform.eulerAngles.z, GridManager.Instance.obstacleLayer);

        foreach (var col in allColliders)
        {
            col.enabled = true;
        }

        if (hit == null)
        {
            StartCoroutine(MoveToGrid(targetPos));
        }
        else
        {
            Debug.Log($"Blocked by: {hit.name} on layer {LayerMask.LayerToName(hit.gameObject.layer)}");
        }
    }

    public void Rotate(float sign)
    {
        if (isMoving) return;

        float targetAngle = transform.eulerAngles.z + (sign * mirrorSign);
        float stepSize = GridManager.Instance != null ? GridManager.Instance.gridSize : 1f;
        Vector2 boxSize = new Vector2(gridCellsX * stepSize * 0.95f, mirrorThickness * stepSize * 0.95f);

        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in allColliders)
        {
            col.enabled = false;
        }

        Collider2D hit = Physics2D.OverlapBox(transform.position, boxSize, targetAngle, GridManager.Instance.obstacleLayer);

        foreach (var col in allColliders)
        {
            col.enabled = true;
        }

        if (hit == null)
        {
            transform.eulerAngles = new Vector3(0, 0, targetAngle);
            UpdateLightGraph();
        }
    }

    private Vector3 GetSnapDirection(Vector3 input)
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            return new Vector3(Mathf.Sign(input.x), 0, 0);
        if (Mathf.Abs(input.y) > 0.1f) 
            return new Vector3(0, Mathf.Sign(input.y), 0);
            
        return Vector3.zero;
    }

    private IEnumerator MoveToGrid(Vector3 targetPos)
    {
        isMoving = true;
        float elapsed = 0f;
        Vector3 start = transform.position;
        float moveDuration = GridManager.Instance.moveDuration;

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, targetPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;

        UpdateLightGraph();
    }

    private void SnapToGrid()
    {
        float size = 1f;

        if (Application.isPlaying)
        {
            if (GridManager.Instance != null) size = GridManager.Instance.gridSize;
        }
        else
        {
            GridManager manager = FindAnyObjectByType<GridManager>();
            if (manager != null) size = manager.gridSize;
        }

        if (size > 0f)
        {
            float snapX = Mathf.Round(transform.position.x / size) * size;
            float snapY = Mathf.Round(transform.position.y / size) * size;
            transform.position = new Vector3(snapX, snapY, transform.position.z);
        }
    }
    
#if UNITY_EDITOR
    private void SnapToGridEditorDelay()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            SnapToGrid();
        };
    }
#endif

    private void UpdateLightGraph()
    {
        if (pipeline != null)
        {
            // pipeline.RecalculateGraph();
        }
    }

    private void OnDrawGizmosSelected()
    {
        float stepSize = 1f;
        GridManager manager = FindAnyObjectByType<GridManager>();
        if (manager != null) stepSize = manager.gridSize;

        Gizmos.color = Color.red;
        Vector2 boxSize = new Vector2(gridCellsX * stepSize * 0.95f, mirrorThickness * stepSize * 0.95f);

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.Euler(0, 0, transform.eulerAngles.z), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
        Gizmos.matrix = oldMatrix;
    }
}