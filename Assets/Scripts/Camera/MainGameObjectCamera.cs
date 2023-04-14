

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameObjectCamera : MonoBehaviour
{
    public static Camera Instance;
    public static GameObject PlayerInstance;

    void Awake()
    {
        Instance = GetComponent<UnityEngine.Camera>();
        PlayerInstance = GetComponent<MainEntityCameraAuthoring>().PlayerObject;
    }
}