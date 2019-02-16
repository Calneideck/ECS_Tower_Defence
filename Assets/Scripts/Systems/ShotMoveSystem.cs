using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

[UpdateAfter(typeof(EnemyMoveSystem))]
public class ShotMoveSystem : JobComponentSystem
{
    [BurstCompile]
    private struct ShotMoveJob : IJobProcessComponentData<Position, Shot>
    {
        public float dt;

        public void Execute(ref Position position, ref Shot shot)
        {
            position.Value += shot.Direction * shot.Speed * dt;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        ShotMoveJob moveJob = new ShotMoveJob()
        {
            dt = Time.deltaTime
        };

        return moveJob.Schedule(this, inputDeps);
    }
}