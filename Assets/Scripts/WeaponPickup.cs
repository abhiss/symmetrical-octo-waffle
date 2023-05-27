using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    private void Start()
    {
        // generate weapon
    }

    // Update is called once per frame
    void Update()
    {
        // spin it around
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }


    }
}
