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

        // �ڽ� �ݶ��̴��� ���� ������ �θ� Ȯ��
        var parent = other.transform.parent ? other.transform.parent.gameObject : null;
        if (parent && TryDamageAny(parent)) { Destroy(gameObject); return; }
    }

    bool TryDamageAny(GameObject go)
    {
        // 1) ���� Enemy
        var e = go.GetComponent<Enemy>();
        if (e != null) { e.TakeDamage(damage); return true; }

        // 2) ������ EnemyMelee
        var m = go.GetComponent<EnemyMelee>();
        if (m != null) { m.TakeDamage(damage); return true; }

        var q = go.GetComponent<Enemy2>(); 
        if (q != null) { q.TakeDamage(damage); return true; }

        var b = go.GetComponent<EnemyBomber>();
        if (b != null) { b.TakeDamage(damage); return true; }

        // 3) �� �߰��� Ÿ�� ������ ���� ��� �� �پ�
        // var r = go.GetComponent<EnemyRanged>(); if (r != null) { r.TakeDamage(damage); return true; }

        Debug.Log($"[Projectile] ���� ��� �ƴ�: {go.name}");
        return false;
    }
}
