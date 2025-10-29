using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public enum EnemyState { Idle, Trace, Attack }
    public EnemyState state = EnemyState.Idle;

    [Header("Move")]
    public float moveSpeed = 2f;
    public float stopDistance = 1.1f;  

    [Header("Ranges")]
    public float traceRange = 15f;
    public float attackRange = 6f;
    public float attackCooldown = 1.5f;

    [Header("Separation (겹침 완화, 선택)")]
    public float separationRadius = 1.0f;
    public float separationStrength = 2.0f;
    public LayerMask enemyMask; 

    [Header("Combat")]
    public GameObject ProjectilePrefab;
    public Transform firePoint;

    [Header("HP")]
    public int maxHP = 5;
    public Slider hpSlider;

    Transform player;
    Rigidbody rb;
    int currentHP;
    float lastAttackTime;

    private DungeonWaveSpawner owner;
    private bool notifiedDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        rb.freezeRotation = true; 
        
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;

        currentHP = maxHP;
        lastAttackTime = -attackCooldown;
        if (hpSlider) hpSlider.value = 1f;
    }

    void Update()
    {
        if (!player) return;

        float dist = Vector3.Distance(player.position, transform.position);

        switch (state)
        {
            case EnemyState.Idle:
                if (dist < traceRange) state = EnemyState.Trace;
                break;

            case EnemyState.Trace:
                if (dist < attackRange) state = EnemyState.Attack;
                else if (dist > traceRange) state = EnemyState.Idle;
                break;

            case EnemyState.Attack:
                if (dist > attackRange) state = EnemyState.Trace;
                else AttackPlayer();
                break;
        }

        
        Vector3 flatTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(flatTarget);
    }

    void FixedUpdate()
    {
        if (!player) return;

        if (state == EnemyState.Trace || state == EnemyState.Attack)
        {
           
            Vector3 flatTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
            Vector3 toPlayer = (flatTarget - transform.position);
            float dist = toPlayer.magnitude;

            Vector3 dirToPlayer = dist > 0.001f ? toPlayer / dist : Vector3.zero;

            
            Vector3 separation = Vector3.zero;
            if (separationRadius > 0.01f)
            {
                var hits = Physics.OverlapSphere(transform.position, separationRadius, enemyMask);
                foreach (var h in hits)
                {
                    if (h.transform == transform) continue;
                    Vector3 away = transform.position - h.transform.position;
                    float d = away.magnitude;
                    if (d > 0.0001f)
                        separation += away.normalized * (1f - Mathf.Clamp01(d / separationRadius));
                }
            }

            Vector3 moveDir = dirToPlayer;
            if (separation != Vector3.zero)
                moveDir = (dirToPlayer + separation * separationStrength).normalized;

            
            if (dist <= stopDistance) moveDir = Vector3.zero;

            
            Vector3 v = rb.velocity;
            v.x = moveDir.x * moveSpeed;
            v.z = moveDir.z * moveSpeed;
           
            rb.velocity = v;
        }
    }

    void AttackPlayer()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;
        lastAttackTime = Time.time;

        if (ProjectilePrefab && firePoint)
        {
            Vector3 dir = (player.position - firePoint.position).normalized;
            
            var proj = Instantiate(ProjectilePrefab, firePoint.position, Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z)));
            var ep = proj.GetComponent<EnemyProjectile>();
            if (ep != null) ep.SetDirection(dir);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (hpSlider) hpSlider.value = (float)currentHP / maxHP;
        if (currentHP <= 0) Destroy(gameObject);
    }

    public void Init(DungeonWaveSpawner spawner) 
    {
        owner = spawner;
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
}
