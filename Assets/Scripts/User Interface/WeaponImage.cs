using UnityEngine;
using UnityEngine.UI; // Namespace for UI elements

public class WeaponImage : MonoBehaviour
{
    public TopDownCharacterShooting characterShooting;
    private Image weaponImage;

    private void Start()
    {
        weaponImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (characterShooting != null)
        {
            weaponImage.sprite = characterShooting.GetCurrentWeapon().weaponImage;
        }
    }
}
