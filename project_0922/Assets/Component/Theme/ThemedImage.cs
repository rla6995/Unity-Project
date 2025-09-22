using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ThemedImage : MonoBehaviour, IThemeApplicable
{
    public Sprite daySprite;
    public Sprite nightSprite;
    private Image image;

    void Awake() => image = GetComponent<Image>();
    private void OnEnable()
    {
        if (ThemeManager.Instance != null)
        {
            // 🎯 핵심: 피버 모드일 때는 테마를 적용하지 않음
            if (FeverModeManager.Instance != null && FeverModeManager.Instance.IsFeverUIActive())
            {
                // 피버 모드일 때는 낮 테마 스프라이트 유지
                if (image == null)
                    image = GetComponent<Image>();
                
                if (image != null && daySprite != null)
                    image.sprite = daySprite;
                return;
            }
            
            ApplyTheme(ThemeManager.Instance.IsNightTheme);
        }
    }
    public void ApplyTheme(bool isNight)
    {
        // 🎯 핵심: 피버 모드일 때는 테마를 적용하지 않음
        if (FeverModeManager.Instance != null && FeverModeManager.Instance.IsFeverUIActive())
        {
            // 피버 모드일 때는 낮 테마 스프라이트 유지
            if (image == null)
                image = GetComponent<Image>();
            
            if (image != null && daySprite != null)
                image.sprite = daySprite;
            return;
        }
        
        if (image == null)
            image = GetComponent<Image>();

        if (image != null)
            image.sprite = isNight ? nightSprite : daySprite;
    }
}
