
using UnityEngine;

public class DoublePressureDoor : MonoBehaviour
{
    [Header("Required Objects")]
    [SerializeField] private Collider crateA;
    [SerializeField] private Collider crateB;

    [Header("Pressure Plates")]
    [SerializeField] private Collider plateA;
    [SerializeField] private Collider plateB;

    [Header("Door")]
    [SerializeField] private PuzzleDoor targetDoor;

    private void Update ()
    {
        if ( DimensionManager.Instance != null && DimensionManager.Instance.Is2D ) return;
        if ( crateA == null || crateB == null || plateA == null || plateB == null || targetDoor == null) return;

        Physics.SyncTransforms();

        bool firstPressed = crateA.enabled && plateA.bounds.Intersects(crateA.bounds);
        bool secondPressed = crateB.enabled && plateB.bounds.Intersects(crateB.bounds);

        targetDoor.SetOpen(firstPressed && secondPressed);
    }
}
