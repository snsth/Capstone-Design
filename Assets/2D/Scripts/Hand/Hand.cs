using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public bool isLeft;
    public SpriteRenderer spriter;

    SpriteRenderer player;

    Vector3 rightPos=new Vector3(0.575f,-0.191f,-45f);
    Vector3 rightPosReverse=new Vector3(-0.575f,0.191f,45f);
    Quaternion leftRot=Quaternion.Euler(0,0,-90);
    Quaternion leftReverse=Quaternion.Euler(0,0,-190);

    void Awake()
    {
        player = GetComponentsInParent<SpriteRenderer>()[1];
    }

    void LateUpdate()
    {
        bool isReverse = player.flipX;

        if (isLeft) // 근접무기
        {
            transform.localRotation = isReverse ? leftReverse : leftRot;
            spriter.flipY=isReverse;
            spriter.sortingOrder =isReverse ? 4:6;
        }
        else // 원거리 무기 
        {
            transform.localPosition = isReverse ? rightPosReverse : rightPos;
            spriter.flipX = isReverse;
            spriter.sortingOrder =isReverse ? 6:4;
        }
    }

}
