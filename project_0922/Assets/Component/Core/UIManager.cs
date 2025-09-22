using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq; // Added for FirstOrDefault

/// <summary>
/// 게임 UI 업데이트를 담당하는 매니저
/// 단일 책임 원칙: UI 표시만 담당
/// </summary>
public class UIManager : MonoBehaviour, IUIManager
{
    public static UIManager Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public Image scoreImageUI;
    public Image gumihoImageUI;
    public Image feverFillImage;
    public Image feverDarkCover;
    public GameObject gameOverPanel;
    public Image judgeImage;
    public TextMeshProUGUI scorePopupText;

    [Header("Fox Expressions")]
    public GameObject foxNormal;
    public GameObject foxBad;
    public GameObject foxNice;
    public GameObject foxWow;

    [Header("Fox Expression Sprites")]
    [SerializeField] private Sprite foxNormalSprite;
    [SerializeField] private Sprite foxBadSprite;
    [SerializeField] private Sprite foxNiceSprite;
    [SerializeField] private Sprite foxWowSprite;

    [Header("Judge Sprites")]
    [SerializeField] private Sprite wowSprite;
    [SerializeField] private Sprite niceSprite;
    [SerializeField] private Sprite badSprite;
    [SerializeField] private Sprite wowSpriteNight;
    [SerializeField] private Sprite niceSpriteNight;
    [SerializeField] private Sprite badSpriteNight;

    private JudgeResult lastJudgeResult = JudgeResult.Bad;

    // 🔧 최적화: 캐시된 테마 상태
    private bool cachedIsNight = false;
    private bool cachedIsFever = false;
    private GameObject currentActiveFox = null;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnScoreChanged -= UpdateScoreUI;
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnFeverGaugeChanged -= UpdateFeverUI;
            ScoreManager.Instance.OnScoreAdded -= OnScoreAdded;
        }
    }

    private void Start()
    {
        // 매니저들이 준비될 때까지 기다린 후 이벤트 구독
        StartCoroutine(SubscribeToEventsWhenReady());
        
        gameOverPanel.SetActive(false);
        judgeImage.gameObject.SetActive(false);
        scorePopupText.gameObject.SetActive(false);
        UpdateScoreUI(0);
        UpdateFeverUI(0f);
        
        // 🔧 최적화: 초기 테마 상태 캐싱
        UpdateCachedThemeState();
    }

    private System.Collections.IEnumerator SubscribeToEventsWhenReady()
    {
        // 매니저들이 준비될 때까지 기다림
        while (GameStateManager.Instance == null || ScoreManager.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        // 이벤트 구독
        GameStateManager.Instance.OnScoreChanged += UpdateScoreUI;
        GameStateManager.Instance.OnGameOverChanged += OnGameOverChanged;
        ScoreManager.Instance.OnFeverGaugeChanged += UpdateFeverUI;
        ScoreManager.Instance.OnScoreAdded += OnScoreAdded;
    }

            // 🔧 최적화: 테마 상태를 한 번에 업데이트
        private void UpdateCachedThemeState()
        {
            // 피버 모드 상태 확인
            bool currentIsFever = IsFeverModeActive();
            
            // 테마 상태 결정
            bool currentIsNight = false;
            if (currentIsFever)
            {
                // 피버 모드일 때는 무조건 낮 테마
                currentIsNight = false;
            }
            else
            {
                // 일반 모드일 때는 ScoreBasedDifficultyManager의 설정 사용
                if (ScoreBasedDifficultyManager.Instance != null && GameStateManager.Instance != null)
                {
                    int currentScore = GameStateManager.Instance.CurrentScore;
                    var setting = ScoreBasedDifficultyManager.Instance.GetSettingForScore(currentScore);
                    if (setting != null)
                    {
                        currentIsNight = (setting.theme != ThemeType.Day);
                    }
                }
            }
            
            // 상태가 변경되었을 때만 업데이트
            if (cachedIsNight != currentIsNight || cachedIsFever != currentIsFever)
            {
                cachedIsNight = currentIsNight;
                cachedIsFever = currentIsFever;
            }
        }
    
    // 🎯 핵심: 피버 모드 상태를 정확하게 감지하는 메서드
    private bool IsFeverModeActive()
    {
        // 디버그: FeverModeManager 인스턴스 상태 확인
        if (FeverModeManager.Instance == null)
        {

            return false;
        }
        
        // UI 전용 피버 상태를 먼저 확인 (더 일찍 설정됨)
        bool isFeverUI = FeverModeManager.Instance.IsFeverUIActive();
        bool isFever = FeverModeManager.Instance.IsFeverActive();
        

        
        if (isFeverUI)
        {

            return true;
        }
        
        if (isFever)
        {

            return true;
        }
        

        return false;
    }

    private void UpdateScoreUI(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    private void UpdateFeverUI(float feverGauge)
    {
        if (feverFillImage != null)
            feverFillImage.fillAmount = feverGauge / 100f;

        if (feverDarkCover != null)
            feverDarkCover.fillAmount = 1f - (feverGauge / 100f);
    }

    private void OnGameOverChanged(bool isGameOver)
    {
        if (isGameOver)
        {
            gameOverPanel.SetActive(true);
        }
    }

    private void OnScoreAdded(int score)
    {

        ShowScorePopup(score);
    }

    public void ShowJudgeText(JudgeResult result)
    {

        lastJudgeResult = result;
        
        // 🎯 핵심: 피버 모드 상태를 먼저 확인하고 테마 상태 업데이트
        UpdateCachedThemeState();
        
        UpdateJudgeSprites(result);
        SetFoxExpression(result);
        PlayJudgeSound(result);
        StartCoroutine(ShowJudgeCoroutine());
    }
    
    private void PlayJudgeSound(JudgeResult result)
    {
        switch (result)
        {
            case JudgeResult.Wow:
                AudioManager.Instance?.PlaySE(1); // Wow 사운드
                break;
            case JudgeResult.Nice:
                AudioManager.Instance?.PlaySE(2); // Nice 사운드
                break;
            case JudgeResult.Bad:
                AudioManager.Instance?.PlaySE(3); // Bad 사운드
                break;
        }
    }

    // 🔧 최적화: 캐시된 테마 상태 사용
    private void UpdateJudgeSprites(JudgeResult result)
    {
        // 피버 모드 상태 확인
        bool isFever = IsFeverModeActive();
        
        Sprite targetSprite;
        if (isFever)
        {
            // 피버 모드일 때는 무조건 낮 테마 판정 스프라이트 사용
            targetSprite = result switch
            {
                JudgeResult.Wow => wowSprite,
                JudgeResult.Nice => niceSprite,
                JudgeResult.Bad => badSprite,
                _ => badSprite
            };
        }
        else
        {
            // 일반 모드일 때는 캐시된 테마 상태 사용
            targetSprite = result switch
            {
                JudgeResult.Wow => cachedIsNight ? wowSpriteNight : wowSprite,
                JudgeResult.Nice => cachedIsNight ? niceSpriteNight : niceSprite,
                JudgeResult.Bad => cachedIsNight ? badSpriteNight : badSprite,
                _ => badSprite
            };
        }

        if (judgeImage != null)
            judgeImage.sprite = targetSprite;
    }

    // 🔧 최적화: 현재 활성화된 fox만 비활성화하여 성능 향상
    private void SetFoxExpression(JudgeResult result)
    {
        // 현재 활성화된 fox만 비활성화
        if (currentActiveFox != null)
        {
            currentActiveFox.SetActive(false);
            currentActiveFox = null;
        }

        // 새로운 fox 활성화
        GameObject targetFox = result switch
        {
            JudgeResult.Wow => foxWow,
            JudgeResult.Nice => foxNice,
            JudgeResult.Bad => foxBad,
            _ => foxNormal
        };

        if (targetFox != null)
        {
                         // 피버 모드 상태 확인
            bool isFever = IsFeverModeActive();

            
            // 테마 상태 업데이트 (피버 모드가 아닐 때만)
            if (!isFever)
            {
                UpdateCachedThemeState();

            }
            
            // 스프라이트 설정
            var targetImage = targetFox.GetComponent<Image>();
            if (targetImage != null)
            {
                if (isFever)
                {
                    // 피버 모드일 때: 무조건 낮 테마 스프라이트 사용

                    switch (result)
                    {
                        case JudgeResult.Wow:
                            if (foxWowSprite != null) 
                            {
                                targetImage.sprite = foxWowSprite;

                            }
                            break;
                        case JudgeResult.Nice:
                            if (foxNiceSprite != null) 
                            {
                                targetImage.sprite = foxNiceSprite;

                            }
                            break;
                        case JudgeResult.Bad:
                            if (foxBadSprite != null) 
                            {
                                targetImage.sprite = foxBadSprite;

                            }
                            break;
                    }
                }
                else
                {
                    // 일반 모드일 때: 현재 테마에 맞는 스프라이트 사용

                    switch (result)
                    {
                        case JudgeResult.Wow:
                            if (foxWowSprite != null) 
                            {
                                targetImage.sprite = foxWowSprite; // 현재는 낮 테마만 있음

                            }
                            break;
                        case JudgeResult.Nice:
                            if (foxNiceSprite != null) 
                            {
                                targetImage.sprite = foxNiceSprite; // 현재는 낮 테마만 있음

                            }
                            break;
                        case JudgeResult.Bad:
                            if (foxBadSprite != null) 
                            {
                                targetImage.sprite = foxBadSprite; // 현재는 낮 테마만 있음

                            }
                            break;
                    }
                }
            }
            
            targetFox.SetActive(true);
            currentActiveFox = targetFox;
        }

        StartCoroutine(RevertFoxToNormal());
    }

    private System.Collections.IEnumerator RevertFoxToNormal()
    {
        yield return new WaitForSeconds(0.5f);
        
        // 🔧 최적화: 현재 활성화된 fox만 비활성화
        if (currentActiveFox != null)
        {
            currentActiveFox.SetActive(false);
            currentActiveFox = null;
        }
        
        if (foxNormal != null)
        {
            foxNormal.SetActive(true);
            currentActiveFox = foxNormal;
        }
    }

    private void ShowScorePopup(int score)
    {
        if (scorePopupText != null)
        {
            scorePopupText.text = $"+{score}";
            scorePopupText.gameObject.SetActive(true);
        }
    }

    private System.Collections.IEnumerator ShowJudgeCoroutine()
    {
        if (judgeImage != null)
            judgeImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        if (judgeImage != null)
            judgeImage.gameObject.SetActive(false);
        if (scorePopupText != null)
            scorePopupText.gameObject.SetActive(false);
    }

    public void UpdateScoreImageByCurrentSetting()
    {
        var setting = ScoreBasedDifficultyManager.Instance.CurrentSetting;
        if (setting == null || scoreImageUI == null) return;

        // 피버 모드 상태 확인
        bool isFever = IsFeverModeActive();
        
        // 🎯 핵심: 피버 모드일 때는 UpdateCachedThemeState() 호출하지 않음
        // 이 메서드가 피버 모드에서 cachedIsNight를 변경하여 문제를 일으킴
        if (!isFever)
        {
            UpdateCachedThemeState();
        }

        int tailCount = ScoreBasedDifficultyManager.Instance.GetTailIndexForCurrentSetting();
        if (tailCount < 0) tailCount = 0;

        // 스프라이트 선택
        Sprite[] spriteSet;
        if (isFever)
        {
            // 피버 모드일 때: 낮 테마 스프라이트
            spriteSet = setting.dayGumihoSprites;
        }
        else
        {
            // 일반 모드일 때: 테마에 따른 스프라이트
            spriteSet = cachedIsNight ? setting.nightGumihoSprites : setting.dayGumihoSprites;
        }

        if (spriteSet != null && tailCount < spriteSet.Length)
        {
            scoreImageUI.sprite = spriteSet[tailCount];
        }
    }

    public void SetGumihoImageSet(Sprite daySprite, Sprite nightSprite)
    {
        if (gumihoImageUI != null)
        {
            // 피버 모드 상태 확인
            bool isFever = IsFeverModeActive();
            
            // 피버 모드가 아닐 때만 테마 상태 업데이트
            if (!isFever)
            {
                UpdateCachedThemeState();
            }
            
            Sprite sprite;
            if (isFever)
            {
                // 피버 모드일 때: 낮 테마 스프라이트
                sprite = daySprite;
            }
            else
            {
                // 일반 모드일 때: 테마에 따른 스프라이트
                sprite = cachedIsNight ? (nightSprite ?? daySprite) : daySprite;
            }
            
                    gumihoImageUI.sprite = sprite;
        }
    }

    public void ForceUpdateJudgeImage()
    {
        bool wasInactive = !judgeImage.gameObject.activeSelf;
        if (wasInactive)
            judgeImage.gameObject.SetActive(true);

        // 🔧 최적화: 캐시된 테마 상태 사용
        UpdateCachedThemeState();

        // 🎯 핵심: 피버 모드일 때는 항상 낮 테마 스프라이트 사용
        bool isFever = IsFeverModeActive();
        
        Sprite targetSprite = lastJudgeResult switch
        {
            JudgeResult.Wow => (isFever || !cachedIsNight) ? wowSprite : wowSpriteNight,
            JudgeResult.Nice => (isFever || !cachedIsNight) ? niceSprite : niceSpriteNight,
            JudgeResult.Bad => (isFever || !cachedIsNight) ? badSprite : badSpriteNight,
            _ => badSprite
        };

        if (judgeImage != null)
            judgeImage.sprite = targetSprite;

        if (wasInactive)
            judgeImage.gameObject.SetActive(false);
    }

    public void ApplyFeverButtonSprite()
    {
        // 🔧 최적화: 캐시된 테마 상태 업데이트
        UpdateCachedThemeState();
        
        // 🔧 성능 최적화: FindAnyObjectByType 제거 - 인스펙터에서 직접 할당
        // var weaponUISetter = FindAnyObjectByType<GameSceneWeaponUISetter>();
        // if (weaponUISetter != null)
        // {
        //     weaponUISetter.ApplyFeverButtonSprite();
        // }
        
        // TODO: GameSceneWeaponUISetter 참조를 인스펙터에서 직접 할당하도록 변경 필요
    }

    public void ResetFeverState()
    {
        // 🔧 최적화: 캐시된 테마 상태 업데이트
        UpdateCachedThemeState();
        
        // 🔧 성능 최적화: FindAnyObjectByType 제거 - 인스펙터에서 직접 할당
        // var weaponUISetter = FindAnyObjectByType<GameSceneWeaponUISetter>();
        // if (weaponUISetter != null)
        // {
        //     weaponUISetter.ResetFeverState();
        // }
        
        // TODO: GameSceneWeaponUISetter 참조를 인스펙터에서 직접 할당하도록 변경 필요
    }

    /// <summary>
    /// 피버 모드일 때 표정과 팝업 이미지를 낮 테마로 변경
    /// </summary>
    public void SetFeverModeExpressions()
    {
        // 🎯 핵심: 기존 RevertFoxToNormal 코루틴 중단
        if (currentActiveFox != null)
        {
            StopAllCoroutines(); // 모든 코루틴 중단
        }
        
        // 🔧 성능 최적화: FindObjectsOfType 제거 - 인스펙터에서 직접 할당하도록 변경
        // if (foxNormal == null) foxNormal = FindObjectsOfType<GameObject>(true).FirstOrDefault(obj => obj.name.Contains("foxNormal"));
        // if (foxBad == null) foxBad = FindObjectsOfType<GameObject>(true).FirstOrDefault(obj => obj.name.Contains("foxBad"));
        // if (foxNice == null) foxNice = FindObjectsOfType<GameObject>(true).FirstOrDefault(obj => obj.name.Contains("foxNice"));
        // if (foxWow == null) foxWow = FindObjectsOfType<GameObject>(true).FirstOrDefault(obj => obj.name.Contains("foxWow"));
        // if (judgeImage == null) judgeImage = FindObjectsOfType<Image>(true).FirstOrDefault(img => img.name.Contains("judgeImage"));
        
        // 🎯 핵심: 모든 fox 오브젝트를 먼저 활성화 (비활성화된 상태에서 접근하기 위해)
        if (foxNormal != null) foxNormal.SetActive(true);
        if (foxBad != null) foxBad.SetActive(true);
        if (foxNice != null) foxNice.SetActive(true);
        if (foxWow != null) foxWow.SetActive(true);
        
        // 🎯 핵심: judgeImage도 활성화 (비활성화된 상태에서 접근하기 위해)
        if (judgeImage != null) judgeImage.gameObject.SetActive(true);
        
        // 🎯 핵심: foxBad, foxNice, foxWow의 스프라이트를 낮 테마로 변경
        if (foxBad != null)
        {
            // 🎯 핵심: UI 이미지이므로 Image 컴포넌트만 처리
            var foxBadImage = foxBad.GetComponent<Image>();
            
            if (foxBadImage != null && foxBadSprite != null)
            {
                foxBadImage.sprite = foxBadSprite; // 낮 테마 스프라이트 사용
            }
        }
        
        if (foxNice != null)
        {
            // 🎯 핵심: UI 이미지이므로 Image 컴포넌트만 처리
            var foxNiceImage = foxNice.GetComponent<Image>();
            
            if (foxNiceImage != null && foxNiceSprite != null)
            {
                foxNiceImage.sprite = foxNiceSprite; // 낮 테마 스프라이트 사용
            }
        }
        
        if (foxWow != null)
        {
            // 🎯 핵심: UI 이미지이므로 Image 컴포넌트만 처리
            var foxWowImage = foxWow.GetComponent<Image>();
            
            if (foxWowImage != null && foxWowSprite != null)
            {
                foxWowImage.sprite = foxWowSprite; // 낮 테마 스프라이트 사용
            }
        }
        
        // 🎯 핵심: 피버 모드일 때 표정을 낮 테마로 변경
        if (foxBad != null) foxBad.SetActive(false);
        if (foxNice != null) foxNice.SetActive(false);
        if (foxWow != null) foxWow.SetActive(false);
        if (judgeImage != null) judgeImage.gameObject.SetActive(false);
        // 피버 모드일 때는 foxNormal을 활성화 (낮 테마)
        if (foxNormal != null) foxNormal.SetActive(true);
        currentActiveFox = foxNormal; // 🎯 핵심: currentActiveFox 업데이트
        
        // 🎯 핵심: 피버 모드일 때 팝업 이미지를 낮 테마로 변경
        // cachedIsNight를 false로 설정하여 낮 테마 스프라이트 사용
        cachedIsNight = false;
        cachedIsFever = true; // 🎯 핵심: 강제로 피버 상태로 설정
        

    }

    /// <summary>
    /// 피버 모드 종료 시 원래 테마로 복원
    /// </summary>
    public void RestoreNormalModeExpressions()
    {
        // 원래 테마 상태로 복원
        UpdateCachedThemeState();
    }
}
