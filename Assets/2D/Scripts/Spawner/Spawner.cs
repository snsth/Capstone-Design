using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public SpawnData[] spawnData;

    [Header("# 무작위 스폰 간격 (초)")]
    public float minSpawnInterval = 0.3f;
    public float maxSpawnInterval = 1.5f;

    float timer;
    float nextSpawnTime;

    void Awake()
    {
        spawnPoints = GetComponentsInChildren<Transform>();
        nextSpawnTime = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void Update()
    {
        if (!Gamemanager.instance.isLive) return;

        timer += Time.deltaTime;

        if (timer >= nextSpawnTime)
        {
            Spawn();
            timer = 0f;
            nextSpawnTime = Random.Range(minSpawnInterval, maxSpawnInterval);
        }
    }

    void Spawn()
    {
        SpawnData data = spawnData[Random.Range(0, spawnData.Length)];

        for (int i = 0; i < data.spawnCount; i++)
        {
            GameObject enemy = Gamemanager.instance.pool.Get(0);
            enemy.transform.position = spawnPoints[Random.Range(1, spawnPoints.Length)].position;
            enemy.GetComponent<EnemyMovement>().Init(data);
        }
    }

    // 팬텀 스폰: 플레이어 주변 원형 범위에 적을 소환하고 목록 반환 (호출자가 나중에 비활성화)
    public List<GameObject> SpawnPhantomWave(int count, float radius)
    {
        List<GameObject> phantoms = new List<GameObject>();
        Vector3 playerPos = Gamemanager.instance.player.transform.position;

        for (int i = 0; i < count; i++)
        {
            SpawnData data = spawnData[Random.Range(0, spawnData.Length)];

            // 플레이어 주변 원형 범위 내 랜덤 위치 (너무 가까이 붙지 않도록 0.5~1.0 반경 사용)
            Vector2 dir = Random.insideUnitCircle.normalized;
            float dist = Random.Range(radius * 0.5f, radius);
            Vector3 pos = playerPos + new Vector3(dir.x * dist, dir.y * dist, 0f);

            GameObject enemy = Gamemanager.instance.pool.Get(0);
            enemy.transform.position = pos;
            enemy.GetComponent<EnemyMovement>().Init(data);
            phantoms.Add(enemy);
        }

        return phantoms;
    }

    // 공포 이벤트: 지정한 수만큼 플레이어 주변 원형 범위에서 한꺼번에 쏟아냄
    public void SpawnHorrorWave(int count, float speedOverride)
    {
        Vector3 playerPos = Gamemanager.instance.player.transform.position;

        for (int i = 0; i < count; i++)
        {
            SpawnData data = spawnData[Random.Range(0, spawnData.Length)];
            SpawnData horrorData = new SpawnData
            {
                spriteType = data.spriteType,
                spawnTime  = 0,
                spawnCount = 1,
                health     = data.health,
                speed      = speedOverride
            };

            Vector2 dir = Random.insideUnitCircle.normalized;
            float dist = Random.Range(12f, 18f);
            Vector3 pos = playerPos + new Vector3(dir.x * dist, dir.y * dist, 0f);

            GameObject enemy = Gamemanager.instance.pool.Get(0);
            enemy.transform.position = pos;
            enemy.GetComponent<EnemyMovement>().Init(horrorData);
        }
    }
}

[System.Serializable]
public class SpawnData
{
    public int spriteType;
    public float spawnTime;
    public int spawnCount;
    public int health;
    public float speed;
}
