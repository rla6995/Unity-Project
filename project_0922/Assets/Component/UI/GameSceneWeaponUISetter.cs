using UnityEngine;
using UnityEngine.UI;

public class GameSceneWeaponUISetter : MonoBehaviour
{
    public RectTransform swingButton;
    public RectTransform judgeButton;

    public Image swingButtonImage;
    public Image judgeButtonImage;

    public Sprite swingLeftSprite;
    public Sprite swingRightSprite;
    public Sprite judgeLeftSprite;
    public Sprite judgeRightSprite;

    public Sprite swingLeftSprite_night;
    public Sprite swingRightSprite_night;
    public Sprite judgeLeftSprite_night;
    public Sprite judgeRightSprite_night;
    public Sprite feverSwingLeftSprite;
    public Sprite feverSwingRightSprite;
    private bool isFeverActive = false;
    private bool lastWeaponState = true; // 이전 무기 상태를 저장
    
    void Start()
    {
        ApplyGameUIButtonState();
        lastWeaponState = WeaponSwapManager.Instance?.IsMainWeaponLeft ?? true;
    }
    
    void Update()
    {
        // 무기 스왑 상태가 변경되었는지 확인
        if (WeaponSwapManager.Instance != null)
        {
            bool currentWeaponState = WeaponSwapManager.Instance.IsMainWeaponLeft;
            if (currentWeaponState != lastWeaponState)
            {
                lastWeaponState = currentWeaponState;
                ApplyGameUIButtonState();
            }
        }
    }
    public void ResetFeverState()
    {
        isFeverActive = false;
        // 피버 상태가 리셋되면 버튼 이미지도 업데이트
        ApplyGameUIButtonState();
    }
    public void ApplyFeverButtonSprite()
    {
        if (WeaponSwapManager.Instance == null) return;

        isFeverActive = true;

        // 피버 상태가 적용되면 버튼 이미지 즉시 업데이트
        ApplyGameUIButtonState();
    }
public void ApplyGameUIButtonState()
{
    if (WeaponSwapManager.Instance == null)
    {

        return;
    }

    bool isLeft = WeaponSwapManager.Instance.IsMainWeaponLeft;
    bool isNight = BackgroundManager.Instance != null && BackgroundManager.Instance.IsNightTheme;



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

    if (swingButtonImage != null && judgeButtonImage != null)
    {
        Sprite swingSprite;

        if (isFeverActive)
        {
            swingSprite = isLeft ? feverSwingLeftSprite : feverSwingRightSprite;
            // 피버 모드일 때 흡수 버튼 비활성화
            if (judgeButton != null) judgeButton.gameObject.SetActive(false);
        }
        else
        {
            swingSprite = isNight
                ? (isLeft ? swingLeftSprite_night : swingRightSprite_night)
                : (isLeft ? swingLeftSprite : swingRightSprite);
            // 피버 모드가 아닐 때 흡수 버튼 활성화
            if (judgeButton != null) judgeButton.gameObject.SetActive(true);
        }

        Sprite judgeSprite = isNight
            ? (isLeft ? judgeRightSprite_night : judgeLeftSprite_night)
            : (isLeft ? judgeRightSprite : judgeLeftSprite);



        swingButtonImage.sprite = swingSprite;
        judgeButtonImage.sprite = judgeSprite;
    }
    else
    {

    }
}

}
