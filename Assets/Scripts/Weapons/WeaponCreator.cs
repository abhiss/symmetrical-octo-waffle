using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Weapon", menuName = "ScriptableObjects/Create Weapon", order = 1)]
public class WeaponCreator : ScriptableObject
{
    public float Damage = 1.0f;
    public float FireRate = 0.0f;
    public int MaxClipSize = 30;
    public int MaxAmmo = 300;
    public float ReloadTime = 1.0f;
    public float MaxDistance = 100.0f;

    [Header("Live Variables")]
    [NonSerialized] public int CurrentAmmo = 0;
    [NonSerialized] public int CurrentClipSize = 0;
}
