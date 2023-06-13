using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class Ammo : MonoBehaviour
{
    public CharacterShooting characterShooting;
    [SerializeField] TextMeshProUGUI ammoTitleText;
    [SerializeField] TextMeshProUGUI ammoWeaponText;

    private void Update()
    {
        if (characterShooting != null)
        {
            var currentWeaponName = characterShooting.GetCurrentWeaponName();
            ammoTitleText.text = currentWeaponName;

            var (currentAmmo, maxAmmo) = characterShooting.GetCurrentWeaponAmmoInfo();
            ammoWeaponText.text = $"{currentAmmo}/{maxAmmo}";
        }
    }
}
