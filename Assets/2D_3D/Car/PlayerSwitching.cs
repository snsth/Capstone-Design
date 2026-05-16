using UnityEngine;

public class PlayerSwitching : MonoBehaviour
{
    [SerializeField] private GameObject carPlayer;
    [SerializeField] private GameObject carObject;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject[] prefabsToDisable;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (carPlayer)
            carPlayer.SetActive(false);

        if (carObject)
            carObject.SetActive(true);

        if (player)
            player.SetActive(true);

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
