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