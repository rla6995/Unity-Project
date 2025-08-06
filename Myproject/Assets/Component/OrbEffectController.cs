using UnityEngine;
public class OrbEffectController : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TriggerEffect()
    {
        Debug.Log("[OrbEffectController] TriggerEffect 실행됨");

        if (animator == null)
        {
            Debug.LogWarning("Animator가 연결되지 않았습니다!");
            return;
        }

        Debug.Log("[OrbEffectController] Play 트리거 발동");
        animator.SetTrigger("Play");
    }
}
