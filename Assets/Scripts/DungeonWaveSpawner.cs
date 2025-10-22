using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DungeonWaveSpawner : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;

    [Header("Wave Settings")]
    public int totalWaves = 3;          
    public int enemiesPerWave = 5;      
    public float timeBetweenWaves = 2f; 

    [Header("Spawn Area (랜덤 스폰)")]
    public Vector3 areaCenter = Vector3.zero;
    public Vector3 areaSize = new Vector3(12, 1, 12);

    [Header("Spawn 간격 보정")]
    public float minSpawnDistance = 1.2f;
    public int maxSpawnTriesPerEnemy = 15;

    
    private readonly List<GameObject> alive = new List<GameObject>();
    private Coroutine waveRoutine;
    private int currentWave = 0;
    private bool running = false;

    public event Action AllWavesCleared;

    public void Activate()
    {
        if (!isActiveAndEnabled)
        {
            Debug.LogWarning("[WaveSpawner] 비활성 상태에서 Activate 호출됨. 활성화 후 다시 호출 필요.");
            return;
        }
    }

    public void Clear()
    {
        StopAll();
        for (int i = alive.Count - 1; i >= 0; i--)
            if (alive[i]) Destroy(alive[i]);
        alive.Clear();
    }

    void StopAll()
    {
        running = false;
        if (waveRoutine != null) StopCoroutine(waveRoutine);
        waveRoutine = null;
        currentWave = 0;
    }

    IEnumerator RunWaves()
    {
        for (currentWave = 1; currentWave <= totalWaves && running; currentWave++)
        {
            SpawnWave(enemiesPerWave);

            while (running)
            {
                alive.RemoveAll(go => go == null);
                if (alive.Count == 0) break;
                yield return null;
            }

            if (running && currentWave < totalWaves)
                yield return new WaitForSeconds(timeBetweenWaves);
        }

        
        AllWavesCleared?.Invoke();
    }




    void SpawnWave(int count)
    {
        if (!enemyPrefab)
        {
            Debug.LogWarning("[WaveSpawner] enemyPrefab 비어있음");
            return;
        }

        alive.Clear();
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = FindSpawnPosition();
            var go = Instantiate(enemyPrefab, pos, Quaternion.identity, transform);
            alive.Add(go);

            var enemy = go.GetComponent<Enemy>();
            if (enemy != null) enemy.Init(this);
            else Debug.LogError("[WaveSpawner] Enemy 컴포넌트 없음");
        }

        Debug.Log($"[WaveSpawner] Wave {currentWave}: {count} spawn, alive={alive.Count}");

    }

    Vector3 FindSpawnPosition()
    {
        Vector3 pos = transform.position + areaCenter;

        for (int tries = 0; tries < maxSpawnTriesPerEnemy; tries++)
        {
            Vector3 candidate = transform.position + areaCenter +
                                new Vector3(
                                    UnityEngine.Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                                    0,
                                    UnityEngine.Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f)
                                );

            bool ok = true;
            foreach (var e in alive)
            {
                if (!e) continue;
                if ((candidate - e.transform.position).sqrMagnitude < minSpawnDistance * minSpawnDistance)
                {
                    ok = false; break;
                }
            }
            if (ok) return candidate;
        }
        return pos; 
    }

   
    public void NotifyEnemyDied(GameObject enemyGO)
    {
        int before = alive.Count;
        alive.Remove(enemyGO);
        Debug.Log($"[WaveSpawner] Enemy died. {before} -> {alive.Count}");
    }

    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.2f, 0.2f, 0.25f);
        Gizmos.DrawCube(transform.position + areaCenter, areaSize);
    }


}
