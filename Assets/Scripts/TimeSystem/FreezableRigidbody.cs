
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FreezableRigidbody : MonoBehaviour
{
    private Rigidbody rb;

    private Vector3 savedVelocity;
    private Vector3 savedAngularVelocity;

    private bool wasKinematic;
    private bool wasUsingGravity;
    private bool isFrozen;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable ()
    {
        TimeFreezeManager.OnFreezeStateChanged += HandleFreeze;
    }

    private void Start ()
    {
        if ( TimeFreezeManager.Instance != null && TimeFreezeManager.Instance.IsFrozen) Freeze();
    }

    private void HandleFreeze ( bool frozen )
    {
        if ( frozen ) Freeze();
        else Unfreeze();
    }

    private void Freeze ()
    {
        if ( isFrozen ) return;

        savedVelocity = rb.linearVelocity;
        savedAngularVelocity = rb.angularVelocity;

        wasKinematic = rb.isKinematic;
        wasUsingGravity = rb.useGravity;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        isFrozen = true;
    }

    private void Unfreeze ()
    {
        if ( ! isFrozen ) return;

        rb.isKinematic = wasKinematic;
        rb.useGravity = wasUsingGravity;

        if ( ! rb.isKinematic )
        {
            rb.linearVelocity = savedVelocity;
            rb.angularVelocity = savedAngularVelocity;
        }

        isFrozen = false;
    }

    private void OnDisable ()
    {
        TimeFreezeManager.OnFreezeStateChanged -= HandleFreeze;

        if ( isFrozen ) Unfreeze();
    }
}
