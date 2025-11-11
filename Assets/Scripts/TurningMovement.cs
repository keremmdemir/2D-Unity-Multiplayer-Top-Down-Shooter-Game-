using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using UnityEngine;

public class TurningMovement : NetworkBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject directionCenter;
    [SerializeField] private float lerpDamp = 15; 
    
    [SerializeField] private float syInterval = 0.1f; 
    private float syncTimer = 0f;
   

    [SyncVar] private float syncedRotation;
    private bool canTurn = true;
    private float defaultLerpDamp; 


    
    private void Start()
    {
        defaultLerpDamp = lerpDamp;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        EventBus.Subscribe<PlayerDiedEvent>(PreventionRotating);
        EventBus.Subscribe<PlayerRespawnedEvent>(ReleaseRotating);
    }

    public override void OnStopLocalPlayer()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(PreventionRotating);
        EventBus.Unsubscribe<PlayerRespawnedEvent>(ReleaseRotating);
    }

    private void PreventionRotating(PlayerDiedEvent evt)
    {
       
        lerpDamp = 100f; 
        canTurn = false;
        transform.rotation = quaternion.Euler(0, 0, 0);
        CmdSendRotation(0);
    }

    private void ReleaseRotating(PlayerRespawnedEvent evt)
    {
        
        lerpDamp = defaultLerpDamp; 
        canTurn = true;
    }
    
    void Update()
    {
        if (isLocalPlayer)
        {
            
            if (canTurn)
            {
                HandleLocalRotation(); 
            }
            
            HandleNetworkThrottling();
        }
        else
        {
            
            RotateClientCharacter();
        }
    }
    
    private void HandleLocalRotation() 
    {

        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, targetAngle);
    }
    
    private void HandleNetworkThrottling()
    {
       
        syncTimer += Time.deltaTime;

       
        if (syncTimer >= syInterval)
        {
            
            if (canTurn)
            {
                CmdSendRotation(transform.rotation.eulerAngles.z);
            }
            
           
            syncTimer = 0f;
        }
    }
   

    private void RotateClientCharacter()
    {
        if (!isLocalPlayer)
        {
           
            float angle = Mathf.LerpAngle(transform.rotation.eulerAngles.z, syncedRotation, Time.deltaTime * lerpDamp);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    [Command(channel = Channels.Unreliable)]
    void CmdSendRotation(float rot)
    {
        syncedRotation = rot;
    }
}