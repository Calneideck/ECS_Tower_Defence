using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class TowerShootSystem : JobComponentSystem
{
    private ComponentGroup enemyGroup;
    private ComponentGroup towerGroup;

    [Inject]
    private ShotSpawnBarrier shotSpawnBarrier;

    protected override void OnCreateManager()
    {
        enemyGroup = GetComponentGroup(typeof(Position), typeof(Enemy));
        towerGroup = GetComponentGroup(typeof(Position), typeof(Tower));
    }

    private struct TowerShootJob : IJobParallelFor
    {
        public float dt;

        [ReadOnly]
        public ComponentDataArray<Position> towerPositions;

        public ComponentDataArray<Tower> towers;

        [ReadOnly]
        public ComponentDataArray<Position> enemyPositions;

        public EntityCommandBuffer.Concurrent buffer;

        [ReadOnly]
        public EntityArchetype shotArchtype;

        public void Execute(int index)
        {
            Tower tower = towers[index];

            float cooldown = tower.Cooldown - dt;
            if (cooldown <= 0)
                for (int i = 0; i < enemyPositions.Length; i++)
                    if (math.lengthsq(enemyPositions[i].Value - towerPositions[index].Value) <= tower.RangeSqrd)
                    {
                        // Shoot
                        cooldown = tower.ShootTime;

                        ShotSpawnData shotSpawn = new ShotSpawnData()
                        {
                            Position = new Position() { Value = towerPositions[index].Value + new float3(0, 0.3f, 0) },
                            Direction = math.normalizesafe(enemyPositions[i].Value - (towerPositions[index].Value + new float3(0, 0.3f, 0))),
                            Damage = tower.Damage,
                            Index = tower.Index
                        };

                        buffer.CreateEntity(index, shotArchtype);
                        buffer.SetComponent(index, shotSpawn);

                        break;
                    }

            tower.Cooldown = cooldown;

            towers[index] = tower;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        ComponentDataArray<Position> towerPositions = towerGroup.GetComponentDataArray<Position>();

        TowerShootJob job = new TowerShootJob()
        {
            dt = Time.deltaTime,
            towerPositions = towerPositions,
            towers = towerGroup.GetComponentDataArray<Tower>(),
            enemyPositions = enemyGroup.GetComponentDataArray<Position>(),
            buffer = shotSpawnBarrier.CreateCommandBuffer().ToConcurrent(),
            shotArchtype = Spawn.ShotArchtype
        };

        return job.Schedule(towerPositions.Length, 64, inputDeps);
    }
}
