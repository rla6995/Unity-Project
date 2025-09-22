using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MergeTailController : MonoBehaviour
{
    private Transform parentHead;
    private Collider2D tailCol;

    private void Awake()
    {
        // 꼬리는 생성될 때 부모 머리를 저장해야 함(스폰 시 부모가 헤드여야 함)
        parentHead = transform.parent;
        tailCol = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        // 꼬리 태그 고정
        gameObject.tag = "MergeTail";

        // 트리거 충돌을 쓰는 경우가 많으니 안전하게 켜둠(프로젝트 설정에 맞춰 필요시 조정)
        if (tailCol != null) tailCol.isTrigger = true;
    }

    /// <summary>
    /// Wow 존에 닿자마자 꼬리 제거(풀 반환)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Wow 존은 "WowZone" 태그로 구분
        if (other.CompareTag("WowZone"))
        {
            OnTailHit();
        }
    }

    public void DetachFromHead()
    {
        // 부모에서 분리(헤드의 OnTransformChildrenChanged가 즉시 호출됨)
        if (parentHead != null && transform.parent == parentHead)
        {
            transform.SetParent(null);
        }
    }

    /// <summary>
    /// 꼬리가 파괴(판정) 되었을 때 호출
    /// </summary>
    public void OnTailHit()
    {
        // ✅ Wow 존 도달 시 점수와 효과 처리
        ProcessTailHit();
        
        // 부모에서 떼고 바로 풀 반환
        DetachFromHead();

        // 혹시 남아있는 콜라이더 중첩 방지
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        gameObject.SetActive(false);
        MultiObjectPool.Instance?.Return(gameObject);
    }

    /// <summary>
    /// 꼬리 히트 시 점수와 효과 처리
    /// </summary>
    private void ProcessTailHit()
    {
        // Wow 존 도달 시 Wow 판정으로 처리
        JudgeResult result = JudgeResult.Wow;
        
        // 점수 추가
        ScoreManager.Instance?.AddScore(result);
        
        // UI 팝업 표시
        UIManager.Instance?.ShowJudgeText(result);
        
        // 효과음 재생 (MergeTail은 2번 효과음)
        AudioManager.Instance?.PlayObjectSE(2);
        

    }
}
