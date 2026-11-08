
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TimingGateIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform movingPlatform;
    [SerializeField] private Transform freezeTarget;

    [Header("Detection")]
    [SerializeField] private float positionTolerance = 0.5f;

    [Header("Colors")]
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color openingColor = Color.cyan;
    [SerializeField] private Color unlockedColor = Color.green;

    [Header("Animation")]
    [SerializeField] private float transitionSpeed = 6f;
    [SerializeField] private float emissionIntensity = 2f;
    [SerializeField] private float openingPulseSpeed = 5f;

    private Material indicatorMaterial;
    private Color currentColor;
    private Color currentEmission;

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
        if ( movingPlatform == null || freezeTarget == null ) return;

        bool isFrozen = TimeFreezeManager.Instance != null && TimeFreezeManager.Instance.IsFrozen;

        float distance = Mathf.Abs(movingPlatform.position.z - freezeTarget.position.z);

        bool isUnlocked = isFrozen && distance <= positionTolerance;

        Color targetColor;
        float targetIntensity;

        if ( isUnlocked )
        {
            targetColor = unlockedColor;
            targetIntensity = emissionIntensity;
        }
        else if ( distance <= positionTolerance && ! isFrozen )
        {
            targetColor = openingColor;

            float pulse = (Mathf.Sin(Time.unscaledTime * openingPulseSpeed) + 1f) * 0.5f;

            targetIntensity = Mathf.Lerp(0.5f, emissionIntensity, pulse);
        }
        else
        {
            targetColor = lockedColor;
            targetIntensity = emissionIntensity;
        }

        float blend = 1f - Mathf.Exp(-transitionSpeed * Time.unscaledDeltaTime);

        currentColor = Color.Lerp(currentColor, targetColor, blend);
        currentEmission = Color.Lerp(currentEmission, targetColor * targetIntensity, blend);

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
