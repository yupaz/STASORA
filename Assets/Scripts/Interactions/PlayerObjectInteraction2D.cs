
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerObjectInteraction2D : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private float interactionRange = 0.35f;

    private CharacterController controller;

    private void Awake ()
    {
        controller = GetComponent<CharacterController>();
    }

    public void MoveHorizontal ( float requestedDelta )
    {
        if ( DimensionTransition.InputLocked ) return;
        if ( Mathf.Abs(requestedDelta) < 0.0001f ) return;

        bool pushHeld = Keyboard.current != null && Keyboard.current.gKey.isPressed;

        if ( ! pushHeld )
        {
            MovePlayer(requestedDelta);
            return;
        }

        MovableObject2D crate = FindCrateInFront(requestedDelta);

        if ( crate == null )
        {
            MovePlayer(requestedDelta);
            return;
        }

        crate.StartMoving();

        float allowedDelta = crate.GetAllowedMovement(requestedDelta);

        if ( Mathf.Abs(allowedDelta) < 0.0001f )
        {
            crate.StopMoving();
            return;
        }

        crate.ApplyMovement(allowedDelta);

        MovePlayer(allowedDelta);

        crate.StopMoving();
    }

    private void MovePlayer ( float delta )
    {
        controller.Move(new Vector3(delta, 0f, 0f));
    }

    private MovableObject2D FindCrateInFront ( float delta )
    {
        MovableObject2D[] crates = FindObjectsByType<MovableObject2D>(FindObjectsSortMode.None);

        float direction = Mathf.Sign(delta);

        foreach ( MovableObject2D crate in crates )
        {
            Projected2DCollider projection = crate.GetComponent<Projected2DCollider>();

            if ( projection == null || ! projection.IsProjected || projection.ProjectedCollider == null) continue;

            Bounds bounds = projection.ProjectedCollider.bounds;
            Bounds playerBounds = controller.bounds;

            bool sameHeight = playerBounds.max.y > bounds.min.y && playerBounds.min.y < bounds.max.y;

            if ( ! sameHeight ) continue;

            float distance;

            if ( direction > 0f ) distance = bounds.min.x - playerBounds.max.x;
            else distance = playerBounds.min.x - bounds.max.x;
            if ( distance >= -0.05f && distance <= interactionRange ) return crate;
        }

        return null;
    }
}
