using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInteract : MonoBehaviour
{
    public bool IsUsingKey = false;

    void Update()
    {
        IsUsingKey = Input.GetKeyDown(KeyCode.E);
    }
}
