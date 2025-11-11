using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.Mathematics;

public class CrosshairController : MonoBehaviour
{
    private RectTransform crosshairRect;
    [SerializeField] RectTransform hitCrosshairRect;
    private Image currentCrossImage;
    [SerializeField] private CanvasGroup hitProjectileCanvasGroup;
    [SerializeField] private Sprite reloadingCrosshairSprite;
    [SerializeField] private Sprite currentCrosshairSprite;

    private void Awake()
    {
        crosshairRect = GetComponent<RectTransform>();
        currentCrossImage = GetComponent<Image>();
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector2 mousePosition = Input.mousePosition;
        crosshairRect.position = mousePosition;
    }

    public void OnEnable()
    {
        EventBus.Subscribe<PlayerReloadStartedEvent>(OnReloadStarted);
        EventBus.Subscribe<PlayerReloadFinishedEvent>(OnReloadFinished);
        EventBus.Subscribe<PlayerHitEvent>(OnPlayerHit);
    }
    public void OnDisable()
    {
        EventBus.Unsubscribe<PlayerReloadStartedEvent>(OnReloadStarted);
        EventBus.Unsubscribe<PlayerReloadFinishedEvent>(OnReloadFinished);
        EventBus.Unsubscribe<PlayerHitEvent>(OnPlayerHit);
    }

    private void OnReloadStarted(PlayerReloadStartedEvent evt)
    {
        currentCrossImage.sprite = reloadingCrosshairSprite;
        crosshairRect.DORotate(new Vector3(0,0,-3600),2,RotateMode.FastBeyond360).SetEase(Ease.Linear);
    }

    private void OnReloadFinished(PlayerReloadFinishedEvent evt)
    {
        currentCrossImage.sprite = currentCrosshairSprite;
    }
    
    private void OnPlayerHit(PlayerHitEvent evt)
    {

        hitCrosshairRect.DOKill();
        hitCrosshairRect.localScale = Vector3.one;


        hitCrosshairRect.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.2f, 2, 0.5f);

        hitProjectileCanvasGroup.DOKill();
        hitProjectileCanvasGroup.alpha = 1f;
        hitProjectileCanvasGroup.DOFade(0, 0.1f).SetDelay(0.2f);
        
    }

}
