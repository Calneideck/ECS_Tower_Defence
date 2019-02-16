using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

public class EnemyMoveSystem : JobComponentSystem
{
    private const int SPEED = 4;
    private const float MAX_RANDOM_MOVE_AMOUNT = 2f;
    private const float MOVE_OFFSET_AMOUNT = 1;
    private const float TURN_PCT = 0.8f;

    private ComponentGroup enemyGroup;

    protected override void OnCreateManager()
    {
        enemyGroup = GetComponentGroup(typeof(Position), typeof(Enemy));
    }

    [BurstCompile]
    private struct EnemyMoveJob : IJobParallelFor
    {
        public float dt;
        public ComponentDataArray<Position> positions;
        public ComponentDataArray<Enemy> enemies;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<float> randomOffsets;

        public void Execute(int index)
        {
            float3 pos = positions[index].Value;

            float moveOffset = math.clamp(enemies[index].MoveOffset + randomOffsets[index] * dt, -1, 1);
            int section = enemies[index].Section;

            #region MoveSections
            if (section == 0)
            {
                if (Section0(ref pos, ref moveOffset, ref dt))
                    ++section;
            }
            else if (section == 1)
            {
                if (Section1(ref pos, ref moveOffset, ref dt))
                    ++section;
            }
            else if (section == 2)
            {
                if (Section2(ref pos, ref moveOffset, ref dt))
                    ++section;
            }
            else if (section == 3)
            {
                if (Section3(ref pos, ref moveOffset, ref dt))
                    ++section;
            }
            else if (section == 4)
            {
                if (Section4(ref pos, ref moveOffset, ref dt))
                    ++section;
            }
            else if (section == 5)
            {
                if (Section5(ref pos, ref moveOffset, ref dt))
                    ++section;
            }
            else if (section == 6)
            {
                if (Section6(ref pos, ref moveOffset, ref dt))
                {
                    // Done
                }
            }
            #endregion

            enemies[index] = new Enemy() { Section = section, MoveOffset = moveOffset };
            positions[index] = new Position() { Value = pos };
        }

        #region Sections
        bool Section0(ref float3 pos, ref float moveOffset, ref float dt)
        {
            pos.x += SPEED * dt;
            pos.z = math.clamp(pos.z + moveOffset * MOVE_OFFSET_AMOUNT * dt, -10.75f, -3.75f);
            float z = math.abs(-2.5f - pos.z) * TURN_PCT;
            return pos.x > 3.25f + z;
        }

        bool Section1(ref float3 pos, ref float moveOffset, ref float dt)
        {
            pos.z += SPEED * dt;
            pos.x = math.clamp(pos.x + moveOffset * MOVE_OFFSET_AMOUNT * dt, 3.25f, 10.75f);
            float x = math.abs(2.5f - pos.x) * TURN_PCT;
            return pos.z > 3.5f + x;
        }

        bool Section2(ref float3 pos, ref float moveOffset, ref float dt)
        {
            pos.x -= SPEED * dt;
            pos.z = math.clamp(pos.z + moveOffset * MOVE_OFFSET_AMOUNT * dt, 3.25f, 10.75f);
            float z = math.abs(11.5f - pos.z) * TURN_PCT;
            return pos.x < -17.5 - z;
        }

        bool Section3(ref float3 pos, ref float moveOffset, ref float dt)
        {
            pos.z += SPEED * dt;
            pos.x = math.clamp(pos.x + moveOffset * MOVE_OFFSET_AMOUNT * dt, -24.75f, -17.25f);
            float x = math.abs(-16.5f - pos.x) * TURN_PCT;
            return pos.z > 17.5f + x;
        }

        bool Section4(ref float3 pos, ref float moveOffset, ref float dt)
        {
            pos.x += SPEED * dt;
            pos.z = math.clamp(pos.z + moveOffset * MOVE_OFFSET_AMOUNT * dt, 17.25f, 24.75f);
            float z = math.abs(16.5f - pos.z) * TURN_PCT;
            return pos.x > 17.5 + z;
        }

        bool Section5(ref float3 pos, ref float moveOffset, ref float dt)
        {
            pos.z -= SPEED * dt;
            pos.x = math.clamp(pos.x + moveOffset * MOVE_OFFSET_AMOUNT * dt, 17.25f, 24.75f);
            float x = math.abs(16.5f - pos.x) * TURN_PCT;
            return pos.z < -17.5f - x;
        }

        bool Section6(ref float3 pos, ref float moveOffset, ref float dt)
        {
            pos.x -= SPEED * dt;
            pos.z = math.clamp(pos.z + moveOffset * MOVE_OFFSET_AMOUNT * dt, -24.75f, -17.25f);
            return pos.x < -26.5f;
        }
        #endregion
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        ComponentDataArray<Position> positions = enemyGroup.GetComponentDataArray<Position>();
        NativeArray<float> randomOffsets = new NativeArray<float>(positions.Length, Allocator.TempJob);

        for (int i = 0; i < randomOffsets.Length; i++)
            randomOffsets[i] = UnityEngine.Random.Range(-MAX_RANDOM_MOVE_AMOUNT, MAX_RANDOM_MOVE_AMOUNT);

        EnemyMoveJob moveJob = new EnemyMoveJob()
        {
            dt = Time.deltaTime,
            positions = positions,
            enemies = enemyGroup.GetComponentDataArray<Enemy>(),
            randomOffsets = randomOffsets
        };

        return moveJob.Schedule(positions.Length, 64, inputDeps);
    }
}