using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// 게임의 전체적인 흐름을 조정하는 매니저
/// 단일 책임 원칙: 게임 흐름 조정만 담당
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Input")]
    public TMP_InputField scoreInputField;

    private SplineRotator rotator;
    private DifficultySetting currentSetting;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // 🔧 성능 최적화: FindAnyObjectByType 제거 - 인스펙터에서 직접 할당
        // rotator = FindAnyObjectByType<SplineRotator>();
        AudioManager.Instance?.PlayBGM(1);

        currentSetting = ScoreBasedDifficultyManager.Instance.GetCurrentSetting(0);
        ScoreBasedDifficultyManager.Instance.ApplyDifficulty(0);
    }

    /// <summary>
    /// 테스트용: 피버 게이지를 100%로 채우고 피버 모드 진입
    /// </summary>
    public void FillFeverAndEnter()
    {
        // 1) 게이지 꽉 채우기
        ScoreManager.Instance?.SetFeverGauge(100f);

        // 2) 피버 모드 진입 시도
        TryEnterFeverMode();
    }

    /// <summary>
    /// 피버 게이지가 100%일 때만 피버 모드에 진입
    /// </summary>
    public void TryEnterFeverMode()
    {
        // 1) 이미 피버 중이면 무시
        if (FeverModeManager.Instance != null && FeverModeManager.Instance.IsFeverActive())
        {

            return;
        }

        // 2) 피버 게이지가 100%인지 확인
        float currentGauge = ScoreManager.Instance?.GetFeverGauge() ?? 0f;
        if (currentGauge < 100f)
        {

            return;
        }

        // 3) 피버 진입 시퀀스 시작

        FeverModeManager.Instance?.StartFeverEntrySequence();
    }

    /// <summary>
    /// 피버 게이지 클릭 시 호출되는 메서드 (UI 버튼에서 사용)
    /// </summary>
    public void OnFeverGaugeClicked()
    {
        TryEnterFeverMode();
    }

    public void SetScoreFromInput()
    {
        if (int.TryParse(scoreInputField.text, out int newScore))
        {
            newScore = Mathf.Max(0, newScore);
            GameStateManager.Instance?.AddScore(newScore - GameStateManager.Instance.CurrentScore);
            ScoreBasedDifficultyManager.Instance.ApplyDifficulty(newScore);
            UIManager.Instance?.UpdateScoreImageByCurrentSetting();
        }
    }

    public void TriggerGameOver()
    {
        if (GameStateManager.Instance?.IsGameOver == true) return;
        
        StartCoroutine(TriggerGameOverCoroutine());
    }

    private IEnumerator TriggerGameOverCoroutine()
    {
        // ✅ 모든 액티브 오브젝트 풀로 반환
        var pool = MultiObjectPool.Instance;
        if (pool != null)
        {
            foreach (var obj in pool.ActiveObjects)
            {
                pool.Return(obj);
            }
        }

        // ✅ 0.3초 대기
        yield return new WaitForSecondsRealtime(0.3f);

        // ✅ 게임오버 상태 설정 (이벤트 시스템을 통해 UI 업데이트)
        GameStateManager.Instance?.SetGameOver(true);
        AudioManager.Instance?.PlaySE(4);
    }

    public void OnGameOverQuitPressed()
    {
        // 🔧 성능 최적화: FindObjectOfType 제거 - 인스펙터에서 직접 할당
        // FindObjectOfType<PauseManager>()?.OpenExitPanel(UIManager.Instance?.gameOverPanel);
        
        // TODO: PauseManager 참조를 인스펙터에서 직접 할당하도록 변경 필요
    }

    public void BonusNoteHitByPlayer()
    {
        // Bonus note hit by player (no score)
    }
}
