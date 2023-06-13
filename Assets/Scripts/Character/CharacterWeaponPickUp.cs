<<<<<<< HEAD
using UnityEngine;
using Unity.Netcode;

public class CharacterWeaponPickup : NetworkBehaviour
{
    public WeaponCreator SniperWeapon;
    private bool isPlayerInside = false;
    private CharacterShooting characterShooting = null;
    [Header("Animation")]
    public float HoverScalar = 0.001f;
    public float RotationSpeed = 50.0f;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            characterShooting = other.GetComponent<CharacterShooting>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            characterShooting = null;
        }
    }

    private void Update()
    {
        // Hover effect
        Vector3 newPos = transform.position;
        newPos.y += 0.001f * Mathf.Sin(Time.time);
        transform.position = newPos;

        // Spin Object
        transform.Rotate(Vector3.up * (RotationSpeed * Time.deltaTime));

        if (isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            if (characterShooting != null)
            {
                characterShooting.ChangeWeapon(SniperWeapon);
                Destroy(gameObject); // Optionally destroy the pickup object after it has been used.
            }
        }
    }
}
=======
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterWeaponPickUp : MonoBehaviour
{
    private CharacterShooting characterShooting;
    public float pickupRadius = 3f; // Radius within which the player can pick up the weapon
    public WeaponCreator currentWeapon;
    private Transform weaponTransform;
    public WeaponCreator newWeapon;

    private void Start()
    {
        characterShooting = GetComponent<CharacterShooting>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        newWeapon = other.GetComponent<WeaponCreator>();
        if (newWeapon != null)
        {
            float distance = Vector3.Distance(transform.position, other.transform.position);
            if (distance <= pickupRadius)
            {
                characterShooting.SetActiveWeapon(newWeapon);
                other.gameObject.SetActive(false);
                Debug.Log("SetActiveWeapon");
            }
            Debug.Log("Weapon Not Null");
        }
        else
        {
            Debug.Log("Weapon Null");
        }
        Debug.Log("Trigger Executed!");

    }

    /*
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                float distance = Vector3.Distance(transform.position, other.transform.position);
                if (distance <= pickupRadius)
                {
                    newWeapon = other.GetComponent<WeaponCreator>();
                    if (newWeapon != null)
                    {
                        characterShooting.SetActiveWeapon(newWeapon);
                        other.gameObject.SetActive(false);
                        Debug.Log("SetActiveWeapon");
                    }
                }
            }
        }
    */
    private void Update()
    {
        if (Input.GetKeyDown("e"))
        {
            if (currentWeapon != null)
            {
                DropWeapon();
                Debug.Log("You have dropped your weapon");
            }
            else
            {
                PickUpWeapon(newWeapon);
                Debug.Log("You have picked up the weapon");
            }
        }
    }

    private void PickUpWeapon(WeaponCreator newWeapon)
    {
        if (currentWeapon != null)
        {
            DropWeapon();
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

>>>>>>> 1365cd3988dcca969b69ce6ed043bc5361f381c7
