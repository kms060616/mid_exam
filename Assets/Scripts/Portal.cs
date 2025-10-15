using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Portal : MonoBehaviour
{
    public Transform target;              
    public Transform exitPoint;           
    public GameObject worldRoot;          
    public GameObject dungeonRoot;        

    
    public CanvasGroup fadeCanvas;        
    public float fadeDuration = 0.25f;

    [Header("던전 체류 시간")]
    public float dungeonStayTime = 120f;   

    [Header("남은 시간 UI")]
    public TextMeshProUGUI timerText;
    public Image timerFill;               
    public Color timerColorNormal = Color.white;
    public Color timerColorWarning = new Color(1f, 0.45f, 0.45f);
    public float warningThreshold = 10f;  
    public float pulseSpeed = 4f;         

    
    public string playerTag = "Player";

    private bool busy = false;
    private bool inDungeon = false;
    private Coroutine dungeonTimer;
    private Collider myCol;

    void Awake()
    {
        myCol = GetComponent<Collider>();
    }

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (busy) return;
        if (!other.CompareTag(playerTag)) return;

        if (!inDungeon)
            StartCoroutine(EnterDungeon(other));
    }

    IEnumerator EnterDungeon(Collider player)
    {
        busy = true;
        inDungeon = true;

        
        if (fadeCanvas) yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        
        var cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        player.transform.SetPositionAndRotation(target.position, target.rotation);
        if (cc) cc.enabled = true;

        
        if (worldRoot) worldRoot.SetActive(false);
        if (dungeonRoot) dungeonRoot.SetActive(true);

        
        InitTimerUI(dungeonStayTime);

        
        if (dungeonTimer != null) StopCoroutine(dungeonTimer);
        dungeonTimer = StartCoroutine(DungeonTimer(player));

        
        if (fadeCanvas) yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        busy = false;
    }

    IEnumerator ExitDungeon(Collider player)
    {
        busy = true;
        inDungeon = false;

        if (fadeCanvas) yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        var cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        player.transform.SetPositionAndRotation(exitPoint.position, exitPoint.rotation);
        if (cc) cc.enabled = true;

        if (worldRoot) worldRoot.SetActive(true);
        if (dungeonRoot) dungeonRoot.SetActive(false);

        
        ClearTimerUI();

        if (fadeCanvas) yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        busy = false;
    }

    IEnumerator DungeonTimer(Collider player)
    {
        float timeLeft = dungeonStayTime;

        while (timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimerUI(Mathf.Max(0f, timeLeft));
            yield return null;
        }

        
        StartCoroutine(ExitDungeon(player));
    }

    

    void InitTimerUI(float total)
    {
        if (timerText)
        {
            timerText.gameObject.SetActive(true);
            timerText.color = timerColorNormal;
            timerText.text = FormatTime(total);
        }
        if (timerFill)
        {
            timerFill.gameObject.SetActive(true);
            timerFill.type = Image.Type.Filled;
            timerFill.fillMethod = Image.FillMethod.Radial360;
            timerFill.fillAmount = 1f;
            timerFill.color = timerColorNormal;
        }
    }

    void UpdateTimerUI(float timeLeft)
    {
        
        if (timerText)
        {
            timerText.text = FormatTime(timeLeft);

            
            if (timeLeft <= warningThreshold)
            {
                float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
                timerText.color = Color.Lerp(timerColorNormal, timerColorWarning, t);
            }
            else
            {
                timerText.color = timerColorNormal;
            }
        }

        
        if (timerFill)
        {
            float ratio = (dungeonStayTime <= 0f) ? 0f : (timeLeft / dungeonStayTime);
            timerFill.fillAmount = Mathf.Clamp01(ratio);

            if (timeLeft <= warningThreshold)
            {
                float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
                timerFill.color = Color.Lerp(timerColorNormal, timerColorWarning, t);
            }
            else
            {
                timerFill.color = timerColorNormal;
            }
        }
    }

    void ClearTimerUI()
    {
        if (timerText)
        {
            timerText.text = "";
            timerText.gameObject.SetActive(false);
        }
        if (timerFill)
        {
            timerFill.fillAmount = 0f;
            timerFill.gameObject.SetActive(false);
        }
    }

    string FormatTime(float t)
    {
        int s = Mathf.CeilToInt(t);
        int m = s / 60;
        int r = s % 60;
        return $"{m:00}:{r:00}";
    }

    

    IEnumerator Fade(float from, float to, float dur)
    {
        if (!fadeCanvas) yield break;
        float t = 0f;
        fadeCanvas.blocksRaycasts = true;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvas.alpha = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }

        fadeCanvas.alpha = to;
        fadeCanvas.blocksRaycasts = to > 0.99f;
    }
}


