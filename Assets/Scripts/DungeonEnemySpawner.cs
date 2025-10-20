using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonEnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;         
    public int spawnCount = 5;             

    [Header("Spawn Area")]
    public Vector3 areaCenter = Vector3.zero; 
    public Vector3 areaSize = new Vector3(10, 1, 10); 

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    public void Activate()
    {
        Clear();

        if (enemyPrefab == null)
        {
            Debug.LogWarning("[Spawner] EnemyPrefab이 비어있습니다!");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 randomPos = GetRandomPosition();
            GameObject enemy = Instantiate(enemyPrefab, randomPos, Quaternion.identity, transform);
            spawnedEnemies.Add(enemy);
        }

        Debug.Log($"[Spawner] {spawnedEnemies.Count}마리 생성 완료!");
    }

    public void Clear()
    {
        foreach (var e in spawnedEnemies)
        {
            if (e != null)
                Destroy(e);
        }
        spawnedEnemies.Clear();
    }

    Vector3 GetRandomPosition()
    {
        Vector3 offset = new Vector3(
            Random.Range(-areaSize.x / 2, areaSize.x / 2),
            0,
            Random.Range(-areaSize.z / 2, areaSize.z / 2)
        );

        return transform.position + areaCenter + offset;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(transform.position + areaCenter, areaSize);
    }
}
