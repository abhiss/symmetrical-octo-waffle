using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct MainEntityCamera : IComponentData
{
    public Entity PlayerEntity;
    public float FOV;
    public float3 CameraOffset;
    public float MinCameraRadius;
    public float MaxCameraRadius;
    public float CameraStep;
    public float CameraSmoothing;
}
