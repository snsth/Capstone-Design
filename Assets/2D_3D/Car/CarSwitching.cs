using UnityEngine;

public class CarSwitching : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Behaviour carControlScript;
    [SerializeField] private GameObject carInterior;
    [SerializeField] private GameObject[] prefabsToDisable;

    [Header("UI")]
    [SerializeField] private GameObject enterCarText;

    private bool playerInRange;
    private bool hasEnteredCar;

    private void Start()
    {
        if (enterCarText != null)
            enterCarText.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange || hasEnteredCar)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            EnterCar();
        }
    }

    private void EnterCar()
    {
        hasEnteredCar = true;

        if (player)
            player.SetActive(false);

        if (carControlScript)
            carControlScript.enabled = true;

        if (carInterior)
            carInterior.SetActive(true);

        if (prefabsToDisable != null)
        {
            foreach (GameObject prefab in prefabsToDisable)
            {
                if (prefab)
                    prefab.SetActive(false);
            }
        }

        if (enterCarText)
            enterCarText.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (hasEnteredCar)
            return;

        playerInRange = true;

        if (enterCarText)
            enterCarText.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (enterCarText)
            enterCarText.SetActive(false);
    }
}