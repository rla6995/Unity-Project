using UnityEngine;

public enum JudgeResult { Bad, Nice, Wow }

/// <summary>
/// 타이밍 판정 시스템
/// 단일 책임 원칙: 타이밍 판정만 담당
/// 의존성 역전 원칙: ITimingJudge 인터페이스 구현
/// </summary>
public class TimingJudgeSystem : MonoBehaviour, ITimingJudge
{
    [Header("판정 영역 설정")]
    public Transform judgeCenter;       // 판정 기준 위치
    public Collider2D wowCollider;      // Wow 판정용 콜라이더 (BoxCollider2D 권장)
    public Collider2D niceCollider;     // Nice 판정용 콜라이더

    public Transform JudgeCenter => judgeCenter;
    public Collider2D NiceCollider => niceCollider;

    public JudgeResult GetJudgeResult(Vector2 notePos)
    {
        if (wowCollider != null && wowCollider.OverlapPoint(notePos))
            return JudgeResult.Wow;
        else if (niceCollider != null && niceCollider.OverlapPoint(notePos))
            return JudgeResult.Nice;
        else
            return JudgeResult.Bad;
    }
}
