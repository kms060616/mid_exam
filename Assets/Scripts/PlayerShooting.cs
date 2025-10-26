using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject projectilePrefab2;
    public Transform firePoint;
    Camera cam;

    public ArmSwing armSwing;

    public LayerMask enemyMask;      // 인스펙터에서 Enemy 체크 (비워도 동작하도록 처리)
    public float meleeRange = 0.7f;
    public float meleeForwardOffset = 0.2f;
    public int meleeDamage = 3;

    bool isSpecial = false;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isSpecial)
                Shoot2();
            else
                Shoot();

        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ChangeWeapon();
        }

        
    }
     
    void ChangeWeapon()
    {
        isSpecial = !isSpecial;
    }

    

    void Shoot()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPoint;
        targetPoint = ray.GetPoint(50f);
        Vector3 direction = (targetPoint - firePoint.position).normalized;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
        
       
    }

    void Shoot2()
    {
        // 무기/팔 기준으로 앞쪽에 히트 중심
        Vector3 center = firePoint.position + firePoint.forward * meleeForwardOffset;

        // enemyMask가 비어 있으면 Everything(~0)로 검사
        int layerMask = (enemyMask.value != 0) ? enemyMask.value : ~0;

        // 트리거도 맞게
        Collider[] hits = Physics.OverlapSphere(
            center,
            meleeRange,
            layerMask,
            QueryTriggerInteraction.Collide
        );

        bool hitSomething = false;

        foreach (var hit in hits)
        {
            if (!hit) continue;

            // 자기 자신/플레이어 무시 (필요시)
            if (hit.CompareTag("Player")) continue;

            // 1) Enemy (부모/자식 모두 탐색)
            var e = hit.GetComponentInParent<Enemy>() ?? hit.GetComponent<Enemy>();
            if (e != null)
            {
                e.TakeDamage(meleeDamage);
                hitSomething = true;
                Debug.Log($"근접공격 히트(Enemy): {hit.name}");
                continue;
            }

            // 2) EnemyMelee (부모/자식 모두 탐색)
            var m = hit.GetComponentInParent<EnemyMelee>() ?? hit.GetComponent<EnemyMelee>();
            if (m != null)
            {
                m.TakeDamage(meleeDamage);
                hitSomething = true;
                Debug.Log($"근접공격 히트(EnemyMelee): {hit.name}");
                continue;
            }
        }

        if (!hitSomething)
        {
            Debug.Log($"근접공격: 히트 없음. center={center} range={meleeRange} mask={(LayerMask)layerMask}");
        }

        if (armSwing != null)
            armSwing.TriggerSwing();
    }

    void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;
        Gizmos.color = new Color(0f, 1f, 0.9f, 0.35f);
        Vector3 center = firePoint.position + firePoint.forward * meleeForwardOffset;
        Gizmos.DrawSphere(center, meleeRange);
    }
}
