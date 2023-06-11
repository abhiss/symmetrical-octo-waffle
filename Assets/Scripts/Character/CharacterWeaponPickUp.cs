using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterWeaponPickUp : MonoBehaviour
{
    private CharacterShooting characterShooting; 
    public Transform WeaponHolder;
    private WeaponCreator currentWeapon;
    private Transform weaponTransform;
    private WeaponCreator newWeapon;

    private void Start()
    {
        // Get a reference to the CharacterShooting script
        characterShooting = GetComponent<CharacterShooting>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            newWeapon = other.GetComponent<WeaponCreator>();
            if (newWeapon != null)
            {
                // Pick up the new weapon
                characterShooting.SetActiveWeapon(newWeapon);

                // Disable the weapon object in the scene
                other.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentWeapon != null)
            {
                // Drop the current weapon
                DropWeapon();
            }
            else
            {
                // Pick up the nearest weapon from the ground
                PickUpWeapon(newWeapon);
            }
        }
    }

    private void PickUpWeapon(WeaponCreator newWeapon)
    {
        if (currentWeapon != null)
        {
            DropWeapon(); // Drop the current weapon before picking up a new one
        }

        currentWeapon = newWeapon;
    }

    private void DropWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon = null;
        }
    }
 

}


