using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int id;
    public int prefabId;
    public float damage;
    public int count; // 무기 배치 갯수
    public float speed; // 무기 회전 속도

    float timer;
    PlayerController player;

    void Awake()
    {
        player = GetComponentInParent<PlayerController>();
    }
    void Start()
    {
        Init();   
    }

    void Update()
    {
        switch (id)
        {
            case 0:
                transform.Rotate(Vector3.back*speed*Time.deltaTime);
                break;
            default:
                timer += Time.deltaTime;

                if (timer > speed)
                {
                    timer = 0f;
                    Fire();
                }
                break;
        }
        // test
        if (Input.GetButtonDown("Jump"))
        {
            LevelUp(20,5);
        }
    }

    public void LevelUp(float damage, int count)
    {
        this.damage = damage;
        this.count += count;
        
        if(id== 0)
            Place();
    }

    public void Init()
    {
        switch (id)
        {
            case 0:
                speed = 150f;
                Place();
                break;
            default:
                speed = 0.3f;
                break;
        }
    }

    void Place()
    {
        for (int index = 0; index < count; index++)
        {
            Transform bullet;
            if (index < transform.childCount)
            {
                bullet = transform.GetChild(index);
            }
            else
            {
                bullet = Gamemanager.instance.pool.Get(prefabId).transform;
                bullet.parent = transform;
            }
               
            
            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;
            
            Vector3 rotaeVector = Vector3.forward * 360 * index/count;
            bullet.Rotate(rotaeVector);
            bullet.Translate(bullet.up * 1.5f, Space.World);
            
            bullet.GetComponent<Bullet>().Init(damage, -1,Vector3.zero); // -1 는 무한으로 관통

        }
    }


    void Fire()
    {
        if(!player.scanner.neareastTarget)
            return;
        
        
        Vector3 targetPos = player.scanner.neareastTarget.position;
        Vector3 dir=targetPos-transform.position;
        dir = dir.normalized;
        
        Transform bullet =Gamemanager.instance.pool.Get(prefabId).transform;
        bullet.position=transform.position;
        bullet.rotation=Quaternion.FromToRotation(Vector3.up,dir);
        bullet.GetComponent<Bullet>().Init(damage, 0, dir);
    }
    
}
