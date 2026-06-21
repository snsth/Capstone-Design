using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BR_FluorescentFlicker : MonoBehaviour
{
    [Header("자동 탐색 키워드")]
    public string[] rendererObjectNames = { "archway_corner", "Ceiling_light", "Wall_light" };
    public string[] lightObjectNames    = { "Point Light" };

    [Tooltip("-1 이면 모든 머티리얼, 특정 번호 지정 시 해당 머티리얼만 Emission 조절")]
    public int emissiveMaterialIndex = -1;

    [Header("개별 깜빡임 간격 (초)")]
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

    [Header("암전")]
    public float blackoutMinInterval = 20f;
    public float blackoutMaxInterval = 60f;
    public float blackoutDuration    = 3f;

    static readonly int HDRPEmission    = Shader.PropertyToID("_EmissiveColor");
    static readonly int BuiltinEmission = Shader.PropertyToID("_EmissionColor");

    class FlickerUnit
    {
        public Light light;
        public Renderer renderer;
        public Material[] mats;
        public Color[] savedEmission;
        public bool blackedOut;
    }

    List<FlickerUnit> units = new();
    bool globalBlackout;

    void Start()
    {
        BuildUnits();
        foreach (var unit in units)
            StartCoroutine(UnitFlickerLoop(unit));
        StartCoroutine(BlackoutLoop());
    }

    void BuildUnits()
    {
        var renderers = new List<Renderer>();
        var lights    = new List<Light>();

        foreach (var r in FindObjectsOfType<Renderer>(true))
            foreach (var kw in rendererObjectNames)
                if (r.gameObject.name.Contains(kw)) { renderers.Add(r); break; }

        foreach (var l in FindObjectsOfType<Light>(true))
            foreach (var kw in lightObjectNames)
                if (l.gameObject.name.Contains(kw)) { lights.Add(l); break; }

        var pairedLights = new HashSet<Light>();

        // Renderer 기준으로 유닛 생성 — 가장 가까운 Point Light와 페어링
        foreach (var r in renderers)
        {
            var unit = new FlickerUnit { renderer = r };
            var mats = r.materials;
            unit.mats = mats;
            unit.savedEmission = new Color[mats.Length];

            int start = emissiveMaterialIndex < 0 ? 0 : emissiveMaterialIndex;
            int end   = emissiveMaterialIndex < 0 ? mats.Length : Mathf.Min(emissiveMaterialIndex + 1, mats.Length);
            for (int i = start; i < end; i++)
            {
                if (mats[i].HasProperty(HDRPEmission))
                    unit.savedEmission[i] = mats[i].GetColor(HDRPEmission);
                else if (mats[i].HasProperty(BuiltinEmission))
                    unit.savedEmission[i] = mats[i].GetColor(BuiltinEmission);
            }

            // 가장 가까운 미페어링 Point Light 찾기
            Light nearest = null;
            float nearestDist = float.MaxValue;
            foreach (var l in lights)
            {
                if (pairedLights.Contains(l)) continue;
                float d = Vector3.Distance(r.transform.position, l.transform.position);
                if (d < nearestDist) { nearestDist = d; nearest = l; }
            }
            if (nearest != null)
            {
                unit.light = nearest;
                pairedLights.Add(nearest);
            }

            units.Add(unit);
        }

        // 페어링 안 된 남은 Light는 단독 유닛으로 추가
        foreach (var l in lights)
            if (!pairedLights.Contains(l))
                units.Add(new FlickerUnit { light = l });
    }

    void SetUnit(FlickerUnit unit, bool on)
    {
        if (unit.light != null)
            unit.light.enabled = on;

        if (unit.renderer != null && unit.mats != null)
        {
            int start = emissiveMaterialIndex < 0 ? 0 : emissiveMaterialIndex;
            int end   = emissiveMaterialIndex < 0 ? unit.mats.Length : Mathf.Min(emissiveMaterialIndex + 1, unit.mats.Length);
            for (int i = start; i < end; i++)
            {
                Color c = on ? unit.savedEmission[i] : Color.black;
                if (unit.mats[i].HasProperty(HDRPEmission))
                    unit.mats[i].SetColor(HDRPEmission, c);
                else if (unit.mats[i].HasProperty(BuiltinEmission))
                    unit.mats[i].SetColor(BuiltinEmission, c);
            }
        }
    }

    IEnumerator UnitFlickerLoop(FlickerUnit unit)
    {
        // 시작 타이밍 분산
        yield return new WaitForSeconds(Random.Range(0f, maxStableTime));

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minStableTime, maxStableTime));

            if (globalBlackout) { yield return new WaitUntil(() => !globalBlackout); continue; }

            int count = Random.Range(minFlickerCount, maxFlickerCount + 1);
            for (int i = 0; i < count; i++)
            {
                SetUnit(unit, false);
                yield return new WaitForSeconds(Random.Range(minOffTime, maxOffTime));
                if (!globalBlackout) SetUnit(unit, true);
                if (i < count - 1)
                    yield return new WaitForSeconds(Random.Range(minOnTime, maxOnTime));
            }
        }
    }

    IEnumerator BlackoutLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(blackoutMinInterval, blackoutMaxInterval));

            globalBlackout = true;
            foreach (var unit in units) SetUnit(unit, false);

            yield return new WaitForSeconds(blackoutDuration);

            globalBlackout = false;
            foreach (var unit in units) SetUnit(unit, true);
        }
    }
}
