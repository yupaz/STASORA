
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PressurePlateVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider crate;
    [SerializeField] private Collider pressurePlate;

    [Header("Colors")]
    [SerializeField] private Color inactiveColor = Color.red;
    [SerializeField] private Color activeColor = Color.green;

    [Header("Animation")]
    [SerializeField] private float transitionSpeed = 6f;
    [SerializeField] private float emissionIntensity = 2f;

    private Material plateMaterial;
    private Color currentColor;
    private Color currentEmission;

    private void Awake ()
    {
        plateMaterial = GetComponent<Renderer>().material;
        plateMaterial.EnableKeyword("_EMISSION");

        currentColor = inactiveColor;
        currentEmission = inactiveColor * emissionIntensity;

        ApplyVisuals();
    }

    private void Update ()
    {
        if ( crate == null || pressurePlate == null ) return;
        if ( DimensionManager.Instance != null && DimensionManager.Instance.Is2D ) return;

        Physics.SyncTransforms();

        bool isPressed = crate.enabled && pressurePlate.bounds.Intersects(crate.bounds);

        Color targetColor = isPressed ? activeColor : inactiveColor;
        Color targetEmission = targetColor * emissionIntensity;

        float blend = 1f - Mathf.Exp(-transitionSpeed * Time.unscaledDeltaTime);

        currentColor = Color.Lerp(currentColor, targetColor, blend);
        currentEmission = Color.Lerp(currentEmission, targetEmission, blend);

        ApplyVisuals();
    }

    private void ApplyVisuals ()
    {
        if ( plateMaterial.HasProperty("_BaseColor") ) plateMaterial.SetColor("_BaseColor", currentColor);
        if ( plateMaterial.HasProperty("_EmissionColor") ) plateMaterial.SetColor("_EmissionColor", currentEmission);
    }

    private void OnDestroy ()
    {
        if ( plateMaterial != null ) Destroy(plateMaterial);
    }
}
