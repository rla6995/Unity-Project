using System.Collections;
using UnityEngine;

public class MergeHeadController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private Coroutine tailCheckCoroutine;
    private bool isStunned = false;

    private OrbitWalkingMonster orbitMover;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        orbitMover = GetComponent<OrbitWalkingMonster>();
    }

    private void OnEnable()
    {
        // ✅ 풀 재사용 시 항상 정상 주행 상태로 초기화
        isStunned = false;
        if (animator != null) animator.SetBool("IsHitLoop", false);
        orbitMover?.ResumeMovement();

        tailCheckCoroutine = null;
    }

    private void OnDisable()
    {
        // 안전망: 비활성화 시에도 이동 정지 상태가 남지 않도록
        orbitMover?.ResumeMovement();
        if (animator != null) animator.SetBool("IsHitLoop", false);
        isStunned = false;

        if (tailCheckCoroutine != null)
        {
            StopCoroutine(tailCheckCoroutine);
            tailCheckCoroutine = null;
        }
    }

    /// <summary>
    /// 머리를 스턴(loop) 상태로 진입시킴 (두 버튼 합동 입력 시)
    /// </summary>
    public void StartHitLoop()
    {
        if (isStunned) return;

        isStunned = true;
        if (animator != null) animator.SetBool("IsHitLoop", true);
        orbitMover?.PauseMovement();

        // 안전용 폴백(이벤트 누락 대비). 즉시성은 OnTransformChildrenChanged가 처리
        tailCheckCoroutine = StartCoroutine(CheckTailsCoroutine());
    }

    public void StopHitLoop()
    {
        if (!isStunned) return;

        isStunned = false;
        if (animator != null) animator.SetBool("IsHitLoop", false);
        orbitMover?.ResumeMovement();

        if (tailCheckCoroutine != null)
        {
            StopCoroutine(tailCheckCoroutine);
            tailCheckCoroutine = null;
        }
    }

    /// <summary>
    /// 자식(꼬리) 변동 시 즉시 확인해 꼬리가 0개면 바로 풀 반환
    /// </summary>
    private void OnTransformChildrenChanged()
    {
        if (!HasAnyTail())
        {
            // 반환 직전 정상 상태로 되돌려 두기
            StopHitLoop();
            MultiObjectPool.Instance?.Return(gameObject);
        }
    }

    private bool HasAnyTail()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("MergeTail"))
                return true;
        }
        return false;
    }

    /// <summary>
    /// (폴백) 주기적으로 꼬리 유무 점검 — 이벤트가 못 잡힌 특수 상황 대비
    /// </summary>
    private IEnumerator CheckTailsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);

            if (!HasAnyTail())
            {
                StopHitLoop();
                MultiObjectPool.Instance?.Return(gameObject);
                yield break;
            }
        }
    }
}
