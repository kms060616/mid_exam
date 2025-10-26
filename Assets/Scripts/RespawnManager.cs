using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    
    
    public static RespawnManager Instance { get; private set; }

    [Header("Roots")]
    public GameObject worldRoot;          // �ʵ� ��Ʈ (����)
    public GameObject dungeonRoot;        // ���� ��Ʈ (����)

    [Header("Where to respawn")]
    public Transform exitPointWorld;      // �ʵ�(����) ��ġ

    [Header("Visuals (����)")]
    public CanvasGroup fadeCanvas;        // ���̵� ĵ����
    public float fadeDuration = 0.25f;
    public Light globalLight;             // �ʵ� ���� ����Ʈ(�ѱ�)
    public PlayerLightToggle playerLightToggle;  // �÷��̾� ����Ʈ(����)

    [Header("Systems (����)")]
    public StageManager stageManager;     // ���� �������� �Ŵ���(������ ����)
    // ��Ż�� ExitDungeonNow()�� �ִٸ� �Ʒ�ó�� �ᵵ ��:
    public Portal portal;                 // ������ portal.ExitDungeonNow() ��� ����

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
        // 1) ���̵� �ƿ�
        if (fadeCanvas) yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // 2) �÷��̾� �Է�/�̵� ��� ��Ȱ��
        var cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        // 3) ���� �ý��� ���� (���̺�/�� ���� + �������� ��Ȱ��)
        if (stageManager != null)
        {
            // StageManager�� Clear/���� ������ ���� ���
            stageManager.StopAllCoroutines();
            // ���� ������/�� ����
            // stageManager �ȿ� Clear�� OnAllStagesCleared�� ���� ���� ������ ������ ȣ��
        }

        // 4) ��Ʈ ��ȯ (���� OFF �� ���� ON)
        if (dungeonRoot) dungeonRoot.SetActive(false);
        if (worldRoot) worldRoot.SetActive(true);

        // 5) ���� ��ȯ
        if (globalLight) globalLight.enabled = true;
        if (playerLightToggle) playerLightToggle.TurnOff();

        // 6) �÷��̾� �ڷ���Ʈ (���� ��ġ��)
        if (exitPointWorld != null)
        {
            player.transform.SetPositionAndRotation(exitPointWorld.position, exitPointWorld.rotation);
        }

        // 7) ü��/���� �ʱ�ȭ
        player.FullHeal();
        // �ӵ�/�߷� ����ȭ
        var rb = player.GetComponent<Rigidbody>();
        if (rb) rb.velocity = Vector3.zero;

        // 8) �÷��̾� ��Ʈ�� ��Ȱ��
        if (cc) cc.enabled = true;

        // 9) ���̵� ��
        if (fadeCanvas) yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        if (portal != null)
        {
            portal.ExitDungeonNow();  // ���� ���� ó��
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
