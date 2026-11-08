
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Checkpoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Renderer indicatorRenderer;

    [Header("Colors")]
    [SerializeField] private Color inactiveColor = Color.cyan;
    [SerializeField] private Color activeColor = Color.green;

    [Header("Visual Settings")]
    [SerializeField] private float emissionIntensity = 2f;
    [SerializeField] private float transitionSpeed = 6f;

    private Material indicatorMaterial;
    private Color currentColor;
    private bool isActive;

    private void Awake ()
    {
        GetComponent<BoxCollider>().isTrigger = true;

        if ( indicatorRenderer != null )
        {
            indicatorMaterial = indicatorRenderer.material;
            indicatorMaterial.EnableKeyword("_EMISSION");

            currentColor = inactiveColor;
            ApplyVisuals();
        }
    }

    private void Update ()
    {
        if ( indicatorMaterial == null ) return;

        Color targetColor = isActive ? activeColor : inactiveColor;

        float blend = 1f - Mathf.Exp(-transitionSpeed * Time.unscaledDeltaTime);

        currentColor = Color.Lerp(currentColor, targetColor, blend);

        ApplyVisuals();
    }

    private void OnTriggerEnter ( Collider other )
    {
        if ( CheckpointManager.Instance == null ) return;
        if ( respawnPoint == null ) return;
        if ( other.GetComponent<PlayerMovement>() == null ) return;
        if ( DimensionManager.Instance != null && DimensionManager.Instance.Is2D ) return;

        CheckpointManager.Instance.SetCheckpoint(respawnPoint);

        Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);

        foreach ( Checkpoint checkpoint in checkpoints ) checkpoint.SetActive(checkpoint == this);

        Debug.Log("STASORA: Checkpoint activated!");
    }

    public void SetActive ( bool active )
    {
        isActive = active;
    }

    private void ApplyVisuals ()
    {
        if ( indicatorMaterial.HasProperty("_BaseColor") )
            indicatorMaterial.SetColor("_BaseColor", currentColor);
        if ( indicatorMaterial.HasProperty("_EmissionColor") )
            indicatorMaterial.SetColor("_EmissionColor", currentColor * emissionIntensity);
    }

    private void OnDestroy ()
    {
        if ( indicatorMaterial != null )
            Destroy(indicatorMaterial);
    }
}
