
using UnityEngine;

public class FreezeTimingGate : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MovingPlatform movingPlatform;
    [SerializeField] private Transform freezeTarget;
    [SerializeField] private PuzzleDoor targetDoor;

    [Header("Detection")]
    [SerializeField] private float positionTolerance = 0.5f;

    private bool wasActivated;

    private void Update ()
    {
        if ( movingPlatform == null || freezeTarget == null || targetDoor == null) return;

        bool isFrozen = TimeFreezeManager.Instance != null && TimeFreezeManager.Instance.IsFrozen;

        float distance = Mathf.Abs(movingPlatform.transform.position.z - freezeTarget.position.z);

        bool isInTarget = distance <= positionTolerance;
        bool shouldOpen = isFrozen && isInTarget;

        targetDoor.SetOpen(shouldOpen);

        if ( shouldOpen != wasActivated )
        {
            Debug.Log(
                shouldOpen
                    ? "STASORA: Freeze timing successful!"
                    : "STASORA: Freeze timing inactive."
            );

            wasActivated = shouldOpen;
        }
    }
}
