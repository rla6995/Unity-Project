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


        if (animator == null)
        {

            return;
        }


        animator.SetTrigger("Play");
    }
}
