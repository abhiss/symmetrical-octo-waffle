using UnityEngine;
using Unity.Netcode;
using Interactables;

public class TerminalInteractable : NetworkBehaviour
{
    public GameObject[] Targets;
    public GameObject TerminalScreen;
    private InputListener _inputListener;

    [Header("Rendering")]
    private Renderer _screenRenderer;
    private Color _targetColor;
    private Color _currentColor;
    private Color _startingColor;

    [Header("Debugging")]
    public bool ShowConnections;

    private void Start()
    {
        _screenRenderer = TerminalScreen.GetComponent<Renderer>();
        _startingColor = _screenRenderer.material.color;
        _currentColor = Color.black;
        _targetColor = Color.black;
    }

    private void Update()
    {
        _targetColor = Color.black;
        if (_inputListener != null)
        {
           _targetColor = _startingColor;
            if (_inputListener.UseKey)
            {
                Interactable.MessageTargets(Targets);
            }
        }

        _currentColor = Vector4.MoveTowards(_currentColor, _targetColor, Time.deltaTime);
        _screenRenderer.material.color = _currentColor;
        _screenRenderer.material.SetColor("_EmissionColor", _currentColor);
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
