using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject projectilePrefab2;
    public Transform firePoint;
    Camera cam;

    public ArmSwing armSwing;

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
        float attackRange = 2f;
        int damage = 3;

        Collider[] hits = Physics.OverlapSphere(firePoint.position, attackRange);
        foreach (Collider hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("근접공격으로 맞음: " + hit.name);
            }
        }

        if (armSwing != null)
            armSwing.TriggerSwing();
    }
}
