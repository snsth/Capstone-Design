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

    void Start()
    {
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
            // 애니메이터 속도 0 = 현재 프레임에서 즉시 동결
            if (animator != null) animator.speed = 0f;
        }
        else
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            else
            {
                Vector3 dir = (player.position - transform.position).normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
                transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            }
            if (animator != null)
            {
                animator.speed = 1f;
                PlayAnim(walkStateName);
            }
        }

        if (!caught && Vector3.Distance(transform.position, player.position) <= killDistance)
        {
            caught = true;
            StartCoroutine(KillPlayer());
        }
    }

    bool IsObservedByPlayer()
    {
        if (playerCam == null) return false;

        Vector3 monsterCenter = transform.position + Vector3.up * 1f;

        // 1) 거리 체크
        float dist = Vector3.Distance(playerCamera.position, monsterCenter);
        if (dist > detectionRange) return false;

        // 2) 뷰포트 체크 — 화면 밖이면 이동 가능
        Vector3 vp = playerCam.WorldToViewportPoint(monsterCenter);
        if (!(vp.z > 0f && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f))
            return false;

        // 3) 벽 감지 — 몬스터 직전까지 레이를 쏴서 뭔가 맞으면 벽 뒤 = 이동 가능
        Vector3 dir = (monsterCenter - playerCamera.position).normalized;
        RaycastHit[] hits = Physics.RaycastAll(playerCamera.position, dir,
            dist - 0.2f, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            // 플레이어 자신은 무시
            if (hit.transform == player || hit.transform.IsChildOf(player)) continue;
            // 다른 몬스터도 무시 (몬스터끼리 서로 가리지 않도록)
            if (hit.transform.GetComponent<BR_Monster>() != null) continue;
            // 그 외 충돌 = 벽 → 이동 가능
            return false;
        }

        // 아무것도 막지 않음 = 시야 확보 → 정지
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
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 씬 에디터에서 시야각 시각화
    void OnDrawGizmosSelected()
    {
        if (playerCamera == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, killDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
