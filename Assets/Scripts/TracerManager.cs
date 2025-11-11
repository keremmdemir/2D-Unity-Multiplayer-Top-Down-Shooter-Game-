using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracerManager : MonoBehaviour
{
    public LineRenderer tracerPrefab; 
    public float tracerDuration = 0.05f; 
    public int poolSize = 100; 

    private Queue<LineRenderer> tracerPool;
    
    private int progressPropertyID; 

    void Start()
    {
        progressPropertyID = Shader.PropertyToID("_Progress");
        
        tracerPool = new Queue<LineRenderer>();
        for (int i = 0; i < poolSize; i++)
        {
            LineRenderer tracer = Instantiate(tracerPrefab, transform);
            tracer.gameObject.SetActive(false);
            tracerPool.Enqueue(tracer);
        }

        EventBus.Subscribe<PlayerFiredEvent>(OnPlayerFired);
    }

    void OnDestroy()
    {
        EventBus.Unsubscribe<PlayerFiredEvent>(OnPlayerFired);
    }

    private void OnPlayerFired(PlayerFiredEvent evt)
    {
        if (tracerPool.Count == 0)
        {
            LineRenderer newTracer = Instantiate(tracerPrefab, transform);
            tracerPool.Enqueue(newTracer);
        }

        LineRenderer tracer = tracerPool.Dequeue();
        
        tracer.SetPosition(0, new Vector3(evt.startPoint.x, evt.startPoint.y, 0));
        tracer.SetPosition(1, new Vector3(evt.endPoint.x, evt.endPoint.y, 0));
        
        tracer.gameObject.SetActive(true);
        
        StartCoroutine(ShowAndFadeTracer(tracer));
    }

    private IEnumerator ShowAndFadeTracer(LineRenderer tracer)
    {
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        
        float timer = 0f;
        
        while (timer < tracerDuration)
        {
            float progress = timer / tracerDuration; 
            
            propBlock.SetFloat(progressPropertyID, progress);
            tracer.SetPropertyBlock(propBlock);
            
            timer += Time.deltaTime;
            yield return null; 
        }
        
        tracer.gameObject.SetActive(false);
        tracerPool.Enqueue(tracer);
    }
}