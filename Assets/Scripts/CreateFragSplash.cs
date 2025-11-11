using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

public class CreateFragSplash : NetworkBehaviour
{

    private CanvasGroup splashSprite;

    private void Awake()
    {
        splashSprite = GetComponent<CanvasGroup>();
    }
    public void OnEnable()
    {
        EventBus.Subscribe<PlayerFragEvent>(FadeFragSplash);
    }

    public void OnDisable()
    {
        EventBus.Unsubscribe<PlayerFragEvent>(FadeFragSplash);

    }
    
    private void FadeFragSplash(PlayerFragEvent evt)
    {
        splashSprite.alpha = 1;
        splashSprite.DOFade(0, 0.35f);
    }
}
