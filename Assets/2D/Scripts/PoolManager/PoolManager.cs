using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오브젝트 풀링 매니저.
/// Instantiate/Destroy 반복 대신 오브젝트를 재활용하여 GC 부하와 성능 비용을 줄인다.
/// prefabs 배열의 인덱스 번호로 풀을 구분한다. (예: 0=Enemy1, 1=Enemy2, 2=Enemy3)
/// </summary>
public class PoolManager : MonoBehaviour
{
    /// <summary>
    /// 풀링할 프리팹 목록. 인스펙터에서 순서대로 등록.
    /// Get(index) 호출 시 이 배열의 인덱스에 해당하는 프리팹이 사용된다.
    /// </summary>
    public GameObject[] prefabs;

    /// <summary>
    /// 각 프리팹에 대응하는 오브젝트 풀 리스트 배열.
    /// pools[0] = prefabs[0]의 풀, pools[1] = prefabs[1]의 풀, ...
    /// </summary>
    List<GameObject>[] pools;

    void Awake()
    {
        // prefabs 배열 크기만큼 풀 리스트 배열 초기화
        pools = new List<GameObject>[prefabs.Length];

        for (int index = 0; index < pools.Length; index++)
        {
            pools[index] = new List<GameObject>();
        }

        Debug.Log(pools.Length);
    }

    /// <summary>
    /// 지정한 인덱스의 풀에서 오브젝트를 하나 꺼내 반환한다.
    /// 비활성화된 오브젝트가 있으면 재사용하고, 없으면 새로 생성해서 풀에 추가한다.
    /// </summary>
    /// <param name="index">사용할 프리팹의 인덱스 (prefabs 배열 기준)</param>
    /// <returns>활성화된 게임오브젝트</returns>
    public GameObject Get(int index)
    {
        GameObject select = null;

        // 해당 풀에서 비활성화된(현재 사용 중이지 않은) 오브젝트 탐색
        foreach (GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {
                // 비활성화 오브젝트 발견 시 재활성화 후 반환
                select = item;
                select.SetActive(true);
                break;
            }
        }

        // 재사용 가능한 오브젝트가 없을 경우 새로 생성
        if (select == null)
        {
            // PoolManager의 자식으로 생성하여 씬 계층 정리
            select = Instantiate(prefabs[index], transform);
            pools[index].Add(select);
        }

        return select;
    }
}