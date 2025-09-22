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
    private Coroutine feverScrollCoroutine; // 🔧 성능 최적화: 코루틴으로 변경
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
    public OrbEffectController orbEffect;
    public Sprite awakenedBackgroundSprite;  // 각성 배경
    public Sprite awakenedOrbSprite;
    public SpriteRenderer orbRenderer;         // 각성 구슬
    public Sprite dayOrbSprite;       // 낮 구슬
    public Sprite nightOrbSprite;     // 밤 구슬
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void ApplyFeverTheme()
    {
        // 🎯 핵심: 피버 모드일 때는 IsNightTheme을 false로 설정
        IsNightTheme = false;
        
        // 🎯 핵심: 피버 배경 스프라이트 즉시 교체
        if (feverSprite != null)
        {
            InstantSwapSprite(feverSprite, keepWhite: true);
        }
        
        // 🎯 핵심: 피버 배경 타일 활성화 및 스크롤 시작
        SetFeverBackgroundActive(true);
        StartFeverScroll();
        
        // 🎯 핵심: 구슬 비활성화 (피버 중에는 숨김)
        if (orbRenderer != null)
        {
            orbRenderer.enabled = false;
        }
        
        // 🎯 피버 테마 UI 요소 적용 (BackgroundManager의 다른 테마 변경 메서드들과 동일한 패턴)
        // 텍스트 색상을 피버 테마에 맞게 변경
        if (scoreText != null) scoreText.color = Color.black;        // 피버는 밝은 색상
        if (scorePopupText != null) scorePopupText.color = Color.black; // 피버는 노란색
        
        // 🎯 핵심: UIManager.UpdateScoreImageByCurrentSetting() 직접 호출하여 스코어 이미지 즉시 업데이트
        // (ScoreBasedDifficultyManager.ApplySetting()은 FeverModeManager에서 이미 호출됨)
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreImageByCurrentSetting();
        }
    }

    public void RemoveFeverTheme()
    {
        // 피버 배경 스크롤 중단 및 타일 비활성화
        StopFeverScroll();
        
        // 배경 이미지만 복원 (테마 적용은 ThemeManager가 담당)
        Sprite targetSprite = IsNightTheme ? nightSprite : daySprite;
        if (targetSprite != null)
        {
            StartTransition(targetSprite);
        }
        
        // 🎯 핵심: 구슬 다시 활성화 (피버 모드 종료 시)
        if (orbRenderer != null)
        {
            orbRenderer.enabled = true;
        }
    }
    public void ApplyAwakenedTheme()
    {
        // 1. 배경 전환
        if (awakenedBackgroundSprite != null)
        {
            StartTransition(awakenedBackgroundSprite);
        }

        // 2. 구슬 스프라이트 전환
        if (orbRenderer != null && awakenedOrbSprite != null)
        {
            orbRenderer.sprite = awakenedOrbSprite;
        }

        // 3. 색상이나 텍스트도 필요 시 바꿀 수 있음
        if (scoreText != null) scoreText.color = Color.white;
        if (scorePopupText != null) scorePopupText.color = Color.yellow;
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
        // 🔧 성능 최적화: FindObjectOfType 제거 - 에디터에서 직접 할당하도록 변경
        // if (orbRenderer == null) orbRenderer = FindObjectOfType<SpriteRenderer>();
    }
    public void ForceSetNightTheme(bool isNight)
    {
        IsNightTheme = isNight;
    }
    // 🔧 성능 최적화: Update() 제거하고 코루틴으로 변경
    // void Update() - 매 프레임 연산 제거됨

    public void ChangeToDay()
    {
        IsNightTheme = false;

        StopFeverScroll();
        StartTransition(daySprite);
        ApplyDayTheme();

        int currentScore = GameStateManager.Instance.CurrentScore;
        var setting = ScoreBasedDifficultyManager.Instance.GetSettingForScore(currentScore);
        ScoreBasedDifficultyManager.Instance.ApplySetting(setting);
    }

    public void ChangeToNight()
    {
        IsNightTheme = true;

        StopFeverScroll();
        StartTransition(nightSprite);
        ApplyNightTheme();

        int currentScore = GameStateManager.Instance.CurrentScore;
        var setting = ScoreBasedDifficultyManager.Instance.GetSettingForScore(currentScore);
        ScoreBasedDifficultyManager.Instance.ApplySetting(setting);
    }
    public void ChangeToFever()
    {
        if (!FeverModeManager.Instance.IsFeverActive())
            StartCoroutine(FeverTransitionWithFlash());
            
        // 🎯 핵심: 피버 진입 시 ScoreBasedDifficultyManager.ApplySetting() 호출 제거
        // 이 메서드가 피버 모드일 때 테마를 덮어쓰는 문제를 일으킴
        // FeverModeManager.EnterFeverMode()에서 필요한 UI 업데이트를 처리함
    }

    private Sprite GetCurrentThemeSprite()
    {
        if (FeverModeManager.Instance != null && FeverModeManager.Instance.IsFeverActive())
            return feverSprite;

        return IsNightTheme ? nightSprite : daySprite;
    }
// BackgroundManager.cs 내부 어딘가(메서드들 중간)에 추가
    private void InstantSwapSprite(Sprite newSprite, bool keepWhite = true)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        backgroundImage.sprite = newSprite;

        // 오버레이가 이미 하양을 덮고 있으므로, 배경 자체는 흰색으로 유지해도 무방
        backgroundImage.color = keepWhite ? Color.white : Color.white;
    }

    // BackgroundManager.cs - FeverTransitionWithFlash() 내부 수정
    private IEnumerator FeverTransitionWithFlash()
    {
        // (기존처럼 FeverModeManager에 맡겨서 UI/플레이어/버튼 등 전체 피버 세팅)
        FeverModeManager.Instance?.StartFeverEntrySequence();

        // 배경 스프라이트는 '즉시' 교체 (내부 페이드 금지)
        InstantSwapSprite(feverSprite, keepWhite: true);

        // 피버 배경 스크롤/타일 활성
        SetFeverBackgroundActive(true);
        StartFeverScroll();

        AudioManager.Instance?.PlayBGM(3); // 피버 BGM
        yield return null;
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
        if (isFeverScrolling) return; // 🔧 성능 최적화: 중복 시작 방지
        
        isFeverScrolling = true;
        feverScrollCoroutine = StartCoroutine(FeverScrollCoroutine());
    }

    public void StopFeverScroll()
    {
        isFeverScrolling = false;
        
        // 🔧 성능 최적화: 코루틴 중단
        if (feverScrollCoroutine != null)
        {
            StopCoroutine(feverScrollCoroutine);
            feverScrollCoroutine = null;
        }
        
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

    // 🔧 성능 최적화: 피버 스크롤을 코루틴으로 변경
    private IEnumerator FeverScrollCoroutine()
    {
        while (isFeverScrolling)
        {
            // 레일 1 스크롤
            feverA1.anchoredPosition += new Vector2(feverScrollSpeed * Time.deltaTime, 0f);
            feverB1.anchoredPosition += new Vector2(feverScrollSpeed * Time.deltaTime, 0f);

            if (feverA1.anchoredPosition.x >= bgWidth)
                feverA1.anchoredPosition = new Vector2(feverB1.anchoredPosition.x - bgWidth, feverA1.anchoredPosition.y);

            if (feverB1.anchoredPosition.x >= bgWidth)
                feverB1.anchoredPosition = new Vector2(feverA1.anchoredPosition.x - bgWidth, feverB1.anchoredPosition.y);

            // 레일 2 스크롤
            feverA2.anchoredPosition += new Vector2(feverScrollSpeed * Time.deltaTime, 0f);
            feverB2.anchoredPosition += new Vector2(feverScrollSpeed * Time.deltaTime, 0f);

            if (feverA2.anchoredPosition.x >= bgWidth)
                feverA2.anchoredPosition = new Vector2(feverB2.anchoredPosition.x - bgWidth, feverA2.anchoredPosition.y);

            if (feverB2.anchoredPosition.x >= bgWidth)
                feverB2.anchoredPosition = new Vector2(feverA2.anchoredPosition.x - bgWidth, feverB2.anchoredPosition.y);

            yield return null;
        }
    }

    public void ApplyDayTheme()
    {
        foreach (GameObject go in nightObjectsToEnable)
            if (go != null) go.SetActive(false);
        foreach (GameObject go in dayObjectsToDisable)
            if (go != null) go.SetActive(true);
        if (orbRenderer != null && dayOrbSprite != null)
        {
            orbRenderer.sprite = dayOrbSprite;
            orbRenderer.enabled = true; // 구슬 활성화
        }
        if (scoreText != null) scoreText.color = Color.black;
        if (scorePopupText != null) scorePopupText.color = Color.black;

        orbEffect?.TriggerEffect();
    }

    public void ApplyNightTheme()
    {
        foreach (GameObject go in dayObjectsToDisable)
            if (go != null) go.SetActive(false);
        foreach (GameObject go in nightObjectsToEnable)
            if (go != null) go.SetActive(true);
        if (orbRenderer != null && nightOrbSprite != null)
        {
            orbRenderer.sprite = nightOrbSprite;
            orbRenderer.enabled = true; // 구슬 활성화
        }
        if (scoreText != null) scoreText.color = Color.white;
        if (scorePopupText != null) scorePopupText.color = Color.white;

        orbEffect?.TriggerEffect();
    }
}
