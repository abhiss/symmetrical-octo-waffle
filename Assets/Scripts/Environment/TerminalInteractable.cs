using UnityEngine;
using Unity.Netcode;
using Interactables;

public class TerminalInteractable : NetworkBehaviour
{
    public GameObject[] Targets;
    private GameObject _terminalModel;
    private InputListener _inputListener;

    [Header("Debugging")]
    public bool ShowConnections;

    private void Start()
    {
        _terminalModel = transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        if(_inputListener != null && _inputListener.UseKey)
        {
            Interactable.MessageTargets(Targets);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        _inputListener = other.GetComponent<InputListener>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        _inputListener = null;
    }

    private void OnDrawGizmos()
    {
        if (ShowConnections)
        {
            Gizmos.color = Color.cyan;
            foreach (var target in Targets)
            {
                Gizmos.DrawLine(transform.position, target.transform.position);
            }
        }
    }
}
