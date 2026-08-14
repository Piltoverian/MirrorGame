using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(Camera))]
public class TopDownCameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool followTarget = true;
    [SerializeField] private float followSmoothTime = 0.12f;

    [Header("Zoom")]
    [SerializeField] private float minOrthographicSize = 3f;
    [SerializeField] private float maxOrthographicSize = 12f;
    [SerializeField] private float wheelZoomSpeed = 2f;
    [SerializeField] private float pinchZoomSpeed = 0.02f;

    [Header("Pan")]
    [SerializeField] private bool allowMousePan = true;
    [SerializeField] private bool allowTouchPan = true;
    [SerializeField] private int mousePanButton = 2;
    [SerializeField] private float panSpeed = 1f;
    [SerializeField] private float focusSmoothTime = 0.08f;

    private Camera targetCamera;
    private Vector3 followVelocity;
    private Vector3 manualPanOffset;
    private Vector3 lastMousePosition;
    private float lastTouchDistance;
    private readonly Vector2[] activeTouchPositions = new Vector2[2];
    private readonly Vector2[] activeTouchDeltas = new Vector2[2];

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        HandleZoom();
        HandlePan();
        FollowTarget();
    }

    public void FocusOnTarget()
    {
        manualPanOffset = Vector3.zero;

        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref followVelocity, focusSmoothTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void FollowTarget()
    {
        if (!followTarget || target == null)
        {
            return;
        }

        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z) + manualPanOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref followVelocity, followSmoothTime);
    }

    private void HandleZoom()
    {
        if (targetCamera == null || !targetCamera.orthographic)
        {
            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            float zoomDelta = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(zoomDelta) > 0.001f)
            {
                targetCamera.orthographicSize = Mathf.Clamp(
                    targetCamera.orthographicSize - zoomDelta * wheelZoomSpeed,
                    minOrthographicSize,
                    maxOrthographicSize
                );
            }
        }

        int touchCount = GetActiveTouches();
        if (touchCount == 2)
        {
            float currentDistance = Vector2.Distance(activeTouchPositions[0], activeTouchPositions[1]);

            if (lastTouchDistance > 0f)
            {
                float pinchDelta = currentDistance - lastTouchDistance;
                targetCamera.orthographicSize = Mathf.Clamp(
                    targetCamera.orthographicSize - pinchDelta * pinchZoomSpeed,
                    minOrthographicSize,
                    maxOrthographicSize
                );
            }

            lastTouchDistance = currentDistance;
        }
        else
        {
            lastTouchDistance = 0f;
        }
    }

    private void HandlePan()
    {
        Mouse mouse = Mouse.current;
        if (allowMousePan && mouse != null)
        {
            Vector2 mousePosition = mouse.position.ReadValue();

            if (WasMousePanPressedThisFrame(mouse))
            {
                lastMousePosition = mousePosition;
            }

            if (IsMousePanPressed(mouse))
            {
                Vector3 delta = (Vector3)mousePosition - lastMousePosition;
                ApplyScreenPan(delta);
                lastMousePosition = mousePosition;
            }
        }

        int touchCount = GetActiveTouches();
        if (allowTouchPan && touchCount == 1)
        {
            ApplyScreenPan(activeTouchDeltas[0]);
        }
    }

    private void ApplyScreenPan(Vector2 screenDelta)
    {
        if (targetCamera == null)
        {
            return;
        }

        float worldUnitsPerPixel = targetCamera.orthographicSize * 2f / Mathf.Max(1f, Screen.height);
        Vector3 worldDelta = new Vector3(-screenDelta.x, -screenDelta.y, 0f) * worldUnitsPerPixel * panSpeed;
        manualPanOffset += worldDelta;
    }

    private int GetActiveTouches()
    {
        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < touchscreen.touches.Count && count < activeTouchPositions.Length; i++)
        {
            TouchControl touch = touchscreen.touches[i];
            if (!touch.press.isPressed)
            {
                continue;
            }

            activeTouchPositions[count] = touch.position.ReadValue();
            activeTouchDeltas[count] = touch.delta.ReadValue();
            count++;
        }

        return count;
    }

    private bool IsMousePanPressed(Mouse mouse)
    {
        if (mousePanButton == 0)
        {
            return mouse.leftButton.isPressed;
        }

        if (mousePanButton == 1)
        {
            return mouse.rightButton.isPressed;
        }

        return mouse.middleButton.isPressed;
    }

    private bool WasMousePanPressedThisFrame(Mouse mouse)
    {
        if (mousePanButton == 0)
        {
            return mouse.leftButton.wasPressedThisFrame;
        }

        if (mousePanButton == 1)
        {
            return mouse.rightButton.wasPressedThisFrame;
        }

        return mouse.middleButton.wasPressedThisFrame;
    }
}
