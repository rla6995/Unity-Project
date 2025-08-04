using UnityEngine;
using UnityEngine.UI;

public class OptionSoundButton : MonoBehaviour
{
    public enum SoundType { BGM, SE }
    public SoundType soundType;         // 이 버튼이 제어하는 사운드 종류

    public Image buttonImage;           // 버튼 이미지
    public Sprite onSprite;             // 켜짐 상태 아이콘
    public Sprite offSprite;            // 꺼짐 상태 아이콘

    private bool isOn = true;
    private string prefKey;

    void Start()
    {
        // PlayerPrefs 키를 BGM 또는 SE로 분기
        prefKey = (soundType == SoundType.BGM) ? "SoundBGM" : "SoundSE";

        // 저장된 상태를 불러옴 (기본값 1 = 켜짐)
        isOn = PlayerPrefs.GetInt(prefKey, 1) == 1;

        // 아이콘 적용
        UpdateIcon();
    }

    public void ToggleSound()
    {
        // 상태 전환
        isOn = !isOn;

        // 저장
        PlayerPrefs.SetInt(prefKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
    if (soundType == SoundType.BGM)
        AudioManager.Instance.SetBGMOn(isOn);
    else
        AudioManager.Instance.SetSEOn(isOn);
        // 아이콘 갱신
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        buttonImage.sprite = isOn ? onSprite : offSprite;
    }
}
