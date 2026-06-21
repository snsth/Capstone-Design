using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class BR_Monster : MonoBehaviour
{
    [Header("시야 감지")]
    [Tooltip("플레이어가 감지할 수 있는 최대 거리")]
    public float detectionRange = 30f;

    [Header("이동")]
    public float moveSpeed = 4f;
    [Tooltip("이 거리 이하로 접근하면 플레이어 사망")]
    public float killDistance = 1.5f;

    [Header("애니메이션")]
    [Tooltip("Animator 컴포넌트 (없으면 자식에서 자동 탐색)")]
    public Animator animator;
    [Tooltip("걷기 애니메이션 상태 이름")]
    public string walkStateName = "walking";

    NavMeshAgent agent;
    Transform player;
    Transform playerCamera;
    Camera playerCam;
    bool caught;

    // 여러 몬스터가 동시에 킬을 트리거하는 것을 막는 전역 플래그
    static bool anyKilling = false;

    void Start()
    {
        anyKilling = false; // 씬 재시작 시 정적 플래그 초기화

        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.angularSpeed = 720f;
        agent.acceleration = 100f;
        agent.stoppingDistance = killDistance;

        var pc = FindObjectOfType<BR_PlayerController>();
        if (pc != null)
        {
            player = pc.transform;
            playerCamera = pc.cameraTransform;
        }

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        playerCam = playerCamera?.GetComponent<Camera>();
    }

    void Update()
    {
        if (player == null || caught) return;

        bool observed = IsObservedByPlayer();

        if (observed)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            if (animator != null) animator.speed = 0f;
        }
        else
        {
            // NavMesh 이탈 상태에서는 벽 관통 이동 금지 — 그냥 멈춤
            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            if (animator != null)
            {
                animator.speed = 1f;
                PlayAnim(walkStateName);
            }
        }

        // 동시 킬 방지: anyKilling이 true면 이 몬스터는 트리거 안 함
        if (!caught && !anyKilling &&
            Vector3.Distance(transform.position, player.position) <= killDistance)
        {
            caught = true;
            anyKilling = true;
            StartCoroutine(KillPlayer());
        }
    }

    bool IsObservedByPlayer()
    {
        if (playerCam == null) return false;

        Vector3 monsterCenter = transform.position + Vector3.up * 1f;

        float dist = Vector3.Distance(playerCamera.position, monsterCenter);
        if (dist > detectionRange) return false;

        Vector3 vp = playerCam.WorldToViewportPoint(monsterCenter);
        if (!(vp.z > 0f && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f))
            return false;

        Vector3 dir = (monsterCenter - playerCamera.position).normalized;
        RaycastHit[] hits = Physics.RaycastAll(playerCamera.position, dir,
            dist - 0.2f, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.transform == player || hit.transform.IsChildOf(player)) continue;
            if (hit.transform.GetComponent<BR_Monster>() != null) continue;
            return false;
        }

        return true;
    }

    void PlayAnim(string stateName)
    {
        if (animator == null) return;
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            animator.Play(stateName, 0);
    }

    IEnumerator KillPlayer()
    {
        yield return new WaitForSeconds(0.5f);

        // 딜레이 동안 플레이어가 도망쳤으면 킬 취소
        if (player == null || Vector3.Distance(transform.position, player.position) > killDistance * 2.5f)
        {
            caught = false;
            anyKilling = false;
            yield break;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnDrawGizmosSelected()
    {
        if (playerCamera == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, killDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
