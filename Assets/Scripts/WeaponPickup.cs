using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public ScriptableObject[] WeaponPool;

    [Header("Animation")]
    public float HoverScalar = 0.001f;
    public float RotationSpeed = 500.0f;

    private void Start()
    {
        // generate weapon
    }

    // Update is called once per frame
    void Update()
    {
        // Hover effect
        Vector3 newPos = transform.position;
        newPos.y += 0.001f * Mathf.Sin(Time.time);
        transform.position = newPos;

        // Spin Object
        transform.Rotate(Vector3.up * (RotationSpeed * Time.deltaTime));
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }


    }
}
