using UnityEngine;
using System.Collections;

public class NoteGlowActivator : MonoBehaviour
{
    [Header("Glow Settings")]
    [Tooltip("발광 지연 시간")]
    public float glowDelay = 0.2f;
    
    [Tooltip("피버 노트인지 확인")]
    public bool isFeverZone = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Note"))
        {
            // 피버 노트인지 확인
            var noteTypeHandler = other.GetComponent<NoteTypeHandler>();
            bool isFeverNote = noteTypeHandler != null && noteTypeHandler.noteType == NoteType.FeverNote;
            
            // 피버 존이거나 피버 노트인 경우에만 발광
            if (isFeverZone || isFeverNote)
            {
                Transform visual = other.transform.Find("9-Sliced");
                if (visual != null && visual.TryGetComponent(out Animator anim))
                {
                    StartCoroutine(DelayedGlow(anim, glowDelay));

                }
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
