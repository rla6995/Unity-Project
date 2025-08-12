// ✅ ScoreBasedDifficultyManager.cs
using UnityEngine;

public class ScoreBasedDifficultyManager : MonoBehaviour
{
    public static ScoreBasedDifficultyManager Instance { get; private set; }

    [Header("난이도 설정 데이터")]
    [SerializeField] private DifficultyDatabase difficultyDatabase;

    private DifficultySetting currentSetting;
    private int lastAppliedIndex = -1;

    public DifficultySetting CurrentSetting => currentSetting;

    private int currentScore = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            int newScore = GameManager.Instance.currentScore;
            if (newScore != currentScore)
            {
                currentScore = newScore;
                ApplyDifficulty(currentScore);
            }
        }
    }

    public DifficultySetting GetCurrentSetting(int score)
    {
        return GetSettingForScore(score);
    }

    public DifficultySetting GetSettingForScore(int score)
    {
        DifficultySetting result = null;
        foreach (var setting in difficultyDatabase.settings)
        {
            if (score >= setting.scoreThreshold)
                result = setting;
            else
                break;
        }
        return result;
    }

    /// <summary>
    /// 점수 변경 시 호출 → 난이도 자동 적용
    /// </summary>
    public void ApplyDifficulty(int score)
    {
        var setting = GetSettingForScore(score);
        int idx = (setting == null) ? -1 : difficultyDatabase.settings.IndexOf(setting);

        if (setting != null && idx != lastAppliedIndex)
        {
            lastAppliedIndex = idx;
            currentSetting = setting;
            ApplySetting(setting);
        }
    }

    public int GetTailIndexForCurrentSetting()
    {
        if (currentSetting == null || difficultyDatabase == null || difficultyDatabase.settings == null)
            return 0;

        return difficultyDatabase.settings.IndexOf(currentSetting);
    }

    /// <summary>
    /// 난이도 세팅 적용.
    /// - 피버 상태에서는 '테마 변경(ThemeManager.ApplySetting)'을 건너뜀
    /// - 나머지(휠 속도, 스폰 제한/확률)는 필요 시 반영
    /// 피버 종료 시(FeverModeManager.ExitFeverMode) 최신 세팅으로 테마가 한 번에 적용됨.
    /// </summary>
    public void ApplySetting(DifficultySetting setting)
    {
        bool isFever = FeverModeManager.Instance != null && FeverModeManager.Instance.IsFeverActive();

        // 1) 휠 속도 (피버 중엔 휠이 꺼져있긴 하지만, 값 업데이트는 무해)
        var rotator = Object.FindAnyObjectByType<SplineRotator>();
        if (rotator != null)
            rotator.rotationSpeed = setting.rotationSpeed;

        // 2) 노트 타입/확률 (피버 중 일반 스폰이 막혀있어도 값 갱신은 문제 없음)
        MultiObjectPool.Instance?.SetAllowedNoteTypes(setting.allowedNoteTypes);
        MultiObjectPool.Instance?.SetNoteSpawnChances(setting.noteSpawnChances);

        // 3) 테마 적용은 피버 상태가 아닐 때만 수행
        if (!isFever)
        {
            ThemeManager.Instance?.ApplySetting(setting);
            // 구미호 이미지 세트도 함께 갱신
            GameManager.Instance?.SetGumihoImageSet(setting.dayGumihoSprite, setting.nightGumihoSprite);
        }
        else
        {
            // 피버 중에는 테마를 건드리지 않음 (피버 종료 시 ExitFeverMode에서 일괄 적용됨)
            // 여기서는 필요한 데이터 값만 최신으로 유지
            // Debug.Log("[SBDM] Fever active: theming deferred.");
        }
    }
}
