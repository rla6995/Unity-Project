using UnityEngine;

/// <summary>
/// 점수와 피버 게이지 관리를 담당하는 매니저
/// 단일 책임 원칙: 점수 관련 기능만 담당
/// </summary>
public class ScoreManager : MonoBehaviour, IScoreManager
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private float feverGauge = 0f;
    
    // 이벤트 시스템을 통한 상태 변경 알림
    public System.Action<float> OnFeverGaugeChanged;
    public System.Action<int> OnScoreAdded;

    public float FeverGauge
    {
        get => feverGauge;
        private set
        {
            if (feverGauge != value)
            {
                feverGauge = Mathf.Clamp(value, 0f, 100f);
                OnFeverGaugeChanged?.Invoke(feverGauge);
            }
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void IncreaseFever(float amount)
    {
        if (FeverModeManager.Instance != null && FeverModeManager.Instance.IsFeverActive()) 
            return;
            
        FeverGauge += amount;
    }

    public void SetFeverGauge(float value)
    {
        FeverGauge = value;
    }

    public float GetFeverGauge()
    {
        return FeverGauge;
    }

    public void ResetFeverGauge()
    {
        FeverGauge = 0f;
    }

    public void AddScore(JudgeResult result)
    {
        int baseScore = result switch
        {
            JudgeResult.Wow => 2,
            JudgeResult.Nice => 1,
            _ => 0
        };

        if (FeverModeManager.Instance?.IsFeverActive() == true)
            baseScore *= 2;

        GameStateManager.Instance?.AddScore(baseScore);
        OnScoreAdded?.Invoke(baseScore);
    }
}
