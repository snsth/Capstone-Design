using UnityEngine;

public class CarSwitching : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Behaviour carControlScript;
    [SerializeField] private GameObject carInterior;
    [SerializeField] private GameObject[] prefabsToDisable;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

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
    }
}
