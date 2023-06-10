using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
public class Ammo : MonoBehaviour
{
    public CharacterShooting characterShooting;
    private TextMeshProUGUI ammoText;

    private void Start()
    {
        ammoText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (characterShooting != null)
        {
            var (currentAmmo, maxAmmo) = characterShooting.GetCurrentWeaponAmmoInfo();
            ammoText.text = $"{currentAmmo}/{maxAmmo}";
        }
    }
}
