using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BR_FluorescentFlicker : MonoBehaviour
{
    [Header("자동 탐색 키워드")]
    public string[] rendererObjectNames = { "archway_corner", "Ceiling_light", "Wall_light" };
    public string[] lightObjectNames    = { "Point Light", "Light", "Light 2" };

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

    [Header("페어링")]
    [Tooltip("렌더러 주변 이 거리 안의 Point Light를 같이 깜빡임")]
    public float maxPairDistance = 8f;

    [Header("암전")]
    public float blackoutMinInterval = 20f;
    public float blackoutMaxInterval = 60f;
    public float blackoutMinDuration = 3f;
    public float blackoutMaxDuration = 5f;

    static readonly int HDRPEmission    = Shader.PropertyToID("_EmissiveColor");
    static readonly int BuiltinEmission = Shader.PropertyToID("_EmissionColor");

    class FlickerUnit
    {
        public List<Light> lights = new();
        public Renderer renderer;
        public Material[] mats;
        public Color[] savedEmission;
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
        var allRenderers = new List<Renderer>();
        var allLights    = new List<Light>();

        foreach (var r in FindObjectsOfType<Renderer>(true))
            foreach (var kw in rendererObjectNames)
                if (r.gameObject.name.IndexOf(kw, System.StringComparison.OrdinalIgnoreCase) >= 0)
                { allRenderers.Add(r); break; }

        foreach (var l in FindObjectsOfType<Light>(true))
        {
            Transform t = l.transform;
            bool added = false;
            while (t != null && !added)
            {
                foreach (var kw in lightObjectNames)
                {
                    if (t.gameObject.name.IndexOf(kw, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    { allLights.Add(l); added = true; break; }
                }
                t = t.parent;
            }
        }

        var pairedLights = new HashSet<Light>();

        // 렌더러마다 유닛 생성 — maxPairDistance 안의 라이트를 전부 페어링
        foreach (var r in allRenderers)
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

            // 거리 안의 모든 라이트 추가 (이미 페어링된 라이트 포함 — 여러 형광등이 같은 라이트 공유 가능)
            foreach (var l in allLights)
            {
                if (Vector3.Distance(r.transform.position, l.transform.position) <= maxPairDistance)
                {
                    unit.lights.Add(l);
                    pairedLights.Add(l);
                }
            }

            units.Add(unit);
        }

        // 어떤 렌더러와도 페어링 안 된 라이트는 단독 유닛으로 추가
        foreach (var l in allLights)
            if (!pairedLights.Contains(l))
                units.Add(new FlickerUnit { lights = new List<Light> { l } });
    }

    void SetUnit(FlickerUnit unit, bool on)
    {
        foreach (var l in unit.lights)
            if (l != null) l.enabled = on;

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

            var allLights = FindObjectsOfType<Light>(true);
            var wasEnabled = new bool[allLights.Length];
            for (int i = 0; i < allLights.Length; i++)
            {
                wasEnabled[i] = allLights[i].enabled;
                allLights[i].enabled = false;
            }

            globalBlackout = true;
            foreach (var unit in units) SetUnit(unit, false);

            yield return new WaitForSeconds(Random.Range(blackoutMinDuration, blackoutMaxDuration));

            for (int i = 0; i < allLights.Length; i++)
                allLights[i].enabled = wasEnabled[i];

            globalBlackout = false;
            foreach (var unit in units) SetUnit(unit, true);
        }
    }
}
