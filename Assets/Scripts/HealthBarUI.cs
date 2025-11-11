using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private CanvasGroup healthBarCanvas;

    void Awake()
    {
        // Referansları al
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();
        
        if (healthBarCanvas == null)
            healthBarCanvas = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        EventBus.Subscribe<PlayerHealthChangedEvent>(OnHealthChanged);
        EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Subscribe<PlayerRespawnedEvent>(OnPlayerRespawned);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnHealthChanged);
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Unsubscribe<PlayerRespawnedEvent>(OnPlayerRespawned);
    }

    private void OnHealthChanged(PlayerHealthChangedEvent evt)
    {
        if (healthSlider.maxValue != evt.maxHealth)
        {
            healthSlider.maxValue = evt.maxHealth;
        }
        healthSlider.value = evt.newHealth;
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        healthBarCanvas.alpha = 0;
    }

    private void OnPlayerRespawned(PlayerRespawnedEvent evt)
    {
        healthBarCanvas.alpha = 1;
        healthSlider.value = healthSlider.maxValue;
    }
}
