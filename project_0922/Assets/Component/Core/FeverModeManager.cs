using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 피버 모드 관리
/// 단일 책임 원칙: 피버 모드만 담당
/// 의존성 역전 원칙: 인터페이스에 의존
/// </summary>
public class FeverModeManager : MonoBehaviour
{
    public static FeverModeManager Instance { get; private set; }
    
    [Header("Fever Objects")]
    public GameObject wheelObject;
    public GameObject straightRailObject;
    public StraightRailScroller railScroller;

    [Header("Player")]
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

    [Header("UI")]
    public GameObject swingButton;
    public GameObject absorbButton;
    public Transform judgeCenter;
    public GameObject pearl;
    public Transform feverSpawnPoint;
    public GameObject feverNotePrefab;
    public MultiObjectPool objectPool;
    
    [Header("UI Components")]
    [Tooltip("게임 씬 무기 UI 설정 컴포넌트")]
    public GameSceneWeaponUISetter gameSceneWeaponUISetter;
    
    [Header("Object Spawner")]
    [SerializeField] private ObjectSpawner objectSpawner; // 🔧 피버 모드용 스폰 제어

    [Header("Fever Position Settings")]
    [Tooltip("피버 모드에서 Player가 이동할 위치")]
    public Vector3 feverPlayerPosition = new Vector3(0f, 0f, 0f);
    [Tooltip("피버 모드에서 JudgeCenter가 이동할 위치")]
    public Vector3 feverJudgeCenterPosition = new Vector3(0f, 0f, 0f);

    [Header("Fever Settings")]
    private float nextSpawnTime;
    private float feverSpawnInterval = 1.0f; // 🔧 피버 노트 스폰 간격 조정 (1초 간격으로 안정적인 스폰)
    private float railWidth;
    private bool isFever = false;
    private bool isFeverUI = false; // UI 전용 피버 상태 (더 일찍 설정됨)
    private Coroutine feverDurationCoroutine;
    private Coroutine feverSpawnCoroutine; // 🔧 성능 최적화: 코루틴으로 변경
    private float feverDuration = 15f; // 🔧 피버 지속시간을 15초로 설정
    
    // 🔧 피버 노트 스폰 관리
    private bool[] occupiedSegments; // 각 칸의 점유 상태 (25칸)
    private int currentSpawnSegment = 0; // 현재 스폰할 칸 인덱스
    
    [Header("Fever View (Cloud Mask)")]
    [SerializeField] private GameObject cloudMaskObject;
    [SerializeField] private GameObject cloudMask;

    [Header("Square 고정 설정")]
    [SerializeField] private Transform squareTransform;

    [Header("Fever Entry Animation")]
    [SerializeField] private GameObject fogUIObject;
    [SerializeField] private GameObject[] fogUIChildren = new GameObject[2]; // FogUI 내의 2개 오브젝트
    [SerializeField] private float playerAnimationDelay = 1f; // 플레이어 애니메이션 완료 대기 시간
    [SerializeField] private float cloudAnimationDelay = 2f; // 구름 애니메이션 완료 대기 시간

    // 🔧 최적화: 캐시된 플레이어 참조
    private GameObject cachedPlayer;
    private Animator cachedPlayerAnim;
    
    // 원래 위치 저장용 변수들
    private Vector3 originalPlayerPosition;
    private Vector3 originalJudgeCenterPosition;
    
    // 위치 기반 동기화용 변수들

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // 🔧 최적화: 플레이어 참조를 한 번만 찾아서 캐시
        if (player == null)
            player = GameObject.FindWithTag("Player");
        
        if (player != null)
        {
            cachedPlayer = player;
            cachedPlayerAnim = player.GetComponentInChildren<Animator>();
            
            // Square Transform을 한 번만 찾아서 캐시
            squareTransform = player.transform.Find("Square");
            
            // 원래 위치 저장
            originalPlayerPosition = player.transform.position;
        }
        
        if (judgeCenter != null)
        {
            originalJudgeCenterPosition = judgeCenter.position;
        }
    }
    
    // 🔧 성능 최적화: Update() 제거하고 코루틴으로 변경
    // private void Update() - 매 프레임 체크 제거됨
    


    public void StartFeverEntrySequence()
    {
        StartCoroutine(FeverEntryCoroutine());
    }

    private IEnumerator FeverEntryCoroutine()
    {
        // 🔧 최적화: 캐시된 플레이어 참조 사용
        if (cachedPlayer == null)
        {
            cachedPlayer = GameObject.FindWithTag("Player");
            if (cachedPlayer != null)
                cachedPlayerAnim = cachedPlayer.GetComponentInChildren<Animator>();
        }

        // 0) 스폰된 모든 오브젝트들을 풀로 반환
        ReturnAllSpawnedObjectsToPool();
        
        // 0-1) ObjectSpawner의 스폰을 즉시 중단 (오브젝트 정리 직후)
        StopObjectSpawning();

        // 1) 플레이어 피버 진입 애니메이션 실행 (Square 오브젝트의 애니메이션)
        if (cachedPlayerAnim != null)
        {
            cachedPlayerAnim.SetTrigger("EnterFever"); // 피버 진입 애니메이션 트리거
        }

        // 2) 플레이어 애니메이션 완료까지 대기
        yield return new WaitForSeconds(playerAnimationDelay);

        // 3) FogUI 내의 2개 오브젝트 활성화 및 구름 애니메이션 실행
        if (fogUIObject != null)
        {
            foreach (var child in fogUIChildren)
            {
                if (child != null)
                {
                    child.SetActive(true);
                    // 각 오브젝트의 애니메이션 실행
                    var childAnimator = child.GetComponent<Animator>();
                    if (childAnimator != null)
                    {
                        childAnimator.SetTrigger("StartExpand");
                    }
                }
            }
        }

        // 4) 구름 애니메이션 완료까지 대기
        yield return new WaitForSeconds(cloudAnimationDelay);
        
        // 5) 피버 모드 활성화 및 설정
        EnterFeverMode();
        
        // 6) 피버 진입 사운드 추가
        AudioManager.Instance?.PlaySE(5);
        if (fogUIObject != null)
            {
                foreach (var child in fogUIChildren)
                {
                    if (child != null)
                    {
                        child.SetActive(false);
                    }
                }
            }
        yield return null;
    }

    private void SwitchToFeverMode()
    {
        // 휠 → 직선 레일 전환
        if (wheelObject != null) wheelObject.SetActive(false);
        if (straightRailObject != null) straightRailObject.SetActive(true);

        // 플레이어 외형 변경
        if (playerSpriteRenderer != null) playerSpriteRenderer.sprite = feverSprite;
        if (playerAnimator != null) playerAnimator.runtimeAnimatorController = feverAnimator;
        if (player != null) player.transform.localScale = feverPlayerScale;
        
        // Player와 JudgeCenter를 피버 모드 위치로 이동
        if (player != null)
        {
            player.transform.position = feverPlayerPosition;

        }
        
        if (judgeCenter != null)
        {
            judgeCenter.position = feverJudgeCenterPosition;

        }
        
        // 🔧 피버 테마 적용
        ApplyFeverTheme();

        // 피버 UI 적용
        UIManager.Instance?.ApplyFeverButtonSprite();
    }

    private void StartFeverDuration()
    {
        if (feverDurationCoroutine != null)
            StopCoroutine(feverDurationCoroutine);


        feverDurationCoroutine = StartCoroutine(FeverDurationCoroutine());
    }

    // 🔧 성능 최적화: 피버 스폰을 코루틴으로 변경
    private void StartFeverSpawning()
    {
        if (feverSpawnCoroutine != null)
            StopCoroutine(feverSpawnCoroutine);

        
        feverSpawnCoroutine = StartCoroutine(FeverSpawnCoroutine());
    }

    private IEnumerator FeverSpawnCoroutine()
    {
        // 🔧 즉시 첫 번째 노트 스폰
        SpawnFeverNote();
        
        // 🔧 무한 스폰 루프 (피버 지속시간이 끝날 때까지 계속)
        float spawnElapsedTime = 0f;
        while (spawnElapsedTime < feverDuration)
        {
            // 🔧 정확한 간격으로 대기
            yield return new WaitForSeconds(feverSpawnInterval);
            
            // 🔧 경과시간 업데이트
            spawnElapsedTime += feverSpawnInterval;
            
            // 🔧 피버 지속시간이 아직 남아있으면 스폰 실행
            if (spawnElapsedTime <= feverDuration)
            {
                SpawnFeverNote();
            }
            else
            {
                // 🔧 마지막 스폰 간격이 피버 지속시간을 초과하면 스폰하지 않음
                break;
            }
        }
        
    }

    /// <summary>
    /// 피버 모드 진입 시 모든 일반 노트를 즉시 풀로 반환
    /// </summary>
    private void ClearAllNormalNotes()
    {
        if (objectPool == null)
        {
            objectPool = MultiObjectPool.Instance;
        }
        
        if (objectPool != null && objectPool.ActiveObjects != null)
        {
            var activeObjects = new List<GameObject>(objectPool.ActiveObjects);
            foreach (var obj in activeObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    // 플레이어 오브젝트는 제외
                    bool isPlayer = false;
                    
                    try
                    {
                        isPlayer = obj.CompareTag("Player");
                    }
                    catch (System.Exception)
                    {
                        // Player 태그가 정의되지 않은 경우 무시
                    }
                    
                    if (!isPlayer)
                    {
                        objectPool.Return(obj);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 일반 노트 스폰을 중단 (피버 모드 진입 시)
    /// </summary>
    private void StopNormalNoteSpawning()
    {
        if (objectSpawner != null)
        {
            objectSpawner.StopSpawning();
        }
        else
        {
            // 🔧 성능 최적화: FindAnyObjectByType 제거 - 인스펙터에서 직접 할당
            // var spawner = FindAnyObjectByType<ObjectSpawner>();
            // if (spawner != null) spawner.StopSpawning();
        }
    }

    /// <summary>
    /// 일반 노트 스폰을 재개 (피버 모드 종료 시)
    /// </summary>
    private void ResumeNormalNoteSpawning()
    {
        if (objectSpawner != null)
        {
            objectSpawner.ResumeSpawning();
        }
        else
        {
            // 🔧 성능 최적화: FindAnyObjectByType 제거 - 인스펙터에서 직접 할당
            // var spawner = FindAnyObjectByType<ObjectSpawner>();
            // if (spawner != null) spawner.ResumeSpawning();
        }
    }

    /// <summary>
    /// 피버 노트 스폰 시스템 초기화
    /// </summary>
    private void InitializeFeverSpawnSystem()
    {
        if (railScroller != null)
        {
            int totalSegments = railScroller.GetTotalSegments();
            occupiedSegments = new bool[totalSegments];
            currentSpawnSegment = 0;
            
                            // 🔧 레일 속도 기반 스폰 간격 계산
                float segmentWidth = railScroller.GetSegmentWidth();
                float scrollSpeed = railScroller.scrollSpeed;
                
                if (segmentWidth > 0f && scrollSpeed > 0f)
                {
                    // 한 칸을 지나가는데 걸리는 시간 계산
                    float timePerSegment = segmentWidth / scrollSpeed;
                    
                    // 🔧 스폰 간격 = 한 칸을 지나가는데 걸리는 시간과 정확히 일치
                    feverSpawnInterval = timePerSegment;
                }
                else
                {
                    // 기본값 사용
                    feverSpawnInterval = 0.5f;
                }
            
            // 모든 칸을 비어있는 상태로 초기화
            for (int i = 0; i < totalSegments; i++)
            {
                occupiedSegments[i] = false;
            }
            
        }
    }

    /// <summary>
    /// 피버 모드 UI 설정 (흡수 버튼 비활성화, 타격 버튼 피버 테마로 변경)
    /// </summary>
    private void SetFeverModeUI()
    {
        // 🔧 흡수 버튼 비활성화
        if (absorbButton != null)
        {
            absorbButton.SetActive(false);
            Debug.Log("[FeverModeManager] 흡수 버튼 비활성화");
        }
        
        // 🔧 타격 버튼 활성화
        if (swingButton != null)
        {
            swingButton.SetActive(true);
            Debug.Log("[FeverModeManager] 타격 버튼 활성화");
        }
        
        // 🔧 GameSceneWeaponUISetter를 통해 피버 버튼 스프라이트 적용
        if (gameSceneWeaponUISetter != null)
        {
            gameSceneWeaponUISetter.ApplyFeverButtonSprite();
            Debug.Log("[FeverModeManager] GameSceneWeaponUISetter를 통해 피버 버튼 스프라이트 적용");
        }
        
        Debug.Log("[FeverModeManager] 피버 모드 UI 설정 완료");
    }

    /// <summary>
    /// 일반 모드 UI 복원 (흡수 버튼 활성화, 타격 버튼 일반 테마로 복원)
    /// </summary>
    private void RestoreNormalModeUI()
    {
        // 🔧 흡수 버튼 활성화
        if (absorbButton != null)
        {
            absorbButton.SetActive(true);
            Debug.Log("[FeverModeManager] 흡수 버튼 활성화");
        }
        
        // 🔧 타격 버튼 활성화
        if (swingButton != null)
        {
            swingButton.SetActive(true);
            Debug.Log("[FeverModeManager] 타격 버튼 활성화");
        }
        
        // 🔧 GameSceneWeaponUISetter를 통해 일반 테마로 복원
        if (gameSceneWeaponUISetter != null)
        {
            gameSceneWeaponUISetter.ResetFeverState();
            Debug.Log("[FeverModeManager] GameSceneWeaponUISetter를 통해 일반 테마로 복원");
        }
        
        Debug.Log("[FeverModeManager] 일반 모드 UI 복원 완료");
    }

    /// <summary>
    /// 피버 모드 전환 코루틴 - 일반 노트 정리 후 연출 완료까지 대기 후 스폰 시작
    /// </summary>
    private IEnumerator FeverModeTransitionCoroutine()
    {
        // 🔧 일반 노트 스폰 중단
        StopNormalNoteSpawning();
        
        // 🔧 잠시 대기 (일반 노트가 완전히 정리되도록)
        yield return new WaitForSeconds(0.1f);
        
        // 🔧 피버 스폰 시작 (게이지와 동시에 시작)
        StartFeverSpawning();
        
    }

    private IEnumerator FeverDurationCoroutine()
    {
        float elapsedTime = 0f;
        float initialGauge = ScoreManager.Instance?.GetFeverGauge() ?? 100f;
        
        
        while (elapsedTime < feverDuration && isFever)
        {
            elapsedTime += Time.deltaTime;
            
            // 피버 게이지를 시간에 따라 점진적으로 감소 (정확히 15초에 0이 되도록)
            float remainingRatio = Mathf.Clamp01(1f - (elapsedTime / feverDuration));
            float currentGauge = initialGauge * remainingRatio;
            
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.SetFeverGauge(currentGauge);
            }
            
            
            yield return null;
        }
        
        // 🔧 피버 지속시간이 끝나면 게이지를 0으로 확실히 설정
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetFeverGauge(0f);
        }
        
        // 피버 모드 종료
        ExitFeverMode();
    }

    private void SwitchToNormalMode()
    {
        // 직선 레일 → 휠 전환
        if (straightRailObject != null) straightRailObject.SetActive(false);
        if (wheelObject != null) 
        {
            wheelObject.SetActive(true);
            
            // SplineRotator를 찾아서 각도를 0으로 강제 설정
            var splineRotator = wheelObject.GetComponent<SplineRotator>();
            if (splineRotator != null)
            {
                splineRotator.SetAngle(0f);

            }
            else
            {
                // SplineRotator가 없는 경우에만 직접 로테이션 설정
                Vector3 currentRotation = wheelObject.transform.localEulerAngles;
                wheelObject.transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, 0f);

            }
        }
        
        // Player와 JudgeCenter를 원래 위치로 복원
        if (player != null)
        {
            player.transform.position = originalPlayerPosition;

        }
        
        if (judgeCenter != null)
        {
            judgeCenter.position = originalJudgeCenterPosition;

        }
        
        // 플레이어 원래 크기로 복원
        if (player != null) player.transform.localScale = Vector3.one;
        
        // 플레이어 원래 외형으로 복원
        RestorePlayerAppearance();
        
        // 피버 UI 복원
        UIManager.Instance?.ResetFeverState();
        
        // 최신 난이도 설정으로 테마 복원 (점수 기반)
        if (ScoreBasedDifficultyManager.Instance != null && GameStateManager.Instance != null)
        {
            int currentScore = GameStateManager.Instance.CurrentScore;
            var setting = ScoreBasedDifficultyManager.Instance.GetSettingForScore(currentScore);
            if (setting != null)
            {
                // 1. BackgroundManager의 IsNightTheme을 먼저 설정
                if (BackgroundManager.Instance != null)
                {
                    BackgroundManager.Instance.IsNightTheme = (setting.theme != ThemeType.Day);
            
                }
                
                // 2. ScoreBasedDifficultyManager.ApplySetting을 통해 전체 테마 적용 (스코어 이미지 포함)
                ScoreBasedDifficultyManager.Instance.ApplySetting(setting);
        
            }
        }
        
        if (feverDurationCoroutine != null)
        {
            StopCoroutine(feverDurationCoroutine);
            feverDurationCoroutine = null;
        }
        
        // 🔧 성능 최적화: 피버 스폰 코루틴 중단
        if (feverSpawnCoroutine != null)
        {
            StopCoroutine(feverSpawnCoroutine);
            feverSpawnCoroutine = null;
        }
        
        // 🔧 피버 모드 종료: 일반 노트 스폰 재개
        ResumeNormalNoteSpawning();
        
        // 🔧 피버 모드 UI 복원
        RestoreNormalModeUI();
    }

    /// <summary>
    /// 피버 오브젝트들을 풀로 반환
    /// </summary>
    private void ReturnFeverObjectsToPool()
    {
        // 🔧 성능 최적화: FindAnyObjectByType 제거 - 싱글톤 인스턴스 사용
        if (objectPool == null)
        {
            objectPool = MultiObjectPool.Instance;
            if (objectPool == null)
            {
                return;
            }
        }

        // 🔧 성능 최적화: FindObjectsOfType 제거 - 오브젝트 풀에서 직접 관리
        if (objectPool != null && objectPool.ActiveObjects != null)
        {
            var activeObjects = new List<GameObject>(objectPool.ActiveObjects);
            foreach (var obj in activeObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    // 플레이어 오브젝트는 제외
                    bool isPlayer = false;
                    
                    try
                    {
                        isPlayer = obj.CompareTag("Player");
                    }
                    catch (System.Exception)
                    {
                        // Player 태그가 정의되지 않은 경우 무시
                    }
                    
                    if (!isPlayer)
                    {
                        objectPool.Return(obj);
                    }
                }
            }
        }


    }

    public void ExitFeverMode()
    {
        if (!isFever) return;
        
        isFever = false;
        isFeverUI = false; // UI 전용 피버 상태도 비활성화
        
        // 1. 피버 테마 제거 (배경 이미지만 복원)
        RemoveFeverTheme();
        
        // 2. 피버 오브젝트들을 풀로 반환
        ReturnFeverObjectsToPool();
        
        // 3. UI 및 오브젝트 원래 상태로 복원 (테마 포함)
        SwitchToNormalMode();
        
        // 🎯 핵심: 피버 모드 종료 시 표정과 팝업 이미지를 원래 테마로 복원
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RestoreNormalModeExpressions();

        }
        
        // 4. 스폰 재개
        ResumeObjectSpawning();
        

    }

    /// <summary>
    /// 피버 모드 테마 적용
    /// </summary>
    private void ApplyFeverTheme()
    {
        // BackgroundManager를 통해 피버 테마 적용
        if (BackgroundManager.Instance != null)
        {
            BackgroundManager.Instance.ApplyFeverTheme();
        }
        else
        {
        }
    }

    /// <summary>
    /// 피버 모드 테마 제거
    /// </summary>
    private void RemoveFeverTheme()
    {
        // BackgroundManager를 통해 피버 테마 제거
        if (BackgroundManager.Instance != null)
        {
            BackgroundManager.Instance.RemoveFeverTheme();

        }
        else
        {

        }
    }

    // 🔧 최적화: 중복 null 체크 제거 및 코드 간소화
    private void RestorePlayerAppearance()
    {
        if (playerSpriteRenderer == null || playerAnimator == null || player == null) return;
        
        // 점수 기반으로 현재 테마 상태 확인 (BackgroundManager 상태가 아닌)
        bool isNight = false;
        if (ScoreBasedDifficultyManager.Instance != null && GameStateManager.Instance != null)
        {
            int currentScore = GameStateManager.Instance.CurrentScore;
            var setting = ScoreBasedDifficultyManager.Instance.GetSettingForScore(currentScore);
            if (setting != null)
            {
                // ThemeType을 기반으로 밤/낮 테마 판단
                isNight = setting.theme != ThemeType.Day;
            }
        }
        
        playerSpriteRenderer.sprite = isNight ? normalNightSprite : normalDaySprite;
        playerAnimator.runtimeAnimatorController = isNight ? normalNightAnimator : normalDayAnimator;
        player.transform.localScale = normalPlayerScale;
        

    }

    /// <summary>
    /// 스폰된 모든 오브젝트들을 풀로 반환
    /// </summary>
    private void ReturnAllSpawnedObjectsToPool()
    {
        // 🔧 성능 최적화: FindAnyObjectByType 제거 - 싱글톤 인스턴스 사용
        if (objectPool == null)
        {
            objectPool = MultiObjectPool.Instance;
            if (objectPool == null)
            {
                return;
            }
        }

        // 🔧 성능 최적화: FindObjectsOfType 제거 - 오브젝트 풀에서 직접 관리
        if (objectPool != null && objectPool.ActiveObjects != null)
        {
            var activeObjects = new List<GameObject>(objectPool.ActiveObjects);
            foreach (var obj in activeObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    // 플레이어 오브젝트는 제외
                    bool isPlayer = false;
                    
                    try
                    {
                        isPlayer = obj.CompareTag("Player");
                    }
                    catch (System.Exception)
                    {
                        // Player 태그가 정의되지 않은 경우 무시
                    }
                    
                    if (!isPlayer)
                    {
                        objectPool.Return(obj);
                    }
                }
            }
        }


    }

    /// <summary>
    /// ObjectSpawner의 스폰을 즉시 중단
    /// </summary>
    private void StopObjectSpawning()
    {
        // 🔧 성능 최적화: FindAnyObjectByType 제거 - 인스펙터에서 직접 할당
        // var objectSpawner = FindAnyObjectByType<ObjectSpawner>();
        // if (objectSpawner != null)
        // {
        //     objectSpawner.StopSpawning();
        // }
        
        // TODO: ObjectSpawner 참조를 인스펙터에서 직접 할당하도록 변경 필요
    }

    /// <summary>
    /// ObjectSpawner의 스폰을 재개
    /// </summary>
    private void ResumeObjectSpawning()
    {
        // 🔧 성능 최적화: FindAnyObjectByType 제거 - 인스펙터에서 직접 할당
        // var objectSpawner = FindAnyObjectByType<ObjectSpawner>();
        // if (objectSpawner != null)
        // {
        //     objectSpawner.ResumeSpawning();
        // }
        
        // TODO: ObjectSpawner 참조를 인스펙터에서 직접 할당하도록 변경 필요
    }

    public bool IsFeverActive() => isFever;
    
    // UI 전용 피버 상태 확인 (더 일찍 설정됨)
    public bool IsFeverUIActive() => isFeverUI;

    public void EnterFeverMode()
    {
        if (isFever) return;
        // 🚨 isFever = true; 를 여기서 제거하고 맨 마지막으로 이동
        
        // 🎯 핵심: UI 전용 피버 상태를 먼저 활성화 (UI매니저가 피버 모드로 인식할 수 있도록)
        isFeverUI = true;

        // 레일 전환
        wheelObject.SetActive(false);
        straightRailObject.SetActive(true);
        
        // 레일 설정 및 스폰 간격 계산
        if (railScroller != null)
        {
            railScroller.RecalculateRailWidthAndPosition();
            railWidth = railScroller.GetRailWidth();
            if (railWidth <= 0f) railWidth = 6f;

            // 레일 너비의 2배를 50개 셀로 나누어 스폰 간격 계산
            float effectiveWidth = railWidth * 2f;
            feverSpawnInterval = (effectiveWidth / 50f) / railScroller.scrollSpeed;
            

        }

        // 플레이어 외형 변경
        if (playerSpriteRenderer != null) playerSpriteRenderer.sprite = feverSprite;
        if (playerAnimator != null) playerAnimator.runtimeAnimatorController = feverAnimator;
        if (player != null) player.transform.localScale = feverPlayerScale;
        
        // Player와 JudgeCenter를 피버 모드 위치로 이동
        if (player != null)
        {
            player.transform.position = feverPlayerPosition;

        }
        
        if (judgeCenter != null)
        {
            judgeCenter.position = feverJudgeCenterPosition;

        }

        // 피버 모드 전환 (테마 적용 포함)
        SwitchToFeverMode();

        // 피버 UI 적용
        UIManager.Instance?.ApplyFeverButtonSprite();
        
        // 🎯 핵심: 먼저 피버 테마 적용 (배경, 구슬 등)
        ApplyFeverTheme();
        
        // 🎯 핵심: 그 다음에 스코어 이미지와 표정을 현재 점수대의 낮 테마로 변경
        if (ScoreBasedDifficultyManager.Instance != null && GameStateManager.Instance != null)
        {
            int currentScore = GameStateManager.Instance.CurrentScore;
            var currentSetting = ScoreBasedDifficultyManager.Instance.GetSettingForScore(currentScore);
            if (currentSetting != null)
            {
                // 🎯 핵심: ScoreBasedDifficultyManager.ApplySetting() 호출하지 말고 UI 요소만 직접 업데이트
                // (피버 배경이 덮어써지지 않도록)
                
                // 1. 스코어 이미지 업데이트
                if (UIManager.Instance != null)
                {
                    // 🎯 핵심: 현재 점수대의 낮 테마 스프라이트를 직접 설정
                    var daySetting = ScoreBasedDifficultyManager.Instance.GetSettingForScore(currentScore);
                    if (daySetting != null)
                    {
                        // 🎯 핵심: UIManager의 스코어 이미지와 표정을 직접 업데이트
                        // 스코어 이미지는 현재 점수대의 낮 테마로 설정
                        if (UIManager.Instance.scoreImageUI != null && daySetting.dayGumihoSprites != null && daySetting.dayGumihoSprites.Length > 0)
                        {
                            var scoreImage = UIManager.Instance.scoreImageUI as Image;
                            if (scoreImage != null)
                            {
                                scoreImage.sprite = daySetting.dayGumihoSprites[0]; // 첫 번째 스프라이트 사용

                            }
                        }
                        
                        // 표정도 현재 점수대의 낮 테마로 설정
                        if (daySetting.dayGumihoSprites != null && daySetting.dayGumihoSprites.Length > 0)
                        {
                            // 🎯 핵심: 표정을 테마 기반으로 변경 (낮 테마로 설정)
                            // dayGumihoSprites[0]을 day와 night 모두에 사용하여 항상 낮 테마로 표시
                            UIManager.Instance.SetGumihoImageSet(daySetting.dayGumihoSprites[0], daySetting.dayGumihoSprites[0]);

                        }
                        

                    }
                }
                // 🎯 핵심: 피버 모드일 때 표정과 팝업 이미지를 낮 테마로 변경
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.SetFeverModeExpressions();

                }
            }
        }
        
        // 🔧 피버 모드 진입: 일반 노트 즉시 풀로 반환 및 스폰 중단
        ClearAllNormalNotes();
        
        // 🔧 피버 노트 스폰 관리 초기화
        InitializeFeverSpawnSystem();
        
        // 🔧 피버 모드 UI 버튼 변경
        SetFeverModeUI();
        
        // 🎯 핵심: 모든 테마 적용이 완료된 후에 isFever = true 설정
        isFever = true;
        
        // 🔧 피버 지속시간 시작 (연출과 동시에 시작)
        StartFeverDuration();
        
        // 🔧 코루틴으로 스폰 중단과 피버 스폰 시작을 순차적으로 실행
        StartCoroutine(FeverModeTransitionCoroutine());
    }

    private void SpawnFeverNote()
    {
        if (feverNotePrefab == null || feverSpawnPoint == null) return;
        
        // objectPool이 null이면 MultiObjectPool.Instance 사용
        if (objectPool == null)
        {
            objectPool = MultiObjectPool.Instance;
        }
        
        if (objectPool != null && railScroller != null && occupiedSegments != null)
        {
            // 🔧 레일 정보 가져오기
            float railWidth = railScroller.GetRailWidth();
            float segmentWidth = railScroller.GetSegmentWidth();
            int totalSegments = railScroller.GetTotalSegments();
            
            if (railWidth > 0f && segmentWidth > 0f && totalSegments > 0)
            {
                // 🔧 비어있는 칸 찾기 (순차적으로)
                int targetSegment = FindEmptySegment();
                if (targetSegment == -1)
                {
                    // 모든 칸이 차있으면 스폰하지 않음
                    return;
                }
                
                // 🔧 해당 칸을 점유 상태로 설정
                occupiedSegments[targetSegment] = true;
                
                GameObject note = objectPool.Get(feverNotePrefab);
                if (note == null) 
                {
                    // 노트를 가져오지 못했으면 점유 상태 해제
                    occupiedSegments[targetSegment] = false;
                    Debug.Log("[FeverModeManager] 노트를 가져오지 못해서 스폰 실패");
                    return;
                }

                // 🔧 레일과 동기화된 스폰 위치 계산
                Vector3 spawnPos = feverSpawnPoint.position;
                
                // 가장 왼쪽 레일의 위치를 기준으로 스폰 위치 조정
                Transform leftMostRail = railScroller.GetFarthestLeftRail();
                if (leftMostRail != null)
                {
                    // 레일의 Y축 위치에 맞춰 스폰
                    spawnPos.y = leftMostRail.position.y + 1f;
                    
                }

                // 노트 위치 및 회전 설정
                note.transform.position = spawnPos;
                note.transform.rotation = Quaternion.identity;

                // 🔧 피버 노트의 움직임을 레일과 완벽 동기화
                var feverMover = note.GetComponent<FeverBonusNoteMover>();
                if (feverMover != null)
                {
                    feverMover.syncWithRail = true;
                    // 레일과 동일한 속도로 설정
                    feverMover.SetSpeed(railScroller != null ? railScroller.scrollSpeed : 5f);
                    // 🔧 할당된 칸 정보 전달
                    feverMover.SetAssignedSegment(targetSegment);
                }

                note.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 비어있는 칸을 순차적으로 찾기
    /// </summary>
    private int FindEmptySegment()
    {
        if (occupiedSegments == null) return -1;
        
        // 🔧 순차적으로 비어있는 칸 찾기
        for (int i = 0; i < occupiedSegments.Length; i++)
        {
            if (!occupiedSegments[i])
            {
                return i;
            }
        }
        
        // 모든 칸이 차있으면 -1 반환
        return -1;
    }

    /// <summary>
    /// 지정된 칸을 비우기 (피버 노트가 사라질 때 호출)
    /// </summary>
    public void ReleaseSegment(int segment)
    {
        if (occupiedSegments != null && segment >= 0 && segment < occupiedSegments.Length)
        {
            occupiedSegments[segment] = false;
        }
    }
}
