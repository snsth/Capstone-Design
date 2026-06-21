using UnityEngine;
using UnityEngine.AI;

public class MonsterChase : MonoBehaviour
{
    private Transform player;
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        agent.SetDestination(player.position);
        
    }
}