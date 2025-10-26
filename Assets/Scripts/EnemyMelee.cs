using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyMelee : MonoBehaviour
{
    [Header("Refs")]
    public Transform modelRoot;           // (선택) 몸통 회전용, 비우면 transform 사용
    public LayerMask playerMask;          // Player가 속한 레이어

    [Header("Stats")]
    public int maxHP = 12;
    public float moveSpeed = 2.8f;
    public float detectRange = 12f;
    public float attackRange = 1f;
    public float attackCooldown = 0.9f;
    public int attackDamage = 6;

    [Header("Movement")]
    public float stopDistance = 1.0f;     // 플레이어와 최소 거리
    public bool freezeXZRotation = true;  // 넘어진거 방지

    [Header("Attack")]
    public float hitRadius = 0.9f;        // 오버랩 구체 반경
    public float hitForwardOffset = 0.9f; // 몸 앞쪽으로 오프셋

    private Transform player;
    private Rigidbody rb;
    private int currentHP;
    private float lastAttack;
    private DungeonWaveSpawner owner;     // 스포너 보고용
    private bool notifiedDead;

    public Slider hpSlider;

    [Header("Attack Origin")]
    public Transform firePoint;

    // 스포너가 호출
    public void Init(DungeonWaveSpawner spawner)
    {
        owner = spawner;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
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
        if (hpSlider) hpSlider.value = 1f;
    }

    void Update()
    {
        if (!player) return;

        // 수평만 바라보게
        var lookTarget = new Vector3(player.position.x, (modelRoot ? modelRoot.position.y : transform.position.y), player.position.z);
        (modelRoot ? modelRoot : transform).LookAt(lookTarget);
    }

    void FixedUpdate()
    {
        if (!player || rb == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        Vector3 v = rb.velocity;

        if (dist <= detectRange)
        {
            // 수평 이동 벡터
            Vector3 flatTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
            Vector3 dir = (flatTarget - transform.position);
            float flatDist = dir.magnitude;
            dir = flatDist > 0.0001f ? dir / flatDist : Vector3.zero;

            if (dist > Mathf.Max(attackRange, stopDistance))
            {
                // 접근
                v.x = dir.x * moveSpeed;
                v.z = dir.z * moveSpeed;
            }
            else
            {
                // 멈추고 공격
                v.x = 0; v.z = 0;

                if (Time.time >= lastAttack + attackCooldown)
                {
                    lastAttack = Time.time;
                    TryHit();
                }
            }
        }
        else
        {
            // 대기
            v.x = 0; v.z = 0;
        }

        // y는 물리에 맡김
        rb.velocity = new Vector3(v.x, rb.velocity.y, v.z);
    }

    void TryHit()
    {
        Vector3 center = (firePoint != null)
        ? firePoint.position
        : transform.position + transform.forward * hitForwardOffset;

        // Enemy의 공격 범위 시각화와 동일하게
        Collider[] hits = Physics.OverlapSphere(center, hitRadius, playerMask);

        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                var pm = h.GetComponent<PlayerMove>();
                if (pm != null)
                {
                    pm.TakeDamage(attackDamage);
                    Debug.Log($"[EnemyMelee] 공격 히트: {h.name}");
                }
            }
        }
        // 여기서 애니메이션/사운드/히트스톱 등 붙여도 됨
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (hpSlider) hpSlider.value = (float)currentHP / maxHP;
        if (currentHP <= 0) Die();
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
        Gizmos.color = new Color(1f, 0.4f, 0.4f, 0.3f);
        Vector3 center = (firePoint != null)
            ? firePoint.position
            : transform.position + transform.forward * hitForwardOffset;
        Gizmos.DrawSphere(center, hitRadius);
    }
}
