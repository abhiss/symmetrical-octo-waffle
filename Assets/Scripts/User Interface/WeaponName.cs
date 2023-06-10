using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Namespace for TextMeshPro

public class WeaponName : MonoBehaviour
{
    public CharacterShooting characterShooting;
    private TextMeshProUGUI weaponNameText;

    private void Start()
    {
        weaponNameText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (characterShooting != null)
        {
            weaponNameText.text = characterShooting.GetCurrentWeaponName();
        }
    }
}
