using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FeverModeManager : MonoBehaviour
{
    public static FeverModeManager Instance;
    public GameObject wheelObject;
    public GameObject straightRailObject;
    public StraightRailScroller railScroller;

    public GameObject player;
    public SpriteRenderer playerSpriteRenderer;
    public Animator playerAnimator;
    public Sprite normalDaySprite;
    public Sprite normalNightSprite;
    public RuntimeAnimatorController normalDayAnimator;
    public RuntimeAnimatorController normalNightAnimator;
    public Sprite feverSprite;
    public RuntimeAnimatorController feverAnimator;
    public Vector3 normalPlayerScale = new Vector3(1f, 1f, 1f);
    public Vector3 feverPlayerScale = new Vector3(1.5f, 1.5f, 1f);
    public GameObject weaponObject;
    public Vector3 feverWeaponPosition;
    public GameObject swingButton;
    public GameObject absorbButton;
    public Transform judgeCenter;
    public GameObject pearl;
    public Transform feverSpawnPoint;
    public GameObject feverNotePrefab;
    public MultiObjectPool objectPool;
    private float nextSpawnTime;
    private float feverSpawnInterval;
    private float railWidth;
    private bool isFever = false;
    private Coroutine feverDurationCoroutine;
    private float feverDuration = 15f;
    [Header("Fever View (Cloud Mask)")]
    [SerializeField] private GameObject cloudMaskObject; // 인스펙터에서 할당
    [SerializeField] private GameObject cloudMask;       // 인스펙터에서 할당
    [Header("Square 고정 설정")]
    [SerializeField] private Transform squareTransform;  // Player의 자식 "Square"를 연결(비워두면 자동 탐색)
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // 누락 경고(에디터에서 바로 확인용)
        if (cloudMaskObject == null)
            Debug.LogWarning("[FeverModeManager] cloudMaskObject 참조가 비었습니다. 인스펙터에서 할당하세요.", this);
        if (cloudMask == null)
            Debug.LogWarning("[FeverModeManager] cloudMask 참조가 비었습니다. 인스펙터에서 할당하세요.", this);

        EnsureSquareReference();
    }
    private void EnsureSquareReference()
    {
        if (squareTransform != null) return;
        if (player != null)
        {
            // 우선 정확한 이름으로 시도
            var direct = player.transform.Find("Square");
            if (direct != null) { squareTransform = direct; return; }

            // 그래도 못 찾으면 하위 전체 탐색
            foreach (var t in player.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Square") { squareTransform = t; return; }
            }
        }
    }
    private void Update()
    {
        if (!isFever) return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnFeverNote();
            nextSpawnTime = Time.time + feverSpawnInterval;
        }
    }
    public void StartFeverEntrySequence()
    {
        StartCoroutine(FeverEntryCoroutine());
    }
    private IEnumerator FeverEntryCoroutine()
    {
        // 1) 플레이어 피버 진입 애니메이션 실행
        GameObject player = GameObject.FindWithTag("Player");
        Animator playerAnim = null;

        if (player != null)
            playerAnim = player.GetComponentInChildren<Animator>();

        if (playerAnim != null)
        {
            playerAnim.SetTrigger("EnterFever");

            // ✨ 애니메이션 ‘발동 순간’에 스폰 차단 & 모든 노트 즉시 반환
            ObjectSpawner.Instance?.PauseSpawningAndClearAll();
        }

        // ✅ 애니메이션 길이만큼 대기(필요 시 Animator 길이로 교체)
        yield return new WaitForSeconds(0.8f);

        // 2) 구름 마스크 활성 (인스펙터 참조 사용)
        if (cloudMaskObject != null)
        {
            SetActiveSafe(cloudMaskObject, true);
            Animator anim = cloudMaskObject.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("StartExpand");
        }

        if (cloudMask != null)
        {
            SetActiveSafe(cloudMask, true);
            Animator anim = cloudMask.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("StartExpand");
        }
        AudioManager.Instance?.PlaySE(5);
        // ✅ 구름 애니메이션 대기 (임시: 필요 길이에 맞게 조정)
        yield return new WaitForSeconds(0.8f);

        // 3) 피버 진입
        BackgroundManager.Instance?.ChangeToFever();

        // 4) 구름 마스크 비활성화 (동시에)
        SetActiveSafe(cloudMaskObject, false);
        SetActiveSafe(cloudMask, false);
    }

    public void EnterFeverMode()
    {
        if (isFever) return;
        isFever = true;

        // 강제로 낮 테마로
        BackgroundManager.Instance?.ForceSetNightTheme(false);
        FindObjectOfType<GameSceneWeaponUISetter>()?.ApplyGameUIButtonState();

        // 구미호 이미지와 점수판 낮 테마로 강제 지정
        var setting = ScoreBasedDifficultyManager.Instance.GetCurrentSetting(GameManager.Instance.CurrentScore);
        GameManager.Instance?.SetGumihoImageSet(setting.dayGumihoSprite, setting.dayGumihoSprite);
        GameManager.Instance?.UpdateScoreImageByCurrentSetting();
        ThemeManager.Instance?.ApplyTheme(ThemeType.Day);

        // 레일 전환
        wheelObject.SetActive(false);
        straightRailObject.SetActive(true);
        railScroller.RecalculateRailWidthAndPosition();
        railWidth = railScroller.GetRailWidth();
        if (railWidth <= 0f) railWidth = 6f;

        float effectiveWidth = railWidth * 2f;
        feverSpawnInterval = (effectiveWidth / 50f) / railScroller.scrollSpeed;

        // 플레이어 위치 및 외형
        player.transform.position = new Vector3(7f, 1f, 0f);
        player.transform.localScale = feverPlayerScale;

        if (playerSpriteRenderer != null) playerSpriteRenderer.sprite = feverSprite;
        if (playerAnimator != null) playerAnimator.runtimeAnimatorController = feverAnimator;

        if (weaponObject != null)
            weaponObject.transform.position = feverWeaponPosition;

        swingButton.SetActive(true);
        absorbButton.SetActive(false);

        if (judgeCenter != null)
            judgeCenter.transform.position = new Vector3(1.3f, -1f, 10f);

        // ✨ 안전상 중복 제거(이미 Entry에서 모두 반환했지만, 혹시 모를 잔여 정리)
        foreach (var obj in objectPool.ActiveObjects.ToArray())
            objectPool.Return(obj);

        if (pearl != null)
            pearl.SetActive(false);

        GameManager.Instance?.ForceUpdateJudgeImage();

        if (GameManager.Instance?.scoreText != null)
            GameManager.Instance.scoreText.color = Color.black;

        if (feverDurationCoroutine != null)
            StopCoroutine(feverDurationCoroutine);
        feverDurationCoroutine = StartCoroutine(FeverCountdown());

        // 첫 스폰
        nextSpawnTime = Time.time + feverSpawnInterval;

        var uiSetter = FindObjectOfType<GameSceneWeaponUISetter>();
        uiSetter?.ApplyGameUIButtonState();  // 원래 낮 테마 설정
        uiSetter?.ApplyFeverButtonSprite();  // 피버 버튼 이미지 적용
        var swapUI = FindObjectOfType<WeaponSwapUI>();
        swapUI?.ApplyFeverButtonSprite();    // 옵션 패널 스프라이트

        // ✅ Square 로컬좌표 (0,0,0) 고정
        EnsureSquareReference();
        ForceSquareLocalZero();
        StartCoroutine(EnforceSquareLocalZeroNextFrame());

        SpawnFeverNote();
    }

    private IEnumerator FeverCountdown()
    {
        float elapsed = 0f;
        float startGauge = GameManager.Instance.FeverGauge;

        while (elapsed < feverDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / feverDuration;
            GameManager.Instance.FeverGauge = Mathf.Lerp(startGauge, 0f, t);
            yield return null;
        }

        GameManager.Instance.FeverGauge = 0f;
        ExitFeverMode();
    }

    public void ExitFeverMode()
    {
        isFever = false;

        // 레일 복귀
        wheelObject.SetActive(true);
        var rotator = wheelObject.GetComponent<SplineRotator>();
        if (rotator != null)
        {
            rotator.SetAngle(0f);
        }
        straightRailObject.SetActive(false);
        absorbButton.SetActive(true);

        // 위치 복귀 (Player 월드 좌표는 변경 유지)
        player.transform.position = new Vector3(6f, -1.55f, 10f);
        player.transform.localScale = normalPlayerScale;

        // ✅ Square 로컬좌표는 복구하지 않고 (0,0,0)으로 고정
        EnsureSquareReference();
        ForceSquareLocalZero();
        StartCoroutine(EnforceSquareLocalZeroNextFrame());

        // 점수 기반 테마 적용
        var setting = ScoreBasedDifficultyManager.Instance.GetCurrentSetting(GameManager.Instance.CurrentScore);
        var theme = setting.theme;

        if (IsNightTheme(theme))
        {
            if (playerSpriteRenderer != null) playerSpriteRenderer.sprite = normalNightSprite;
            if (playerAnimator != null) playerAnimator.runtimeAnimatorController = normalNightAnimator;
        }
        else
        {
            if (playerSpriteRenderer != null) playerSpriteRenderer.sprite = normalDaySprite;
            if (playerAnimator != null) playerAnimator.runtimeAnimatorController = normalDayAnimator;
        }

        if (weaponObject != null)
            weaponObject.transform.position = new Vector3(0.2f, -4.5f, 10f);

        if (judgeCenter != null)
            judgeCenter.transform.position = new Vector3(1.5f, -2.35f, 10f);

        // 피버 노트만 제거
        foreach (var obj in objectPool.ActiveObjects.ToArray())
        {
            var typeHandler = obj.GetComponent<NoteTypeHandler>();
            if (typeHandler != null && typeHandler.noteType == NoteType.FeverNote)
                objectPool.Return(obj);
        }

        if (pearl != null)
            pearl.SetActive(true);

        // 테마 및 이미지 복원
        ThemeManager.Instance?.ApplySetting(setting);
        GameManager.Instance?.SetGumihoImageSet(setting.dayGumihoSprite, setting.nightGumihoSprite);
        GameManager.Instance?.UpdateScoreImageByCurrentSetting();

        // 인게임/옵션 UI 상태 원복
        var uiSetter = FindObjectOfType<GameSceneWeaponUISetter>();
        uiSetter?.ResetFeverState();
        uiSetter?.ApplyGameUIButtonState();

        var swapUI = FindObjectOfType<WeaponSwapUI>();
        if (swapUI != null)
        {
            swapUI.ResetFeverState();
            swapUI.ApplySwapState();
        }

        // ✨ 휠 스폰 재개(반칸부터 정확히 시작)
        ObjectSpawner.Instance?.ResumeAfterFever();
    }

    private void SpawnFeverNote()
    {
        float chance = Random.Range(0f, 1f);
        if (chance > 0.8f)
        {
            // 20% 확률로 스폰하지 않음
            return;
        }

        GameObject note = objectPool.Get(feverNotePrefab);
        if (note == null) return;

        Vector3 spawnPos = feverSpawnPoint.position;

        float effectiveWidth = railWidth * 2f;
        float cellWidth = effectiveWidth / 50f;
        Transform leftMostRail = railScroller.GetFarthestLeftRail();
        float leftEdgeX = leftMostRail.position.x - railWidth / 2f;
        float firstCellCenterX = leftEdgeX + cellWidth / 2f;

        float distance = spawnPos.x - firstCellCenterX;
        float cellsFromLeft = distance / cellWidth;
        int nearestCellIndex = Mathf.RoundToInt(cellsFromLeft);
        float alignedX = firstCellCenterX + nearestCellIndex * cellWidth;
        spawnPos.x = alignedX;

        note.transform.position = spawnPos;
        note.transform.rotation = Quaternion.identity;

        var mover = note.GetComponent<FeverBonusNoteMover>();
        if (mover != null)
            mover.SetSpeed(railScroller.scrollSpeed);

        note.SetActive(true);
    }

    public bool IsFeverActive() => isFever;

    private bool IsNightTheme(ThemeType theme)
    {
        return theme == ThemeType.Night1 ||
               theme == ThemeType.Night2 ||
               theme == ThemeType.Night3 ||
               theme == ThemeType.Night4 ||
               theme == ThemeType.BurningNight;
    }

    // --- Square 고정 유틸 ---
    private void ForceSquareLocalZero()
    {
        if (squareTransform == null) return;
        squareTransform.localPosition = Vector3.zero;
    }

    private IEnumerator EnforceSquareLocalZeroNextFrame()
    {
        // 애니메이션이 다음 프레임에 덮어쓰는 것을 방지
        yield return null;
        ForceSquareLocalZero();
    }

    // --- 안전 활성/비활성 유틸 ---
    private static void SetActiveSafe(GameObject go, bool state)
    {
        if (go == null) return;
        if (go.activeSelf == state) return;
        go.SetActive(state);
    }
}
