using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

public class ShotRemoveBarrier : BarrierSystem
{ }


[UpdateAfter(typeof(EnemyMoveSystem))]
public class ShotDamageSystem : JobComponentSystem
{
    private const float SHOT_DIST = 3 * 3;

    ComponentGroup enemyGroup;

    [Inject]
    private ShotRemoveBarrier shotRemoveBarrier;

    protected override void OnCreateManager()
    {
        enemyGroup = GetComponentGroup(typeof(Position), typeof(Health));
    }

    [BurstCompile]
    private struct DamageJob : IJobProcessComponentDataWithEntity<Position, Shot>
    {
        [ReadOnly]
        public ComponentDataArray<Position> EnemyPositions;

        public ComponentDataArray<Health> EnemyHealths;

        [ReadOnly]
        public EntityArray EnemyEntities;

        [NativeDisableParallelForRestriction]
        public EntityCommandBuffer buffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref Position position, [ReadOnly] ref Shot shot)
        {
            if (position.Value.y <= 0)
            {
                for (int i = 0; i < EnemyPositions.Length; i++)
                    if (math.lengthsq(EnemyPositions[i].Value - position.Value) < SHOT_DIST)
                        EnemyHealths[i] = new Health() { Value = EnemyHealths[i].Value - shot.Damage };

                buffer.DestroyEntity(entity);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        DamageJob damageJob = new DamageJob()
        {
            EnemyPositions = enemyGroup.GetComponentDataArray<Position>(),
            EnemyHealths = enemyGroup.GetComponentDataArray<Health>(),
            EnemyEntities = enemyGroup.GetEntityArray(),
            buffer = shotRemoveBarrier.CreateCommandBuffer()
        };

        return damageJob.ScheduleSingle(this, inputDeps);
    }
}