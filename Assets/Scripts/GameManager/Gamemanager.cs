using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 전체를 관리하는 싱글톤 매니저.
/// 씬 어디서든 Gamemanager.instance 로 접근하여 플레이어, 풀 매니저 등 핵심 참조를 가져올 수 있다.
/// </summary>
public class Gamemanager : MonoBehaviour
{
    /// <summary>
    /// 싱글톤 인스턴스. 씬 전체에서 하나만 존재한다.
    /// </summary>
    public static Gamemanager instance;

    [Header("# Game Control")]
    public float gameTime;
    public float maxGameTime = 4 * 10f;
    
    [Header("# Player Info")]
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = {10, 30 , 60 ,100, 150, 210, 280, 360, 450, 550};

    /// <summary>
    /// 플레이어 컨트롤러 참조. 이동 입력 벡터 등을 다른 스크립트에서 읽을 때 사용.
    /// </summary>
    [Header("# Game Object Info")]
    public PlayerController player;

    /// <summary>
    /// 오브젝트 풀 매니저 참조. 적 스폰 등 풀에서 오브젝트를 꺼낼 때 사용.
    /// </summary>
    public PoolManager pool;

    void Awake()
    {
        // 싱글톤 초기화: 이 오브젝트를 전역 인스턴스로 등록
        instance = this;
    }
    
    void Update()
    {
        
        gameTime += Time.deltaTime;

        
        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
        }
    }

    public void GetExp()
    {
        exp++;
        if (exp == nextExp[level])
        {
            level++;
            exp = 0;
            
        }
    }
}