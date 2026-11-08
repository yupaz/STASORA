
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerRespawn : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerMovement2D playerMovement2D;

    private bool isRespawning;

    private void Awake ()
    {
        if ( characterController == null )
            characterController = GetComponent<CharacterController>();
        if ( playerMovement2D == null )
            playerMovement2D = GetComponent<PlayerMovement2D>();
    }

    public void Respawn ()
    {
        if ( isRespawning ) return;
        if ( CheckpointManager.Instance == null ) return;

        isRespawning = true;

        Vector3 respawnPosition = CheckpointManager.Instance.CheckpointPosition;
        Quaternion respawnRotation = CheckpointManager.Instance.CheckpointRotation;

        if ( characterController != null )
            characterController.enabled = false;

        if ( DimensionManager.Instance != null )
            DimensionManager.Instance.ResetForRespawn();

        if ( playerMovement2D != null )
            playerMovement2D.Initialize2D();

        transform.SetPositionAndRotation(respawnPosition, respawnRotation);

        Physics.SyncTransforms();

        if ( characterController != null )
            characterController.enabled = true;

        Debug.Log(
            "STASORA: Player respawned at " + respawnPosition
        );

        isRespawning = false;
    }
}
