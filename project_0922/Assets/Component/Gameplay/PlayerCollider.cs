using UnityEngine;

/// <summary>
/// 플레이어 충돌 처리
/// 단일 책임 원칙: 충돌 처리만 담당
/// 의존성 역전 원칙: 인터페이스에 의존
/// </summary>
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
                ProcessNoteCollision(typeHandler, other.gameObject);
            }
        }
    }

    /// <summary>
    /// 노트 충돌 처리 (단일 책임 원칙 적용)
    /// </summary>
    private void ProcessNoteCollision(NoteTypeHandler typeHandler, GameObject noteObject)
    {
        switch (typeHandler.noteType)
        {
            case NoteType.BonusNote:
                HandleBonusNote(noteObject);
                break;

            case NoteType.FeverNote:
                HandleFeverNote(noteObject);
                break;

            default:
                HandleRegularNote();
                break;
        }
    }

    /// <summary>
    /// 보너스 노트 처리
    /// </summary>
    private void HandleBonusNote(GameObject noteObject)
    {
        // 보너스 노트는 점수 없이 피버 게이지만 증가
        ScoreManager.Instance?.IncreaseFever(10f);
        ReturnNoteToPool(noteObject);
    }

    /// <summary>
    /// 피버 노트 처리
    /// </summary>
    private void HandleFeverNote(GameObject noteObject)
    {
        // 피버 노트는 게임오버 없이 제거만
        // 🔧 피버노트가 플레이어에 닿았을 때 칸 해제
        var feverMover = noteObject.GetComponent<FeverBonusNoteMover>();
        if (feverMover != null)
        {
            // FeverBonusNoteMover의 ReturnToPool을 호출하여 칸 해제
            feverMover.ReturnToPool();
        }
        else
        {
            // FeverBonusNoteMover가 없으면 직접 풀로 반환
            ReturnNoteToPool(noteObject);
        }
    }

    /// <summary>
    /// 일반 노트 처리
    /// </summary>
    private void HandleRegularNote()
    {
        // 일반 노트는 게임오버
        playerAnimator?.SetTrigger("BadTrigger");
        GameManager.Instance?.TriggerGameOver();
    }

    /// <summary>
    /// 노트를 풀로 반환
    /// </summary>
    private void ReturnNoteToPool(GameObject noteObject)
    {
        MultiObjectPool pool = FindAnyObjectByType<MultiObjectPool>();
        if (pool != null)
            pool.Return(noteObject);
        else
            noteObject.SetActive(false);
    }
}
