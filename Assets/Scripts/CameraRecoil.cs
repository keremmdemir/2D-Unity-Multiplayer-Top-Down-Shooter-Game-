using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.Basic;
using UnityEngine;

public class CameraRecoil : NetworkBehaviour
{
    private AimSway aimSway;
    private Vector3 recoilOffset;
    private Vector3 recoilTarget;
    private float recoilForce = 4f;
    private float recoilReturnSpeed = 70f;

    private void Awake()
    {
        aimSway = GetComponent<AimSway>();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerFiredEvent>(OnShoot);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerFiredEvent>(OnShoot);
    }

    private void OnShoot(PlayerFiredEvent evnt)
    {
        if(evnt.shooterNetId == this.netId)
        {
            Vector2 recoilDir = -aimSway.dir.normalized;
            recoilTarget = new Vector3(recoilDir.x, recoilDir.y, 0) * recoilForce;    
        }
        
    }

    private void Update()
    {
        recoilOffset = Vector3.Lerp(recoilOffset, recoilTarget, Time.deltaTime * 30f);
        recoilTarget = Vector3.Lerp(recoilTarget, Vector3.zero, Time.deltaTime * recoilReturnSpeed);

        aimSway.additionalOffset = recoilOffset;
    }
}
