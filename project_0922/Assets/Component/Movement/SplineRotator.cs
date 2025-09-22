using UnityEngine;

[DefaultExecutionOrder(-200)] // ← 회전이 항상 먼저 갱신되도록
public class SplineRotator : MonoBehaviour
{
    [Tooltip("반시계(+) z축 회전 속도 (deg/sec)")]
    public float rotationSpeed = 20f;

    // 내부 각도(로컬 Z), 필요 시 인스펙터 확인용으로 public
    [SerializeField] private float currentAngle = 0f;

    /// <summary> 0~360 정규화된 현재 로컬 Z 각도 </summary>
    public float CurrentRotation => Mathf.Repeat(currentAngle, 360f);

    private void Start()
    {
        // 시작 각도 적용
        ApplyRotation();
    }

    private void Update()
    {
        // 매 프레임 회전(반시계 +)
        currentAngle = Mathf.Repeat(currentAngle + rotationSpeed * Time.deltaTime, 360f);
        ApplyRotation();
    }

    /// <summary> 외부에서 각도를 강제로 세팅(예: 0도 스냅) </summary>
    public void SetAngle(float angleDeg)
    {
        currentAngle = angleDeg;
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }
}
