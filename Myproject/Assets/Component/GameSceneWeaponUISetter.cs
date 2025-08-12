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
    void Start()
    {
        ApplyGameUIButtonState();
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
public void ApplyGameUIButtonState()
{
    if (WeaponSwapManager.Instance == null)
    {
        Debug.LogWarning("[GameSceneWeaponUISetter] WeaponSwapManager 없음");
        return;
    }

    bool isLeft = WeaponSwapManager.Instance.IsMainWeaponLeft;
    bool isNight = BackgroundManager.Instance != null && BackgroundManager.Instance.IsNightTheme;

    Debug.Log($"[GameSceneWeaponUISetter] 버튼 위치 및 이미지 설정 | isNight: {isNight}, isLeft: {isLeft}");

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
        }
        else
        {
            swingSprite = isNight
                ? (isLeft ? swingLeftSprite_night : swingRightSprite_night)
                : (isLeft ? swingLeftSprite : swingRightSprite);
        }

        Sprite judgeSprite = isNight
            ? (isLeft ? judgeRightSprite_night : judgeLeftSprite_night)
            : (isLeft ? judgeRightSprite : judgeLeftSprite);

        Debug.Log($"[GameSceneWeaponUISetter] 스윙 스프라이트: {swingSprite?.name}, 판정 스프라이트: {judgeSprite?.name}");

        swingButtonImage.sprite = swingSprite;
        judgeButtonImage.sprite = judgeSprite;
    }
    else
    {
        Debug.LogWarning("[GameSceneWeaponUISetter] 스프라이트 이미지가 연결되지 않았습니다.");
    }
}

}
