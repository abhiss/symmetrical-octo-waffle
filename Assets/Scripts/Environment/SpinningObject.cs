using UnityEngine;
using Interactables;

public class SpinningObject : MonoBehaviour, IInteractable
{
    public Vector3 RotationAxis;
    public float RotationSpeed;
    public bool Actived = false;

    public void Interact()
    {
        Actived = !Actived;
    }

    private void Update()
    {
        if (!Actived)
        {
            return;
        }

        transform.Rotate(RotationAxis * RotationSpeed * Time.deltaTime);
    }
}
