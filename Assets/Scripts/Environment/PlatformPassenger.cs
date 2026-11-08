
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlatformPassenger : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float groundCheckDistance = 0.35f;

    private CharacterController controller;
    private MovingPlatform currentPlatform;
    private PlayerMovement playerMovement;

    private Vector3 lastPlatformPosition;

    private void Awake ()
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void LateUpdate ()
    {
        if ( DimensionManager.Instance != null && DimensionManager.Instance.Is2D )
        {
            currentPlatform = null;
            return;
        }

        DetectPlatform();

        if ( currentPlatform == null ) return;

        Vector3 platformDelta = currentPlatform.transform.position - lastPlatformPosition;

        if ( platformDelta.sqrMagnitude > 0.000001f ) controller.Move(platformDelta);

        lastPlatformPosition = currentPlatform.transform.position;
    }

    private void DetectPlatform ()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.2f;

        bool found = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckDistance + 0.2f, ~0, QueryTriggerInteraction.Ignore);

        MovingPlatform detectedPlatform = null;

        if ( found && controller.isGrounded ) detectedPlatform = hit.collider.GetComponentInParent<MovingPlatform>();
        if ( detectedPlatform != currentPlatform )
        {
            currentPlatform = detectedPlatform;

            if ( currentPlatform != null ) lastPlatformPosition = currentPlatform.transform.position;
        }
    }
}
