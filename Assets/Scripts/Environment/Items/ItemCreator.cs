using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Create Item", order = 1)]
public class Item : ScriptableObject
{
    [Header("Flavor")]
    public string ItemName;
    public string ItemDescription;

    [Header("Appearance")]
    public Texture2D ItemTexture;
    public GameObject ItemModel;
}
