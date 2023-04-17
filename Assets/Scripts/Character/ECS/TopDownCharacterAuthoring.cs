using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;
using Unity.CharacterController;
using Unity.Physics;
using System.Collections.Generic;
using UnityEngine.Serialization;

// Description: ECS Multiplayer Top Down Character Controller
[DisallowMultipleComponent]
[RequireComponent(typeof(PhysicsShapeAuthoring))]
public class TopDownCharacterAuthoring : MonoBehaviour
{
    [Header("Core Functionality")]
    public GameObject TopDownCamera;
    public GameObject CharacterController;

    // [Header("Misc")]
    // Add flavor here for the future (VFX, player specific things, etc)

    public AuthoringKinematicCharacterProperties CharacterProperties = AuthoringKinematicCharacterProperties.GetDefault();
    public TopDownCharacterComponent Character = TopDownCharacterComponent.GetDefault();

    public class Baker : Baker<TopDownCharacterAuthoring>
    {
        public override void Bake(TopDownCharacterAuthoring authoring)
        {
            KinematicCharacterUtilities.BakeCharacter(this, authoring, authoring.CharacterProperties);

            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            // authoring.Character.ViewEntity = GetEntity(authoring.ViewObject, TransformUsageFlags.Dynamic);
            // authoring.Character.NameTagSocketEntity = GetEntity(authoring.NameTagSocket, TransformUsageFlags.Dynamic);

            AddComponent(entity, authoring.Character);
            // AddComponent(entity, new FirstPersonCharacterControl());
            // AddComponent(entity, new OwningPlayer());
        }
    }
}
