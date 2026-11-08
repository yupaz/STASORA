
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimeFreezeManager : MonoBehaviour
{
    public static TimeFreezeManager Instance { get; private set; }

    public static event Action<bool> OnFreezeStateChanged;

    [Header("Freeze Settings")]
    [SerializeField] private Key freezeKey = Key.F;

    public bool IsFrozen { get; private set; }

    private void Awake ()
    {
        if ( Instance != null && Instance != this )
        {
            Debug.LogError(
                "Only one TimeFreezeManager is allowed."
            );

            enabled = false;
            return;
        }

        Instance = this;
    }

    private void Update ()
    {
        if ( DimensionTransition.InputLocked ) return;
        if ( Keyboard.current == null ) return;
        if ( DimensionManager.Instance != null && DimensionManager.Instance.Is2D) return;
        if ( Keyboard.current[freezeKey].wasPressedThisFrame ) ToggleFreeze();
    }

    public void ToggleFreeze ()
    {
        SetFrozen(!IsFrozen);
    }

    public void SetFrozen ( bool frozen )
    {
        if ( IsFrozen == frozen ) return;

        IsFrozen = frozen;

        OnFreezeStateChanged?.Invoke(IsFrozen);

        Debug.Log(
            IsFrozen
                ? "STASORA: Time frozen"
                : "STASORA: Time resumed"
        );
    }

    private void OnDestroy ()
    {
        if ( Instance != this ) return;
        if ( IsFrozen )
        {
            IsFrozen = false;
            OnFreezeStateChanged?.Invoke(false);
        }

        Instance = null;
    }
}
