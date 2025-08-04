using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource seSource;
    public AudioSource weaponSESource;      // ✅ 무기 사운드 전용
    public AudioSource objectSESource;      // ✅ 오브젝트 파괴 전용
    [Header("BGM & SE Lists")]
    public List<AudioClip> bgmList;
    public List<AudioClip> seList;
    public List<AudioClip> weaponSEList = new List<AudioClip>();        // 무기 사운드용
    public List<AudioClip> objectSEList = new List<AudioClip>();        // 오브젝트 파괴용
    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        ApplySavedSettings();
    }

    // 특정 BGM 인덱스를 재생 (Loop)
    public void PlayBGM(int index)
    {
        if (IsValidIndex(bgmList, index))
        {
            bgmSource.clip = bgmList[index];
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    // 특정 SE 인덱스를 재생 (OneShot)
    public void PlaySE(int index)
    {
        if (IsValidIndex(seList, index))
        {
            seSource.PlayOneShot(seList[index]);
        }
    }
public void PlayWeaponSE(int index)
{
    if (IsValidIndex(weaponSEList, index))
        weaponSESource.PlayOneShot(weaponSEList[index]);
}

public void PlayObjectSE(int index)
{
    if (IsValidIndex(objectSEList, index))
        objectSESource.PlayOneShot(objectSEList[index]);
}
    // BGM/SE ON/OFF
    public void SetBGMOn(bool isOn)
    {
        bgmSource.mute = !isOn;
        
        PlayerPrefs.SetInt("SoundBGM", isOn ? 1 : 0);
    }

    public void SetSEOn(bool isOn)
    {
        seSource.mute = !isOn;
        weaponSESource.mute = !isOn;
        objectSESource.mute = !isOn;
        PlayerPrefs.SetInt("SoundSE", isOn ? 1 : 0);
    }

    public bool IsBGMOn() => !bgmSource.mute;
    public bool IsSEOn() => !seSource.mute;

    private void ApplySavedSettings()
    {
        SetBGMOn(PlayerPrefs.GetInt("SoundBGM", 1) == 1);
        SetSEOn(PlayerPrefs.GetInt("SoundSE", 1) == 1);
    }

    private bool IsValidIndex(List<AudioClip> list, int index)
    {
        return list != null && index >= 0 && index < list.Count;
    }
}
