using UnityEngine;

public class ThemedAnimator : MonoBehaviour, IThemeApplicable
{
    public RuntimeAnimatorController dayController;
    public RuntimeAnimatorController nightController;
    private Animator animator;

    void Awake() => animator = GetComponent<Animator>();
    private void OnEnable()
    {
        if (ThemeManager.Instance != null)
        {
            ApplyTheme(ThemeManager.Instance.IsNightTheme);
        }
    }
    public void ApplyTheme(bool isNight)
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator != null)
            animator.runtimeAnimatorController = isNight ? nightController : dayController;
        Debug.Log($"[ThemedAnimator] {(isNight ? "Night" : "Day")} 컨트롤러로 교체됨: {animator.runtimeAnimatorController.name}");
        Debug.Log($"[ThemedAnimator] 현재 상태: {animator.GetCurrentAnimatorStateInfo(0).IsName("YourStateNameHere")}");
    }
}
