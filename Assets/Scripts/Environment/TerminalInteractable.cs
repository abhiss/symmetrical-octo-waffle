using UnityEngine;
using Interactables;

public class TerminalInteractable : MonoBehaviour
{
    public GameObject[] Targets;

    [Header("Debugging")]
    public bool ShowConnections;
    private GameObject _terminalModel;

    private void Start()
    {
        _terminalModel = transform.GetChild(0).gameObject;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // TODO: NOT OPTIMAL
        CharacterInteract playerListener = other.GetComponent<CharacterInteract>();
        if (playerListener.IsUsingKey)
        {
            Interactable.MessageTargets(Targets);
        }
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
