using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.CharacterController;
using Unity.NetCode;
using UnityEngine;

// Reference: https://github.com/Unity-Technologies/CharacterControllerSamples/blob/master/OnlineFPS/Assets/Scripts/Character/FirstPersonCharacterAspect.cs#L241
public struct TopDownCharacterUpdateContext
{
    // Here, you may add additional global data for your character updates, such as ComponentLookups, Singletons, NativeCollections, etc...
    // The data you add here will be accessible in your character updates and all of your character "callbacks".
    // [ReadOnly]
    // public ComponentLookup<WeaponVisualFeedback> WeaponVisualFeedbackLookup;
    // [ReadOnly]
    // public ComponentLookup<WeaponControl> WeaponControlLookup;

    public void OnSystemCreate(ref SystemState state)
    {
        // WeaponVisualFeedbackLookup = state.GetComponentLookup<WeaponVisualFeedback>(true);
        // WeaponControlLookup = state.GetComponentLookup<WeaponControl>(true);
    }

    public void OnSystemUpdate(ref SystemState state)
    {
        // WeaponVisualFeedbackLookup.Update(ref state);
        // WeaponControlLookup.Update(ref state);
    }
}

public readonly partial struct TopDownCharacterAspect : IAspect, IKinematicCharacterProcessor<TopDownCharacterUpdateContext>
{
    public readonly KinematicCharacterAspect CharacterAspect;
    public readonly RefRW<TopDownCharacterComponent> CharacterComponent;
    public readonly RefRW<TopDownCharacterControl> CharacterControl;

    public void PhysicsUpdate(ref TopDownCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
    {
        ref TopDownCharacterComponent characterComponent = ref CharacterComponent.ValueRW;
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref float3 characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;

        // First phase of default character update
        CharacterAspect.Update_Initialize(in this, ref context, ref baseContext, ref characterBody, baseContext.Time.DeltaTime);
        CharacterAspect.Update_ParentMovement(in this, ref context, ref baseContext, ref characterBody, ref characterPosition, characterBody.WasGroundedBeforeCharacterUpdate);
        CharacterAspect.Update_Grounding(in this, ref context, ref baseContext, ref characterBody, ref characterPosition);

        // Update desired character velocity after grounding was detected, but before doing additional processing that depends on velocity
        HandleVelocityControl(ref context, ref baseContext);

        // Second phase of default character update
        CharacterAspect.Update_PreventGroundingFromFutureSlopeChange(in this, ref context, ref baseContext, ref characterBody, in characterComponent.StepAndSlopeHandling);
        CharacterAspect.Update_GroundPushing(in this, ref context, ref baseContext, characterComponent.Gravity);
        CharacterAspect.Update_MovementAndDecollisions(in this, ref context, ref baseContext, ref characterBody, ref characterPosition);
        CharacterAspect.Update_MovingPlatformDetection(ref baseContext, ref characterBody);
        CharacterAspect.Update_ParentMomentum(ref baseContext, ref characterBody);
        CharacterAspect.Update_ProcessStatefulCharacterHits();
    }

    public void HandleVelocityControl(ref TopDownCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
    {
        float deltaTime = baseContext.Time.DeltaTime;
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref TopDownCharacterComponent characterComponent = ref CharacterComponent.ValueRW;
        ref TopDownCharacterControl characterControl = ref CharacterControl.ValueRW;

        // Rotate move input and velocity to take into account parent rotation
        if(characterBody.ParentEntity != Entity.Null)
        {
            characterControl.MoveVector = math.rotate(characterBody.RotationFromParent, characterControl.MoveVector);
            characterBody.RelativeVelocity = math.rotate(characterBody.RotationFromParent, characterBody.RelativeVelocity);
        }

        if (characterBody.IsGrounded)
        {
            // Move on ground
            float3 targetVelocity = characterControl.MoveVector * characterComponent.MovementSpeed;
            CharacterControlUtilities.StandardGroundMove_Interpolated(ref characterBody.RelativeVelocity, targetVelocity, characterComponent.MovementSpeedSharpness, deltaTime, characterBody.GroundingUp, characterBody.GroundHit.Normal);

            // If desired, add "jump" controls here
        }
        else
        {
            // If desired, add back air control features

            // Gravity
            CharacterControlUtilities.AccelerateVelocity(ref characterBody.RelativeVelocity, characterComponent.Gravity, deltaTime);
        }
    }

    public void VariableUpdate(ref TopDownCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext) {
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref TopDownCharacterComponent characterComponent = ref CharacterComponent.ValueRW;
        ref quaternion characterRotation = ref CharacterAspect.LocalTransform.ValueRW.Rotation;
        TopDownCharacterControl characterControl = CharacterControl.ValueRO;

        // Add rotation from parent body to the character rotation
        // (this is for allowing a rotating moving platform to rotate your character as well, and handle interpolation properly)
        KinematicCharacterUtilities.AddVariableRateRotationFromFixedRateRotation(ref characterRotation, characterBody.RotationFromParent, baseContext.Time.DeltaTime, characterBody.LastPhysicsUpdateDeltaTime);

        // Compute utilities
    }

    #region Character Processor Callbacks
    public void UpdateGroundingUp(
        ref TopDownCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext)
    {
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;

        CharacterAspect.Default_UpdateGroundingUp(ref characterBody);
    }

    public bool CanCollideWithHit(
        ref TopDownCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        in BasicHit hit)
    {
        return PhysicsUtilities.IsCollidable(hit.Material);
    }

    public bool IsGroundedOnHit(
        ref TopDownCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        in BasicHit hit,
        int groundingEvaluationType)
    {
        TopDownCharacterComponent characterComponent = CharacterComponent.ValueRO;

        return CharacterAspect.Default_IsGroundedOnHit(
            in this,
            ref context,
            ref baseContext,
            in hit,
            in characterComponent.StepAndSlopeHandling,
            groundingEvaluationType);
    }

    public void OnMovementHit(
            ref TopDownCharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext,
            ref KinematicCharacterHit hit,
            ref float3 remainingMovementDirection,
            ref float remainingMovementLength,
            float3 originalVelocityDirection,
            float hitDistance)
    {
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref float3 characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;
        TopDownCharacterComponent characterComponent = CharacterComponent.ValueRO;

        CharacterAspect.Default_OnMovementHit(
            in this,
            ref context,
            ref baseContext,
            ref characterBody,
            ref characterPosition,
            ref hit,
            ref remainingMovementDirection,
            ref remainingMovementLength,
            originalVelocityDirection,
            hitDistance,
            characterComponent.StepAndSlopeHandling.StepHandling,
            characterComponent.StepAndSlopeHandling.MaxStepHeight,
            characterComponent.StepAndSlopeHandling.CharacterWidthForStepGroundingCheck);
    }

    public void OverrideDynamicHitMasses(
        ref TopDownCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        ref PhysicsMass characterMass,
        ref PhysicsMass otherMass,
        BasicHit hit)
    {
    }

    public void ProjectVelocityOnHits(
        ref TopDownCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        ref float3 velocity,
        ref bool characterIsGrounded,
        ref BasicHit characterGroundHit,
        in DynamicBuffer<KinematicVelocityProjectionHit> velocityProjectionHits,
        float3 originalVelocityDirection)
    {
        TopDownCharacterComponent characterComponent = CharacterComponent.ValueRO;

        CharacterAspect.Default_ProjectVelocityOnHits(
            ref velocity,
            ref characterIsGrounded,
            ref characterGroundHit,
            in velocityProjectionHits,
            originalVelocityDirection,
            characterComponent.StepAndSlopeHandling.ConstrainVelocityToGroundPlane);
    }
    #endregion
}
