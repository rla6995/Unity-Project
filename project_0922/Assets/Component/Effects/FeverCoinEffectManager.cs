using UnityEngine;

public class FeverCoinEffectManager : MonoBehaviour
{
    public static FeverCoinEffectManager Instance { get; private set; }
    public GameObject coinEffectObject; // 전체 Effect 오브젝트 (FeverCoinEffect)
    public Animator coinAnimator;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (coinEffectObject != null)
            coinEffectObject.SetActive(false); // 시작 시 숨기기
    }

    public void Play()
    {
        if (coinEffectObject == null || coinAnimator == null) return;

        coinEffectObject.SetActive(true);
        coinAnimator.SetTrigger("DropTrigger");
        StartCoroutine(DisableAfterDelay(0.2f)); // 애니메이션 길이에 따라 조정
    }

    private System.Collections.IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        coinEffectObject.SetActive(false);
    }
}
