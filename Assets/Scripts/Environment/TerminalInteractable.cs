using UnityEngine;
using Unity.Netcode;
using Interactables;

public class TerminalInteractable : NetworkBehaviour
{
    public GameObject[] Targets;
    public GameObject TerminalScreen;
    private InputListener _inputListener;
    private AudioSource _audioSrc;

    [Header("Rendering")]
    private Renderer _screenRenderer;
    private Color _targetColor;
    private Color _currentColor;
    private Color _startingColor;
    private bool _playedOnce = false;

    [Header("Debugging")]
    public bool ShowConnections;

    private void Start()
    {
        _audioSrc = GetComponent<AudioSource>();
        _screenRenderer = TerminalScreen.GetComponent<Renderer>();
        _startingColor = _screenRenderer.material.color;
        _currentColor = Color.black;
        _targetColor = Color.black;
    }

    private void Update()
    {
        ScreenGlow();

        if (_inputListener == null)
        {
            return;
        }

        if (_inputListener.UseKey)
        {
            Interactable.MessageTargets(Targets);
        }

        if (!_playedOnce)
        {
            _audioSrc.Play();
            _playedOnce = true;
        }
    }

    private void ScreenGlow()
    {
        _targetColor = Color.black;
        if (_inputListener != null)
        {
            _targetColor = _startingColor;
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

        _playedOnce = false;
        _inputListener = other.GetComponent<InputListener>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        _playedOnce = false;
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
