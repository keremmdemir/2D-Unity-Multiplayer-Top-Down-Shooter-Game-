using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CharacterLightManager : MonoBehaviour
{

    [SerializeField] private Light2D dimLight;
    [SerializeField] private Light2D visionLight;
    private bool canScaleLight = false;
    
    public void OnEnable()
    {
        EventBus.Subscribe<PlayerDiedEvent>(DeadLightProps);
        EventBus.Subscribe<PlayerRespawnedEvent>(AliveLightProps);
    }

    public void OnDisable()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(DeadLightProps);
        EventBus.Unsubscribe<PlayerRespawnedEvent>(AliveLightProps);
    }

    private void AliveLightProps(PlayerRespawnedEvent evt)
    {
        dimLight.color = new Color32(255, 255, 255, 255);
        visionLight.enabled = true;
        canScaleLight = false;
        dimLight.shapeLightFalloffSize = 0.1f;
    }

    private void DeadLightProps(PlayerDiedEvent evt)
    {
        dimLight.color = new Color32(255, 0, 0, 255);
        visionLight.enabled = false;
        canScaleLight = true;
    }
    private void Update()
    {
        if (!canScaleLight) return;
        dimLight.shapeLightFalloffSize = Mathf.Lerp(dimLight.shapeLightFalloffSize, 9f, Time.deltaTime * 7f);
    }
}
