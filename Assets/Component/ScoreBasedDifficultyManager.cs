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
        if (setting != null && difficultyDatabase.settings.IndexOf(setting) != lastAppliedIndex)
        {
            lastAppliedIndex = difficultyDatabase.settings.IndexOf(setting);
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

    public void ApplySetting(DifficultySetting setting)
    {
        // 1. 휠 속도
        var rotator = FindAnyObjectByType<SplineRotator>();
        if (rotator != null)
            rotator.rotationSpeed = setting.rotationSpeed;

        // 2. 노트 타입 제한
        MultiObjectPool.Instance?.SetAllowedNoteTypes(setting.allowedNoteTypes);
        MultiObjectPool.Instance?.SetNoteSpawnChances(setting.noteSpawnChances);

        // 3. 테마 일괄 적용 (배경, BGM, UI 등)
        ThemeManager.Instance?.ApplySetting(setting);

        // 5. 구미호 이미지
        GameManager.Instance?.SetGumihoImageSet(setting.dayGumihoSprite, setting.nightGumihoSprite);
    }
}
