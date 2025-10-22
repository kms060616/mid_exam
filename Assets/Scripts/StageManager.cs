using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [System.Serializable]
    public class Stage
    {
        public GameObject stageRoot;            
        public DungeonWaveSpawner waveSpawner;   
        public Transform playerSpawn;            
    }

    [Header("Stages (5�� ����)")]
    public List<Stage> stages = new List<Stage>(); 

    [Header("Refs")]
    public Transform player;                      
    public Light globalLight;
    public PlayerLightToggle playerLightToggle;


    [Header("Settings")]
    public float stageFadeDelay = 0.5f;

    int currentIndex = -1;

    void Awake()
    {
        
        for (int i = 0; i < stages.Count; i++)
            if (stages[i].stageRoot) stages[i].stageRoot.SetActive(false);
    }

    public void StartFromStage0()
    {
        GoToStage(0);
    }

    void GoToStage(int index)
    {

        currentIndex = index;
        if (currentIndex >= stages.Count) { OnAllStagesCleared(); return; }

        var stage = stages[currentIndex];

        // 1) �������� ��Ʈ ���� �ѱ� (���� �߿�)
        if (stage.stageRoot) stage.stageRoot.SetActive(true);

        // 2) (����) waveSpawner ������ ��������� �ڽĿ��� ã�ƿ���
        if (stage.waveSpawner == null && stage.stageRoot)
            stage.waveSpawner = stage.stageRoot.GetComponentInChildren<DungeonWaveSpawner>(true);

        // 3) �÷��̾� �̵�
        if (player && stage.playerSpawn)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
            player.SetPositionAndRotation(stage.playerSpawn.position, stage.playerSpawn.rotation);
            if (cc) cc.enabled = true;
        }

        // 4) ���� �

        // 5) ������ �̺�Ʈ ���� �� ������ �����ӡ��� Activate
        if (stage.waveSpawner)
        {
            stage.waveSpawner.AllWavesCleared -= OnStageCleared;
            stage.waveSpawner.AllWavesCleared += OnStageCleared;
            StartCoroutine(ActivateSpawnerNextFrame(stage.waveSpawner));
        }
    }

    IEnumerator ActivateSpawnerNextFrame(DungeonWaveSpawner spawner)
    {
        yield return null; // Ȱ��ȭ ����
        if (!spawner.gameObject.activeInHierarchy) spawner.gameObject.SetActive(true);
        if (!spawner.enabled) spawner.enabled = true;
        spawner.Activate();
    }

    void OnStageCleared()
    {
        
        StartCoroutine(NextStageAfterDelay());
    }

    IEnumerator NextStageAfterDelay()
    {
        yield return new WaitForSeconds(stageFadeDelay);
        GoToStage(currentIndex + 1);
    }

    void OnAllStagesCleared()
    {
        Debug.Log("[StageManager] ��� �������� Ŭ����!");
        
    }
}
