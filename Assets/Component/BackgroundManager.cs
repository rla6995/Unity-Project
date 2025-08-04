using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance { get; private set; }

    public bool IsNightTheme { get; set; } = false;

    public Image backgroundImage;
    public Sprite daySprite;
    public Sprite nightSprite;
    public Sprite feverSprite;

    public RectTransform feverA1;
    public RectTransform feverB1;
    public RectTransform feverA2;
    public RectTransform feverB2;
    public float feverScrollSpeed = 100f;

    private Coroutine transitionCoroutine;
    private bool isFeverScrolling = false;
    private float bgWidth;
    public Image feverFlashOverlay;
    public float flashDuration = 0.5f;

    [Header("테마에 따라 색상 변경할 텍스트들")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scorePopupText;

    [Header("밤 테마 오브젝트 적용")]
    public List<GameObject> dayObjectsToDisable;
    public List<GameObject> nightObjectsToEnable;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void ApplyFeverTheme()
    {
        foreach (GameObject go in dayObjectsToDisable)
            if (go != null) go.SetActive(false);
        foreach (GameObject go in nightObjectsToEnable)
            if (go != null) go.SetActive(false); // 둘 다 끄기

        if (scoreText != null) scoreText.color = Color.black;  // 예: 피버는 검정 텍스트
        if (scorePopupText != null) scorePopupText.color = Color.yellow; // 강조 색상

        // 필요 시 피버 전용 오브젝트도 여기에 켤 수 있음
    }

    void Start()
    {
        backgroundImage.sprite = daySprite;
        backgroundImage.color = Color.white;

        if (feverA1 != null)
            bgWidth = feverA1.rect.width;
        if (feverA2 != null)
            bgWidth = feverA2.rect.width;

        SetFeverBackgroundActive(false);
        ApplyDayTheme();
    }
    public void ForceSetNightTheme(bool isNight)
    {
        IsNightTheme = isNight;
    }
    void Update()
    {
        if (!isFeverScrolling) return;

        feverA1.anchoredPosition += new Vector2(feverScrollSpeed * Time.deltaTime, 0f);
        feverB1.anchoredPosition += new Vector2(feverScrollSpeed * Time.deltaTime, 0f);

        if (feverA1.anchoredPosition.x >= bgWidth)
            feverA1.anchoredPosition = new Vector2(feverB1.anchoredPosition.x - bgWidth, feverA1.anchoredPosition.y);

        if (feverB1.anchoredPosition.x >= bgWidth)
            feverB1.anchoredPosition = new Vector2(feverA1.anchoredPosition.x - bgWidth, feverB1.anchoredPosition.y);

        feverA2.anchoredPosition += new Vector2(feverScrollSpeed * Time.deltaTime, 0f);
        feverB2.anchoredPosition += new Vector2(feverScrollSpeed * Time.deltaTime, 0f);

        if (feverA2.anchoredPosition.x >= bgWidth)
            feverA2.anchoredPosition = new Vector2(feverB2.anchoredPosition.x - bgWidth, feverA2.anchoredPosition.y);

        if (feverB2.anchoredPosition.x >= bgWidth)
            feverB2.anchoredPosition = new Vector2(feverA2.anchoredPosition.x - bgWidth, feverB2.anchoredPosition.y);
    }

    public void ChangeToDay()
    {
        IsNightTheme = false;

        StopFeverScroll();
        StartTransition(daySprite);
        ApplyDayTheme();

        int currentScore = GameManager.Instance.CurrentScore;
        var setting = ScoreBasedDifficultyManager.Instance.GetSettingForScore(currentScore);
        ScoreBasedDifficultyManager.Instance.ApplySetting(setting);
    }

    public void ChangeToNight()
    {
        IsNightTheme = true;

        StopFeverScroll();
        StartTransition(nightSprite);
        ApplyNightTheme();

        int currentScore = GameManager.Instance.CurrentScore;
        var setting = ScoreBasedDifficultyManager.Instance.GetSettingForScore(currentScore);
        ScoreBasedDifficultyManager.Instance.ApplySetting(setting);
    }
    public void ChangeToFever()
    {
        if (!FeverModeManager.Instance.IsFeverActive())
            StartCoroutine(FeverTransitionWithFlash());
            
        int currentScore = GameManager.Instance.CurrentScore;
        var setting = ScoreBasedDifficultyManager.Instance.GetSettingForScore(currentScore);
        ScoreBasedDifficultyManager.Instance.ApplySetting(setting);
    }

    private Sprite GetCurrentThemeSprite()
    {
        if (FeverModeManager.Instance != null && FeverModeManager.Instance.IsFeverActive())
            return feverSprite;

        return IsNightTheme ? nightSprite : daySprite;
    }

    private IEnumerator FeverTransitionWithFlash()
    {
        float brightenDuration = 0.2f;
        float dimDuration = 0.8f;
        float elapsed = 0f;

        while (elapsed < brightenDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / brightenDuration);
            feverFlashOverlay.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        // ✅ 피버 모드 진입 시
        AudioManager.Instance?.PlaySE(5);
        FeverModeManager.Instance?.EnterFeverMode();
        backgroundImage.sprite = feverSprite;
        backgroundImage.color = Color.white;
        StartTransition(feverSprite);
        SetFeverBackgroundActive(true);
        StartFeverScroll();

        yield return null;

        elapsed = 0f;
        while (elapsed < dimDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / dimDuration);
            feverFlashOverlay.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        feverFlashOverlay.color = new Color(1f, 1f, 1f, 0f);
        AudioManager.Instance?.PlayBGM(3); // 피버 BGM
    }

    public void StartTransition(Sprite newSprite)
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(TransitionBackground(newSprite));
    }

    private IEnumerator TransitionBackground(Sprite newSprite)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            backgroundImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        backgroundImage.sprite = newSprite;

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            backgroundImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        backgroundImage.color = Color.white;
    }

    private void StartFeverScroll()
    {
        isFeverScrolling = true;
    }

    public void StopFeverScroll()
    {
        isFeverScrolling = false;
        feverA1.anchoredPosition = new Vector2(0f, feverA1.anchoredPosition.y);
        feverB1.anchoredPosition = new Vector2(bgWidth, feverB1.anchoredPosition.y);
        feverA2.anchoredPosition = new Vector2(0f, feverA2.anchoredPosition.y);
        feverB2.anchoredPosition = new Vector2(bgWidth, feverB2.anchoredPosition.y);
        SetFeverBackgroundActive(false);
    }

    private void SetFeverBackgroundActive(bool isActive)
    {
        if (feverA1 != null) feverA1.gameObject.SetActive(isActive);
        if (feverB1 != null) feverB1.gameObject.SetActive(isActive);
        if (feverA2 != null) feverA2.gameObject.SetActive(isActive);
        if (feverB2 != null) feverB2.gameObject.SetActive(isActive);
    }

    public void ApplyDayTheme()
    {
        foreach (GameObject go in nightObjectsToEnable)
            if (go != null) go.SetActive(false);
        foreach (GameObject go in dayObjectsToDisable)
            if (go != null) go.SetActive(true);

        if (scoreText != null) scoreText.color = Color.black;
        if (scorePopupText != null) scorePopupText.color = Color.black;
    }

    public void ApplyNightTheme()
    {
        foreach (GameObject go in dayObjectsToDisable)
            if (go != null) go.SetActive(false);
        foreach (GameObject go in nightObjectsToEnable)
            if (go != null) go.SetActive(true);

        if (scoreText != null) scoreText.color = Color.white;
        if (scorePopupText != null) scorePopupText.color = Color.white;
    }
}
