
using System.Collections;
using UnityEngine;

public class DimensionTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup fadeOverlay;

    [Header("Transition Settings")]
    [SerializeField] private float fadeOutDuration = 0.25f;
    [SerializeField] private float fadeInDuration = 0.25f;

    private bool isTransitioning;

    public bool IsTransitioning => isTransitioning;

    public static bool InputLocked { get; private set; }

    private void Awake ()
    {
        if ( fadeOverlay != null )
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.interactable = false;
            fadeOverlay.blocksRaycasts = false;
        }
    }

    public void PlayTransition ( System.Action onMidpoint )
    {
        if ( isTransitioning ) return;
        if ( fadeOverlay == null )
        {
            Debug.LogWarning(
                "DimensionTransition: FadeOverlay missing."
            );

            onMidpoint?.Invoke();
            return;
        }

        StartCoroutine(TransitionRoutine(onMidpoint));
    }

    private IEnumerator TransitionRoutine ( System.Action onMidpoint )
    {
        isTransitioning = true;
        InputLocked = true;

        fadeOverlay.blocksRaycasts = true;

        yield return Fade(0f, 1f, fadeOutDuration);

        onMidpoint?.Invoke();

        yield return Fade(1f, 0f, fadeInDuration);

        fadeOverlay.blocksRaycasts = false;

        InputLocked = false;
        isTransitioning = false;
    }

    private IEnumerator Fade ( float from, float to, float duration )
    {
        if ( duration <= 0f )
        {
            fadeOverlay.alpha = to;
            yield break;
        }

        float elapsed = 0f;

        while ( elapsed < duration )
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            fadeOverlay.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        fadeOverlay.alpha = to;
    }

    private void OnDisable ()
    {
        StopAllCoroutines();

        InputLocked = false;
        isTransitioning = false;

        if ( fadeOverlay != null )
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
        }
    }
}
