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
    public int currentXP = 0; // �� �������� ���� XP

    [Header("Optional UI")]
    public Slider xpSlider;     // 0~1�� ä��� (����)
    public Text levelText;      // "Lv. X" (����)

    public event Action<int> OnLevelUp; // (����) ������ �̺�Ʈ

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // ��/���� ��ȯ �� ����
        Debug.Log($"[XP] Awake ����. ����: {level}, XP: {currentXP}"); // Awake ���� ���� �α�

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
        while (level < maxLevel && currentXP >= xpPerLevel)
        {
            currentXP -= xpPerLevel;
            level++;

            
            ApplyLevelUpReward();

            OnLevelUp?.Invoke(level);
        }

        if (level >= maxLevel) currentXP = 0;
        UpdateUI();
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
    void ApplyLevelUpReward()
    {
        var pm = FindObjectOfType<PlayerMove>();
        if (pm != null) pm.GainMaxHP(10, true);

        var ps = FindObjectOfType<PlayerShooting>();
        if (ps != null) ps.meleeBonus += 1;

    }
    int GetCurrentHP(PlayerMove pm)
    {
        var cur = typeof(PlayerMove).GetField("currentHP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (cur != null) ? (int)cur.GetValue(pm) : pm.maxHP;
    }
}
