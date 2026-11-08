
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class FreezeTargetVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform movingPlatform;

    [Header("Detection")]
    [SerializeField] private float positionTolerance = 0.5f;

    [Header("Colors")]
    [SerializeField] private Color idleColor =
        new Color(0.4f, 0.45f, 0.5f);

    [SerializeField] private Color readyColor = Color.cyan;
    [SerializeField] private Color activeColor = Color.green;

    [Header("Transitions")]
    [SerializeField] private float colorTransitionSpeed = 6f;

    [Header("Emission")]
    [SerializeField] private float readyEmissionIntensity = 2f;
    [SerializeField] private float activeEmissionIntensity = 3f;
    [SerializeField] private float pulseSpeed = 3f;

    private Renderer targetRenderer;
    private Material targetMaterial;

    private Color currentColor;
    private Color currentEmission;

    private void Awake ()
    {
        targetRenderer = GetComponent<Renderer>();
        targetMaterial = targetRenderer.material;
        targetMaterial.EnableKeyword("_EMISSION");

        currentColor = idleColor;
        currentEmission = Color.black;

        ApplyVisuals();
    }

    private void Update ()
    {
        if ( movingPlatform == null ) return;

        float distance = Mathf.Abs(movingPlatform.position.z - transform.position.z);

        bool isAligned = distance <= positionTolerance;
        bool isFrozen = TimeFreezeManager.Instance != null && TimeFreezeManager.Instance.IsFrozen;

        Color targetColor = idleColor;
        Color targetEmission = Color.black;

        if ( isAligned && isFrozen )
        {
            targetColor = activeColor;
            targetEmission = activeColor * activeEmissionIntensity;
        }
        else if ( isAligned )
        {
            targetColor = readyColor;

            float pulse = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
            float intensity = Mathf.Lerp(0.5f, readyEmissionIntensity, pulse);

            targetEmission = readyColor * intensity;
        }

        float blend = 1f - Mathf.Exp(-colorTransitionSpeed * Time.unscaledDeltaTime);

        currentColor = Color.Lerp(currentColor, targetColor, blend);
        currentEmission = Color.Lerp(currentEmission, targetEmission, blend);

        ApplyVisuals();
    }

    private void ApplyVisuals ()
    {
        if ( targetMaterial.HasProperty("_BaseColor") ) targetMaterial.SetColor("_BaseColor", currentColor);
        if (targetMaterial.HasProperty("_EmissionColor")) targetMaterial.SetColor("_EmissionColor", currentEmission);
    }

    private void OnDestroy ()
    {
        if ( targetMaterial != null ) Destroy(targetMaterial);
    }
}
