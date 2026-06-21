using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BR_FluorescentFlicker : MonoBehaviour
{
    [Header("깜빡일 라이트 (비워두면 이름으로 자동 탐색)")]
    public Light[] lights;

    [Header("형광등 메시 Renderer (비워두면 이름으로 자동 탐색)")]
    public Renderer[] targetRenderers;

    [Header("자동 탐색 오브젝트 이름 (번호 붙은 사본도 자동 포함)")]
    public string[] rendererObjectNames = { "archway_corner", "Ceiling_light", "Wall_light" };
    public string[] lightObjectNames    = { "Point Light" };

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
        AutoFindObjects();

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

    void AutoFindObjects()
    {
        // Renderer 자동 탐색 — Inspector에 이미 채워져 있으면 건너뜀
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            var found = new List<Renderer>();
            var allRenderers = GetComponentsInChildren<Renderer>(true);
            foreach (var r in allRenderers)
            {
                foreach (var name in rendererObjectNames)
                {
                    // "Ceiling_light", "Ceiling_light (1)", "Ceiling_light (2)" 등 전부 포함
                    if (r.gameObject.name == name || r.gameObject.name.StartsWith(name + " ("))
                    {
                        found.Add(r);
                        break;
                    }
                }
            }
            if (found.Count > 0)
                targetRenderers = found.ToArray();
        }

        // Light 자동 탐색 — Inspector에 이미 채워져 있으면 건너뜀
        if (lights == null || lights.Length == 0)
        {
            var found = new List<Light>();
            var allLights = GetComponentsInChildren<Light>(true);
            foreach (var l in allLights)
            {
                foreach (var name in lightObjectNames)
                {
                    if (l.gameObject.name == name || l.gameObject.name.StartsWith(name + " ("))
                    {
                        found.Add(l);
                        break;
                    }
                }
            }
            if (found.Count > 0)
                lights = found.ToArray();
        }
    }

    void SetState(bool on)
    {
        if (lights != null)
            foreach (var l in lights)
                if (l != null) l.enabled = on;

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
