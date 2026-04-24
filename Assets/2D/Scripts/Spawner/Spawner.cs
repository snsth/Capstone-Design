using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 일정 시간마다 적을 스폰하는 스포너.
/// 자식 오브젝트의 Transform을 스폰 포인트로 사용하며,
/// PoolManager를 통해 오브젝트 풀에서 적을 꺼내 배치한다.
/// </summary>
public class Spawner : MonoBehaviour
{
    /// <summary>
    /// 적이 등장할 수 있는 스폰 포인트 목록.
    /// Awake에서 자식 Transform을 자동으로 수집한다.
    /// </summary>
    public Transform[] spawnPoints;
    public SpawnData[]  spawnData;

    /// <summary>
    /// 스폰 간격을 측정하기 위한 타이머 (초 단위).
    /// </summary>
    float timer;

    int level;
    

    void Awake()
    {
        // 이 오브젝트의 모든 자식 Transform을 스폰 포인트로 등록
        // 인덱스 0은 자기 자신(부모)이므로 Spawn()에서 1부터 사용
        spawnPoints = GetComponentsInChildren<Transform>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        level = Mathf.Min(Mathf.FloorToInt(Gamemanager.instance.gameTime / 10f), spawnData.Length - 1);

        // 0.5초마다 적 스폰
        if (timer > (spawnData[level].spawnTime))
        {
            Spawn();
            timer = 0;
        }
    }

    void Spawn()
    {
        // 풀에서 0~3 인덱스 중 랜덤으로 적 종류 선택 (Enemy1, Enemy2, Enemy3)
        // Random.Range(int, int)는 최댓값을 포함하지 않으므로 3으로 설정
        GameObject enemy = Gamemanager.instance.pool.Get(0);

        // 스폰 포인트 중 랜덤 위치에 배치 (인덱스 0은 부모 자신이므로 1부터 시작)
        enemy.transform.position = spawnPoints[Random.Range(1, spawnPoints.Length)].position;
        
        enemy.GetComponent<EnemyMovement>().Init(spawnData[level]);
    }
}
[System.Serializable]
public class SpawnData
{
    public int spriteType;
    public float spawnTime;
    public int health;
    public float speed;
    
}