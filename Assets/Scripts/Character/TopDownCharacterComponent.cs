using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.CharacterController;
using Unity.NetCode;

[Serializable]
[GhostComponent()]
public struct TopDownCharacterComponent : IComponentData
{
    [Header("Movement")]
    public float MovementSpeed;
    public float MovementSpeedSharpness;
    public float3 Gravity;
    public bool PreventAirAccelerationAgainstUngroundedHits;
    public BasicStepAndSlopeHandlingParameters StepAndSlopeHandling;

    public static TopDownCharacterComponent GetDefault()
    {
        return new TopDownCharacterComponent
        {
            MovementSpeed = 10f,
            MovementSpeedSharpness = 15f,
            Gravity = math.up() * -30f,

            StepAndSlopeHandling = BasicStepAndSlopeHandlingParameters.GetDefault(),
        };
    }
}

[Serializable]
public struct TopDownCharacterControl : IComponentData
{
    public float3 MoveVector;
}

[Serializable]
public struct TopDownCharacterView : IComponentData
{
    public Entity CharacterEntity;
}

[Serializable]
public struct CharacterClientCleanup : ICleanupComponentData
{
    public Entity DeathVFX;
    public float3 DeathVFXSpawnWorldPosition;
}

[Serializable]
[GhostComponent()]
public struct OwningPlayer : IComponentData
{
    [GhostField()]
    public Entity Entity;
}