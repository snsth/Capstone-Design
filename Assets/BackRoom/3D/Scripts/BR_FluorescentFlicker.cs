using System.Collections;
using UnityEngine;

public class BR_FluorescentFlicker : MonoBehaviour
{
    [Header("깜빡일 라이트 (형광등 Light + 포인트 라이트 드래그)")]
    public Light[] lights;

    [Header("형광등 메시 Renderer (여러 개 드래그 가능)")]
    public Renderer[] targetRenderers;
    [Tooltip("-1 이면 모든 머티리얼, 특정 번호 지정 시 해당 머티리얼만 Emission 조절")]
    public int emissiveMaterialIndex = -1;

    [Header("안정 구간 (초)")]
    public float minStableTime = 2f;
    public float maxStableTime = 10f;

    [Header("깜빡임 횟수")]
    public int minFlickerCount = 1;
    public int maxFlickerCount = 6;

    [Header("깜빡임 속도 (초)")]
    public float minOffTime = 0.03f;
    public float maxOffTime  = 0.15f;
    public float minOnTime  = 0.04f;
    public float maxOnTime  = 0.12f;

    Material[][] instancedMats;
    Color[][] savedEmission;

    static readonly int HDRPEmission    = Shader.PropertyToID("_EmissiveColor");
    static readonly int BuiltinEmission = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        if (targetRenderers != null && targetRenderers.Length > 0)
        {
            instancedMats = new Material[targetRenderers.Length][];
            savedEmission = new Color[targetRenderers.Length][];

            for (int r = 0; r < targetRenderers.Length; r++)
            {
                if (targetRenderers[r] == null) continue;

                instancedMats[r] = targetRenderers[r].materials;
                savedEmission[r] = new Color[instancedMats[r].Length];

                int start = emissiveMaterialIndex < 0 ? 0 : emissiveMaterialIndex;
                int end   = emissiveMaterialIndex < 0 ? instancedMats[r].Length : emissiveMaterialIndex + 1;
                end = Mathf.Min(end, instancedMats[r].Length);

                for (int i = start; i < end; i++)
                {
                    var mat = instancedMats[r][i];
                    if (mat.HasProperty(HDRPEmission))
                        savedEmission[r][i] = mat.GetColor(HDRPEmission);
                    else if (mat.HasProperty(BuiltinEmission))
                        savedEmission[r][i] = mat.GetColor(BuiltinEmission);
                }
            }
        }

        StartCoroutine(FlickerLoop());
    }

    void SetState(bool on)
    {
        // 라이트 켜기/끄기
        if (lights != null)
            foreach (var l in lights)
                if (l != null) l.enabled = on;

        // Emission 켜기/끄기
        if (instancedMats == null) return;

        for (int r = 0; r < instancedMats.Length; r++)
        {
            if (instancedMats[r] == null) continue;

            int start = emissiveMaterialIndex < 0 ? 0 : emissiveMaterialIndex;
            int end   = emissiveMaterialIndex < 0 ? instancedMats[r].Length : emissiveMaterialIndex + 1;
            end = Mathf.Min(end, instancedMats[r].Length);

            for (int i = start; i < end; i++)
            {
                var mat = instancedMats[r][i];
                Color c = on ? savedEmission[r][i] : Color.black;
                if (mat.HasProperty(HDRPEmission))
                    mat.SetColor(HDRPEmission, c);
                else if (mat.HasProperty(BuiltinEmission))
                    mat.SetColor(BuiltinEmission, c);
            }
        }
    }

    IEnumerator FlickerLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minStableTime, maxStableTime));

            int count = Random.Range(minFlickerCount, maxFlickerCount + 1);
            for (int i = 0; i < count; i++)
            {
                SetState(false);
                yield return new WaitForSeconds(Random.Range(minOffTime, maxOffTime));

                SetState(true);
                if (i < count - 1)
                    yield return new WaitForSeconds(Random.Range(minOnTime, maxOnTime));
            }
        }
    }
}
