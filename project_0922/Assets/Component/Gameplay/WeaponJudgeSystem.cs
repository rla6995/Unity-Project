using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoteInputType { Absorb, Swing, MergeHead, MergeTail }

/// <summary>
/// 무기 입력에 따른 노트 판정 시스템
/// 단일 책임 원칙: 무기 판정만 담당
/// 의존성 역전 원칙: 인터페이스에 의존
/// </summary>
public class WeaponJudgeSystem : MonoBehaviour
{
    public static WeaponJudgeSystem Instance { get; private set; }
    
    [Header("Dependencies")]
    [SerializeField] private ITimingJudge timingJudge;
    [SerializeField] private Animator playerAnimator;

    // 🔧 최적화: 컴포넌트 캐싱
    private Dictionary<GameObject, NoteTypeHandler> handlerCache = new();
    private const int MAX_CACHE_SIZE = 100; // 캐시 크기 제한

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // 인터페이스 구현체 찾기
        if (timingJudge == null)
            timingJudge = FindObjectOfType<TimingJudgeSystem>();
    }

    // 🔧 최적화: 캐시된 NoteTypeHandler 반환
    private NoteTypeHandler GetCachedHandler(GameObject note)
    {
        if (note == null) return null;
        
        if (!handlerCache.TryGetValue(note, out NoteTypeHandler handler))
        {
            handler = note.GetComponent<NoteTypeHandler>();
            
            // 캐시 크기 제한으로 메모리 사용량 조절
            if (handlerCache.Count >= MAX_CACHE_SIZE)
            {
                var firstKey = handlerCache.Keys.GetEnumerator();
                if (firstKey.MoveNext())
                    handlerCache.Remove(firstKey.Current);
            }
            
            handlerCache[note] = handler;
        }
        return handler;
    }

    // 🔧 최적화: 캐시 정리 (노트가 풀로 반환될 때)
    public void ClearHandlerCache(GameObject note)
    {
        if (note != null && handlerCache.ContainsKey(note))
            handlerCache.Remove(note);
    }

    public JudgeResult TryJudge(NoteInputType inputType)
    {
        if (timingJudge == null) return JudgeResult.Bad;

        // ✅ MergeTail: 다중 판정 처리 (niceCollider 기준)
        if (inputType == NoteInputType.MergeTail)
        {
            GameObject[] notes = NoteHitDetector.GetAllNotesInNiceZone(timingJudge.NiceCollider);
            bool anyHit = false;

            foreach (GameObject note in notes)
            {
                if (note == null) continue;

                // 🔧 최적화: 캐시된 handler 사용
                var handler = GetCachedHandler(note);
                if (handler == null || handler.noteType != NoteType.MergeTail) continue;

                JudgeResult result = timingJudge.GetJudgeResult(note.transform.position);
                if (result == JudgeResult.Bad) continue;

                ProcessSuccessfulHit(note, result);
                anyHit = true;
            }

            return anyHit ? JudgeResult.Nice : JudgeResult.Bad;
        }

        // ✅ 단일 판정
        GameObject singleNote = NoteHitDetector.GetNearestNote(timingJudge.JudgeCenter.position);
        if (singleNote == null) return JudgeResult.Bad;

        // 🔧 최적화: 캐시된 handler 사용
        var handlerSingle = GetCachedHandler(singleNote);
        if (handlerSingle == null) return JudgeResult.Bad;

        NoteType type = handlerSingle.noteType;

        if (type == NoteType.FeverNote && !FeverModeManager.Instance.IsFeverActive())
        {
            UIManager.Instance?.ShowJudgeText(JudgeResult.Bad);
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

                // ✅ 머지 헤드는 꼬리들이 모두 파괴된 후에 풀로 반환되므로
                // 여기서는 ProcessSuccessfulHit을 호출하지 않음
                // 점수와 효과음만 처리
                PlayHitSound(type);
                ScoreManager.Instance?.AddScore(resultSingle);
                UIManager.Instance?.ShowJudgeText(resultSingle);
                
                return resultSingle;
            }
        }

        ProcessSuccessfulHit(singleNote, resultSingle);
        return resultSingle;
    }

    /// <summary>
    /// 성공적인 히트 처리 (단일 책임 원칙 적용)
    /// 🔧 최적화: 배치 처리로 성능 향상
    /// </summary>
    private void ProcessSuccessfulHit(GameObject note, JudgeResult result)
    {
        // 🔧 최적화: 캐시된 handler 사용
        var handler = GetCachedHandler(note);
        if (handler == null) return;

        NoteType type = handler.noteType;

        // 🔧 최적화: 모든 작업을 한 번에 처리
        StartCoroutine(ProcessHitCoroutine(note, type, result));
    }

    // 🔧 최적화: 배치 처리 코루틴
    private IEnumerator ProcessHitCoroutine(GameObject note, NoteType type, JudgeResult result)
    {
        // 효과음 재생
        PlayHitSound(type);

        // 점수 추가 및 UI 업데이트
        ScoreManager.Instance?.AddScore(result);
        UIManager.Instance?.ShowJudgeText(result);

        // 특수 효과 처리
        ProcessSpecialEffects(type, result);

        // 히트 애니메이션 및 풀 반환
        yield return StartCoroutine(PlayHitAnimationAndReturn(note));
    }

    /// <summary>
    /// 히트 효과음 재생 (단일 책임 원칙 적용)
    /// </summary>
    private void PlayHitSound(NoteType type)
    {
        switch (type)
        {
            case NoteType.ManualNote: AudioManager.Instance.PlayObjectSE(0); break;
            case NoteType.WeaponNote: AudioManager.Instance.PlayObjectSE(1); break;
            case NoteType.MergeHead: AudioManager.Instance.PlayObjectSE(2); break;
            case NoteType.MergeTail: AudioManager.Instance.PlayObjectSE(2); break;
            case NoteType.BonusNote: AudioManager.Instance.PlayObjectSE(3); break;
            case NoteType.FeverNote: AudioManager.Instance.PlayObjectSE(4); break;
        }
    }

    /// <summary>
    /// 특수 효과 처리 (단일 책임 원칙 적용)
    /// </summary>
    private void ProcessSpecialEffects(NoteType type, JudgeResult result)
    {
        switch (type)
        {
            case NoteType.BonusNote:
                float feverAmount = result switch
                {
                    JudgeResult.Wow => 15f,
                    JudgeResult.Nice => 10f,
                    _ => 0f
                };
                ScoreManager.Instance?.IncreaseFever(feverAmount);
                break;

            case NoteType.FeverNote:
                if (result == JudgeResult.Wow)
                {
                    FeverCoinEffectManager.Instance?.Play();
                }
                break;
        }
    }

    private IEnumerator PlayHitAnimationAndReturn(GameObject note)
    {
        // 🔧 최적화: 캐시에서 제거
        ClearHandlerCache(note);
        
        // 🔧 피버노트인 경우 칸 해제
        if (note.TryGetComponent(out NoteTypeHandler handler) && handler.noteType == NoteType.FeverNote)
        {
            var feverMover = note.GetComponent<FeverBonusNoteMover>();
            if (feverMover != null)
            {
                // 칸 해제는 하되 풀 반환은 하지 않음 (아래에서 처리)
                feverMover.ReleaseSegmentOnly();
            }
        }
        
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

    // 🔧 최적화: 메모리 정리
    private void OnDestroy()
    {
        handlerCache.Clear();
    }
}
