
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DimensionManager : MonoBehaviour
{
    public static DimensionManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ThirdPersonCamera thirdPersonCamera;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform player;

    [Header("2D Settings")]
    [SerializeField] private float planeZ = 0f;
    [SerializeField] private float cameraDistance = 15f;
    [SerializeField] private float orthographicSize = 7f;

    [Header("Dimension Travel")]
    [SerializeField] private float groundCheckDistance = 0.35f;
    [SerializeField] private float exitSkinWidth = 0.03f;

    [Header("Transition")]
    [SerializeField] private DimensionTransition dimensionTransition;

    private bool is2D;

    private Vector3 saved3DPosition;
    private Quaternion saved3DRotation;
    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;
    private float savedFieldOfView;

    private readonly List<Projected2DCollider> availableSupports = new List<Projected2DCollider>();

    private Projected2DCollider selectedSupport;
    private int selectedSupportIndex;

    public bool Is2D => is2D;

    public float SelectedDepth => selectedSupport != null ? selectedSupport.OriginalZ : saved3DPosition.z;
    public int AvailableDepthCount => availableSupports.Count;

    private void Awake ()
    {
        if ( Instance != null && Instance != this )
        {
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void Update ()
    {
        if ( DimensionTransition.InputLocked ) return;
        if ( is2D )
        {
            RefreshAvailableSupports();

            if ( Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame )
                SelectNextSupport();
        }
        if ( Keyboard.current == null ) return;
        if ( Keyboard.current.eKey.wasPressedThisFrame )
            ToggleDimension();
    }

    public void ToggleDimension ()
    {
        if ( DimensionTransition.InputLocked ) return;
        if ( dimensionTransition != null && dimensionTransition.IsTransitioning ) return;
        if ( is2D )
        {
            RefreshAvailableSupports();

            if ( ! TryGetExitPosition(out Vector3 exitPosition) )
            {
                Debug.Log(
                    "STASORA: No valid 3D exit at this position."
                );
                return;
            }

            if ( dimensionTransition != null )
                dimensionTransition.PlayTransition(() => Exit2D(exitPosition));
            else
                Exit2D(exitPosition);

            return;
        }

        if ( TimeFreezeManager.Instance == null || ! TimeFreezeManager.Instance.IsFrozen )
        {
            Debug.Log(
                "Freeze time before entering 2D."
            );
            return;
        }
        if ( dimensionTransition != null )
            dimensionTransition.PlayTransition(Enter2D);
        else
            Enter2D();
    }

    private void Enter2D ()
    {
        if ( player == null || mainCamera == null || playerMovement == null || thirdPersonCamera == null ) return;

        saved3DPosition = player.position;
        saved3DRotation = player.rotation;

        savedCameraPosition = mainCamera.transform.position;
        savedCameraRotation = mainCamera.transform.rotation;
        savedFieldOfView = mainCamera.fieldOfView;

        availableSupports.Clear();
        selectedSupport = null;
        selectedSupportIndex = 0;

        is2D = true;

        thirdPersonCamera.enabled = false;
        playerMovement.enabled = false;

        CharacterController controller = player.GetComponent<CharacterController>();

        if ( controller != null ) controller.enabled = false;

        Vector3 position = player.position;
        position.z = planeZ;
        player.position = position;
        player.rotation = Quaternion.identity;

        if ( controller != null ) controller.enabled = true;

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = orthographicSize;

        Update2DCamera();

        Physics.SyncTransforms();

        Debug.Log("STASORA: Entered 2D mode");
    }

    private void RefreshAvailableSupports ()
    {
        Projected2DCollider previousSelection = selectedSupport;

        availableSupports.Clear();

        if ( player == null )
        {
            selectedSupport = null;
            selectedSupportIndex = 0;
            return;
        }

        CharacterController controller = player.GetComponent<CharacterController>();

        if ( controller == null || ! controller.enabled )
        {
            selectedSupport = null;
            selectedSupportIndex = 0;
            return;
        }

        Physics.SyncTransforms();

        Vector3 rayOrigin = new Vector3(player.position.x, controller.bounds.min.y + 0.15f, planeZ);

        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, groundCheckDistance + 0.15f, ~0, QueryTriggerInteraction.Ignore);

        foreach ( RaycastHit hit in hits )
        {
            Projected2DSource source = hit.collider.GetComponent<Projected2DSource>();

            if ( source == null || source.Source == null ) continue;

            Projected2DCollider candidate = source.Source;

            if ( ! candidate.IsProjected || ! candidate.CanBeDimensionSupport) continue;
            if ( ! availableSupports.Contains(candidate) ) availableSupports.Add(candidate);
        }

        availableSupports.Sort((a, b) => a.OriginalZ.CompareTo(b.OriginalZ));

        if ( availableSupports.Count == 0 )
        {
            selectedSupport = null;
            selectedSupportIndex = 0;
            return;
        }

        int previousIndex = availableSupports.IndexOf(previousSelection);

        if ( previousIndex >= 0 ) selectedSupportIndex = previousIndex;
        else selectedSupportIndex = 0;

        selectedSupport = availableSupports[selectedSupportIndex];
    }

    private void SelectNextSupport ()
    {
        if ( ! is2D || availableSupports.Count <= 1 ) return;

        selectedSupportIndex = (selectedSupportIndex + 1) % availableSupports.Count;
        selectedSupport = availableSupports[selectedSupportIndex];

        Debug.Log(
            "STASORA: Selected depth Z = " +
            selectedSupport.OriginalZ +
            " (" +
            (selectedSupportIndex + 1) +
            "/" +
            availableSupports.Count +
            ")"
        );
    }

    private bool TryGetExitPosition ( out Vector3 exitPosition )
    {
        exitPosition = Vector3.zero;

        if ( selectedSupport == null ) return false;

        exitPosition = new Vector3(player.position.x, player.position.y, selectedSupport.OriginalZ);

        return IsExitPositionClear(exitPosition, selectedSupport);
    }

    private bool IsExitPositionClear ( Vector3 exitPosition, Projected2DCollider support)
    {
        CharacterController controller = player.GetComponent<CharacterController>();

        if ( controller == null ) return false;

        Vector3 center = exitPosition + saved3DRotation * controller.center;

        float radius = controller.radius;
        float height = Mathf.Max(controller.height, radius * 2f);
        float halfSegment = height * 0.5f - radius;

        Vector3 top = center + Vector3.up * halfSegment;
        Vector3 bottom = center - Vector3.up * halfSegment;

        Collider[] overlaps = Physics.OverlapCapsule(bottom, top, Mathf.Max(0.01f, radius - exitSkinWidth), ~0, QueryTriggerInteraction.Ignore);

        foreach ( Collider overlap in overlaps )
        {
            if ( overlap == controller ) continue;
            if ( overlap.transform.IsChildOf(player) ) continue;
            if ( overlap.gameObject == support.gameObject ) continue;
            if ( overlap.GetComponent<Projected2DSource>() != null ) continue;

            return false;
        }

        return true;
    }

    private void Exit2D ( Vector3 exitPosition )
    {
        CharacterController controller = player.GetComponent<CharacterController>();

        if ( controller != null ) controller.enabled = false;

        player.position = exitPosition;
        player.rotation = saved3DRotation;

        is2D = false;

        Physics.SyncTransforms();

        if ( controller != null ) controller.enabled = true;

        mainCamera.orthographic = false;
        mainCamera.fieldOfView = savedFieldOfView;
        mainCamera.transform.position = savedCameraPosition;
        mainCamera.transform.rotation = savedCameraRotation;

        playerMovement.enabled = true;
        thirdPersonCamera.enabled = true;

        availableSupports.Clear();
        selectedSupport = null;
        selectedSupportIndex = 0;

        Debug.Log(
            "STASORA: Returned to 3D at Z = " +
            exitPosition.z
        );
    }

    private void LateUpdate ()
    {
        if ( is2D ) Update2DCamera();
    }

    private void Update2DCamera ()
    {
        if ( player == null || mainCamera == null ) return;

        mainCamera.transform.position = new Vector3(player.position.x, player.position.y + 2f, planeZ - cameraDistance);
        mainCamera.transform.rotation = Quaternion.identity;
    }

    private void OnGUI ()
    {
        if ( ! is2D || availableSupports.Count <= 1 ) return;

        GUI.Box(new Rect(10, 70, 240, 65), "Dimension Depth");
        GUI.Label(new Rect(20, 95, 220, 25), "R : Z = " + SelectedDepth + " (" + (selectedSupportIndex + 1) + "/" + availableSupports.Count + ")");
    }

    private void OnDestroy ()
    {
        if ( Instance == this ) Instance = null;
    }

    public void ResetForRespawn ()
    {
        if ( player == null || mainCamera == null ) return;

        is2D = false;

        availableSupports.Clear();
        selectedSupport = null;
        selectedSupportIndex = 0;

        mainCamera.orthographic = false;

        if ( savedFieldOfView > 0f )
            mainCamera.fieldOfView = savedFieldOfView;

        if ( thirdPersonCamera != null )
            thirdPersonCamera.enabled = true;

        if ( playerMovement != null )
        {
            playerMovement.enabled = false;
            playerMovement.enabled = true;
        }

        Debug.Log("STASORA: Dimension reset to 3D for respawn.");
    }

}
