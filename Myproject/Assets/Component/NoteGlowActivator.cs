using UnityEngine;
using System.Collections;

public class NoteGlowActivator : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Note"))
        {
            Transform visual = other.transform.Find("9-Sliced");
            if (visual != null && visual.TryGetComponent(out Animator anim))
            {
                StartCoroutine(DelayedGlow(anim, 0.15f)); // 💡 0.05초 뒤에 발광
            }
        }
    }

    private IEnumerator DelayedGlow(Animator anim, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (anim != null) // 중간에 제거되었을 수도 있으니 확인
        {
            anim.SetTrigger("Glow");
        }
    }
}
