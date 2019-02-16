using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using System.Collections.Generic;

public class TowerBuilder : MonoBehaviour
{
    public GameObject towerPrefab;
    public TowerData[] towerDatas;
    public TextMeshProUGUI[] towerCosts;
    public Button[] towerButtons;

    public LayerMask buildingMask;
    public LayerMask towerMask;
    public Mesh cubeMesh;
    public MeshInstanceRenderer towerBaseLook;
    public Color goodColour;
    public Color badColour;

    private GameObject currentTower = null;
    private Renderer currentTowerRenderer;
    private int currentTowerIndex;

    private EntityManager em;
    private EntityArchetype towerType;
    private EntityArchetype towerBaseType;
    private Dictionary<Vector2Int, Entity> entityPositions = new Dictionary<Vector2Int, Entity>();

    private void Awake()
    {
        for (int i = 0; i < towerDatas.Length; i++)
            towerCosts[i].text = towerDatas[i].cost.ToString("N0") + " $";
    }

    private void Start()
    {
        em = World.Active.GetOrCreateManager<EntityManager>();
        towerType = em.CreateArchetype(typeof(Position), typeof(Scale), typeof(Tower), typeof(Parent), typeof(MeshInstanceRenderer));
        towerBaseType = em.CreateArchetype(typeof(Position), typeof(Rotation), typeof(MeshInstanceRenderer));

        for (int i = 0; i < towerButtons.Length; i++)
        {
            int index = i;
            towerButtons[index].onClick.AddListener(() =>
            {
                if (MainController.Instance.Money >= towerDatas[index].cost)
                    BuildTower(index);
            });
        }
    }

    void Update()
    {
        // Get tower (hotkeys)
        for (int i = 1; i <= 6; i++)
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                if (MainController.Instance.Money >= towerDatas[i - 1].cost)
                    BuildTower(i - 1);

                break;
            }

        if (currentTower != null)
            MoveTower();
    }

    void MoveTower()
    {
        Vector3 pos = GetMousePositionOnXZPlane();
        pos.x = Mathf.Round(pos.x);
        pos.z = Mathf.Round(pos.z);
        currentTower.transform.position = pos;

        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
        bool isGood = !entityPositions.ContainsKey(gridPos);

        if (isGood)
        {
            // Check if tower is on one of the walls
            Collider[] colliders = Physics.OverlapBox(pos + Vector3.up * 0.5f, Vector3.one * 0.5f, Quaternion.identity, buildingMask);
            isGood = colliders.Length > 0;
        }

        currentTowerRenderer.material.color = isGood ? goodColour : badColour;

        if (Input.GetMouseButton(0))
        {
            if (isGood)
            {
                if (MainController.Instance.Money >= towerDatas[currentTowerIndex].cost && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    PlaceTower(pos);

                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    Destroy(currentTower);
                    currentTower = null;
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Destroy(currentTower);
            currentTower = null;
        }
    }

    public void BuildTower(int index)
    {
        if (currentTower != null)
            Destroy(currentTower);

        currentTower = Instantiate(towerPrefab);
        currentTowerRenderer = currentTower.transform.Find("Base").GetComponent<Renderer>();
        currentTower.transform.Find("Tower").GetComponent<Renderer>().material = towerDatas[index].material;
        currentTowerIndex = index;
        currentTowerRenderer.material.color = badColour;
    }

    void PlaceTower(Vector3 pos)
    {
        pos.y = 1;
        // Create tower and base entities
        Entity towerBaseEntity = em.CreateEntity(towerBaseType);
        em.SetComponentData(towerBaseEntity, new Position() { Value = (float3)pos + new float3(0, 0.01f, 0) });
        em.SetComponentData(towerBaseEntity, new Rotation() { Value = quaternion.EulerXYZ(math.radians(90), 0, 0) });
        em.SetSharedComponentData(towerBaseEntity, towerBaseLook);

        Entity towerEntity = em.CreateEntity(towerType);
        em.SetComponentData(towerEntity, new Position() { Value = (float3)pos + new float3(0, 0.3f, 0) });
        em.SetComponentData(towerEntity, new Scale() { Value = new float3(0.25f, 0.6f, 0.25f) });
        em.SetComponentData(towerEntity, new Tower() { Damage = towerDatas[currentTowerIndex].damage, ShootTime = towerDatas[currentTowerIndex].shootTime,
            Cooldown = towerDatas[currentTowerIndex].shootTime, RangeSqrd = Mathf.Pow(towerDatas[currentTowerIndex].range, 2), Index = currentTowerIndex });
        em.SetComponentData(towerEntity, new Parent() { Child = towerBaseEntity });
        em.SetSharedComponentData(towerEntity, new MeshInstanceRenderer() { mesh = cubeMesh, material = towerDatas[currentTowerIndex].material });

        entityPositions.Add(new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z)), towerEntity);
        MainController.Instance.Money -= towerDatas[currentTowerIndex].cost;
    }

    public void Clear()
    {
        if (currentTower)
            Destroy(currentTower);
    }

    Vector3 GetMousePositionOnXZPlane()
    {
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane XZPlane = new Plane(Vector3.up, Vector3.up * 1.05f);
        if (XZPlane.Raycast(ray, out distance))
            return ray.GetPoint(distance);

        return Vector3.up;
    }
}
