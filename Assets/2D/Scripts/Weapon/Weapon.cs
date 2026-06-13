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
        player = Gamemanager.instance.player;
    }
   

    void Update()
    {
        if (Gamemanager.instance.isLive == false) return;
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
       
    }

    public void LevelUp(float damage, int count)
    {
        this.damage = damage;
        this.count += count;
        
        if(id== 0)
            Place();

        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    public void Init(ItemData data)
    {
        // Basic Set
        name = "Weapon " + data.itemId;
        transform.parent = player.transform;
        transform.localPosition=Vector3.zero;
        
        // Property Set
        id = data.itemId;
        damage = data.baseDamage;
        count = data.baseCount;

        for (int index = 0; index < Gamemanager.instance.pool.prefabs.Length; index++)
        {
            if (data.projectile == Gamemanager.instance.pool.prefabs[index])
            {
                prefabId = index;
                break;
            }
        }
        
        switch (id)
        {
            case 0:
                speed = 150f;
                Place();
                break;
            default:
                speed = 0.4f;
                break;
        }

        /*
        //Hand Set
        Hand hand = player.hands[(int)data.itemtype];
        hand.spriter.sprite=data.hand;
        hand.gameObject.SetActive(true);
        */
        player.BroadcastMessage("ApplyGear",SendMessageOptions.DontRequireReceiver);
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
