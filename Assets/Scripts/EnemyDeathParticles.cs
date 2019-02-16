using UnityEngine;

public class EnemyDeathParticles : MonoBehaviour
{
    public GameObject particlesPrefab;
    public ObjectPooler objectPool;

    public static EnemyDeathParticles Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogError("Duplicate EnemyDeathParticles on " + name);
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        objectPool.Setup(particlesPrefab);
    }

    public void SpawnParticles(Vector3 pos)
    {
        GameObject particles = objectPool.GetObject(particlesPrefab);
        pos.y = 0.4f;
        particles.transform.position = pos;
        particles.GetComponent<ParticleSystem>().Play();
    }
}
