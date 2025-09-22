using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 피버 게이지 버튼 클릭 시 피버 모드 진입을 처리하는 컴포넌트
/// 피버 게이지가 100%일 때만 피버 모드에 진입
/// </summary>
public class FeverGaugeButton : MonoBehaviour, IPointerClickHandler
{
    [Header("Fever Gauge UI")]
    [SerializeField] private Image feverFillImage;
    [SerializeField] private Image feverDarkCover;
    
    
    private void Start()
    {
        // 피버 게이지 UI 참조가 없으면 자동으로 찾기
        if (feverFillImage == null)
        {
            feverFillImage = GameObject.Find("FeverFillImage")?.GetComponent<Image>();
        }
        
        if (feverDarkCover == null)
        {
            feverDarkCover = GameObject.Find("FeverDarkCover")?.GetComponent<Image>();
        }
    }
    
    /// <summary>
    /// 피버 게이지 클릭 시 호출되는 메서드
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        TryEnterFeverMode();
    }
    
    /// <summary>
    /// 피버 모드 진입 시도
    /// </summary>
    public void TryEnterFeverMode()
    {
        // 1) 이미 피버 중이면 무시
        if (FeverModeManager.Instance != null && FeverModeManager.Instance.IsFeverActive())
        {

            return;
        }
        
        // 2) 피버 게이지가 100%인지 확인
        float currentGauge = ScoreManager.Instance?.GetFeverGauge() ?? 0f;
        if (currentGauge < 100f)
        {

            
            // 시각적 피드백 (게이지가 부족할 때)
            ShowInsufficientGaugeFeedback();
            return;
        }
        
        // 3) 피버 진입 시퀀스 시작

        
        FeverModeManager.Instance?.StartFeverEntrySequence();
    }
    
    /// <summary>
    /// 게이지가 부족할 때 시각적 피드백 표시
    /// </summary>
    private void ShowInsufficientGaugeFeedback()
    {
        // 간단한 시각적 피드백 (게이지 깜빡임 등)
        if (feverFillImage != null)
        {
            StartCoroutine(FlashGaugeCoroutine());
        }
    }
    
    /// <summary>
    /// 게이지 깜빡임 효과
    /// </summary>
    private System.Collections.IEnumerator FlashGaugeCoroutine()
    {
        if (feverFillImage == null) yield break;
        
        Color originalColor = feverFillImage.color;
        Color flashColor = Color.red;
        
        // 3번 깜빡임
        for (int i = 0; i < 3; i++)
        {
            feverFillImage.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            feverFillImage.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    /// <summary>
    /// 외부에서 호출할 수 있는 피버 모드 진입 메서드
    /// </summary>
    public void OnFeverGaugeClicked()
    {
        TryEnterFeverMode();
    }
}
