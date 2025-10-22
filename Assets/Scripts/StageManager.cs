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

    public Portal portal;              
    public Transform exitPointWorld;


    [Header("Settings")]
    public float stageFadeDelay = 0.5f;

    int currentIndex = -1;

    bool stageStarting = false;

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

        if (index < 0 || index >= stages.Count)
        {
            OnAllStagesCleared();  // �� ��� Ŭ���� �� ����/���� ��
            return;
        }

        // ���� �������� ����
        if (currentIndex >= 0 && currentIndex < stages.Count)
        {
            var prev = stages[currentIndex];
            if (prev.waveSpawner)
            {
                prev.waveSpawner.AllWavesCleared -= OnStageCleared;
                prev.waveSpawner.Clear();
            }
            if (prev.stageRoot) prev.stageRoot.SetActive(false);
        }

        currentIndex = index;

        var stage = stages[currentIndex];

        // ���� ���� ���� �״��: ��Ʈ �ѱ� �� ������ ã�� �� �÷��̾� �ڷ���Ʈ �� ���� ������ Activate
        if (stage.stageRoot) stage.stageRoot.SetActive(true);
        if (stage.waveSpawner == null && stage.stageRoot)
            stage.waveSpawner = stage.stageRoot.GetComponentInChildren<DungeonWaveSpawner>(true);

        if (player && stage.playerSpawn)
            TeleportPlayer(stage.playerSpawn, 0.5f);

        if (stage.waveSpawner)
        {
            stage.waveSpawner.AllWavesCleared -= OnStageCleared;
            stage.waveSpawner.AllWavesCleared += OnStageCleared;
            StartCoroutine(ActivateSpawnerNextFrame(stage.waveSpawner, currentIndex));
        }
    }

    IEnumerator ActivateSpawnerNextFrame(DungeonWaveSpawner spawner , int idx)
    {
        if (stageStarting) yield break;   // �̹� ���� ���̸� ����
        stageStarting = true;

        
        if (!spawner.gameObject.activeSelf) spawner.gameObject.SetActive(true);
        if (!spawner.enabled) spawner.enabled = true;

        
        yield return null;

        
        spawner.Activate();
        Debug.Log($"[StageManager] Stage{idx} -> Activate ȣ�� �Ϸ�");

        stageStarting = false;
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
        Debug.Log("[StageManager] ��� �������� Ŭ���� �� ���� ����");

        // ���� ������ ����
        if (currentIndex >= 0 && currentIndex < stages.Count)
        {
            var cur = stages[currentIndex];
            if (cur.waveSpawner)
            {
                cur.waveSpawner.AllWavesCleared -= OnStageCleared;
                cur.waveSpawner.Clear();
            }
            if (cur.stageRoot) cur.stageRoot.SetActive(false);
        }

        // ��Ż�� ������ ��Ż �������� �����ϰ� ����
        if (portal != null)
        {
            portal.ExitDungeonNow();   // �� ���̵� + world/dungeon ��� + ��ġ �̵�
            return;
        }

    }

    bool TeleportPlayer(Transform target, float upOffset = 0.5f, LayerMask groundMask = default)
    {
        if (player == null || target == null)
        {
            Debug.LogError("[StageManager] Teleport ����: player �Ǵ� target�� null");
            return false;
        }

        var cc = player.GetComponent<CharacterController>();

        // 1) CC ��Ȱ��
        if (cc) cc.enabled = false;

        // 2) ��¦ ���� ��ġ(��ħ ȸ��)
        Vector3 pos = target.position + Vector3.up * upOffset;

        // 3) �ٴ� ����(����)
        if (groundMask.value != 0)
        {
            if (Physics.Raycast(pos, Vector3.down, out var hit, 5f, groundMask))
                pos = hit.point + Vector3.up * 0.05f;
        }

        player.SetPositionAndRotation(pos, target.rotation);

        // 4) CC ��Ȱ��
        if (cc) cc.enabled = true;

        return true;
    }
}
