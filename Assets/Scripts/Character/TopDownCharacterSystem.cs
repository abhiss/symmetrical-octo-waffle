using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.CharacterController;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct BuildCharacterRotationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<LocalTransform, TopDownCharacterComponent>().Build());
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        BuildCharacterRotationJob job = new BuildCharacterRotationJob
        { };
        job.Schedule();
    }

    [BurstCompile]
    public partial struct BuildCharacterRotationJob : IJobEntity
    {
        void Execute(ref LocalTransform localTransform, in TopDownCharacterComponent characterComponent)
        {
            //TopDownCharacterUtilities.ComputeRotationFromYAngleAndUp(characterComponent.CharacterYDegrees, math.up(), out quaternion tmpRotation);
            // localTransform.Rotation = tmpRotation;
        }
    }
}

[UpdateInGroup(typeof(KinematicCharacterPhysicsUpdateGroup))]
[BurstCompile]
public partial struct TopDownCharacterPhysicsUpdateSystem : ISystem
{
    private EntityQuery _characterQuery;
    private TopDownCharacterUpdateContext _context;
    private KinematicCharacterUpdateContext _baseContext;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _characterQuery = KinematicCharacterUtilities.GetBaseCharacterQueryBuilder()
            .WithAll<
                TopDownCharacterComponent,
                TopDownCharacterControl>()
            .Build(ref state);

        _context = new TopDownCharacterUpdateContext();
        _context.OnSystemCreate(ref state);
        _baseContext = new KinematicCharacterUpdateContext();
        _baseContext.OnSystemCreate(ref state);

        state.RequireForUpdate(_characterQuery);
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<NetworkTime>())
            return;

        _context.OnSystemUpdate(ref state);
        _baseContext.OnSystemUpdate(ref state, SystemAPI.Time, SystemAPI.GetSingleton<PhysicsWorldSingleton>());

        TopDownCharacterPhysicsUpdateJob job = new TopDownCharacterPhysicsUpdateJob
        {
            Context = _context,
            BaseContext = _baseContext,
        };
        job.ScheduleParallel();
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct TopDownCharacterPhysicsUpdateJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        public TopDownCharacterUpdateContext Context;
        public KinematicCharacterUpdateContext BaseContext;

        void Execute(ref TopDownCharacterAspect characterAspect)
        {
            characterAspect.PhysicsUpdate(ref Context, ref BaseContext);
        }

        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            BaseContext.EnsureCreationOfTmpCollections();
            return true;
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
        {

        }
    }
}

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[UpdateAfter(typeof(PredictedFixedStepSimulationSystemGroup))]
[BurstCompile]
public partial struct TopDownCharacterVariableUpdateSystem : ISystem
{
    private EntityQuery _characterQuery;
    private TopDownCharacterUpdateContext _context;
    private KinematicCharacterUpdateContext _baseContext;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _characterQuery = KinematicCharacterUtilities.GetBaseCharacterQueryBuilder()
            .WithAll<
                TopDownCharacterComponent,
                TopDownCharacterControl>()
            .Build(ref state);

        _context = new TopDownCharacterUpdateContext();
        _context.OnSystemCreate(ref state);
        _baseContext = new KinematicCharacterUpdateContext();
        _baseContext.OnSystemCreate(ref state);

        state.RequireForUpdate(_characterQuery);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _context.OnSystemUpdate(ref state);
        _baseContext.OnSystemUpdate(ref state, SystemAPI.Time, SystemAPI.GetSingleton<PhysicsWorldSingleton>());

        // Schedule the jobs
        TopDownCharacterVariableUpdateJob variableUpdateJob = new TopDownCharacterVariableUpdateJob
        {
            Context = _context,
            BaseContext = _baseContext,
        };
        variableUpdateJob.ScheduleParallel();
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct TopDownCharacterVariableUpdateJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        public TopDownCharacterUpdateContext Context;
        public KinematicCharacterUpdateContext BaseContext;

        void Execute(ref TopDownCharacterAspect characterAspect)
        {
            characterAspect.VariableUpdate(ref Context, ref BaseContext);
        }

        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            BaseContext.EnsureCreationOfTmpCollections();
            return true;
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
        { }
    }
}