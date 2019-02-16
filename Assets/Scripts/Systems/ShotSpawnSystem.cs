using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class ShotSpawnBarrier : BarrierSystem
{ }

public class ShotSpawnSystem : ComponentSystem
{
    private ComponentGroup spawnedShots;
    private TowerBuilder towerBuilder;

    protected override void OnCreateManager()
    {
        spawnedShots = GetComponentGroup(typeof(ShotSpawnData));
    }

    protected override void OnUpdate()
    {
        if (towerBuilder == null)
            towerBuilder = GameObject.Find("TowerBuilder").GetComponent<TowerBuilder>();

        EntityCommandBuffer buffer = PostUpdateCommands;
        int length = spawnedShots.CalculateLength();

        EntityArray entities = spawnedShots.GetEntityArray();
        ComponentDataArray<ShotSpawnData> shotSpawns = spawnedShots.GetComponentDataArray<ShotSpawnData>();

        for (int i = 0; i < length; ++i)
        {
            ShotSpawnData sd = shotSpawns[i];
            Entity shotEntity = entities[i];

            // Remove the spawn data - no longer necessary on entity
            buffer.RemoveComponent<ShotSpawnData>(shotEntity);
            // Add required component data
            buffer.AddComponent(shotEntity, new Shot()
            {
                Damage = sd.Damage,
                Direction = sd.Direction,
                Speed = 25
            });
            buffer.AddComponent(shotEntity, sd.Position);
            buffer.AddComponent(shotEntity, new Scale() { Value = 0.1f });
            buffer.AddSharedComponent(shotEntity, new Unity.Rendering.MeshInstanceRenderer() { mesh = towerBuilder.cubeMesh, material = towerBuilder.towerDatas[sd.Index].material });
        }
    }
}
