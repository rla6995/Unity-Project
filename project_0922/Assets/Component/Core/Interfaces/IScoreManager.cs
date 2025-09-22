/// <summary>
/// 점수 관리를 위한 인터페이스
/// 의존성 역전 원칙: 구체 클래스 대신 인터페이스에 의존
/// </summary>
public interface IScoreManager
{
    float FeverGauge { get; }
    
    void IncreaseFever(float amount);
    void SetFeverGauge(float value);
    float GetFeverGauge();
    void ResetFeverGauge();
    void AddScore(JudgeResult result);
}
