
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 9f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Jump and Gravity")]
    [SerializeField] private float jumpHeight = 2.8f;
    [SerializeField] private float gravity = -22f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Jump Assistance")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.25f;
    [SerializeField] private LayerMask groundLayer = ~0;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool jumpHeldLastFrame;

    private void Awake ()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start ()
    {
        if ( cameraTransform == null && Camera.main != null ) cameraTransform = Camera.main.transform;
    }

    private void OnEnable ()
    {
        verticalVelocity = Vector3.zero;
        coyoteCounter = 0f;
        jumpBufferCounter = 0f;
        jumpHeldLastFrame = false;
    }

    private void Update ()
    {
        if ( DimensionTransition.InputLocked ) return;
        if ( Keyboard.current == null ) return;

        bool grounded = CheckGround();

        if ( grounded )
        {
            coyoteCounter = coyoteTime;

            if ( verticalVelocity.y < 0f ) verticalVelocity.y = -2f;
        }
        else coyoteCounter -= Time.deltaTime;

        if ( Keyboard.current.spaceKey.wasPressedThisFrame ) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        bool jumpStarted = false;

        if ( jumpBufferCounter > 0f && coyoteCounter > 0f )
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            jumpStarted = true;
        }

        bool jumpHeld = Keyboard.current.spaceKey.isPressed;

        if ( ! jumpStarted && jumpHeldLastFrame && ! jumpHeld && verticalVelocity.y > 0f ) verticalVelocity.y *= jumpCutMultiplier;

        jumpHeldLastFrame = jumpHeld;

        Vector2 input = Vector2.zero;

        if ( Keyboard.current.zKey.isPressed || Keyboard.current.wKey.isPressed ) input.y += 1f;
        if ( Keyboard.current.sKey.isPressed ) input.y -= 1f;
        if ( Keyboard.current.qKey.isPressed || Keyboard.current.aKey.isPressed ) input.x -= 1f;
        if ( Keyboard.current.dKey.isPressed ) input.x += 1f;

        input = Vector2.ClampMagnitude(input, 1f);

        bool running = Keyboard.current.leftShiftKey.isPressed;

        float speed = running ? runSpeed : walkSpeed;

        Vector3 movement;

        if ( cameraTransform != null )
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            movement = forward * input.y + right * input.x;
        }
        else movement = new Vector3(input.x, 0f, input.y);

        movement = Vector3.ClampMagnitude(movement, 1f);

        if ( movement.sqrMagnitude > 0.001f )
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 finalMovement = movement * speed + verticalVelocity;

        CollisionFlags flags = controller.Move(finalMovement * Time.deltaTime);

        if ( (flags & CollisionFlags.Above) != 0 && verticalVelocity.y > 0f ) verticalVelocity.y = 0f;
        if ( (flags & CollisionFlags.Below) != 0 && verticalVelocity.y < 0f ) verticalVelocity.y = -2f;
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
