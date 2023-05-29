using UnityEngine;
using Unity.Netcode;

public class AmmoPickup : NetworkBehaviour
{
    public int AmmoStock = 50;
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
        if(!other.CompareTag("Player"))
        {
            return;
        }

        CharacterShooting characterShooting = other.GetComponent<CharacterShooting>();
        WeaponCreator currentWeapon = characterShooting.CurrentWeapon;
        if (currentWeapon.CurrentAmmo >= currentWeapon.MaxAmmo || _oneShot)
        {
            return;
        }

        characterShooting.CurrentWeapon.CurrentAmmo += AmmoStock;
        currentWeapon.CurrentAmmo = Mathf.Min(currentWeapon.CurrentAmmo, currentWeapon.MaxAmmo);
        _audioSrc.Play();
        _oneShot = true;
        _model.SetActive(false);
    }
}