
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class OutOfBoundsZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerRespawn playerRespawn;

    private void Reset ()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter ( Collider other )
    {
        if ( playerRespawn == null ) return;

        PlayerRespawn player = other.GetComponent<PlayerRespawn>();

        if ( player == null || player != playerRespawn ) return;

        player.Respawn();
    }
}
