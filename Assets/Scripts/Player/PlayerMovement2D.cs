
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpHeight = 2.8f;
    [SerializeField] private float gravity = -22f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Jump Assistance")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer = ~0;

    private CharacterController controller;
    private PlayerObjectInteraction2D interaction;

    private float verticalVelocity;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool jumpHeldLastFrame;
    private bool wasIn2D;

    private void Awake ()
    {
        controller = GetComponent<CharacterController>();
        interaction = GetComponent<PlayerObjectInteraction2D>();
    }

    public void Initialize2D ()
    {
        verticalVelocity = -2f;
        coyoteCounter = 0f;
        jumpBufferCounter = 0f;
        jumpHeldLastFrame = false;
    }

    private void Update ()
    {
        if ( DimensionManager.Instance == null || ! DimensionManager.Instance.Is2D )
        {
            wasIn2D = false;
            return;
        }

        if ( ! wasIn2D )
        {
            Initialize2D();
            wasIn2D = true;
        }

        if ( DimensionTransition.InputLocked ) return;
        if ( Keyboard.current == null ) return;

        bool grounded = CheckGround();

        if ( grounded )
        {
            coyoteCounter = coyoteTime;

            if ( verticalVelocity < 0f ) verticalVelocity = -2f;
        }
        else coyoteCounter -= Time.deltaTime;

        if ( Keyboard.current.spaceKey.wasPressedThisFrame ) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        bool jumpStarted = false;

        if ( jumpBufferCounter > 0f && coyoteCounter > 0f )
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            jumpStarted = true;
        }

        bool jumpHeld = Keyboard.current.spaceKey.isPressed;

        if ( ! jumpStarted && jumpHeldLastFrame && ! jumpHeld && verticalVelocity > 0f ) verticalVelocity *= jumpCutMultiplier;

        jumpHeldLastFrame = jumpHeld;

        float horizontal = 0f;

        if ( Keyboard.current.qKey.isPressed || Keyboard.current.aKey.isPressed ) horizontal -= 1f;
        if ( Keyboard.current.dKey.isPressed ) horizontal += 1f;

        verticalVelocity += gravity * Time.deltaTime;

        float horizontalDelta = horizontal * moveSpeed * Time.deltaTime;

        if ( interaction != null ) interaction.MoveHorizontal(horizontalDelta);
        else controller.Move(new Vector3(horizontalDelta, 0f, 0f));

        CollisionFlags flags = controller.Move(new Vector3(0f, verticalVelocity * Time.deltaTime, 0f));

        if ( (flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f ) verticalVelocity = 0f;
        if ( (flags & CollisionFlags.Below) != 0 && verticalVelocity < 0f ) verticalVelocity = -2f;
        if ( Mathf.Abs(horizontal) > 0.01f ) transform.rotation = Quaternion.Euler(0f, horizontal > 0f ? 90f : -90f, 0f);
    }

    private bool CheckGround ()
    {
        if ( controller.isGrounded ) return true;

        Vector3 origin = new Vector3(controller.bounds.center.x, controller.bounds.min.y + groundCheckRadius + 0.05f, controller.bounds.center.z);

        RaycastHit[] hits = Physics.SphereCastAll(origin, groundCheckRadius, Vector3.down, groundCheckDistance, groundLayer, QueryTriggerInteraction.Ignore);

        foreach ( RaycastHit hit in hits )
        {
            if ( hit.collider.transform.IsChildOf(transform) ) continue;
            if ( hit.normal.y < 0.5f ) continue;

            return true;
        }

        return false;
    }
}
