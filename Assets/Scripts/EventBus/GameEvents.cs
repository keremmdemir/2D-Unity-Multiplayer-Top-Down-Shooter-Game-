using Unity.VisualScripting;
using UnityEngine;

public struct PlayerFiredEvent
{
    public uint shooterNetId;
    public Vector2 startPoint;
    public Vector2 endPoint;
}

public enum ImpactEffectType
{
    None,
    Blood,
    Sparks,
    Dust,
    Smoke
}

public struct PlayerDamagedEvent
{
    public uint victimNetId;
    public int damageAmount;
}

public struct ServerImpactEvent
{
    public ImpactEffectType effectType;
    public Vector2 position;
    public Vector2 normal;
}

public struct PlayerDiedEvent { }
public struct PlayerRespawnedEvent { }
public struct PlayerReloadStartedEvent { }
public struct PlayerReloadFinishedEvent { }
public struct PlayerHitEvent { }
public struct PlayerFragEvent
{
    public uint victimNetId;
}

public struct AmmoChangedEvent
{
    public int newAmount;
}

public struct PlayerHealthChangedEvent
{
    public int newHealth;
    public int maxHealth;
}