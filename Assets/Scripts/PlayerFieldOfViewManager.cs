using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerFieldOfViewManager : NetworkBehaviour
{
    [SerializeField] private GameObject visionLight;
    [SerializeField] private GameObject dimLight;

    private void Update()
    {
        if (isLocalPlayer)
        {
            visionLight.SetActive(true);
            dimLight.SetActive(true);
        }
        else if (!isLocalPlayer)
        {
            visionLight.SetActive(false);
            dimLight.SetActive(false);
        }
    }
}
