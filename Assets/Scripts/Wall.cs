using Mirror;
using UnityEngine;

public class Wall : NetworkBehaviour, IShootable
{
    public void TakeDamage(int damage, Vector2 hitPosition, Vector2 hitNormal,uint killerNetId)
    {
        EventBus.Publish(new ServerImpactEvent
        {
            effectType = ImpactEffectType.Dust,
            position = hitPosition,
            normal = hitNormal
        });
        Debug.Log("duvar vuruldu");
    }
}