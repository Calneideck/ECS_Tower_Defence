using UnityEngine;

[CreateAssetMenu(fileName = "Tower", menuName = "Tower")]
public class TowerData : ScriptableObject
{
    public int cost;
    public Material material;
    public int damage;
    public float shootTime;
    public float range;
    public float aoe;
}
