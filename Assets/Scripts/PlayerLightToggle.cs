using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLightToggle : MonoBehaviour
{
    public Light playerLight;   

    void Start()
    {
        
        if (playerLight != null)
            playerLight.enabled = false;
    }

    public void TurnOn()
    {
        if (playerLight != null)
            playerLight.enabled = true;
    }

    public void TurnOff()
    {
        if (playerLight != null)
            playerLight.enabled = false;
    }
}
