using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyMelee : MonoBehaviour
{
    [Header("Refs")]
    public Transform modelRoot;           // (����) ���� ȸ����, ���� transform ���
    public LayerMask playerMask;          // Player�� ���� ���̾�

    [Header("Stats")]
    public int maxHP = 12;
    public float moveSpeed = 2.8f;
    public float detectRange = 12f;
    public float attackRange = 1.6f;
    public float attackCooldown = 0.9f;
    public int attackDamage = 6;

    [Header("Movement")]
    public float stopDistance = 1.0f;     // �÷��̾�� �ּ� �Ÿ�
    public bool freezeXZRotation = true;  // �Ѿ����� ����

    [Header("Attack")]
    public float hitRadius = 0.9f;        // ������ ��ü �ݰ�
    public float hitForwardOffset = 0.9f; // �� �������� ������

    private Transform player;
    private Rigidbody rb;
    private int currentHP;
    private float lastAttack;
    private DungeonWaveSpawner owner;     // ������ �����
    private bool notifiedDead;

    // �����ʰ� ȣ��
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
    }

    void Update()
    {
        if (!player) return;

        // ���� �ٶ󺸰�
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
            // ���� �̵� ����
            Vector3 flatTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
            Vector3 dir = (flatTarget - transform.position);
            float flatDist = dir.magnitude;
            dir = flatDist > 0.0001f ? dir / flatDist : Vector3.zero;

            if (dist > Mathf.Max(attackRange, stopDistance))
            {
                // ����
                v.x = dir.x * moveSpeed;
                v.z = dir.z * moveSpeed;
            }
            else
            {
                // ���߰� ����
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
            // ���
            v.x = 0; v.z = 0;
        }

        // y�� ������ �ñ�
        rb.velocity = new Vector3(v.x, rb.velocity.y, v.z);
    }

    void TryHit()
    {
        Vector3 center = transform.position + transform.forward * hitForwardOffset;
        Collider[] hits = Physics.OverlapSphere(center, hitRadius, playerMask);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                var pm = h.GetComponent<PlayerMove>();
                if (pm != null) pm.TakeDamage(attackDamage);
            }
        }
        // ���⼭ �ִϸ��̼�/����/��Ʈ���� �� �ٿ��� ��
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
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
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.35f);
        Vector3 center = transform.position + transform.forward * hitForwardOffset;
        Gizmos.DrawSphere(center, hitRadius);
    }
}
