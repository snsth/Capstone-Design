using UnityEngine;

public class PlayerSwitching : MonoBehaviour
{
    [SerializeField] private GameObject carPlayer;
    [SerializeField] private GameObject carObject;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject[] prefabsToDisable;
    [SerializeField]
    private Transform exitPoint;

    [Header("UI")]
    [SerializeField] private GameObject exitCarText;

    private bool playerInRange;
    private bool hasExitedCar;

    private void Start()
    {
        if (exitCarText != null)
            exitCarText.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange || hasExitedCar)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            ExitCar();
        }
    }

    private void ExitCar()
    {
        hasExitedCar = true;

        if (carPlayer)
            carPlayer.SetActive(false);

        if (carObject)
            carObject.SetActive(true);

        if (player)
        {
            player.transform.position = exitPoint.position;
            player.transform.rotation = exitPoint.rotation;

            player.SetActive(true);
        }

        if (prefabsToDisable != null)
        {
            foreach (GameObject prefab in prefabsToDisable)
            {
                if (prefab)
                    prefab.SetActive(false);
            }
        }

        if (exitCarText)
            exitCarText.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (hasExitedCar)
            return;

        playerInRange = true;

        if (exitCarText)
            exitCarText.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (exitCarText)
            exitCarText.SetActive(false);
    }
}