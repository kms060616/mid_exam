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

    [Header("Stages (5개 구성)")]
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
            OnAllStagesCleared();  // ← 모두 클리어 시 퇴장/보상 등
            return;
        }

        // 이전 스테이지 정리
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

        // 이하 기존 순서 그대로: 루트 켜기 → 스포너 찾기 → 플레이어 텔레포트 → 다음 프레임 Activate
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
        if (stageStarting) yield break;   // 이미 시작 중이면 무시
        stageStarting = true;

        
        if (!spawner.gameObject.activeSelf) spawner.gameObject.SetActive(true);
        if (!spawner.enabled) spawner.enabled = true;

        
        yield return null;

        
        spawner.Activate();
        Debug.Log($"[StageManager] Stage{idx} -> Activate 호출 완료");

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
        Debug.Log("[StageManager] 모든 스테이지 클리어 → 던전 퇴장");

        // 현재 스포너 정리
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

        // 포탈이 있으면 포탈 로직으로 안전하게 퇴장
        if (portal != null)
        {
            portal.ExitDungeonNow();   // ← 페이드 + world/dungeon 토글 + 위치 이동
            return;
        }

    }

    bool TeleportPlayer(Transform target, float upOffset = 0.5f, LayerMask groundMask = default)
    {
        if (player == null || target == null)
        {
            Debug.LogError("[StageManager] Teleport 실패: player 또는 target이 null");
            return false;
        }

        var cc = player.GetComponent<CharacterController>();

        // 1) CC 비활성
        if (cc) cc.enabled = false;

        // 2) 살짝 위에 배치(겹침 회피)
        Vector3 pos = target.position + Vector3.up * upOffset;

        // 3) 바닥 스냅(선택)
        if (groundMask.value != 0)
        {
            if (Physics.Raycast(pos, Vector3.down, out var hit, 5f, groundMask))
                pos = hit.point + Vector3.up * 0.05f;
        }

        player.SetPositionAndRotation(pos, target.rotation);

        // 4) CC 재활성
        if (cc) cc.enabled = true;

        return true;
    }
}
