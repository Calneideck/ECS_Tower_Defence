using Unity.Entities;
using Unity.Transforms;

[AlwaysUpdateSystem]
public class RemoveEnemiesSystem : ComponentSystem
{
    private ComponentGroup enemies;
    private bool started;

    protected override void OnCreateManager()
    {
        enemies = GetComponentGroup(typeof(Position), typeof(Health), typeof(Bounty));
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer buffer = PostUpdateCommands;
        EntityArray entities = enemies.GetEntityArray();
        int length = entities.Length;

        if (length > 0 && !started)
            started = true;

        if (length <= 0 && started)
            if (MainController.Instance.RoundEnded())
                started = false;

        ComponentDataArray<Health> healths = enemies.GetComponentDataArray<Health>();
        ComponentDataArray<Bounty> bounties = enemies.GetComponentDataArray<Bounty>();
        ComponentDataArray<Position> positions = enemies.GetComponentDataArray<Position>();

        for (int i = 0; i < length; i++)
            if (healths[i].Value <= 0)
            {
                MainController.Instance.Money += bounties[i].Value;
                EnemyDeathParticles.Instance.SpawnParticles(positions[i].Value);
                buffer.DestroyEntity(entities[i]);
            }
            else if (positions[i].Value.x < -26 && positions[i].Value.z < -16)
            {
                buffer.DestroyEntity(entities[i]);
                MainController.Instance.LifeLost();
            }
    }
}
