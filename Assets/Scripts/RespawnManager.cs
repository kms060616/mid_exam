using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    
    
    public static RespawnManager Instance { get; private set; }

    [Header("Roots")]
    public GameObject worldRoot;          // 필드 루트 (켜짐)
    public GameObject dungeonRoot;        // 던전 루트 (꺼짐)

    [Header("Where to respawn")]
    public Transform exitPointWorld;      // 필드(퇴장) 위치

    [Header("Visuals (선택)")]
    public CanvasGroup fadeCanvas;        // 페이드 캔버스
    public float fadeDuration = 0.25f;
    public Light globalLight;             // 필드 전역 라이트(켜기)
    public PlayerLightToggle playerLightToggle;  // 플레이어 라이트(끄기)

    [Header("Systems (선택)")]
    public StageManager stageManager;     // 던전 스테이지 매니저(있으면 정리)
    // 포탈에 ExitDungeonNow()가 있다면 아래처럼 써도 됨:
    public Portal portal;                 // 있으면 portal.ExitDungeonNow() 사용 가능

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RespawnPlayer(PlayerMove player)
    {
        StartCoroutine(RespawnRoutine(player));
    }

    IEnumerator RespawnRoutine(PlayerMove player)
    {
        // 1) 페이드 아웃
        if (fadeCanvas) yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // 2) 플레이어 입력/이동 잠시 비활성
        var cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        // 3) 던전 시스템 정리 (웨이브/적 제거 + 스테이지 비활성)
        if (stageManager != null)
        {
            // StageManager가 Clear/퇴장 로직을 가진 경우
            stageManager.StopAllCoroutines();
            // 현재 스포너/적 비우기
            // stageManager 안에 Clear나 OnAllStagesCleared와 유사 정리 로직이 있으면 호출
        }

        // 4) 루트 전환 (던전 OFF → 월드 ON)
        if (dungeonRoot) dungeonRoot.SetActive(false);
        if (worldRoot) worldRoot.SetActive(true);

        // 5) 조명 전환
        if (globalLight) globalLight.enabled = true;
        if (playerLightToggle) playerLightToggle.TurnOff();

        // 6) 플레이어 텔레포트 (퇴장 위치로)
        if (exitPointWorld != null)
        {
            player.transform.SetPositionAndRotation(exitPointWorld.position, exitPointWorld.rotation);
        }

        // 7) 체력/상태 초기화
        player.FullHeal();
        // 속도/중력 안정화
        var rb = player.GetComponent<Rigidbody>();
        if (rb) rb.velocity = Vector3.zero;

        // 8) 플레이어 컨트롤 재활성
        if (cc) cc.enabled = true;

        // 9) 페이드 인
        if (fadeCanvas) yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        if (portal != null)
        {
            portal.ExitDungeonNow();  // 던전 종료 처리
        }
    }

    IEnumerator Fade(float from, float to, float dur)
    {
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
