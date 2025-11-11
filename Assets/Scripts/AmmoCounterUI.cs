using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AmmoCounterUI : MonoBehaviour
{
    private TextMeshProUGUI ammoText;

    private void Awake()
    {
        ammoText = GetComponent<TextMeshProUGUI>();
    }
    void OnEnable()
    {
        EventBus.Subscribe<AmmoChangedEvent>(OnAmmoChanged);
        EventBus.Subscribe<PlayerReloadStartedEvent>(OnReloadStarted);
        EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Subscribe<PlayerRespawnedEvent>(OnPlayerRespawned);
        

    }
    void OnDisable()
    {
        EventBus.Unsubscribe<AmmoChangedEvent>(OnAmmoChanged);
        EventBus.Unsubscribe<PlayerReloadStartedEvent>(OnReloadStarted);
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Unsubscribe<PlayerRespawnedEvent>(OnPlayerRespawned);
    }

    private void OnReloadStarted(PlayerReloadStartedEvent evt)
    {
        ammoText.text = "RELOADING..";
    }

    private void OnAmmoChanged(AmmoChangedEvent evt)
    {
        ammoText.text = $"{evt.newAmount.ToString()}/25";
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        ammoText.text = "";
    }
    
    private void OnPlayerRespawned(PlayerRespawnedEvent evt)
    {
        ammoText.text = "25/25";
    }
    
}
