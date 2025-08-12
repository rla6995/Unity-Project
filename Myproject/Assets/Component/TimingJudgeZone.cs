using UnityEngine;

public class TimingJudgeZone : MonoBehaviour
{
    public Transform judgeCenter;       // 판정 기준 위치
    public Collider2D wowCollider;      // Wow 판정용 콜라이더 (BoxCollider2D 권장)
    public Collider2D niceCollider;     // Nice 판정용 콜라이더
}
