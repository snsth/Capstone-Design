using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    public float scanRange;
    public LayerMask targetLayer;
    public RaycastHit2D[] targets;
    public Transform neareastTarget;

    // 기법2 무기정지: true이면 탐지 중단 → neareastTarget = null → Weapon 자동발사 멈춤
    public static bool isDisabled = false;

    private void FixedUpdate()
    {
        if (isDisabled)
        {
            targets = new RaycastHit2D[0];
            neareastTarget = null;
            return;
        }

        targets=Physics2D.CircleCastAll(transform.position,scanRange,Vector2.zero,0,targetLayer);
        neareastTarget = GetNearest();
    }

    Transform GetNearest()
    {
        Transform result = null;
        float diff = 100;

        foreach (RaycastHit2D target in targets)
        {
            Vector3 myPos=transform.position;
            Vector3 targetPos=target.transform.position;
            float curDiff = Vector3.Distance(myPos, targetPos);

            if (curDiff < diff)
            {
                diff = curDiff;
                result = target.transform;
            }
            
        }

        return result;
    }
}
