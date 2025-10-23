using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLightControl : MonoBehaviour
{
    [Header("Light")]
    public Light playerLight;          
    public float onIntensity = 30f;   
    public float offIntensity = 0f;    
    public float range = 15f;           
    public Color color = new Color(1f, 0.94f, 0.78f); 
    public bool startOn = false;       
    public bool useFade = true;        
    public float fadeDuration = 0.25f; 

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.L; 

    bool isOn;
    Coroutine fadeCo;

    void Awake()
    {
        if (!playerLight) playerLight = GetComponentInChildren<Light>(true);
        if (!playerLight)
        {
            
            GameObject go = new GameObject("PlayerLight");
            go.transform.SetParent(transform, false);
            playerLight = go.AddComponent<Light>();
            playerLight.type = LightType.Point;
        }

        
        playerLight.type = LightType.Point;
        playerLight.range = 15f;
        playerLight.color = color;
        playerLight.shadows = LightShadows.None; 
    }

    void Start()
    {
        if (startOn) TurnOnImmediate();
        else TurnOffImmediate();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    
    public void Toggle()
    {
        if (isOn) TurnOff();
        else TurnOn();
    }

    public void TurnOn()
    {
        isOn = true;
        if (useFade) StartFadeTo(onIntensity);
        else ApplyIntensity(onIntensity);
    }

    public void TurnOff()
    {
        isOn = false;
        if (useFade) StartFadeTo(offIntensity);
        else ApplyIntensity(offIntensity);
    }

    public void TurnOnImmediate()
    {
        isOn = true;
        ApplyIntensity(onIntensity);
    }

    public void TurnOffImmediate()
    {
        isOn = false;
        ApplyIntensity(offIntensity);
    }
    

    void StartFadeTo(float target)
    {
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeIntensity(target, fadeDuration));
    }

    IEnumerator FadeIntensity(float target, float dur)
    {
        float start = playerLight.intensity;
        float t = 0f;
        
        if (!playerLight.enabled) playerLight.enabled = true;

        while (t < dur)
        {
            t += Time.deltaTime;
            playerLight.intensity = Mathf.Lerp(start, target, t / dur);
            yield return null;
        }

        playerLight.intensity = target;
        
        playerLight.enabled = playerLight.intensity > 0.001f;
    }

    void ApplyIntensity(float v)
    {
        playerLight.intensity = v;
        playerLight.enabled = v > 0.001f;
    }
}
