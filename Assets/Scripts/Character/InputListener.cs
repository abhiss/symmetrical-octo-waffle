using Unity.Netcode;
using UnityEngine;

public class InputListener : NetworkBehaviour
{
    [Header("Settings")]
    public LayerMask WallMask;
    public LayerMask ObstructionMask;
    public bool DisableInput = false;

    [Header("Live Variables")]
    public bool UseKey = false;
    public bool ReloadKey = false;
    public bool GrenadeKey = false;
    public bool FireKeyDown = false;
    public bool FireKey = false;
    public bool AltFire = false;
    public bool AltFireDown = false;
    public bool SpaceDown = false;
    public bool ShiftKey = false;
    public Vector3 MousePosition;

    public Vector3 CursorWorldPosition()
    {
        if (DisableInput)
        {
            return Vector3.zero;
        }


        Vector3 target = Vector3.zero;
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Hit floor
        if (Physics.Raycast(cameraRay, out RaycastHit hit, Mathf.Infinity, ~ObstructionMask, QueryTriggerInteraction.Ignore))
        {
            target = hit.point;
        }

        return target;
    }

    public Vector3 MousePlanePosition()
    {
        if (DisableInput)
        {
            return Vector3.zero;
        }

        Vector3 planePosition = transform.position;
        Plane cursorPlane = new Plane(transform.position, Vector3.up);

        Ray ray = Camera.main.ScreenPointToRay(MousePosition);
        if (cursorPlane.Raycast(ray, out float dist))
        {
            planePosition = ray.GetPoint(dist);
        }
        return planePosition;
    }

    public Vector3 GetAxisInput()
    {
        if (DisableInput)
        {
            return Vector3.zero;
        }

        Vector3 rawInput = new Vector3(-Input.GetAxis("Horizontal"), 0, -Input.GetAxis("Vertical"));
        return rawInput;
    }

    private void Update()
    {
        if (!IsOwner) return;

        // TODO: This should really be a singleton. But I dont think we have the time.
        if (DisableInput)
        {
            UseKey = false;
            ReloadKey = false;
            GrenadeKey = false;
            FireKey = false;
            FireKeyDown = false;
            AltFire = false;
            AltFireDown = false;
            SpaceDown = false;
            ShiftKey = false;
            return;
        }

        MousePosition = Input.mousePosition;

        // Shooting
        FireKeyDown = Input.GetKeyDown(KeyCode.Mouse0);
        FireKey = Input.GetKey(KeyCode.Mouse0);

        AltFire = Input.GetKey(KeyCode.Mouse1);
        AltFireDown = Input.GetKeyDown(KeyCode.Mouse1);

        // Interactions
        UseKey = Input.GetKeyDown(KeyCode.E);
        ReloadKey = Input.GetKeyDown(KeyCode.R);
        GrenadeKey = Input.GetKeyDown(KeyCode.G);
        SpaceDown = Input.GetKeyDown(KeyCode.Space);
        ShiftKey = Input.GetKeyDown(KeyCode.LeftShift);
    }
}