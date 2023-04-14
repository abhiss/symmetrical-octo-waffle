using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[DisallowMultipleComponent]
public class MainEntityCameraAuthoring : MonoBehaviour
{
    public GameObject PlayerObject;

    [Header("Settings")]
    public float FOV;
    public Vector3 CameraOffset;
    public float MinCameraRadius;
    public float MaxCameraRadius;
    public float CameraStep;
    public float CameraSmoothing;

    public class Baker : Baker<MainEntityCameraAuthoring>
    {
        public override void Bake(MainEntityCameraAuthoring authoring)
        {
            // Self entity
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            // Player entity
            MainEntityCamera properties = new MainEntityCamera();
            properties.PlayerEntity = GetEntity(
                authoring.PlayerObject,
                TransformUsageFlags.Dynamic
            );
            properties.FOV = authoring.FOV;
            properties.CameraOffset = authoring.CameraOffset;
            properties.MinCameraRadius = authoring.MinCameraRadius;
            properties.MaxCameraRadius = authoring.MaxCameraRadius;
            properties.CameraStep = authoring.CameraStep;

            AddComponent<MainEntityCamera>(entity, properties);
        }
    }
}