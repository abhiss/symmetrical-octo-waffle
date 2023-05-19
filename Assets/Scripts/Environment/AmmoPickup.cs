using UnityEngine;
using Unity.Netcode;

public class AmmoPickup : NetworkBehaviour
{
    public int AmmoStock = 50;
    public float RotationSpeed = 2.0f;
    private void Update()
    {
        transform.Rotate(Vector3.up * (RotationSpeed * Time.deltaTime));
    }

    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player"))
        {
            return;
        }

        TopDownCharacterShooting characterShooting = other.GetComponent<TopDownCharacterShooting>();
        WeaponManager currentWeapon = characterShooting.CurrentWeapon;
        if (currentWeapon.CurrentAmmo >= currentWeapon.MaxAmmo)
        {
            return;
        }

        characterShooting.CurrentWeapon.CurrentAmmo += AmmoStock;
        currentWeapon.CurrentAmmo = Mathf.Min(currentWeapon.CurrentAmmo, currentWeapon.MaxAmmo);
        Destroy(gameObject);
    }
}