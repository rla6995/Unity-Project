using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
    public Animator playerAnimator;
void OnTriggerEnter2D(Collider2D other)
{
    
    if (other.CompareTag("Note"))
        {
            var typeHandler = other.GetComponent<NoteTypeHandler>();

            if (typeHandler != null)
            {
                if (typeHandler.noteType == NoteType.BonusNote)
                {
                    GameManager.Instance?.BonusNoteHitByPlayer();
                    MultiObjectPool pool = FindAnyObjectByType<MultiObjectPool>();
                    if (pool != null) pool.Return(other.gameObject);
                    else other.gameObject.SetActive(false);
                    return;
                }

                // ✅ FeverNote일 경우 게임오버 없이 제거만
                if (typeHandler.noteType == NoteType.FeverNote)
                {
                    MultiObjectPool pool = FindAnyObjectByType<MultiObjectPool>();
                    if (pool != null) pool.Return(other.gameObject);
                    else other.gameObject.SetActive(false);
                    return;
                }
            }
            // 일반 노트는 게임오버
            playerAnimator?.SetTrigger("BadTrigger");
            GameManager.Instance.TriggerGameOver();
        }
}

}
