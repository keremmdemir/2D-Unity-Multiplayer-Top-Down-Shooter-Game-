using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Mirror;
using Mirror.Examples.Basic;
using UnityEngine;

public enum PlayerState{
    Dead,
    Alive
}

public class PlayerHealthManager : NetworkBehaviour, IShootable
{

    [SyncVar(hook = nameof(OnStateChanged))] public PlayerState currentState = PlayerState.Alive;
    [SyncVar(hook = nameof(OnHealthChanged))] public int health;

    [SyncVar] public int maxHealth = 100;


    public override void OnStartServer()
    {
        base.OnStartServer();
        health = maxHealth;
        currentState = PlayerState.Alive;
    }

    private void OnHealthChanged(int oldHealth,int newHealth)
    {
        if (isLocalPlayer)
        {
            EventBus.Publish(new PlayerHealthChangedEvent
            {
                newHealth = newHealth,
                maxHealth = this.maxHealth
            });
        }
    }
    public void TakeDamage(int amount, Vector2 endPosition, Vector2 normal, uint killerNetId)
    {
        CmdApplyDamage(amount, endPosition, normal, killerNetId);

    }

    private void OnStateChanged(PlayerState oldState, PlayerState newState)
    {
        if (oldState == newState) return;

        if (isLocalPlayer)
        {
            if (newState == PlayerState.Dead)
            {
                EventBus.Publish(new PlayerDiedEvent());
            }
            if (newState == PlayerState.Alive)
            {
                EventBus.Publish(new PlayerRespawnedEvent());
            }
        }

    }

    [Server]
    private void CmdApplyDamage(int damageValue, Vector2 hitPosition, Vector2 hitNormal, uint killerNetId)
    {

        if (currentState == PlayerState.Dead) return;

        Debug.Log(health);
        health -= damageValue;

        EventBus.Publish(new PlayerDamagedEvent
        {
            victimNetId = this.netId,
            damageAmount = damageValue
        });

        EventBus.Publish(new ServerImpactEvent
        {
            effectType = ImpactEffectType.Blood,
            position = hitPosition,
            normal = hitNormal
        });

        if (killerNetId != this.netId)
        {
            if (NetworkServer.spawned.TryGetValue(killerNetId, out NetworkIdentity killerIdentity))
            {
                RpcConfirmHit(killerIdentity.connectionToClient);
            }
        }

        if (health <= 0)
        {
            health = 0;
            currentState = PlayerState.Dead;
            StartCoroutine(RespawnRoutine());

            if (killerNetId != this.netId)
            {
                if (NetworkServer.spawned.TryGetValue(killerNetId, out NetworkIdentity killerIdentity))
                {
                    PlayerHealthManager killerHealth = killerIdentity.GetComponent<PlayerHealthManager>();
                    if (killerHealth != null)
                    {
                        killerHealth.HealToFull();
                        RpcShowFragSplash(killerIdentity.connectionToClient);
                    }
                }
            }
        }
    }

    [TargetRpc]
    private void RpcConfirmHit(NetworkConnection target)
    {
        EventBus.Publish(new PlayerHitEvent());
    }
    
    [TargetRpc]
    private void RpcShowFragSplash(NetworkConnection target)
    {
        EventBus.Publish(new PlayerFragEvent());
    }

    [Server]
    public void HealToFull()
    {
        if (currentState == PlayerState.Dead) return;
        health = 100; 
    }

    [Server]
    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(3f);
        Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
        Vector3 newPosition = Vector3.zero;
        if (spawnPoint != null) { newPosition = spawnPoint.position; }
        health = 100;
        currentState = PlayerState.Alive;
        TargetRpcRespawn(this.connectionToClient, newPosition);
    }
    
    [TargetRpc]
    private void TargetRpcRespawn(NetworkConnection target, Vector3 position)
    {
        transform.position = position;
    }
}
