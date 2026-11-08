
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Projected2DCollider : MonoBehaviour
{
    [Header("2D Projection")]
    [SerializeField] private float projectionZ = 0f;
    [SerializeField] private float colliderDepth = 3f;

    [Header("Dimension Travel")]
    [SerializeField] private bool canBeDimensionSupport = true;

    private BoxCollider originalCollider;
    private BoxCollider projectedCollider;
    private GameObject projectedObject;

    private bool originalEnabledState;
    private bool isProjected;

    public BoxCollider ProjectedCollider => projectedCollider;
    public bool IsProjected => isProjected;
    public bool CanBeDimensionSupport => canBeDimensionSupport;

    public float OriginalZ => transform.TransformPoint(originalCollider.center).z;

    private void Awake ()
    {
        originalCollider = GetComponent<BoxCollider>();

        projectedObject = new GameObject( name + "_2DCollision");
        projectedObject.layer = gameObject.layer;

        projectedCollider = projectedObject.AddComponent<BoxCollider>();
        projectedCollider.isTrigger = originalCollider.isTrigger;

        Projected2DSource source = projectedObject.AddComponent<Projected2DSource>();

        source.Initialize(this);

        projectedObject.SetActive(false);
    }

    private void Update ()
    {
        bool shouldProject = DimensionManager.Instance != null && DimensionManager.Instance.Is2D;

        if ( shouldProject && ! isProjected ) EnableProjection();
        else if ( ! shouldProject && isProjected ) DisableProjection();

        if ( isProjected ) RefreshProjection();
    }

    private void EnableProjection ()
    {
        if ( isProjected ) return;

        originalEnabledState = originalCollider.enabled;

        RefreshProjection();

        originalCollider.enabled = false;
        projectedObject.SetActive(true);

        isProjected = true;

        Physics.SyncTransforms();
    }

    private void DisableProjection ()
    {
        if ( ! isProjected ) return;

        projectedObject.SetActive(false);

        originalCollider.enabled = originalEnabledState;

        isProjected = false;

        Physics.SyncTransforms();
    }

    public void RefreshProjection ()
    {
        if ( originalCollider == null || projectedCollider == null ) return;

        Vector3 worldCenter = transform.TransformPoint(originalCollider.center);
        Vector3 scaledSize = Vector3.Scale(originalCollider.size, transform.lossyScale);
        Vector3 worldSize = new Vector3(Mathf.Abs(scaledSize.x), Mathf.Abs(scaledSize.y), Mathf.Abs(scaledSize.z));

        projectedObject.transform.position = new Vector3( worldCenter.x, worldCenter.y, projectionZ);
        projectedObject.transform.rotation = Quaternion.identity;
        projectedObject.transform.localScale = Vector3.one;

        projectedCollider.center = Vector3.zero;
        projectedCollider.isTrigger = originalCollider.isTrigger;
        projectedCollider.size = new Vector3(worldSize.x, worldSize.y, colliderDepth);
    }

    private void OnDisable ()
    {
        DisableProjection();
    }

    private void OnDestroy ()
    {
        if ( projectedObject != null ) Destroy(projectedObject);
    }
}
