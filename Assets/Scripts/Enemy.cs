using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public enum EnemyState { Idle, Trace, Attack, RunAway }

    public EnemyState state = EnemyState.Idle;


    public float moveSpeed = 2f;
    private Transform player;

    public GameObject ProjectilePrefab;

    public Transform firePoit;

    public float traceRage = 15f;

    public float attackRange = 6f;

    public float attackCooldown = 1.5f;

    

    

    private float lastAttackTime;

    public int maxHP = 5; 
    private int currentHP;

    public Slider hpSlider;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHP = maxHP;
        lastAttackTime = -attackCooldown;

        currentHP = maxHP;
        hpSlider.value = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (currentHP <= maxHP * 0.2 && state != EnemyState.Idle)
            state = EnemyState.RunAway;

        switch (state)
        {
            case EnemyState.Idle:
                if (dist < traceRage)
                    state = EnemyState.Trace;
                break;

            case EnemyState.Trace:
                if (dist < attackRange)
                    state = EnemyState.Attack;
                else if (dist > traceRage)
                    state = EnemyState.Idle;
                else
                    TracePlayer();
                break;

            case EnemyState.Attack:
                if (dist > attackRange)
                    state = EnemyState.Trace;
                else
                    AttackPlayer();
                break;

            case EnemyState.RunAway:
                if (dist > traceRage)
                    state = EnemyState.Idle;
                else
                    Run();
                break;

                
        }

    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        hpSlider.value = (float)currentHP / maxHP;

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
       Destroy(gameObject);
    }

    void TracePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.LookAt(player.position);
    }

    void AttackPlayer()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            ShootProjectile();
        }
    }    

    void ShootProjectile()
    {
        if (ProjectilePrefab != null && firePoit != null)
        {
            transform.LookAt(player.position);
            GameObject proj = Instantiate(ProjectilePrefab, firePoit.position, firePoit.rotation);
            EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
            if (ep != null)
            {
                Vector3 dir = (player.position - firePoit.position).normalized;
                ep.SetDirection(dir);
            }    
        }
    }

    void Run()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position -= dir * moveSpeed * Time.deltaTime;
    }
}
