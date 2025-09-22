using UnityEngine;

/// <summary>
/// 타이밍 판정을 위한 인터페이스
/// 의존성 역전 원칙: 구체 클래스 대신 인터페이스에 의존
/// </summary>
public interface ITimingJudge
{
    Transform JudgeCenter { get; }
    Collider2D NiceCollider { get; }
    JudgeResult GetJudgeResult(Vector2 notePos);
}
