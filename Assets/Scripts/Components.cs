using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

struct Health : IComponentData
{
    public int Value;
}

struct Bounty : IComponentData
{
    public int Value;
}

struct Enemy : IComponentData
{
    public int Section;
    public float MoveOffset;
}

struct Tower : IComponentData
{
    public int Damage;
    public float ShootTime;
    public float Cooldown;
    public float RangeSqrd;
    public int Index;
}

struct Parent : IComponentData
{
    public Entity Child;
}

public struct ShotSpawnData : IComponentData
{
    public Position Position;
    public float3 Direction;
    public int Damage;
    public int Index;
}

public struct Shot : IComponentData
{
    public int Damage;
    public float Aoe;
    public float3 Direction;
    public float Speed;
}