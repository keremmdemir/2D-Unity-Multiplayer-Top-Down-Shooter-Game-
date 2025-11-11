using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
{
    [Header("Shooting")]
    public Transform pivotDirection;
    public float weaponRange = 10f;
    public LayerMask hitMask;
    public int damage = 2;
    private float fireTimer = 0;
    private float fireCoolDown = 0.15f;
    private bool canShoot = true;
    public int maxAmmo = 20;

    [SyncVar(hook = nameof(OnAmmoChanged))]
    private int currentAmmo;

    [SyncVar(hook = nameof(OnReloadStateChanged))] 
    private bool isReloading = false;

    public override void OnStartServer()
    {
        base.OnStartServer();
        currentAmmo = maxAmmo;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        EventBus.Subscribe<PlayerDiedEvent>(PreventionShoothing);
        EventBus.Subscribe<PlayerRespawnedEvent>(ReleaseShootingEvent);
    }

    public override void OnStopLocalPlayer()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(PreventionShoothing);
        EventBus.Unsubscribe<PlayerRespawnedEvent>(ReleaseShootingEvent);
    }

    private void PreventionShoothing(PlayerDiedEvent evt)
    {
        canShoot = false;
    }

    private void ReleaseShootingEvent(PlayerRespawnedEvent evt)
    {
        canShoot = true;
        ReloadAmmoInRespawned();
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (canShoot) PlayerShooting();

    }

    private void PlayerShooting()
    {
        fireTimer += Time.deltaTime;

        if (Input.GetButton("Fire1") && fireTimer >= fireCoolDown)
        {
            if (isReloading) return;


            Vector2 direction = pivotDirection.right;
            direction.Normalize();
            Vector2 origin = (Vector2)pivotDirection.position + direction * 0.1f;
            if (currentAmmo > 0) { CmdShoot(origin, direction); }
            fireTimer = 0f;

        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isReloading)
            {
                CmdReload();
            }
        }
    }

    [Command]
    private void ReloadAmmoInRespawned()
    {
        currentAmmo = maxAmmo;
    }

    [Command]
    void CmdShoot(Vector2 origin, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, weaponRange, hitMask);

        Vector2 endPoint;

        if (hit.collider != null)
        {

            endPoint = hit.point;

            GameObject myRoot = transform.root.gameObject;
            GameObject hitRoot = hit.collider.transform.root.gameObject;

            if (hitRoot != myRoot)
            {
                IShootable playerTarget = hitRoot.GetComponent<IShootable>();
                IShootable propTarget = hit.collider.gameObject.GetComponent<IShootable>();

                if (playerTarget != null)
                {
                    playerTarget.TakeDamage(damage, endPoint, hit.normal, this.netId);
                }

                if (propTarget != null)
                {
                    propTarget.TakeDamage(damage, endPoint, hit.normal, this.netId);
                }
            }
        }
        else
        {
            endPoint = origin + direction * weaponRange;
        }

        if (isReloading || currentAmmo <= 0)
        {
            return;
        }

        currentAmmo--;

        RpcPublishFireEvent(origin, endPoint);

    }

    [ClientRpc]
    private void RpcPublishFireEvent(Vector2 start,Vector2 end)
    {

        EventBus.Publish(new PlayerFiredEvent
        {
            shooterNetId = this.netId,
            startPoint = start,
            endPoint = end
        });
    }

    [Command]
    void CmdReload()
    {
        if (isReloading) return;
        StartCoroutine(ReloadRoutine());

    }

    [Server]
    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(2.0f);
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    private void OnReloadStateChanged(bool oldState, bool newState)
    {
        if (!isLocalPlayer || oldState == newState) return;

        if(newState == true)
        {
            EventBus.Publish(new PlayerReloadStartedEvent());
        }
        else
        {
            EventBus.Publish(new PlayerReloadFinishedEvent());
        }
    }

    private void OnAmmoChanged(int oldAmmo,int newAmmo)
    {
        if (isLocalPlayer)
        {
            EventBus.Publish(new AmmoChangedEvent { newAmount = newAmmo });
        }
    }
}

