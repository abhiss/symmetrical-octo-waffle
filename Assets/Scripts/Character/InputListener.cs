using UnityEngine;

public class InputListener : MonoBehaviour
{
    [Header("Settings")]
    public bool DisableInput = false;

    [Header("Live Variables")]
    public bool UseKey = false;
    public bool ReloadKey = false;
    public bool FireKeyDown = false;
    public bool FireKey = false;
    public bool AltFire = false;
    public bool AltFireDown = false;
    public bool SpaceDown = false;
    public Vector3 MousePosition;

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
        // TODO: This should really be a singleton. But I dont think we have the time.
        if (DisableInput)
        {
            UseKey = false;
            ReloadKey = false;
            FireKey = false;
            FireKeyDown = false;
            AltFire = false;
            AltFireDown = false;
            SpaceDown = false;
            return;
        }

        MousePosition = Input.mousePosition;

        UseKey = Input.GetKeyDown(KeyCode.E);
        ReloadKey = Input.GetKeyDown(KeyCode.R);

        FireKeyDown = Input.GetKeyDown(KeyCode.Mouse0);
        FireKey = Input.GetKey(KeyCode.Mouse0);

        AltFire = Input.GetKey(KeyCode.Mouse1);
        AltFireDown = Input.GetKeyDown(KeyCode.Mouse1);

        SpaceDown = Input.GetKeyDown(KeyCode.Space);
    }
}