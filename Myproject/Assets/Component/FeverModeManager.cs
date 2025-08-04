using System.Collections.Generic;
using UnityEngine;
using System.Collections;
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

    private Dictionary<Transform, Vector3> originalChildLocalPositions = new();
    private Coroutine feverDurationCoroutine;
    private float feverDuration = 10f;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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

    public void EnterFeverMode()
    {
        if (isFever) return;
        isFever = true;

        // 자식 위치 저장 및 중앙 정렬
        originalChildLocalPositions.Clear();
        foreach (Transform child in player.transform)
        {
            originalChildLocalPositions[child] = child.localPosition;
            child.localPosition = Vector3.zero;
        }

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
        player.transform.position = new Vector3(6.5f, 2f, 0f);
        player.transform.localScale = feverPlayerScale;

        if (playerSpriteRenderer != null) playerSpriteRenderer.sprite = feverSprite;
        if (playerAnimator != null) playerAnimator.runtimeAnimatorController = feverAnimator;

        if (weaponObject != null)
            weaponObject.transform.position = feverWeaponPosition;

        swingButton.SetActive(true);
        absorbButton.SetActive(false);

        if (judgeCenter != null)
            judgeCenter.transform.position = new Vector3(1.3f, 0f, 10f);

        // 기존 노트 제거
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
        wheelObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        straightRailObject.SetActive(false);
        absorbButton.SetActive(true);

        // 위치 복귀
        player.transform.position = new Vector3(4.7f, -2.15f, 10f);
        player.transform.localScale = normalPlayerScale;

        foreach (Transform child in player.transform)
        {
            if (originalChildLocalPositions.TryGetValue(child, out Vector3 originalPos))
                child.localPosition = originalPos;
        }

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
}
