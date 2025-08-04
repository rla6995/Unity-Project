using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoteInputType { Absorb, Swing, MergeHead, MergeTail }

public class WeaponJudgeSystem : MonoBehaviour
{
    public TimingJudgeSystem timingJudge;
    public static WeaponJudgeSystem Instance { get; private set; }
    public Animator playerAnimator;  // Inspector 연결 또는 외부에서 설정

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public JudgeResult TryJudge(NoteInputType inputType)
    {
        // ✅ MergeTail: 다중 판정 처리 (niceCollider 기준)
        if (inputType == NoteInputType.MergeTail)
        {
            GameObject[] notes = NoteHitDetector.GetAllNotesInNiceZone(timingJudge.NiceCollider);
            bool anyHit = false;

            foreach (GameObject note in notes)
            {
                if (note == null) continue;

                var handler = note.GetComponent<NoteTypeHandler>();
                if (handler == null || handler.noteType != NoteType.MergeTail) continue;

                JudgeResult result = timingJudge.GetJudgeResult(note.transform.position);
                if (result == JudgeResult.Bad) continue;

                GameManager.Instance?.AddScore(result);
                GameManager.Instance?.ShowJudgeText(result);
                AudioManager.Instance.PlayObjectSE(2);

                if (note.TryGetComponent(out MergeTailController tailCtrl))
                    tailCtrl.OnTailHit();
                else
                    StartCoroutine(PlayHitAnimationAndReturn(note));
                anyHit = true;
            }

            return anyHit ? JudgeResult.Nice : JudgeResult.Bad;
        }

        // ✅ 단일 판정
        GameObject singleNote = NoteHitDetector.GetNearestNote(timingJudge.JudgeCenter.position);
        if (singleNote == null) return JudgeResult.Bad;

        var handlerSingle = singleNote.GetComponent<NoteTypeHandler>();
        if (handlerSingle == null) return JudgeResult.Bad;

        NoteType type = handlerSingle.noteType;

        if (type == NoteType.FeverNote && !FeverModeManager.Instance.IsFeverActive())
        {
            GameManager.Instance?.ShowJudgeText(JudgeResult.Bad);
            return JudgeResult.Bad;
        }

        bool valid = inputType switch
        {
            NoteInputType.Absorb => type == NoteType.ManualNote || type == NoteType.BonusNote,
            NoteInputType.Swing => type == NoteType.WeaponNote || type == NoteType.BonusNote || type == NoteType.FeverNote,
            NoteInputType.MergeHead => type == NoteType.MergeHead || type == NoteType.BonusNote || type == NoteType.MergeTail,
            _ => false
        };

        JudgeResult resultSingle = timingJudge.GetJudgeResult(singleNote.transform.position);

        if (!valid || resultSingle == JudgeResult.Bad)
        {
            if (type != NoteType.FeverNote)
            {
                GameManager.Instance?.ShowJudgeText(JudgeResult.Bad);
                playerAnimator?.SetTrigger("BadTrigger");
            }
            return JudgeResult.Bad;
        }

        if (inputType == NoteInputType.MergeHead && type == NoteType.MergeHead)
        {
            if (resultSingle == JudgeResult.Wow || resultSingle == JudgeResult.Nice)
            {
                if (singleNote.TryGetComponent(out MergeHeadController headCtrl))
                {
                    headCtrl.StartHitLoop();
                }

                GameManager.Instance?.AddScore(resultSingle);
                GameManager.Instance?.ShowJudgeText(resultSingle);
                AudioManager.Instance.PlayObjectSE(2);
                return resultSingle;
            }
        }

        switch (type)
        {
            case NoteType.ManualNote: AudioManager.Instance.PlayObjectSE(0); break;
            case NoteType.WeaponNote: AudioManager.Instance.PlayObjectSE(1); break;
            case NoteType.MergeHead: AudioManager.Instance.PlayObjectSE(2); break;
            case NoteType.MergeTail: AudioManager.Instance.PlayObjectSE(2); break;
            case NoteType.BonusNote: AudioManager.Instance.PlayObjectSE(3); break;
            case NoteType.FeverNote: AudioManager.Instance.PlayObjectSE(4); break;
        }

        GameManager.Instance?.AddScore(resultSingle);
        GameManager.Instance?.ShowJudgeText(resultSingle);

        if (type == NoteType.BonusNote)
        {
            float feverAmount = resultSingle switch
            {
                JudgeResult.Wow => 15f,
                JudgeResult.Nice => 10f,
                _ => 0f
            };
            GameManager.Instance?.IncreaseFever(feverAmount);
        }

        StartCoroutine(PlayHitAnimationAndReturn(singleNote));
        return resultSingle;
    }

    private IEnumerator PlayHitAnimationAndReturn(GameObject note)
    {
        foreach (var col in note.GetComponentsInChildren<Collider2D>())
            col.enabled = false;
        Transform visual = note.transform.Find("9-Sliced");
        if (visual != null)
        {
            Animator animator = visual.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
        }
        yield return new WaitForSeconds(0.02f);
        MultiObjectPool.Instance?.Return(note);
    }
}
