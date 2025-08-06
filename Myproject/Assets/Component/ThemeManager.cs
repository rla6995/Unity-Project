using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }

    public ThemeType CurrentThemeType { get; private set; } = ThemeType.Day;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// ThemeType에 따라 낮/밤 테마 적용 (비활성 오브젝트 포함)
    /// </summary>
    public void ApplyTheme(ThemeType themeType)
    {
        CurrentThemeType = themeType;

        bool isNightTheme = themeType != ThemeType.Day;

        ApplyThemeToAllIncludingInactive<ThemedSpriteRenderer>(isNightTheme);
        ApplyThemeToAllIncludingInactive<ThemedImage>(isNightTheme);
        ApplyThemeToAllIncludingInactive<ThemedAnimator>(isNightTheme);
    }

    /// <summary>
    /// 비활성 오브젝트도 포함하여 테마 적용
    /// </summary>
    private void ApplyThemeToAllIncludingInactive<T>(bool isNight) where T : MonoBehaviour, IThemeApplicable
    {
        var objects = Resources.FindObjectsOfTypeAll<T>();
        foreach (var obj in objects)
        {
            // 에디터에만 존재하는 오브젝트 제외
            if (obj == null || obj.gameObject == null) continue;

            if (obj.gameObject.hideFlags == HideFlags.NotEditable ||
                obj.gameObject.hideFlags == HideFlags.HideAndDontSave)
                continue;

            obj.ApplyTheme(isNight);
        }
    }

    /// <summary>
    /// DifficultySetting 기반 전체 테마 설정 (배경, UI, BGM, Theme 적용 포함)
    /// </summary>
    public void ApplySetting(DifficultySetting setting)
    {
        CurrentThemeType = setting.theme;

        // 1. 피버 배경 정지
        BackgroundManager.Instance?.StopFeverScroll();

        // 2. 배경 전환
        Sprite targetSprite = null;
        switch (setting.theme)
        {
            case ThemeType.Day:
                BackgroundManager.Instance.IsNightTheme = false;
                targetSprite = BackgroundManager.Instance?.daySprite;
                BackgroundManager.Instance?.ApplyDayTheme();
                break;
            case ThemeType.BurningNight:
                BackgroundManager.Instance.IsNightTheme = true;
                BackgroundManager.Instance?.ApplyAwakenedTheme();
                var feverButton1 = GameObject.Find("FeverDarkCover");
                if (feverButton1 != null)
                    feverButton1.SetActive(false);
                var feverButton2 = GameObject.Find("FeverFillImage");
                if (feverButton2 != null)
                    feverButton2.SetActive(false);
                break;
            case ThemeType.Night1:
            case ThemeType.Night2:
            case ThemeType.Night3:
            case ThemeType.Night4:
                BackgroundManager.Instance.IsNightTheme = true;
                targetSprite = BackgroundManager.Instance?.nightSprite;
                BackgroundManager.Instance?.ApplyNightTheme();
                break;
            case ThemeType.Ending:
                targetSprite = BackgroundManager.Instance?.daySprite;
                BackgroundManager.Instance?.ApplyDayTheme();
                break;
            default:
                targetSprite = BackgroundManager.Instance?.daySprite;
                BackgroundManager.Instance?.ApplyDayTheme();
                break;
        }

        if (targetSprite != null)
            BackgroundManager.Instance?.StartTransition(targetSprite);
        if (setting.theme == ThemeType.BurningNight)
        {
            BackgroundManager.Instance?.ApplyAwakenedTheme();  // 내부에서 orb 스프라이트 설정

            // 테마 적용 이후 Themed 오브젝트가 다시 덮어쓸 수 있으므로 재적용
            if (BackgroundManager.Instance?.orbRenderer != null && BackgroundManager.Instance.awakenedOrbSprite != null)
            {
                BackgroundManager.Instance.orbRenderer.sprite = BackgroundManager.Instance.awakenedOrbSprite;
            }
        }
        // 3. BGM 변경
        switch (setting.theme)
        {
            case ThemeType.Day:
                AudioManager.Instance?.PlayBGM(1);
                break;
            case ThemeType.BurningNight:
                AudioManager.Instance?.PlayBGM(5); // 불타는 밤 전용 BGM
                break;
            case ThemeType.Ending:
                AudioManager.Instance?.PlayBGM(6); // 엔딩 BGM
                break;
            default:
                AudioManager.Instance?.PlayBGM(2); // 일반 밤 BGM
                break;
        
        }

        // 4. Themed 오브젝트 일괄 적용
        ApplyTheme(setting.theme);
        // 5. 버튼 이미지 및 스왑 UI 이미지 갱신
        GameManager.Instance.SetJudgeSprites(setting.wowJudgeSprite, setting.niceJudgeSprite, setting.badJudgeSprite);
        GameManager.Instance.ForceUpdateJudgeImage();
        // ✅ 게임씬 버튼 UI 갱신
        var setter = GameObject.FindObjectOfType<GameSceneWeaponUISetter>();
        if (setter != null)
            setter.ApplyGameUIButtonState();

        
    }

    public bool IsNightTheme => CurrentThemeType != ThemeType.Day;
    public bool IsBurningTheme => CurrentThemeType == ThemeType.BurningNight;
}
