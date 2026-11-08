
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Vector3 movementDirection = Vector3.right;
    [SerializeField] private float movementDistance = 3f;
    [SerializeField] private float movementSpeed = 2f;

    private Rigidbody rb;

    private Vector3 startPosition;
    private Vector3 endPosition;

    private float progress;
    private int direction = 1;

    private bool isFrozen;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void OnEnable ()
    {
        TimeFreezeManager.OnFreezeStateChanged += HandleFreeze;
    }

    private void Start ()
    {
        startPosition = rb.position;

        endPosition = startPosition + movementDirection.normalized * movementDistance;

        if ( TimeFreezeManager.Instance != null ) isFrozen = TimeFreezeManager.Instance.IsFrozen;
    }

    private void FixedUpdate ()
    {
        if ( isFrozen ) return;
        if ( movementDirection.sqrMagnitude < 0.001f || movementDistance <= 0f || movementSpeed <= 0f ) return;

        float pathLength = Vector3.Distance(startPosition, endPosition);

        if ( pathLength < 0.001f ) return;

        progress += direction * (movementSpeed / pathLength) * Time.fixedDeltaTime;

        if ( progress >= 1f )
        {
            progress = 1f;
            direction = -1;
        }
        else if ( progress <= 0f )
        {
            progress = 0f;
            direction = 1;
        }

        Vector3 nextPosition = Vector3.Lerp(startPosition, endPosition, progress);

        rb.MovePosition(nextPosition);
    }

    private void HandleFreeze ( bool frozen )
    {
        isFrozen = frozen;
    }

    private void OnDisable ()
    {
        TimeFreezeManager.OnFreezeStateChanged -= HandleFreeze;
    }
}
