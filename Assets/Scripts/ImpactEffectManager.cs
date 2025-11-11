using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class ImpactEffectManager : NetworkBehaviour
{
    public GameObject bloodEffectPrefab;
    public GameObject dustEffectPrefab;
    public GameObject smokeEffectPrefab;


    public override void OnStartServer()
    {
        base.OnStartServer();
        EventBus.Subscribe<ServerImpactEvent>(OnServerImpact);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        EventBus.Unsubscribe<ServerImpactEvent>(OnServerImpact);
    }

    [Server]
    private void OnServerImpact(ServerImpactEvent evt)
    {
        RpcSpawnImpactEffect(evt.effectType, evt.position, evt.normal);
    }

    [ClientRpc]
    private void RpcSpawnImpactEffect(ImpactEffectType type, Vector2 position, Vector2 normal)
    {
        GameObject prefabToSpawn = null;

        switch (type)
        {
            case ImpactEffectType.Blood:
                prefabToSpawn = bloodEffectPrefab;
                break;
            case ImpactEffectType.Dust:
                prefabToSpawn = dustEffectPrefab;
                break;
            case ImpactEffectType.Smoke:
                prefabToSpawn = smokeEffectPrefab;
                break;
        }

        Debug.Log("kod spawn edildi");

        if (prefabToSpawn != null)
        {
            
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, normal);

           
            GameObject effect = Instantiate(prefabToSpawn, position, rotation);

            Destroy(effect, 1f);
        }
    }
}