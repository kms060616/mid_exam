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
        Debug.Log($"[XP] AddXP ȣ���: +{amount}, ���� ����: {level}, ���� XP: {currentXP}");
        // ���� ���� ���� ��µ� ó��
        while (level < maxLevel && currentXP >= xpPerLevel)
        {
            currentXP -= xpPerLevel;
            level++;
            Debug.Log($"[XP] ������! �� ����: {level}"); // ������ ���� �α�
            OnLevelUp?.Invoke(level);
        }
        Debug.Log($"[XP] AddXP ȣ�� ��: ���� ����: {level}, ���� XP: {currentXP}");

        // ���� ���� �� XP ���
        if (level >= maxLevel)
        {
            currentXP = 0;
        }

        UpdateUI();
        Debug.Log($"[XP] AddXP ȣ���: +{amount}");
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
