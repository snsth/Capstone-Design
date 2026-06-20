using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject monsterPrefab;

    [SerializeField]
    private Transform spawnPoint;

    private bool spawned;

    public void SpawnMonster()
    {
        if (spawned)
            return;

        spawned = true;

        Instantiate(
            monsterPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
}