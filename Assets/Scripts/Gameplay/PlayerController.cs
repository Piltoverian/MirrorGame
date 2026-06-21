using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private MirrorPushZone activePushZone = null;
    private PlayerInputActions inputActions;
    private Vector2 currentMoveInput;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += context => currentMoveInput = context.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += context => currentMoveInput = Vector2.zero;

        inputActions.Player.Push.performed += OnPush;

        inputActions.Player.Rotate.performed += context => 
        {
            float rotateInput = context.ReadValue<float>();
            if (rotateInput != 0f) OnRotate(rotateInput);
        };
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void FixedUpdate()
    {
        Vector2 moveDir = currentMoveInput.normalized;
        rb.linearVelocity = moveDir * moveSpeed;
    }

    private void OnPush(InputAction.CallbackContext context)
    {
        if (activePushZone == null) return;
        
        PushableMirror mirror = activePushZone.GetMirror();
        if (mirror != null)
        {
            Vector3 pushDir = activePushZone.GetCalculatedPushDirection();
            mirror.Push(pushDir);
        }
    }

    private void OnRotate(float directionSign)
    {
        if (activePushZone != null && activePushZone.GetMirror() != null)
        {
            activePushZone.GetMirror().Rotate(directionSign);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var zone = other.GetComponent<MirrorPushZone>();
        if (zone != null)
        {
            if (activePushZone != null) activePushZone.Highlight(false); 
            
            activePushZone = zone;
            activePushZone.Highlight(true); 
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var zone = other.GetComponent<MirrorPushZone>();
        if (zone != null && activePushZone == zone)
        {
            activePushZone.Highlight(false); 
            activePushZone = null;
        }
    }
}