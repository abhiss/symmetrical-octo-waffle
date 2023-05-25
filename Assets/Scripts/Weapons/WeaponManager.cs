using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "ScriptableObjects/Create Weapon", order = 1)]
public class WeaponManager : ScriptableObject
{
    public float Damage = 1.0f;
    public float Interval = 0.0f;
    public float MaxDistance = 100.0f;
}
