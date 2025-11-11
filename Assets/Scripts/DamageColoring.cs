using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DamageColoring : NetworkBehaviour
{
    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public override void OnStartServer()
    {
        EventBus.Subscribe<PlayerDamagedEvent>(ChangeCharColor);
    }

    public override void OnStopServer()
    {
        EventBus.Unsubscribe<PlayerDamagedEvent>(ChangeCharColor);
    }

    private void ChangeCharColor(PlayerDamagedEvent evt)
    {

        if (evt.victimNetId == this.netId)
        {
            RpcFlashRed();
        }
    }
    
    [ClientRpc]
    private void RpcFlashRed()
    {
        StartCoroutine(ResetColor());
    }
    
    private IEnumerator ResetColor()
    {
       if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }

}
