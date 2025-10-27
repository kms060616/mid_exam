using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBomber : MonoBehaviour
{
    [Header("Refs")]
    public Transform modelRoot;            // 선택: 바디 회전용 (없으면 transform 사용)
    public LayerMask playerMask;           // 플레이어 레이어

    [Header("Stats")]
    public int maxHP = 3;
    public float moveSpeed = 3.2f;
    public float detectRange = 14f;

    [Header("Explosion")]
    public float triggerRange = 2.2f;      // 폭발을 시작하는 거리(퓨즈 시작)
    public float fuseTime = 0.8f;          // 퓨즈(딜레이)
    public float explosionRadius = 3.0f;
    public int explosionDamage = 12;
    public float knockbackForce = 10f;
    public bool damageSelfOnlyOnce = true; // 중복 폭발 방지

    

    [Header("Stability")]
    public bool freezeXZRotation = true;   // 넘어짐 방지

    // 내부 상태
    Transform player;
    Rigidbody rb;
    int currentHP;
    bool exploding;
    bool notifiedDead;
    DungeonWaveSpawner owner;

    public Slider hpSlider;

    public void Init(DungeonWaveSpawner spawner) { owner = spawner; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.useGravity = true;
            if (freezeXZRotation)
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        currentHP = maxHP;
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;
    }

    void Update()
    {
        if (!player) return;
        var lookY = (modelRoot ? modelRoot.position.y : transform.position.y);
        var lookTarget = new Vector3(player.position.x, lookY, player.position.z);
        (modelRoot ? modelRoot : transform).LookAt(lookTarget);
    }

    void FixedUpdate()
    {
        if (!player || rb == null || exploding) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // 추적
        if (dist <= detectRange)
        {
            Vector3 flatTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
            Vector3 dir = (flatTarget - transform.position);
            float d = dir.magnitude;
            dir = d > 0.001f ? dir / d : Vector3.zero;

            rb.velocity = new Vector3(dir.x * moveSpeed, rb.velocity.y, dir.z * moveSpeed);
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        // 폭발 트리거
        if (dist <= triggerRange)
            StartFuse();
    }

    public void TakeDamage(int dmg)
    {
        if (exploding) return;
        currentHP -= dmg;
        if (hpSlider) hpSlider.value = (float)currentHP / maxHP;
        if (currentHP <= 0)
        {
            // 즉시 폭발 (체력 0시)
            StartFuse(true);
        }
    }

    void StartFuse(bool instant = false)
    {
        if (exploding) return;
        exploding = true;
        StartCoroutine(ExplodeRoutine(instant ? 0f : fuseTime));
    }

    IEnumerator ExplodeRoutine(float delay)
    {
       

        // 살짝 서서히 멈추기
        if (rb) rb.velocity = new Vector3(0, rb.velocity.y * 0.5f, 0);

        float t = 0f;
        while (t < delay)
        {
            t += Time.deltaTime;
            // 여기에 모델 깜빡임/스케일 펄스 등 넣어도 됨
            yield return null;
        }

        DoExplosion();
        Die(); // Notify 후 파괴
    }

    void DoExplosion()
    {
        

        // 피해 판정
        int mask = (playerMask.value != 0) ? playerMask.value : ~0; // 마스크 비워도 동작
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, mask, QueryTriggerInteraction.Collide);

        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                var pm = h.GetComponent<PlayerMove>();
                if (pm != null) pm.TakeDamage(explosionDamage);

                // 넉백
                var prb = h.attachedRigidbody;
                if (prb != null)
                {
                    Vector3 dir = (h.transform.position - transform.position).normalized;
                    prb.AddForce(dir * knockbackForce, ForceMode.Impulse);
                }
            }
        }
    }

    void Die()
    {
        if (!notifiedDead && owner)
        {
            owner.NotifyEnemyDied(gameObject);
            notifiedDead = true;
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (!notifiedDead && owner)
        {
            owner.NotifyEnemyDied(gameObject);
            notifiedDead = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
        Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, triggerRange);
    }
}
