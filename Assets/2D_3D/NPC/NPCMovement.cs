using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    [SerializeField] Transform[] waypoints;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float arriveDistance = 0.1f;
    [SerializeField] Transform player;
    [SerializeField] Animator animator;
    [SerializeField] string walkingParam = "IsWalking";
    [SerializeField] float lookThreshold = 0.7f;
    [SerializeField] float notLookedDuration = 1f;
    [SerializeField] float lookAtSpeed = 5f;
    [SerializeField] float playerRange = 5f;

    int currentIndex;
    float notLookedTimer;

    void Update()
    {
        bool playerInRange = IsPlayerInRange();
        bool playerLooking = playerInRange && IsPlayerLooking();
        notLookedTimer = playerLooking || !playerInRange ? 0f : notLookedTimer + Time.deltaTime;
        bool shouldIdle = playerInRange && notLookedTimer >= notLookedDuration;

        if (animator != null)
        {
            animator.SetBool(walkingParam, !shouldIdle);
        }

        if (shouldIdle)
        {
            LookAtPosition(player.position);
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        Transform target = waypoints[currentIndex];
        if (target == null)
        {
            return;
        }

        LookAtPosition(target.position);
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) <= arriveDistance)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
    }

    bool IsPlayerLooking()
    {
        if (player == null)
        {
            return false;
        }

        Vector3 toNpc = (transform.position - player.position).normalized;
        return Vector3.Dot(player.forward, toNpc) >= lookThreshold;
    }

    bool IsPlayerInRange()
    {
        if (player == null)
        {
            return false;
        }

        float distance = Vector3.Distance(player.position, transform.position);
        return distance <= playerRange;
    }

    void LookAtPosition(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookAtSpeed * Time.deltaTime);
    }
}
