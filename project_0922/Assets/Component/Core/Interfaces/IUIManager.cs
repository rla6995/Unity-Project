using UnityEngine;

/// <summary>
/// UI 관리를 위한 인터페이스
/// 의존성 역전 원칙: 구체 클래스 대신 인터페이스에 의존
/// </summary>
public interface IUIManager
{
    void ShowJudgeText(JudgeResult result);
    void UpdateScoreImageByCurrentSetting();
    void SetGumihoImageSet(Sprite daySprite, Sprite nightSprite);
    void ForceUpdateJudgeImage();
}
