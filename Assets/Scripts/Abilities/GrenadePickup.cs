using UnityEngine;
using Unity.Netcode;

public class GrenadePickup : NetworkBehaviour
{
    public int GrenadeStock = 1;
    public float RotationSpeed = 2.0f;
    private AudioSource _audioSrc;
    private GameObject _model;
    private bool _oneShot = false;

    private void Start()
    {
        _audioSrc = GetComponent<AudioSource>();
        _model = transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * (RotationSpeed * Time.deltaTime));
        if (!_audioSrc.isPlaying && _oneShot)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        GrenadeThrower grenadeThrower = other.GetComponent<GrenadeThrower>();
        if (grenadeThrower.GrenadeCount >= grenadeThrower.MaxGrenades || _oneShot)
        {
            return;
        }

        grenadeThrower.GrenadeCount += GrenadeStock;
        _audioSrc.Play();
        _oneShot = true;
        _model.SetActive(false);
    }
}