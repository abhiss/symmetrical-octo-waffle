using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

/*
public class ItemPickup : MonoBehaviour
{
    private Transform weaponTransform;
    private Transform weaponDrawer;
    private Transform currentWeaponPickup;

    private void Start()
    {
        // Assuming you have a reference to the weapon drawer
        weaponDrawer = GameObject.Find("WeaponDrawer").transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WeaponPickup"))
        {
            // Save the reference to the current weapon pickup
            currentWeaponPickup = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == currentWeaponPickup)
        {
            // Clear the reference to the current weapon pickup
            currentWeaponPickup = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentWeaponPickup != null)
        {
            PickUpWeapon(currentWeaponPickup);
        }

        if (Input.GetKeyDown(KeyCode.Q) && weaponTransform != null)
        {
            DropWeapon();
        }
    }

    private void PickUpWeapon(Transform weapon)
    {
        string weaponName = weapon.name;

        // Disable the current weapon, if any
        if (weaponTransform != null)
        {
            weaponTransform.gameObject.SetActive(false);
        }

        // Activate the new weapon
        weapon.gameObject.SetActive(true);
        weaponTransform = weapon;

        // Update references to muzzle and muzzle flash
        Transform muzzleTransform = weaponTransform.Find("MuzzleLocation");
        Light muzzleFlash = muzzleTransform.GetComponent<Light>();
        // ... do something with the muzzleTransform and muzzleFlash references ...

        // Remove the weapon from the pickup location
        weapon.SetParent(null);

        // Move the weapon to the player's position (or any desired position)
        weapon.position = transform.position;
        weapon.rotation = transform.rotation;

        // Reset the current weapon pickup reference
        currentWeaponPickup = null;
    }

    private void DropWeapon()
    {
        // Disable the current weapon
        weaponTransform.gameObject.SetActive(false);

        // Remove the weapon from the player
        weaponTransform.SetParent(null);

        // Move the weapon to the drop location (or any desired position)
        weaponTransform.position = transform.position + transform.forward * 2f;
        weaponTransform.rotation = transform.rotation;

        // Reset the weaponTransform reference
        weaponTransform = null;
    }
}

*/


