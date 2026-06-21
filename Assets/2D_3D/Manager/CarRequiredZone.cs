using UnityEngine;

public class CarRequiredZone : MonoBehaviour
{
    [SerializeField] private CarSwitching carSwitching;
    [SerializeField] private GameObject warningText;

    private void Start()
    {
        if (warningText != null)
            warningText.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (carSwitching.HasEnteredCar)
            return;

        if (warningText != null)
            warningText.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (warningText != null)
            warningText.SetActive(false);
    }
}