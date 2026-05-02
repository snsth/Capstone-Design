using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    [SerializeField] Transform[] waypoints;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float arriveDistance = 0.1f;

    int currentIndex;

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        Transform target = waypoints[currentIndex];
        if (target == null)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) <= arriveDistance)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
    }
}
