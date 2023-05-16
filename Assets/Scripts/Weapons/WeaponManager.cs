using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "ScriptableObjects/Create Weapon", order = 1)]
public class WeaponManager : ScriptableObject
{
    public float Damage = 1.0f;
    public float FireRate = 0.0f;
    public int CurrentClipSize = 0;
    public int MaxClipSize = 30;
    public int CurrentAmmo = 0;
    public int MaxAmmo = 300;
    public float ReloadTime = 1.0f;
    public float MaxDistance = 100.0f;
}
