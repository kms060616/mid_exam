using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 2f;
    public int damage = 1;

    
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (TryDamageAny(other.gameObject)) { Destroy(gameObject); return; }

        // 자식 콜라이더일 수도 있으니 부모도 확인
        var parent = other.transform.parent ? other.transform.parent.gameObject : null;
        if (parent && TryDamageAny(parent)) { Destroy(gameObject); return; }
    }

    bool TryDamageAny(GameObject go)
    {
        // 1) 기존 Enemy
        var e = go.GetComponent<Enemy>();
        if (e != null) { e.TakeDamage(damage); return true; }

        // 2) 근접형 EnemyMelee
        var m = go.GetComponent<EnemyMelee>();
        if (m != null) { m.TakeDamage(damage); return true; }

        var q = go.GetComponent<Enemy2>(); 
        if (q != null) { q.TakeDamage(damage); return true; }

        var b = go.GetComponent<EnemyBomber>();
        if (b != null) { b.TakeDamage(damage); return true; }

        // 3) 더 추가할 타입 있으면 여기 계속 한 줄씩
        // var r = go.GetComponent<EnemyRanged>(); if (r != null) { r.TakeDamage(damage); return true; }

        Debug.Log($"[Projectile] 피해 대상 아님: {go.name}");
        return false;
    }
}
