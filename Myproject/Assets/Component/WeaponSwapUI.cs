using UnityEngine;
using UnityEngine.UI;

public class WeaponSwapUI : MonoBehaviour
{
    [Header("옵션 패널 UI 아이콘")]
    public Image leftIcon;
    public Image rightIcon;

    [Header("무기 아이콘 이미지")]
    public Sprite weapon1;
    public Sprite weapon2;
    public Sprite weapon1_night;
    public Sprite weapon2_night;

    [Header("게임 상의 버튼들")]
    public RectTransform swingButton;
    public RectTransform judgeButton;

    [Header("버튼 이미지 스프라이트")]
    public Sprite swingLeftSprite;
    public Sprite swingRightSprite;
    public Sprite judgeLeftSprite;
    public Sprite judgeRightSprite;

    public Sprite swingLeftSprite_night;
    public Sprite swingRightSprite_night;
    public Sprite judgeLeftSprite_night;
    public Sprite judgeRightSprite_night;

    [Header("버튼의 Image 컴포넌트")]
    public Image swingButtonImage;
    public Image judgeButtonImage;

    private void Start() => Initialize();

    private void OnEnable() => ApplySwapState();

    public void Initialize() => ApplySwapState();

    public void ApplySwapState()
    {
        if (leftIcon == null || rightIcon == null) return;

        bool isLeft = WeaponSwapManager.Instance?.IsMainWeaponLeft ?? true;

        // 아이콘 이미지 설정
        UpdateIcons();

        // 버튼 위치
        Vector3 tempPos = swingButton.localPosition;
        if (isLeft)
        {
            swingButton.localPosition = new Vector3(-Mathf.Abs(tempPos.x), tempPos.y, tempPos.z);
            judgeButton.localPosition = new Vector3(Mathf.Abs(tempPos.x), tempPos.y, tempPos.z);
        }
        else
        {
            swingButton.localPosition = new Vector3(Mathf.Abs(tempPos.x), tempPos.y, tempPos.z);
            judgeButton.localPosition = new Vector3(-Mathf.Abs(tempPos.x), tempPos.y, tempPos.z);
        }

        UpdateButtonImages(isLeft);
    }

    public void SwapWeapons()
    {
        if (leftIcon == null || rightIcon == null) return;

        WeaponSwapManager.Instance?.ToggleWeaponSide();
        bool isLeft = WeaponSwapManager.Instance.IsMainWeaponLeft;

        UpdateIcons();
        UpdateButtonImages(isLeft);

        // 위치만 스왑
        Vector3 temp = swingButton.localPosition;
        swingButton.localPosition = judgeButton.localPosition;
        judgeButton.localPosition = temp;

        FindObjectOfType<GameSceneWeaponUISetter>()?.ApplyGameUIButtonState();
    }

    private void UpdateIcons()
    {
        if (WeaponSwapManager.Instance == null) return;

        bool isNight = BackgroundManager.Instance != null && BackgroundManager.Instance.IsNightTheme;

        if (WeaponSwapManager.Instance.IsMainWeaponLeft)
        {
            leftIcon.sprite = isNight ? weapon1_night : weapon1;
            rightIcon.sprite = isNight ? weapon2_night : weapon2;
        }
        else
        {
            leftIcon.sprite = isNight ? weapon2_night : weapon2;
            rightIcon.sprite = isNight ? weapon1_night : weapon1;
        }
    }

    private void UpdateButtonImages(bool isMainWeaponLeft)
    {
        bool isNight = BackgroundManager.Instance != null && BackgroundManager.Instance.IsNightTheme;
        Debug.Log($"[WeaponSwapUI] 버튼 이미지 설정 실행됨 | isNight: {isNight}, isLeft: {isMainWeaponLeft}");
    if (swingButtonImage != null && judgeButtonImage != null)
    {
        Sprite swingSprite = isNight
            ? (isMainWeaponLeft ? swingLeftSprite_night : swingRightSprite_night)
            : (isMainWeaponLeft ? swingLeftSprite : swingRightSprite);

        Sprite judgeSprite = isNight
            ? (isMainWeaponLeft ? judgeRightSprite_night : judgeLeftSprite_night)
            : (isMainWeaponLeft ? judgeRightSprite : judgeLeftSprite);

        Debug.Log($"[WeaponSwapUI] 스윙 스프라이트: {swingSprite?.name}, 판정 스프라이트: {judgeSprite?.name}");

        swingButtonImage.sprite = swingSprite;
        judgeButtonImage.sprite = judgeSprite;
    }
    else
    {
        Debug.LogWarning("[WeaponSwapUI] 스프라이트 이미지가 연결되지 않았습니다.");
    }
    }
}
