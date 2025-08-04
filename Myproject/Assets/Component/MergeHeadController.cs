using System.Collections;
using UnityEngine;

public class MergeHeadController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private Coroutine tailCheckCoroutine;
    private bool isStunned = false;

    private OrbitWalkingMonster orbitMover; // ✅ 추가

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        orbitMover = GetComponent<OrbitWalkingMonster>(); // ✅ 연결
    }

    public void StartHitLoop()
    {
        if (isStunned) return;

        isStunned = true;
        animator.SetBool("IsHitLoop", true);
        orbitMover?.PauseMovement();  // ✅ 이동 정지

        tailCheckCoroutine = StartCoroutine(CheckTailsCoroutine());
    }

    public void StopHitLoop()
    {
        if (!isStunned) return;

        isStunned = false;
        animator.SetBool("IsHitLoop", false);
        orbitMover?.ResumeMovement();  // ✅ 이동 재개

        if (tailCheckCoroutine != null)
            StopCoroutine(tailCheckCoroutine);
    }

    private IEnumerator CheckTailsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);

            bool hasTail = false;
            foreach (Transform child in transform)
            {
                if (child.CompareTag("MergeTail"))
                {
                    hasTail = true;
                    break;
                }
            }

            if (!hasTail)
            {
                MultiObjectPool.Instance?.Return(gameObject);
                yield break;
            }
        }
    }
}
