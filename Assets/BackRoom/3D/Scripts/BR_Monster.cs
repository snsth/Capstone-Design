using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class BR_Monster : MonoBehaviour
{
    [Header("시야 감지")]
    [Tooltip("이 거리 안에서 플레이어가 바라보면 몬스터 동결")]
    public float detectionRange = 40f;

    [Header("이동")]
    public float moveSpeed = 4f;
    [Tooltip("이 거리 이내로 플레이어가 접근하면 추격 시작")]
    public float chaseRange = 500f;
    [Tooltip("이 거리 이하로 접근하면 플레이어 사망")]
    public float killDistance = 1.5f;

    [Header("죽음 연출")]
    [Tooltip("몬스터 머리 높이 (월드 Y 오프셋)")]
    public float headHeight = 1.7f;
    [Tooltip("클로즈업 카메라와 머리 사이 거리")]
    public float closeupDistance = 0.55f;
    [Tooltip("클로즈업 이동 시간(초)")]
    public float closeupDuration = 0.4f;
    [Tooltip("비명 후 씬 리로드까지 대기 시간(초)")]
    public float deathHoldTime = 1.2f;
    [Tooltip("이동 중 흔들림 세기")]
    public float shakeIntensityMove = 0.14f;
    [Tooltip("유지 중 흔들림 세기")]
    public float shakeIntensityHold = 0.07f;

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

    // 문 열린 뒤 모든 몬스터를 강제 추격으로 전환하는 전역 플래그
    public static bool alwaysChase = false;
    // 여러 몬스터가 동시에 킬을 트리거하는 것을 막는 전역 플래그
    static bool anyKilling = false;

    void Start()
    {
        // alwaysChase는 씬 로드 시점에만 초기화 (문 열린 뒤 스폰된 몬스터가 리셋하지 않도록
        // anyKilling만 초기화)
        anyKilling = false;

        agent = GetComponent<NavMeshAgent>();
        agent.speed            = moveSpeed;
        agent.angularSpeed     = 720f;
        agent.acceleration     = 100f;
        agent.stoppingDistance = killDistance;

        var pc = FindObjectOfType<BR_PlayerController>();
        if (pc != null)
        {
            player       = pc.transform;
            playerCamera = pc.cameraTransform;
        }

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        playerCam = playerCamera?.GetComponent<Camera>();
    }

    void Update()
    {
        if (player == null || caught) return;

        bool observed = !alwaysChase && IsObservedByPlayer();

        if (observed)
        {
            if (agent.isOnNavMesh) { agent.isStopped = true; agent.velocity = Vector3.zero; }
            if (animator != null) animator.speed = 0f;
        }
        else
        {
            bool inChaseRange = Vector3.Distance(transform.position, player.position) <= chaseRange;

            if (agent.isOnNavMesh)
            {
                agent.isStopped = !inChaseRange;
                if (inChaseRange) agent.SetDestination(player.position);
                else agent.velocity = Vector3.zero;
            }

            if (animator != null)
            {
                animator.speed = inChaseRange ? 1f : 0f;
                if (inChaseRange) PlayAnim(walkStateName);
            }
        }

        if (!anyKilling && Vector3.Distance(transform.position, player.position) <= killDistance)
        {
            caught     = true;
            anyKilling = true;
            StartCoroutine(KillPlayer());
        }
    }

    bool IsObservedByPlayer()
    {
        if (playerCam == null) return false;

        Vector3[] checkPoints =
        {
            transform.position + Vector3.up * 0.2f,
            transform.position + Vector3.up * 1.0f,
            transform.position + Vector3.up * 1.8f,
        };

        foreach (var point in checkPoints)
        {
            float dist = Vector3.Distance(playerCamera.position, point);
            if (dist > detectionRange) continue;

            Vector3 vp = playerCam.WorldToViewportPoint(point);
            if (!(vp.z > 0f && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f))
                continue;

            Vector3 dir = (point - playerCamera.position).normalized;
            RaycastHit[] hits = Physics.RaycastAll(playerCamera.position, dir,
                dist - 0.2f, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            bool blocked = false;
            foreach (var hit in hits)
            {
                if (hit.transform == player || hit.transform.IsChildOf(player)) continue;
                if (hit.transform.GetComponent<BR_Monster>() != null) continue;
                blocked = true;
                break;
            }

            if (!blocked) return true;
        }

        return false;
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

        if (player == null || Vector3.Distance(transform.position, player.position) > killDistance * 2.5f)
        {
            caught     = false;
            anyKilling = false;
            yield break;
        }

        // 플레이어 입력 차단 + 몬스터 정지
        var pc = player.GetComponent<BR_PlayerController>();
        if (pc != null) pc.enabled = false;
        if (agent.isOnNavMesh) { agent.isStopped = true; agent.velocity = Vector3.zero; }

        if (playerCamera != null)
        {
            Vector3    headPos   = transform.position + Vector3.up * headHeight;
            Vector3    camTarget = headPos + transform.forward * closeupDistance;
            Quaternion rotTarget = Quaternion.LookRotation(headPos - camTarget);

            playerCamera.SetParent(null);

            // Phase 1: 클로즈업으로 빠르게 이동 + 강한 흔들기
            float      elapsed  = 0f;
            Vector3    startPos = playerCamera.position;
            Quaternion startRot = playerCamera.rotation;

            while (elapsed < closeupDuration)
            {
                elapsed += Time.deltaTime;
                float   t        = Mathf.SmoothStep(0f, 1f, elapsed / closeupDuration);
                Vector3 shake    = Random.insideUnitSphere * shakeIntensityMove;
                Vector3 rotShake = new Vector3(
                    Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f))
                    * shakeIntensityMove * 40f;

                playerCamera.position = Vector3.Lerp(startPos, camTarget, t) + shake;
                playerCamera.rotation = Quaternion.Slerp(startRot, rotTarget, t)
                                      * Quaternion.Euler(rotShake);
                yield return null;
            }

            BR_SoundManager.Instance?.PlayZombieScream();

            // Phase 2: 클로즈업 유지 + 서서히 약해지는 흔들기
            elapsed = 0f;
            while (elapsed < deathHoldTime)
            {
                elapsed += Time.deltaTime;
                float   mag      = shakeIntensityHold * (1f - Mathf.Clamp01(elapsed / deathHoldTime));
                Vector3 shake    = Random.insideUnitSphere * mag;
                Vector3 rotShake = new Vector3(
                    Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f))
                    * mag * 30f;

                playerCamera.position = camTarget + shake;
                playerCamera.rotation = rotTarget * Quaternion.Euler(rotShake);
                yield return null;
            }
        }
        else
        {
            BR_SoundManager.Instance?.PlayZombieScream();
            yield return new WaitForSeconds(deathHoldTime);
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
