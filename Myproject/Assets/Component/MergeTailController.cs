using System.Collections;
using UnityEngine;

public class MergeTailController : MonoBehaviour
{
    private Transform parentHead;

    private void Awake()
    {
        // 꼬리는 생성될 때 부모 머리를 저장해야 함
        parentHead = transform.parent;
    }

    private void OnEnable()
    {
        // 꼬리 태그 자동 설정
        gameObject.tag = "MergeTail";
    }

    public void DetachFromHead()
    {
        if (parentHead != null && transform.parent == parentHead)
        {
            transform.SetParent(null);  // 부모에서 분리
        }
    }

    // WeaponJudgeSystem에서 꼬리를 파괴할 때 호출
public void OnTailHit()
{
    DetachFromHead();
    gameObject.SetActive(false);  // 먼저 비활성화
    MultiObjectPool.Instance?.Return(gameObject);  // 그리고 풀 반환
}
}
