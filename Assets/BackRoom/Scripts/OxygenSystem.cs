using UnityEngine;

public class OxygenSystem : MonoBehaviour
{
    [Header("Oxygen Settings")]
    public float maxOxygen = 100f;
    public float drainRatePerSecond = 8f;
    public float rechargeRatePerSecond = 25f;

    float currentOxygen;
    bool isDraining;
    bool isRefilling;

    public float CurrentOxygen => currentOxygen;
    public float MaxOxygen => maxOxygen;
    public float OxygenPercent => currentOxygen / maxOxygen;
    public bool IsDepleted => currentOxygen <= 0f;

    public System.Action OnOxygenDepleted;
    public System.Action OnOxygenFull;

    void Start()
    {
        currentOxygen = maxOxygen;
    }

    void Update()
    {
        if (isDraining)
        {
            currentOxygen = Mathf.Max(0f, currentOxygen - drainRatePerSecond * Time.deltaTime);
            if (currentOxygen <= 0f)
                OnOxygenDepleted?.Invoke();
        }
        else if (isRefilling)
        {
            bool wasFull = currentOxygen >= maxOxygen;
            currentOxygen = Mathf.Min(maxOxygen, currentOxygen + rechargeRatePerSecond * Time.deltaTime);
            if (!wasFull && currentOxygen >= maxOxygen)
                OnOxygenFull?.Invoke();
        }
    }

    public void StartDraining()
    {
        isDraining = true;
        isRefilling = false;
    }

    public void StartRefilling()
    {
        isDraining = false;
        isRefilling = true;
    }

    public void StopAll()
    {
        isDraining = false;
        isRefilling = false;
    }
}
