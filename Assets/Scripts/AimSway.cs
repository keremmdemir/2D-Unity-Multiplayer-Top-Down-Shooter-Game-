using System;
using System.Collections;
using System.Collections.Generic;
using Mirror.Examples.Basic;
using Mirror.Examples.Common;
using Unity.VisualScripting;
using UnityEngine;

public class AimSway : MonoBehaviour
{
    [SerializeField] Camera playerCamera;
    [SerializeField] GameObject player;
    [SerializeField] private float swayDamp = 0.65f;
    public Vector2 dir;

    [HideInInspector] public Vector3 additionalOffset;
    private bool CanSway = true;


    public void OnEnable()
    {
        EventBus.Subscribe<PlayerDiedEvent>(PreventionSwaying);
        EventBus.Subscribe<PlayerRespawnedEvent>(ReleaseSwaying);
    
    }

    public void OnDisable()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(PreventionSwaying);
        EventBus.Unsubscribe<PlayerRespawnedEvent>(ReleaseSwaying);
    }

    private void ReleaseSwaying(PlayerRespawnedEvent evt)
    {
        CanSway = true;
    }

    private void PreventionSwaying(PlayerDiedEvent evt)
    {
        CanSway = false;
    }

    private void Awake()
    {
        playerCamera = GetComponent<Camera>();
    }
    private void Update()
    {
        if (CanSway)
        {
            SwayCamera();
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0,0,transform.localPosition.z), 3f * Time.deltaTime);
        }
            
    }

    private void SwayCamera()
    {
        Vector2 mousePos = playerCamera.ScreenToWorldPoint(Input.mousePosition);
        dir = (mousePos - (Vector2)player.transform.position);
        Vector3 clampedDir = Vector3.ClampMagnitude(dir, 7);
        Vector3 swayCamPos = new Vector3(clampedDir.x * swayDamp, clampedDir.y * swayDamp, transform.localPosition.z);

        Vector3 targetPos = swayCamPos + additionalOffset;

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, 5f * Time.deltaTime);
    }
}
