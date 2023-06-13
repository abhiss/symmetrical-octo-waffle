using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private GameObject player;
    [SerializeField] private SphereCollider entryArea;

    private Rigidbody rb;
    private bool playerInside = false;
    private bool playerNearby = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Read inputs
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input.Normalize();

        // If player is nearby and presses the 'E' key, toggle whether they are inside the car
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            playerInside = !playerInside;
            player.SetActive(!playerInside);
        }

        if (!playerInside)
            return;

        // Calculate rotation amount
        float rotationAmount = input.x * rotationSpeed * Time.deltaTime;

        // Rotate the car
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, rotationAmount, 0));

        // Calculate forward movement
        Vector3 forward = transform.forward;
        float speedAmount = input.y * speed * Time.deltaTime;

        // Move the car
        rb.MovePosition(rb.position + forward * speedAmount);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            playerNearby = true;
            Debug.Log("Player has entered the car's radius.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            playerNearby = false;
            // If the player was inside the car but moves out of range, eject them from the car
            if (playerInside)
            {
                playerInside = false;
                player.SetActive(true);
            }
            Debug.Log("Player has left the car's radius.");
        }
    }
}
