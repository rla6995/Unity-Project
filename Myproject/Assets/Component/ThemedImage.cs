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
            ApplyTheme(ThemeManager.Instance.IsNightTheme);
        }
    }
    public void ApplyTheme(bool isNight)
    {
        if (image == null)
            image = GetComponent<Image>();

        if (image != null)
            image.sprite = isNight ? nightSprite : daySprite;
    }
}
