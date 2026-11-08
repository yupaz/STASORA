
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera")]
    [SerializeField] private float distance = 6f;
    [SerializeField] private float sensitivity = 0.10f;
    [SerializeField] private float pitch = 15f;

    [Header("Vertical Limits")]
    [SerializeField] private float minPitch = -20f;
    [SerializeField] private float maxPitch = 55f;

    [Header("Vertical Follow")]
    [SerializeField] private float verticalSmoothTime = 0.25f;

    private float yaw;
    private float cameraHeight;
    private float verticalVelocity;

    private bool cursorLocked = true;

    private void Start ()
    {
        yaw = transform.eulerAngles.y;

        if ( target != null )
        {
            cameraHeight = target.position.y;
            UpdateCameraPosition();
        }

        SetCursorLocked(true);
    }

    private void LateUpdate ()
    {
        if ( DimensionTransition.InputLocked ) return;

        HandleCursor();

        if ( target == null ) return;
        if ( cursorLocked && Mouse.current != null )
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            yaw += delta.x * sensitivity;

            pitch -= delta.y * sensitivity;
            pitch = Mathf.Clamp( pitch, minPitch, maxPitch);
        }

        cameraHeight = Mathf.SmoothDamp(cameraHeight, target.position.y, ref verticalVelocity, verticalSmoothTime);

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition ()
    {
        Vector3 focusPoint = new Vector3(target.position.x, cameraHeight, target.position.z);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        transform.position = focusPoint - rotation * Vector3.forward * distance;
        transform.rotation = rotation;
    }

    private void HandleCursor ()
    {
        if ( Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame )
            SetCursorLocked(false);

        if ( Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && ! cursorLocked)
            SetCursorLocked(true);
    }

    private void SetCursorLocked ( bool locked )
    {
        cursorLocked = locked;

        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = ! locked;
    }

    private void OnDisable ()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
