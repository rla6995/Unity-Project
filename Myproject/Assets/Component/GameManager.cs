using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    public Image judgeImage;
    public TextMeshProUGUI scorePopupText;

    public Image scoreImageUI;
    public Image gumihoImageUI;

    private int score = 0;
    private bool isGameOver = false;
    private SplineRotator rotator;
    private DifficultySetting currentSetting;
    public int CurrentScore => score;

    [SerializeField] private Sprite wowSprite;
    [SerializeField] private Sprite niceSprite;
    [SerializeField] private Sprite badSprite;
    [SerializeField] private Sprite wowSpriteNight;
    [SerializeField] private Sprite niceSpriteNight;
    [SerializeField] private Sprite badSpriteNight;

    [Header("Fever 관련 UI")]
    [SerializeField] private Image feverFillImage;
    [SerializeField] private Image feverDarkCover;

    [SerializeField] private float feverGauge = 0f;
    public float FeverGauge
    {
        get => feverGauge;
        set
        {
            feverGauge = Mathf.Clamp(value, 0f, 100f);
            UpdateFeverUI();
        }
    }

    [SerializeField] private GameObject foxNormal;
    [SerializeField] private GameObject foxBad;
    [SerializeField] private GameObject foxNice;
    [SerializeField] private GameObject foxWow;

    public int currentScore => score;

    public TMP_InputField scoreInputField;
    private JudgeResult lastJudgeResult = JudgeResult.Bad;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Time.timeScale = 1;
        score = 0;
        isGameOver = false;
        gameOverPanel.SetActive(false);
        judgeImage.gameObject.SetActive(false);
        scorePopupText.gameObject.SetActive(false);

        rotator = FindAnyObjectByType<SplineRotator>();
        AudioManager.Instance?.PlayBGM(1);

        currentSetting = ScoreBasedDifficultyManager.Instance.GetCurrentSetting(score);
        ScoreBasedDifficultyManager.Instance.ApplyDifficulty(score);
        UpdateScoreUI();
    }

    public void IncreaseFever(float amount)
    {
        if (FeverModeManager.Instance != null && FeverModeManager.Instance.IsFeverActive()) return;
        FeverGauge += amount;
    }

    public void UpdateFeverUI()
    {
        if (feverFillImage != null)
            feverFillImage.fillAmount = feverGauge / 100f;

        if (feverDarkCover != null)
            feverDarkCover.fillAmount = 1f - (feverGauge / 100f);
    }

    public void TryEnterFeverMode()
    {
        if (FeverGauge >= 100f && !FeverModeManager.Instance.IsFeverActive())
        {
            BackgroundManager.Instance?.ChangeToFever(); // 이 안에서 EnterFeverMode 호출됨
        }
    }

    public void SetScoreFromInput()
    {
        if (int.TryParse(scoreInputField.text, out int newScore))
        {
            score = Mathf.Max(0, newScore);
            UpdateScoreUI();
            ScoreBasedDifficultyManager.Instance.ApplyDifficulty(score);
            UpdateScoreImageByCurrentSetting();
        }
    }

    public void AddScore(JudgeResult result)
    {
        if (isGameOver) return;

        int baseScore = (result == JudgeResult.Wow) ? 2 :
                        (result == JudgeResult.Nice) ? 1 : 0;

        if (FeverModeManager.Instance?.IsFeverActive() == true)
            baseScore *= 2;

        score += baseScore;
        UpdateScoreUI();

        ScoreBasedDifficultyManager.Instance.ApplyDifficulty(score);
    }

    void UpdateScoreUI()
    {
        scoreText.text = score.ToString();
    }

public void TriggerGameOver()
{
    if (isGameOver) return;
    StartCoroutine(TriggerGameOverCoroutine());
}

private IEnumerator TriggerGameOverCoroutine()
{
    isGameOver = true;

    // ✅ 모든 액티브 오브젝트 풀로 반환
    var pool = MultiObjectPool.Instance;
    if (pool != null)
    {
        foreach (var obj in pool.ActiveObjects)
        {
            pool.Return(obj);
        }
    }

    // ✅ 0.05초 대기
    yield return new WaitForSecondsRealtime(0.5f);

    // ✅ 게임 정지 및 UI 표시
    Time.timeScale = 0;
    AudioManager.Instance?.PlaySE(4);
    gameOverPanel.SetActive(true);
}


    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowJudgeText(JudgeResult result)
    {
        StartCoroutine(ShowJudgeCoroutine(result));
    }

    private IEnumerator ShowJudgeCoroutine(JudgeResult result)
    {
        UpdateJudgeSprites(result);

        int baseScore = (result == JudgeResult.Wow) ? 2 :
                        (result == JudgeResult.Nice) ? 1 : 0;

        int displayScore = baseScore;
        if (FeverModeManager.Instance?.IsFeverActive() == true)
            displayScore *= 2;

        scorePopupText.text = $"+{displayScore}";
        scorePopupText.gameObject.SetActive(true);
        judgeImage.gameObject.SetActive(true);

        SetFoxExpression(result);
        yield return new WaitForSeconds(0.5f);
        judgeImage.gameObject.SetActive(false);
        scorePopupText.gameObject.SetActive(false);
    }

    private void SetFoxExpression(JudgeResult result)
    {
        foxNormal.SetActive(false);
        foxBad.SetActive(false);
        foxNice.SetActive(false);
        foxWow.SetActive(false);

        switch (result)
        {
            case JudgeResult.Wow: foxWow.SetActive(true); break;
            case JudgeResult.Nice: foxNice.SetActive(true); break;
            case JudgeResult.Bad: foxBad.SetActive(true); break;
        }

        StartCoroutine(RevertFoxToNormal());
    }

    private IEnumerator RevertFoxToNormal()
    {
        yield return new WaitForSeconds(0.5f);
        foxBad.SetActive(false);
        foxNice.SetActive(false);
        foxWow.SetActive(false);
        foxNormal.SetActive(true);
    }

    public bool IsGameOver() => isGameOver;

    public void OnGameOverQuitPressed()
    {
        FindObjectOfType<PauseManager>()?.OpenExitPanel(gameOverPanel);
    }

    public void UpdateJudgeSprites(JudgeResult result)
    {
        lastJudgeResult = result;
        bool isFever = FeverModeManager.Instance?.IsFeverActive() == true;

        bool isNight = !isFever && BackgroundManager.Instance?.IsNightTheme == true;

        switch (result)
        {
            case JudgeResult.Wow:
                judgeImage.sprite = isNight ? wowSpriteNight : wowSprite;
                AudioManager.Instance?.PlaySE(1);
                break;
            case JudgeResult.Nice:
                judgeImage.sprite = isNight ? niceSpriteNight : niceSprite;
                AudioManager.Instance?.PlaySE(2);
                break;
            case JudgeResult.Bad:
                judgeImage.sprite = isNight ? badSpriteNight : badSprite;
                AudioManager.Instance?.PlaySE(3);
                break;
        }
    }

    public void ForceUpdateJudgeImage()
    {
        bool wasInactive = !judgeImage.gameObject.activeSelf;
        if (wasInactive)
            judgeImage.gameObject.SetActive(true);

        UpdateJudgeSprites(lastJudgeResult);

        if (wasInactive)
            judgeImage.gameObject.SetActive(false);
    }

    public void SetJudgeSprites(Sprite wow, Sprite nice, Sprite bad)
    {
        wowSprite = wow;
        niceSprite = nice;
        badSprite = bad;
    }

    public void UpdateScoreImageByCurrentSetting()
    {
        var setting = ScoreBasedDifficultyManager.Instance.CurrentSetting;
        if (setting == null || scoreImageUI == null) return;

        bool isFever = FeverModeManager.Instance?.IsFeverActive() == true;
        bool isNight = ThemeManager.Instance?.IsNightTheme == true;

        int tailCount = ScoreBasedDifficultyManager.Instance.GetTailIndexForCurrentSetting();
        if (tailCount < 0) tailCount = 0;

        Sprite[] spriteSet = isFever
            ? setting.dayGumihoSprites
            : isNight ? setting.nightGumihoSprites : setting.dayGumihoSprites;

        if (spriteSet != null && tailCount < spriteSet.Length)
        {
            scoreImageUI.sprite = spriteSet[tailCount];
        }
    }

    public void UpdateScoreUIImage(Sprite _) => UpdateScoreImageByCurrentSetting();

    public void SetGumihoImageSet(Sprite daySprite, Sprite nightSprite)
    {
        if (gumihoImageUI != null)
        {
            var sprite = FeverModeManager.Instance?.IsFeverActive() == true
                ? daySprite
                : nightSprite ?? daySprite;

            gumihoImageUI.sprite = sprite;
        }
    }
    public void BonusNoteHitByPlayer()
    {
        Debug.Log("Bonus note hit by player (no score)");
    }
}
