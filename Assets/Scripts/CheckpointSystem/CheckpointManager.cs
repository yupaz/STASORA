
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform startingPoint;

    private Vector3 checkpointPosition;
    private Quaternion checkpointRotation;

    public Vector3 CheckpointPosition => checkpointPosition;
    public Quaternion CheckpointRotation => checkpointRotation;

    private void Awake ()
    {
        if ( Instance != null && Instance != this )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if ( startingPoint != null )
        {
            checkpointPosition = startingPoint.position;
            checkpointRotation = startingPoint.rotation;
        }
        else if ( player != null )
        {
            checkpointPosition = player.position;
            checkpointRotation = player.rotation;
        }
        else
        {
            Debug.LogError(
                "CheckpointManager: Player and StartingPoint are missing!"
            );

            return;
        }

        Debug.Log(
            "STASORA: Initial checkpoint = " + checkpointPosition
        );
    }

    public void SetCheckpoint (Transform checkpoint)
    {
        if ( checkpoint == null ) return;

        checkpointPosition = checkpoint.position;
        checkpointRotation = checkpoint.rotation;

        Debug.Log(
            "STASORA: Checkpoint updated at " + checkpointPosition
        );
    }

    public void ResetToStartingPoint ()
    {
        if ( startingPoint == null ) return;

        checkpointPosition = startingPoint.position;
        checkpointRotation = startingPoint.rotation;

        Debug.Log("STASORA: Checkpoint reset to starting point.");
    }
}
