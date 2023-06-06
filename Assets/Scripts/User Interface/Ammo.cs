using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Namespace for TextMeshPro

public class Ammo : MonoBehaviour
{
    public TopDownCharacterShooting characterShooting;
    private TextMeshProUGUI ammoText;

    private void Start()
    {
        ammoText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (characterShooting != null)
        {
            var currentWeapon = characterShooting.GetCurrentWeapon();
            ammoText.text = currentWeapon.currentAmmo.ToString() + "/" + currentWeapon.maxAmmo.ToString();
        }
    }
}
