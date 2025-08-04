using System.Collections.Generic;
using UnityEngine;

public class ThemedSpriteRenderer : MonoBehaviour, IThemeApplicable
{
    public Sprite daySprite;
    public Sprite nightSprite;
    private SpriteRenderer spriteRenderer;

    void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();
    private void OnEnable()
    {
        if (ThemeManager.Instance != null)
        {
            ApplyTheme(ThemeManager.Instance.IsNightTheme);
        }
    }
    public void ApplyTheme(bool isNight)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.sprite = isNight ? nightSprite : daySprite;
            Debug.Log($"[Theme] {gameObject.name} → {(isNight ? "Night" : "Day")} sprite 적용됨");
    }
}
