using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PushableMirror : MonoBehaviour
{
    [Header("Mirror Gameplay")]
    [SerializeField] private bool isPushable = true;

    [Header("Mirror Size Settings")]
    [SerializeField] private float gridCellsX = 1f;
    [SerializeField] private float mirrorThickness = 0.2f;

    [Header("Visual State")]
    [SerializeField] private SpriteRenderer[] renderersToTint;
    [SerializeField, Range(0.1f, 1f)] private float notPushableBrightness = 0.45f;

    [Header("Physics Push Settings")]
    [SerializeField] private float mirrorMass = 8f;
    [SerializeField] private float linearDamping = 8f;
    [SerializeField] private float maxPushSpeed = 1.6f;

    [Header("Rotation")]
    [SerializeField] private float mirrorSign = 22.5f;

    [Header("Rotation Collision Check")]
    [Tooltip("Extra thickness, in grid cells, used only when checking if the mirror can rotate. This does not change the visual thickness or push collider.")]
    [Min(0f)]
    [SerializeField] private float rotationCheckThickness = 0.8f;
    [Tooltip("Extra length, in grid cells, added only to the rotation check box. This does not change the visual length or push collider.")]
    [Min(0f)]
    [SerializeField] private float rotationCheckLengthPadding = 0.1f;

    [Header("Optional Grid Snap")]
    [SerializeField] private bool snapToGridOnStart = true;
    [SerializeField] private bool snapToGridWhenStopped = false;
    [SerializeField] private float stoppedVelocityThreshold = 0.03f;
    [SerializeField] private float snapDelay = 0.2f;

    private Rigidbody2D rb;
    private LightRendererPipeLine pipeline;
    private float stoppedTimer;

    private Color[] originalColors;

    public bool IsPushable => isPushable;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        CacheRenderers();
        CacheOriginalColors();

        ApplyPhysicsSettings();
        ApplyVisualState();
    }

    private void Start()
    {
        pipeline = FindAnyObjectByType<LightRendererPipeLine>();

        if (snapToGridOnStart)
        {
            SnapToGrid();
        }

        UpdateVisualScale();
    }

    private void FixedUpdate()
    {
        if (!isPushable)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

        LimitPushSpeed();

        if (snapToGridWhenStopped)
        {
            HandleSnapWhenStopped();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateVisualScale();

        if (renderersToTint == null || renderersToTint.Length == 0)
        {
            renderersToTint = GetComponentsInChildren<SpriteRenderer>();
        }

        if (Application.isPlaying)
        {
            rb = GetComponent<Rigidbody2D>();

            CacheOriginalColors();
            ApplyPhysicsSettings();
            ApplyVisualState();
        }
        else
        {
            ApplyVisualStateInEditor();
            SnapToGridEditorDelay();
        }
    }
#endif

    private void CacheRenderers()
    {
        if (renderersToTint == null || renderersToTint.Length == 0)
        {
            renderersToTint = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    private void CacheOriginalColors()
    {
        if (renderersToTint == null) return;

        originalColors = new Color[renderersToTint.Length];

        for (int i = 0; i < renderersToTint.Length; i++)
        {
            if (renderersToTint[i] != null)
            {
                originalColors[i] = renderersToTint[i].color;
            }
        }
    }

    private void ApplyPhysicsSettings()
    {
        if (rb == null) return;

        rb.gravityScale = 0f;
        rb.angularDamping = 999f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (isPushable)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = mirrorMass;
            rb.linearDamping = linearDamping;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void ApplyVisualState()
    {
        if (renderersToTint == null) return;

        for (int i = 0; i < renderersToTint.Length; i++)
        {
            SpriteRenderer renderer = renderersToTint[i];
            if (renderer == null) continue;

            Color baseColor = Color.white;

            if (originalColors != null && i < originalColors.Length)
            {
                baseColor = originalColors[i];
            }
            else
            {
                baseColor = renderer.color;
            }

            renderer.color = isPushable
                ? baseColor
                : DarkenColor(baseColor, notPushableBrightness);
        }
    }

#if UNITY_EDITOR
    private void ApplyVisualStateInEditor()
    {
        if (renderersToTint == null) return;

        for (int i = 0; i < renderersToTint.Length; i++)
        {
            SpriteRenderer renderer = renderersToTint[i];
            if (renderer == null) continue;

            Color current = renderer.color;

            renderer.color = isPushable
                ? new Color(1f, 1f, 1f, current.a)
                : DarkenColor(new Color(1f, 1f, 1f, current.a), notPushableBrightness);
        }
    }
#endif

    private Color DarkenColor(Color color, float brightness)
    {
        return new Color(
            color.r * brightness,
            color.g * brightness,
            color.b * brightness,
            color.a
        );
    }

    private void LimitPushSpeed()
    {
        if (rb == null) return;

        if (rb.linearVelocity.magnitude > maxPushSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxPushSpeed;
        }
    }

    private void HandleSnapWhenStopped()
    {
        if (rb.linearVelocity.sqrMagnitude <= stoppedVelocityThreshold * stoppedVelocityThreshold)
        {
            stoppedTimer += Time.fixedDeltaTime;

            if (stoppedTimer >= snapDelay)
            {
                stoppedTimer = 0f;

                rb.linearVelocity = Vector2.zero;
                SnapToGrid();
                UpdateLightGraph();
            }
        }
        else
        {
            stoppedTimer = 0f;
        }
    }

    private void UpdateVisualScale()
    {
        float stepSize = 1f;

        if (Application.isPlaying)
        {
            if (GridManager.Instance != null)
            {
                stepSize = GridManager.Instance.gridSize;
            }
        }
        else
        {
            GridManager manager = FindAnyObjectByType<GridManager>();
            if (manager != null)
            {
                stepSize = manager.gridSize;
            }
        }

        transform.localScale = new Vector3(
            gridCellsX * stepSize,
            mirrorThickness * stepSize,
            transform.localScale.z
        );
    }

    public void Rotate(float sign)
    {
        if (GridManager.Instance == null) return;

        if (rb != null && rb.linearVelocity.sqrMagnitude > stoppedVelocityThreshold * stoppedVelocityThreshold)
        {
            return;
        }

        float targetAngle = transform.eulerAngles.z + sign * mirrorSign;

        if (!CanRotateTo(targetAngle))
        {
            return;
        }

        if (rb != null)
        {
            rb.angularVelocity = 0f;
            rb.SetRotation(targetAngle);
        }
        else
        {
            transform.eulerAngles = new Vector3(0f, 0f, targetAngle);
        }

        UpdateLightGraph();
    }

    private bool CanRotateTo(float targetAngle)
    {
        float stepSize = GridManager.Instance != null ? GridManager.Instance.gridSize : 1f;
        Vector2 boxSize = GetRotationCheckBoxSize(stepSize);

        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in allColliders)
        {
            col.enabled = false;
        }

        Collider2D hit = Physics2D.OverlapBox(
            transform.position,
            boxSize,
            targetAngle,
            GridManager.Instance.obstacleLayer
        );

        foreach (Collider2D col in allColliders)
        {
            col.enabled = true;
        }

        return hit == null;
    }

    private Vector2 GetRotationCheckBoxSize(float stepSize)
    {
        float safeStepSize = Mathf.Max(0.0001f, stepSize);
        float safeLengthCells = Mathf.Max(0f, gridCellsX + rotationCheckLengthPadding);
        float safeThicknessCells = Mathf.Max(mirrorThickness, rotationCheckThickness);

        return new Vector2(
            safeLengthCells * safeStepSize * 0.95f,
            safeThicknessCells * safeStepSize * 0.95f
        );
    }

    public void Push(Vector3 direction)
    {
        Debug.LogWarning($"{name}: Push() is deprecated. Mirror is now pushed by Rigidbody2D physics.");
    }

    private void SnapToGrid()
    {
        float size = 1f;

        if (Application.isPlaying)
        {
            if (GridManager.Instance != null)
            {
                size = GridManager.Instance.gridSize;
            }
        }
        else
        {
            GridManager manager = FindAnyObjectByType<GridManager>();
            if (manager != null)
            {
                size = manager.gridSize;
            }
        }

        if (size <= 0f) return;

        float snapX = Mathf.Round(transform.position.x / size) * size;
        float snapY = Mathf.Round(transform.position.y / size) * size;

        Vector2 snapPosition = new Vector2(snapX, snapY);

        if (Application.isPlaying && rb != null)
        {
            rb.position = snapPosition;
        }
        else
        {
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

        }
    }

    private void OnDrawGizmosSelected()
    {
        float stepSize = 1f;

        GridManager manager = FindAnyObjectByType<GridManager>();
        if (manager != null)
        {
            stepSize = manager.gridSize;
        }

        Gizmos.color = isPushable ? Color.green : Color.gray;

        Vector2 boxSize = GetRotationCheckBoxSize(stepSize);

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            transform.position,
            Quaternion.Euler(0, 0, transform.eulerAngles.z),
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, boxSize);

        Gizmos.matrix = oldMatrix;
    }
}