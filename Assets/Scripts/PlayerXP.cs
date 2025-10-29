using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerXP : MonoBehaviour
{
    public static PlayerXP Instance { get; private set; }

    [Header("Level Settings")]
    public int level = 0;
    public int maxLevel = 10;
    public int xpPerLevel = 5;

    [Header("Runtime")]
    public int currentXP = 0; // 현 레벨에서 누적 XP

    [Header("Optional UI")]
    public Slider xpSlider;     // 0~1로 채우기 (선택)
    public Text levelText;      // "Lv. X" (선택)

    public event Action<int> OnLevelUp; // (선택) 레벨업 이벤트

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬/던전 전환 시 유지
        Debug.Log($"[XP] Awake 실행. 레벨: {level}, XP: {currentXP}"); // Awake 실행 시점 로깅

        InitUI();
    }

    void InitUI()
    {
        if (xpSlider) xpSlider.value = 0f;
        if (levelText) levelText.text = $"Lv. {level}";
    }

    public void AddXP(int amount)
    {
        if (level >= maxLevel) return;
        
        currentXP += amount;
        Debug.Log($"[XP] AddXP 호출됨: +{amount}, 현재 레벨: {level}, 누적 XP: {currentXP}");
        // 여러 레벨 연속 상승도 처리
        while (level < maxLevel && currentXP >= xpPerLevel)
        {
            currentXP -= xpPerLevel;
            level++;
            Debug.Log($"[XP] 레벨업! 새 레벨: {level}"); // 레벨업 순간 로깅
            OnLevelUp?.Invoke(level);
        }
        Debug.Log($"[XP] AddXP 호출 끝: 현재 레벨: {level}, 누적 XP: {currentXP}");

        // 만렙 도달 시 XP 잠금
        if (level >= maxLevel)
        {
            currentXP = 0;
        }

        UpdateUI();
        Debug.Log($"[XP] AddXP 호출됨: +{amount}");
    }

    void UpdateUI()
    {
        if (xpSlider)
        {
            float ratio = (level >= maxLevel) ? 1f : (float)currentXP / xpPerLevel;
            xpSlider.value = ratio;
        }
        if (levelText) levelText.text = $"Lv. {level}";
    }
}
