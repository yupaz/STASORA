
using UnityEngine;

public class PuzzleDoor : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Vector3 openOffset =
        new Vector3(0f, 4f, 0f);

    [SerializeField] private float moveSpeed = 3f;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen;

    private void Start ()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
    }

    private void Update ()
    {
        Vector3 targetPosition = isOpen ? openPosition : closedPosition;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    public void SetOpen ( bool open )
    {
        isOpen = open;
    }
}
