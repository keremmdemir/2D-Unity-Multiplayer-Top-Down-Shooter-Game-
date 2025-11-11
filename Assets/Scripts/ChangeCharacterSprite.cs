using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ChangeCharacterSprite : NetworkBehaviour
{
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    [SerializeField] private Sprite skullSprite;
    [SerializeField] private Sprite initCharacterSprite;

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        EventBus.Subscribe<PlayerDiedEvent>(ChangeDeadSprite);
        EventBus.Subscribe<PlayerRespawnedEvent>(ChangeSpriteToCharacter);
        

    }

    public override void OnStopLocalPlayer()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(ChangeDeadSprite);
        EventBus.Unsubscribe<PlayerRespawnedEvent>(ChangeSpriteToCharacter);
    }

    [Command]
    private void ChangeSpriteToCharacter(PlayerRespawnedEvent evt)
    {
        RpcChangeSpriteToCharacter();
    }

    [ClientRpc]
    private void RpcChangeSpriteToCharacter()
    {
        spriteRenderer.sprite = initCharacterSprite;
        circleCollider.enabled = true;
    }

    [Command]
    private void ChangeDeadSprite(PlayerDiedEvent evt)
    {
        RpcChangeSprite();
    }

    [ClientRpc]
    private void RpcChangeSprite()
    {
        spriteRenderer.sprite = skullSprite;
        circleCollider.enabled = false;
    }
}
