using UnityEngine;

public class JudgeInputHandler : MonoBehaviour
{
    public WeaponJudgeSystem weaponJudgeSystem;
    public Animator playerAnimator;

    private bool isMergeActive = false;
    private bool isAbsorbHeld = false;
    private bool isSwingHeld = false;

    // === 키/버튼 공통 인터페이스 ===
    public void OnJudgeButtonDown()  // Z 버튼 누름
    {
        TryStartMerge();
    }

    public void OnJudgeButtonUp()  // Z 버튼 뗌
    {
        TryStopMerge();

        // 단독 흡수 판정 (합동공격 중이 아니고, 스윙을 누르고 있지 않을 때)
        if (!isSwingHeld && !isMergeActive)
        {
            JudgeResult result = weaponJudgeSystem.TryJudge(NoteInputType.Absorb);
            AudioManager.Instance.PlayWeaponSE(0);
            if (playerAnimator) playerAnimator.SetTrigger("AbsorbTrigger");
        }
    }

    public void OnSwingButtonDown()  // X 버튼 누름
    {
        TryStartMerge();
    }

    public void OnSwingButtonUp()  // X 버튼 뗌
    {
        TryStopMerge();

        // 단독 스윙 판정 (합동공격 중이 아니고, 흡수를 누르고 있지 않을 때)
        if (!isAbsorbHeld && !isMergeActive)
        {
            JudgeResult result = weaponJudgeSystem.TryJudge(NoteInputType.Swing);
            AudioManager.Instance.PlayWeaponSE(1);
            if (playerAnimator) playerAnimator.SetTrigger("AttackTrigger");
        }
    }

    // === UI 버튼에서 호출 (터치) ===
    public void SetAbsorbHeld(bool isHeld)
    {
        isAbsorbHeld = isHeld;
        if (isHeld) TryStartMerge();
        else TryStopMerge();
    }

    public void SetSwingHeld(bool isHeld)
    {
        isSwingHeld = isHeld;
        if (isHeld) TryStartMerge();
        else TryStopMerge();
    }

    // === 합동공격 제어 ===
    private void TryStartMerge()
    {
        if (isAbsorbHeld && isSwingHeld && !isMergeActive)
        {
            JudgeResult result = weaponJudgeSystem.TryJudge(NoteInputType.MergeHead);
            isMergeActive = true;
            if (playerAnimator) playerAnimator.SetBool("isMerging", true);
            AudioManager.Instance.PlayWeaponSE(2);
        }
    }

    private void TryStopMerge()
    {
        if (isMergeActive && (!isAbsorbHeld || !isSwingHeld))
        {
            isMergeActive = false;
            if (playerAnimator) playerAnimator.SetBool("isMerging", false);

            // 머지 버튼 해제 시 머리 노트 이동 재개
            var active = MultiObjectPool.Instance != null ? MultiObjectPool.Instance.ActiveObjects : null;
            if (active != null)
            {
                foreach (var obj in active)
                {
                    if (obj != null && obj.activeInHierarchy && obj.TryGetComponent(out MergeHeadController headCtrl))
                    {
                        headCtrl.StopHitLoop();
                    }
                }
            }
        }
    }

    private float mergeTailJudgeCooldown = 0f;

    private void Update()
    {
        // === 키보드 입력에서도 터치와 동일하게 상태 갱신 ===
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.Z))
        {
            isAbsorbHeld = true;
            OnJudgeButtonDown();
        }
        if (Input.GetKeyUp(KeyCode.Z))
        {
            isAbsorbHeld = false;
            OnJudgeButtonUp();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            isSwingHeld = true;
            OnSwingButtonDown();
        }
        if (Input.GetKeyUp(KeyCode.X))
        {
            isSwingHeld = false;
            OnSwingButtonUp();
        }
#endif

        // ✅ 키보드 + 버튼 방식 모두 작동: 합동공격 꼬리 판정 주기적으로 시도
        if (isAbsorbHeld && isSwingHeld && isMergeActive)
        {
            mergeTailJudgeCooldown -= Time.deltaTime;
            if (mergeTailJudgeCooldown <= 0f)
            {
                weaponJudgeSystem.TryJudge(NoteInputType.MergeTail);
                mergeTailJudgeCooldown = 0.05f;  // 0.05초 쿨타임
            }
        }
    }
}
