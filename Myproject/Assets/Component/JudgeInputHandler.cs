using UnityEngine;

public class JudgeInputHandler : MonoBehaviour
{
    public WeaponJudgeSystem weaponJudgeSystem;
    public Animator playerAnimator;
    private bool isMergeActive = false;
    private bool isAbsorbHeld = false;
    private bool isSwingHeld = false;
    public void OnJudgeButtonDown()  // Z 버튼 누름
    {
        TryStartMerge();
    }

    public void OnJudgeButtonUp()  // Z 버튼 뗌
    {
        TryStopMerge();

        if (!isSwingHeld && !isMergeActive)
        {
            JudgeResult result = weaponJudgeSystem.TryJudge(NoteInputType.Absorb);
            AudioManager.Instance.PlayWeaponSE(0);
            playerAnimator.SetTrigger("AbsorbTrigger");
        }
    }

    public void OnSwingButtonDown()  // X 버튼 누름
    {
        TryStartMerge();
    }

    public void OnSwingButtonUp()  // X 버튼 뗌
    {
        TryStopMerge();

        if (!isAbsorbHeld && !isMergeActive)
        {
            JudgeResult result = weaponJudgeSystem.TryJudge(NoteInputType.Swing);
            AudioManager.Instance.PlayWeaponSE(1);
            playerAnimator.SetTrigger("AttackTrigger");
        }
    }
// UI 버튼에서 호출할 함수들
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

    private void TryStartMerge()
    {
        if (isAbsorbHeld && isSwingHeld && !isMergeActive)
        {
            JudgeResult result = weaponJudgeSystem.TryJudge(NoteInputType.MergeHead);
            isMergeActive = true;
            playerAnimator.SetBool("isMerging", true);
            AudioManager.Instance.PlayWeaponSE(2);
        }
    }

    private void TryStopMerge()
    {
        if (isMergeActive && (!isAbsorbHeld || !isSwingHeld))
        {
            isMergeActive = false;
            playerAnimator.SetBool("isMerging", false);

            // 머지 버튼 해제 시 머리 노트 이동 재개
            foreach (var obj in MultiObjectPool.Instance.ActiveObjects)
            {
                if (obj != null && obj.activeInHierarchy && obj.TryGetComponent(out MergeHeadController headCtrl))
                {
                    headCtrl.StopHitLoop();
                }
            }
        }
    }

private float mergeTailJudgeCooldown = 0f;

private void Update()
{
#if UNITY_EDITOR || UNITY_STANDALONE
    if (Input.GetKeyDown(KeyCode.Z)) OnJudgeButtonDown();
    if (Input.GetKeyUp(KeyCode.Z)) OnJudgeButtonUp();

    if (Input.GetKeyDown(KeyCode.X)) OnSwingButtonDown();
    if (Input.GetKeyUp(KeyCode.X)) OnSwingButtonUp();
#endif

    // ✅ 키보드 + 버튼 방식 모두 작동
    if (isAbsorbHeld && isSwingHeld && isMergeActive)
    {
        mergeTailJudgeCooldown -= Time.deltaTime;
        if (mergeTailJudgeCooldown <= 0f)
        {
            weaponJudgeSystem.TryJudge(NoteInputType.MergeTail);
            mergeTailJudgeCooldown = 0.05f;  // 0.1초 쿨타임
        }
    }
}

}
