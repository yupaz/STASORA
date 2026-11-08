
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PressurePlate : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PuzzleDoor targetDoor;

    [Header("Detection")]
    [SerializeField] private MovableObject2D requiredObject;

    private BoxCollider plateCollider;
    private bool isPressed;

    private void Awake ()
    {
        plateCollider = GetComponent<BoxCollider>();
    }

    private void Update ()
    {
        if ( targetDoor == null || requiredObject == null ) return;

        bool isIn2D = DimensionManager.Instance != null && DimensionManager.Instance.Is2D;
        bool shouldPress = false;

        if ( ! isIn2D )
        {
            Collider crateCollider = requiredObject.GetComponent<Collider>();

            if ( crateCollider != null )
            {
                Bounds plateBounds = plateCollider.bounds;
                Bounds crateBounds = crateCollider.bounds;

                shouldPress = plateBounds.Intersects(crateBounds);
            }
        }

        if ( shouldPress == isPressed ) return;

        isPressed = shouldPress;
        targetDoor.SetOpen(isPressed);
    }
}
