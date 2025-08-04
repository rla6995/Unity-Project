using UnityEngine;

public enum JudgeResult { Bad, Nice, Wow }

public class TimingJudgeSystem : MonoBehaviour
{
    public TimingJudgeZone judgeZone;

    public Transform JudgeCenter => judgeZone.judgeCenter;
    public Collider2D NiceCollider => judgeZone.niceCollider;

    public JudgeResult GetJudgeResult(Vector2 notePos)
    {
        if (judgeZone.wowCollider.OverlapPoint(notePos))
            return JudgeResult.Wow;
        else if (judgeZone.niceCollider.OverlapPoint(notePos))
            return JudgeResult.Nice;
        else
            return JudgeResult.Bad;
    }
}
