using UnityEngine;
using System.Collections.Generic;

public enum ThemeType
{
    Day,
    Night1,
    Night2,
    Night3,
    Night4,
    BurningNight,
    Ending
}

[System.Serializable]
public class GumihoImageSet
{
    public Sprite dayImage;   // 낮 테마일 때 사용할 구미호 이미지
    public Sprite nightImage; // 밤 테마일 때 사용할 구미호 이미지
}

[CreateAssetMenu(fileName = "DifficultySetting", menuName = "Game/Difficulty Setting")]
public class DifficultySetting : ScriptableObject
{
    [Header("기준 점수")]
    public int scoreThreshold;

    [Header("테마 정보")]
    public ThemeType theme;

    [Header("휠 속도")]
    public float rotationSpeed;

    [Header("출현 노트 타입")]
    public List<NoteType> allowedNoteTypes;

    [Header("구미호 이미지 (낮/밤)")]
    public GumihoImageSet gumihoImages;
    public Sprite dayGumihoSprite;
    public Sprite nightGumihoSprite;
    [Header("구미호 꼬리 스프라이트")]
    public Sprite[] dayGumihoSprites;
    public Sprite[] nightGumihoSprites;
    public Sprite[] GetGumihoSpriteForTheme(ThemeType theme)
    {
        switch (theme)
        {
            case ThemeType.Night1:
            case ThemeType.Night2:
            case ThemeType.Night3:
            case ThemeType.Night4:
            case ThemeType.BurningNight:
                return nightGumihoSprites;
            default:
                return dayGumihoSprites;
        }
    }
    [Header("노트별 스폰 확률 (합계는 자동 정규화됨)")]
    public List<NoteSpawnChance> noteSpawnChances = new();
    public Sprite wowJudgeSprite;
    public Sprite niceJudgeSprite;
    public Sprite badJudgeSprite;

}
