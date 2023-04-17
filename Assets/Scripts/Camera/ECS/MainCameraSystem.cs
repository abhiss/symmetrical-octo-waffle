using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class MainCameraSystem : SystemBase
{
    private Entity CameraEntity;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        if (MainGameObjectCamera.Instance == null || !SystemAPI.HasSingleton<MainEntityCamera>())
        {
            return;
        }

        // Need atleast one component to exist to run code
        state.RequireForUpdate<MainEntityCamera>();
        CameraEntity = SystemAPI.GetSingletonEntity<MainEntityCamera>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (MainGameObjectCamera.Instance == null || !SystemAPI.HasSingleton<MainEntityCamera>())
        {
            return;
        }

        LocalToWorld targetLocalToWorld = SystemAPI.GetComponent<LocalToWorld>(CameraEntity);
        float3 playerPosition = MainGameObjectCamera.PlayerInstance.transform.position;
        MainGameObjectCamera.Instance.transform.position = targetLocalToWorld.Position;
    }
}