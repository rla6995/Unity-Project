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
    private bool isFeverActive = false;

    [Header("피버용 스프라이트")]
    public Sprite feverSwingLeftSprite;
    public Sprite feverSwingRightSprite;

    private void Start() => Initialize();

    private void OnEnable() => ApplySwapState();

    public void Initialize() => ApplySwapState();
    
    public void ApplySwapState()
    {
        if (leftIcon == null || rightIcon == null) return;

        bool isLeft = WeaponSwapManager.Instance?.IsMainWeaponLeft ?? true;

        // 아이콘 이미지 설정
        UpdateIcons();
        
        // 버튼 위치 설정
        UpdateButtonPositions(isLeft);
        
        // 버튼 이미지 설정
        UpdateButtonImages(isLeft, isFeverActive);
    }

    public void SwapWeapons()
    {
        if (leftIcon == null || rightIcon == null) return;


        
        WeaponSwapManager.Instance?.ToggleWeaponSide();
        bool isLeft = WeaponSwapManager.Instance.IsMainWeaponLeft;
        


        // 아이콘 이미지 설정
        UpdateIcons();
        
        // 버튼 위치 설정
        UpdateButtonPositions(isLeft);
        
        // 버튼 이미지 설정
        UpdateButtonImages(isLeft, isFeverActive);
        
        // 게임 씬의 UI는 GameSceneWeaponUISetter가 자체적으로 처리
        // 옵션 메뉴에서는 GameSceneWeaponUISetter를 찾을 수 없으므로 호출하지 않음
    }

    private void UpdateIcons()
    {
        if (WeaponSwapManager.Instance == null) return;

        bool isNight = BackgroundManager.Instance != null && BackgroundManager.Instance.IsNightTheme;
        bool isLeft = WeaponSwapManager.Instance.IsMainWeaponLeft;
        


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

    private void UpdateButtonPositions(bool isLeft)
    {
        if (swingButton == null || judgeButton == null) return;

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
    }

    private void UpdateButtonImages(bool isMainWeaponLeft, bool useFeverSprite = false)
    {
        if (swingButtonImage == null || judgeButtonImage == null) return;

        bool isNight = BackgroundManager.Instance != null && BackgroundManager.Instance.IsNightTheme;


        Sprite swingSprite;

        // ✅ 피버 모드일 경우 피버용 스프라이트 강제 적용
        if (useFeverSprite)
        {
            swingSprite = isMainWeaponLeft ? feverSwingLeftSprite : feverSwingRightSprite;
        }
        else
        {
            swingSprite = isNight
                ? (isMainWeaponLeft ? swingLeftSprite_night : swingRightSprite_night)
                : (isMainWeaponLeft ? swingLeftSprite : swingRightSprite);
        }

        // ✅ 판정 버튼은 그대로 테마 기반 적용
        Sprite judgeSprite = isNight
            ? (isMainWeaponLeft ? judgeRightSprite_night : judgeLeftSprite_night)
            : (isMainWeaponLeft ? judgeRightSprite : judgeLeftSprite);



        swingButtonImage.sprite = swingSprite;
        judgeButtonImage.sprite = judgeSprite;
    }

    public void ResetFeverState()
    {
        isFeverActive = false;
    }

    public void ApplyFeverButtonSprite()
    {
        if (WeaponSwapManager.Instance == null) return;

        isFeverActive = true;

        bool isLeft = WeaponSwapManager.Instance.IsMainWeaponLeft;

        if (swingButtonImage != null)
        {
            swingButtonImage.sprite = isLeft ? feverSwingLeftSprite : feverSwingRightSprite;
        }
    }
}
