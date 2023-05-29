using UnityEngine;
using Interactables;

public class InteractableDoor : MonoBehaviour, IInteractable
{
    private bool _isActive = false;
    private Animator _animator;

    private void Start()
    {
        // _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!_isActive)
        {
            return;
        }

        // Play door animation;
        transform.Rotate(0,1,0);
    }

    public void Interact()
    {
        _isActive = true;
    }
}
