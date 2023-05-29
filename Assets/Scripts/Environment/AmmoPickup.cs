using UnityEngine;
using Unity.Netcode;

public class AmmoPickup : NetworkBehaviour
{
    public int AmmoStock = 50;
    public float RotationSpeed = 2.0f;
    private AudioSource _audioSrc;
    private MeshRenderer _meshRenderer;

    private void Start()
    {
        _audioSrc = GetComponent<AudioSource>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * (RotationSpeed * Time.deltaTime));
        if (!_audioSrc.isPlaying && !_meshRenderer.enabled)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player"))
        {
            return;
        }

        CharacterShooting characterShooting = other.GetComponent<CharacterShooting>();
        WeaponCreator currentWeapon = characterShooting.CurrentWeapon;
        if (currentWeapon.CurrentAmmo >= currentWeapon.MaxAmmo || !_meshRenderer.enabled)
        {
            return;
        }

        characterShooting.CurrentWeapon.CurrentAmmo += AmmoStock;
        currentWeapon.CurrentAmmo = Mathf.Min(currentWeapon.CurrentAmmo, currentWeapon.MaxAmmo);
        _audioSrc.Play();
        _meshRenderer.enabled = false;
    }
}