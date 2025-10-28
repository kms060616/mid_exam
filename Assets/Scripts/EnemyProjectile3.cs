using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile3 : MonoBehaviour
{
    public int damage = 5;
    public float speed = 4f;


    public float lifeTime = 2f;

    private Vector3 moveDir;

    public void SetDirection(Vector3 dir)
    {
        moveDir = dir.normalized;
    }


    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += moveDir * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMove pc = other.GetComponent<PlayerMove>();
            if (pc != null) pc.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
