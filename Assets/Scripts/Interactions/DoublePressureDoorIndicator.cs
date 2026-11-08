
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DoublePressureDoorIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider crateA;
    [SerializeField] private Collider crateB;
    [SerializeField] private Collider plateA;
    [SerializeField] private Collider plateB;

    [Header("Colors")]
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color partialColor = Color.cyan;
    [SerializeField] private Color unlockedColor = Color.green;

    [Header("Animation")]
    [SerializeField] private float transitionSpeed = 6f;
    [SerializeField] private float emissionIntensity = 2f;

    private Material indicatorMaterial;
    private Color currentColor;
    private Color currentEmission;

    private int activePlateCount;

    private void Awake ()
    {
        indicatorMaterial = GetComponent<Renderer>().material;
        indicatorMaterial.EnableKeyword("_EMISSION");

        currentColor = lockedColor;
        currentEmission = lockedColor * emissionIntensity;

        ApplyVisuals();
    }

    private void Update ()
    {
        if ( crateA == null || crateB == null || plateA == null || plateB == null) return;
        if ( DimensionManager.Instance != null && DimensionManager.Instance.Is2D ) return;

        Physics.SyncTransforms();

        bool pressedA = crateA.enabled && plateA.bounds.Intersects(crateA.bounds);
        bool pressedB = crateB.enabled && plateB.bounds.Intersects(crateB.bounds);

        activePlateCount = (pressedA ? 1 : 0) + (pressedB ? 1 : 0);

        Color targetColor = lockedColor;

        if ( activePlateCount == 1 ) targetColor = partialColor;
        else if ( activePlateCount == 2 ) targetColor = unlockedColor;

        float blend = 1f - Mathf.Exp(-transitionSpeed * Time.unscaledDeltaTime);

        currentColor = Color.Lerp(currentColor, targetColor, blend);
        currentEmission = Color.Lerp(currentEmission, targetColor * emissionIntensity, blend);

        ApplyVisuals();
    }

    private void ApplyVisuals ()
    {
        if ( indicatorMaterial.HasProperty("_BaseColor") ) indicatorMaterial.SetColor("_BaseColor", currentColor);
        if ( indicatorMaterial.HasProperty("_EmissionColor") ) indicatorMaterial.SetColor("_EmissionColor", currentEmission);
    }

    private void OnDestroy ()
    {
        if ( indicatorMaterial != null ) Destroy(indicatorMaterial);
    }
}
