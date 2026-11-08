
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Projected2DCollider))]
public class MovableObject2D : MonoBehaviour
{
    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayers = ~0;
    [SerializeField] private float skinWidth = 0.02f;

    private Projected2DCollider projection;
    private bool isBeingMoved;

    public bool IsBeingMoved => isBeingMoved;

    private void Awake ()
    {
        projection = GetComponent<Projected2DCollider>();
    }

    public void StartMoving ()
    {
        if ( DimensionManager.Instance == null || ! DimensionManager.Instance.Is2D) return;

        isBeingMoved = true;
    }

    public void StopMoving ()
    {
        isBeingMoved = false;
    }

    public float GetAllowedMovement ( float horizontalDelta )
    {
        if ( ! isBeingMoved || projection == null || ! projection.IsProjected ) return 0f;
        if ( Mathf.Abs(horizontalDelta) < 0.0001f ) return 0f;

        BoxCollider box = projection.ProjectedCollider;

        Physics.SyncTransforms();

        Bounds bounds = box.bounds;

        Vector3 direction = horizontalDelta > 0f ? Vector3.right : Vector3.left;

        float requestedDistance = Mathf.Abs(horizontalDelta);

        Vector3 halfExtents = Vector3.Max(bounds.extents - Vector3.one * skinWidth, Vector3.one * 0.01f);

        float allowedDistance = requestedDistance;

        RaycastHit[] hits = Physics.BoxCastAll(bounds.center, halfExtents, direction, Quaternion.identity, requestedDistance + skinWidth, obstacleLayers, QueryTriggerInteraction.Ignore);

        foreach ( RaycastHit hit in hits )
        {
            if ( hit.collider == box ) continue;
            if ( hit.collider.transform == transform ) continue;
            if (hit.collider.GetComponentInParent<CharacterController>() != null ) continue;

            float safeDistance = Mathf.Max(0f, hit.distance - skinWidth);

            allowedDistance = Mathf.Min(allowedDistance, safeDistance);
        }

        return allowedDistance * Mathf.Sign(horizontalDelta);
    }

    public void ApplyMovement ( float horizontalDelta )
    {
        if ( ! isBeingMoved ) return;
        if ( Mathf.Abs(horizontalDelta) < 0.0001f ) return;

        Vector3 position = transform.position;
        position.x += horizontalDelta;

        transform.position = position;

        Physics.SyncTransforms();

        projection.RefreshProjection();

        Physics.SyncTransforms();
    }

    private void Update ()
    {
        if ( DimensionManager.Instance == null || ! DimensionManager.Instance.Is2D ) isBeingMoved = false;
    }
}
