using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public bool isLive;

    [Header("# Player Info")] 
    public float health;
    public float maxHealth;
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = {10, 30 , 60 ,100, 150, 210, 280, 360, 450, 550};

    /// <summary>
    /// 플레이어 컨트롤러 참조. 이동 입력 벡터 등을 다른 스크립트에서 읽을 때 사용.
    /// </summary>
    [Header("# Game Object Info")]
    public PlayerController player;
    public LevelUp uilevelUp;
    public Result UIResult;
    /// <summary>
    /// 오브젝트 풀 매니저 참조. 적 스폰 등 풀에서 오브젝트를 꺼낼 때 사용.
    /// </summary>
    public PoolManager pool;
    void Awake()
    {
        // 싱글톤 초기화: 이 오브젝트를 전역 인스턴스로 등록
        instance = this;
    }

    public void GameStart()
    {
        health = maxHealth;
        // 임시
        uilevelUp.Select(0);
        Resume();

        AudioManager.instance.PlaySfx(AudioManager.SFX.Select);
        AudioManager.instance.PlayBgm(true);
    }
    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        isLive = false;

        yield return new WaitForSeconds(0.5f);

        // SFX 전부 정지 + 공포 이벤트 상태 초기화 → Lose 사운드가 차단되지 않도록
        AudioManager.instance.StopAllSfx();
        UIResult.gameObject.SetActive(true);
        UIResult.Lose();
        Stop();
        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.SFX.Lose);
    }

     public void GameVictory()
    {
        StartCoroutine(GameVictoryRoutine());
    }

    IEnumerator GameVictoryRoutine()
    {
        isLive = false;

        yield return new WaitForSeconds(0.5f);

        AudioManager.instance.StopAllSfx();
        UIResult.gameObject.SetActive(true);
        UIResult.Win();
        Stop();
        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.SFX.Win);
    }
    public void GameRetry()
    {
        SceneManager.LoadScene(0);
    }
    void Update()
    {
        if (!isLive) return;
        gameTime += Time.deltaTime;

        
        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
            GameVictory();
        }
    }

    public void GetExp()
    {
        if(isLive == false) return;
        exp++;
        if (exp == nextExp[Mathf.Min(level, nextExp.Length - 1)])
        {
            level++;
            exp = 0;
            uilevelUp.Show();
        }
    }

    public void Stop()
    {
        isLive = false;
        Time.timeScale =0f;
    }

    public void Resume()
    {
        isLive = true;
        Time.timeScale =1f;
    }
}