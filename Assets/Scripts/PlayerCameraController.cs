using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerCameraController : NetworkBehaviour
{
    private Camera playerCam;

    void Start()
    {
        playerCam = GetComponentInChildren<Camera>();

        if (isLocalPlayer)
        {
            playerCam.gameObject.SetActive(true);
        }
        else
        {
            playerCam.gameObject.SetActive(false);
        }
    }
}
