using UnityEngine;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;
using System.Collections;

public class Spawn : MonoBehaviour
{
    public BoxCollider spawnZone;
    public MeshInstanceRenderer enemyLook;
    public MeshInstanceRenderer shotLook;
    public GameObject roundTimerHolder;
    public TextMeshProUGUI txtTimer;
    public TextMeshProUGUI txtRound;

    [Header("Stats")]
    public float roundTime = 10;
    public float timeBetweenWaves = 0.8f;

    public static EntityArchetype ShotArchtype;
    public static MeshInstanceRenderer ShotLook;

    private EntityManager em;
    private EntityArchetype enemyType;
    private int round = 0;
    private bool roundStarted;
    private bool spawning;
    private float currentTimeLeft;

    private void Awake()
    {
        ShotLook = shotLook;
    }

    void Start()
    {
        em = World.Active.GetOrCreateManager<EntityManager>();
        enemyType = em.CreateArchetype(typeof(Position), typeof(Scale), typeof(Health), typeof(Enemy), typeof(Bounty), typeof(MeshInstanceRenderer));
        ShotArchtype = em.CreateArchetype(typeof(ShotSpawnData));

        currentTimeLeft = 10;
    }

    private void Update()
    {
        if (roundStarted)
            return;

        if (currentTimeLeft > 0)
        {
            currentTimeLeft -= Time.deltaTime;
            txtTimer.text = currentTimeLeft.ToString("F1");
        }

        if (currentTimeLeft <= 0)
        {
            // Start round
            txtRound.text = (++round).ToString();
            roundTimerHolder.SetActive(false);
            StartCoroutine(SpawnEnemies());
            roundStarted = true;
        }
    }

    IEnumerator SpawnEnemies()
    {
        spawning = true;
        int waveCount = round + 1;
        int amountPerWave = 5 + Mathf.CeilToInt(round * 4.5f);
        int health = 30 + (int)Mathf.Pow(round * 2, 3.2f) * 2;
        int bounty = Mathf.CeilToInt(health / 150f);

        while (waveCount-- > 0)
        {
            // Create entites
            NativeArray<Entity> entities = new NativeArray<Entity>(amountPerWave, Allocator.Temp);
            em.CreateEntity(enemyType, entities);

            // Set component data on each entity
            for (int i = 0; i < amountPerWave; i++)
            {
                float3 size = new float3(UnityEngine.Random.Range(0.2f, 0.6f));

                em.SetComponentData(entities[i], new Position()
                {
                    Value = new float3(UnityEngine.Random.Range(spawnZone.bounds.min.x, spawnZone.bounds.max.x), size.x * 0.5f,
                    UnityEngine.Random.Range(spawnZone.bounds.min.z, spawnZone.bounds.max.z))
                });
                em.SetComponentData(entities[i], new Scale() { Value = size });
                em.SetComponentData(entities[i], new Health() { Value = health });
                em.SetComponentData(entities[i], new Enemy() { Section = 0, MoveOffset = UnityEngine.Random.Range(-1, 1) });
                em.SetComponentData(entities[i], new Bounty() { Value = bounty });
                em.SetSharedComponentData(entities[i], enemyLook);
            }

            // Deallocate array memory
            entities.Dispose();

            yield return new WaitForSeconds(timeBetweenWaves);
        }

        spawning = false;
    }

    public bool RoundEnded()
    {
        if (!spawning && enabled)
        {
            if (round == 20)
                MainController.Instance.Win();
            else
            {
                currentTimeLeft = roundTime;
                roundTimerHolder.SetActive(true);
                roundStarted = false;
            }

            return true;
        }

        return false;
    }

    public void KillAll()
    {
        em.DestroyEntity(em.CreateComponentGroup(typeof(Enemy)));
    }

    public void Restart()
    {
        NativeArray<Entity> entities = em.GetAllEntities(Allocator.Temp);
        em.DestroyEntity(entities);
        entities.Dispose();
    }
}
