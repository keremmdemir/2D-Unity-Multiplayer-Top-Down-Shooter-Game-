using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MuzzleEffect : NetworkBehaviour
{

    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void OnStartClient()
    {
        EventBus.Subscribe<PlayerFiredEvent>(CreateMuzzleEffect);
    }

    public override void OnStopClient()
    {
        EventBus.Unsubscribe<PlayerFiredEvent>(CreateMuzzleEffect);
    }

    private void CreateMuzzleEffect(PlayerFiredEvent evt)
    {
        if (evt.shooterNetId == this.netId)
        {
            Debug.Log("karakter ateş etti");
            animator.SetTrigger("shoot");
        }
    }
}
