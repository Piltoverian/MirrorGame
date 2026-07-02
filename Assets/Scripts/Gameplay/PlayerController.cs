using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float pushMoveSpeed = 2f;

    private PlayerInputActions inputActions;
    private Vector2 currentMoveInput;
    private Rigidbody2D rb;

    private PushableMirror activeMirror;
    private PushableMirror activeRotationMirror;
    private MirrorPushZone activeRotationZone;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ApplyPhysicsSettings();

        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += context =>
        {
            currentMoveInput = context.ReadValue<Vector2>();
        };

        inputActions.Player.Move.canceled += context =>
        {
            currentMoveInput = Vector2.zero;
        };

        inputActions.Player.Rotate.performed += context =>
        {
            float rotateInput = context.ReadValue<float>();

            if (rotateInput != 0f)
            {
                OnRotate(rotateInput);
            }
        };
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void FixedUpdate()
    {
        Vector2 moveDir = currentMoveInput.normalized;

        float currentSpeed = moveSpeed;

        if (activeMirror != null && activeMirror.IsPushable)
        {
            currentSpeed = pushMoveSpeed;
        }

        rb.linearVelocity = moveDir * currentSpeed;
    }

    private void ApplyPhysicsSettings()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void OnRotate(float directionSign)
    {
        PushableMirror mirrorToRotate = activeRotationMirror != null
            ? activeRotationMirror
            : activeMirror;

        if (mirrorToRotate == null) return;

        mirrorToRotate.Rotate(directionSign);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PushableMirror mirror = collision.collider.GetComponentInParent<PushableMirror>();

        if (mirror != null)
        {
            activeMirror = mirror;

            if (activeRotationMirror == null)
            {
                activeRotationMirror = mirror;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        PushableMirror mirror = collision.collider.GetComponentInParent<PushableMirror>();

        if (mirror != null)
        {
            activeMirror = mirror;

            if (activeRotationMirror == null)
            {
                activeRotationMirror = mirror;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        PushableMirror mirror = collision.collider.GetComponentInParent<PushableMirror>();

        if (mirror != null && mirror == activeMirror)
        {
            activeMirror = null;
        }

        if (activeRotationZone == null && mirror != null && mirror == activeRotationMirror)
        {
            activeRotationMirror = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TrySetRotationMirrorFromZone(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TrySetRotationMirrorFromZone(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        MirrorPushZone zone = other.GetComponentInParent<MirrorPushZone>();
        if (zone == null) return;

        zone.Highlight(false);

        if (zone != activeRotationZone) return;

        activeRotationZone = null;
        activeRotationMirror = activeMirror;
    }

    private void TrySetRotationMirrorFromZone(Collider2D other)
    {
        MirrorPushZone zone = other.GetComponentInParent<MirrorPushZone>();
        if (zone == null) return;

        PushableMirror mirror = zone.GetMirror();
        if (mirror == null) return;

        if (activeRotationZone != null && activeRotationZone != zone)
        {
            activeRotationZone.Highlight(false);
        }

        activeRotationZone = zone;
        activeRotationMirror = mirror;
        zone.Highlight(true);
    }
}