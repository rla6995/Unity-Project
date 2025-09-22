using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)] // Rotator(Update) 이후, Spawner(LateUpdate) 실행
public class ObjectSpawner : MonoBehaviour
{
    public static ObjectSpawner Instance { get; private set; }

    [SerializeField] private Transform wheelCenter;
    [SerializeField] private float wheelRadius = 2f;
    [SerializeField] private float spawnOffset = 0f;
    [SerializeField] private SplineRotator rotator;
    [SerializeField] private int totalSlots = 32;

    private bool[] occupiedSlots;

    // 각도 기반 스케줄링 상태
    private float angleAccumulator = 0f;      // 누적 회전 각도(전진분만)
    private float prevAngleDeg = float.NaN;   // 지난 프레임 각도

    public bool spawnEnabled = true;

    private enum SpawnState { Normal, Merged }
    private SpawnState currentState = SpawnState.Normal;

    private int pendingMergeTailCount = 0;
    private GameObject currentHeadNote;

    // 초기 즉시 스폰을 첫 LateUpdate에서 실행하기 위한 플래그
    private bool doInitialImmediateSpawn = true;

    // 🔧 최적화: 캐시된 값들
    private float cachedAnglePerSlot;
    private float cachedDiscontinuityThreshold;
    private const int MAX_SLOT_SEARCH_COUNT = 8; // 최대 8개 슬롯만 확인

    // 🔧 누락된 변수들 추가
    [SerializeField] private float feverSpawnInterval = 1.0f;
    [SerializeField] private float railWidth = 10f;
    [SerializeField] private float feverDuration = 15f;
    [SerializeField] private float spawnInterval = 2.0f; // 일반 스폰 간격
    private float nextSpawnTime; // 다음 스폰 시간

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (rotator == null) rotator = FindAnyObjectByType<SplineRotator>();
        totalSlots = Mathf.Max(1, totalSlots);
        occupiedSlots = new bool[totalSlots];

        // 🔧 최적화: 캐시된 값들을 미리 계산
        cachedAnglePerSlot = 360f / totalSlots;
        cachedDiscontinuityThreshold = cachedAnglePerSlot * 2.0f;

        // ✅ Start에서는 즉시 스폰하지 않고, 첫 LateUpdate에서 Rotator 갱신 이후 1회 스폰
        doInitialImmediateSpawn = true;

        // 스폰 타이머 초기화
        nextSpawnTime = Time.time + spawnInterval;

        // 씬 입장 시 동기화 (prevAngle 초기화)
        SyncToRotatorNow();
    }

    // Rotator의 Update()가 끝난 뒤 최신 각도를 읽기 위해 LateUpdate 사용
    private void LateUpdate()
    {
        // 피버 모드 중에는 모든 스폰 차단
        if (FeverModeManager.Instance != null && FeverModeManager.Instance.IsFeverActive())
        {
            return;
        }

        if (!spawnEnabled || rotator == null) return;

        float currAngleDeg = rotator.CurrentRotation;

        // 첫 프레임: 기준각 세팅
        if (float.IsNaN(prevAngleDeg))
        {
            prevAngleDeg = currAngleDeg;
            return;
        }

        // 🔧 최적화: 캐시된 값 사용
        // float anglePerSlot = 360f / totalSlots; // 제거됨

        // 이번 프레임 회전량(역방향은 0으로 취급)
        float deltaDeg = Mathf.DeltaAngle(prevAngleDeg, currAngleDeg);

        // 🔒 큰 불연속(로딩/스냅/타임스케일 변화 등) 자동 방어:
        // 한 프레임에 여러 슬롯(예: 2칸 이상)을 훌쩍 넘는 회전이면, 그 프레임은 스폰 누적을 버리고 동기화만 수행
        // 🔧 최적화: 캐시된 값 사용
        // float discontinuityThreshold = anglePerSlot * 2.0f; // 제거됨
        if (Mathf.Abs(deltaDeg) > cachedDiscontinuityThreshold)
        {
            prevAngleDeg = currAngleDeg;
            angleAccumulator = 0f; // 누적값 폐기
            // front 계산도 currAngle 기준으로 하도록 초기화
            // (GetFrontSlotIndex에서 현각만 사용하므로 별도 상태 불필요)
            // 초기 1회 즉시 스폰은 유지
        }
        else
        {
            // 🔒 프레임 히치 완화: 전진(양수)만 누적 + 프레임당 최대 1슬롯만 인정
            float forwardDeg = Mathf.Max(0f, deltaDeg);
            forwardDeg = Mathf.Min(forwardDeg, cachedAnglePerSlot); // 프레임당 최대 1칸만 누적
            angleAccumulator += forwardDeg;
        }

        prevAngleDeg = currAngleDeg;

        // ✅ 첫 LateUpdate에서만 즉시 1회 스폰 (피버 중이면 건너뜀)
        if (doInitialImmediateSpawn)
        {
            TrySpawnImmediatelyOnce();
            doInitialImmediateSpawn = false;
        }

        // 프레임당 최대 1개
        if (angleAccumulator >= cachedAnglePerSlot)
        {
            if (currentState == SpawnState.Merged && pendingMergeTailCount > 0)
            {
                SpawnMergeTail();
                pendingMergeTailCount--;
                if (pendingMergeTailCount <= 0)
                    currentState = SpawnState.Normal;
            }
            else
            {
                SpawnMonster();
            }

            angleAccumulator -= cachedAnglePerSlot;
            if (angleAccumulator < 0f) angleAccumulator = 0f; // 수치 안전
        }
    }

    private void TrySpawnImmediatelyOnce()
    {
        if (FeverModeManager.Instance != null && FeverModeManager.Instance.IsFeverActive())
            return;

        if (currentState == SpawnState.Normal)
        {
            SpawnMonster();
        }
        else if (pendingMergeTailCount > 0)
        {
            SpawnMergeTail();
            pendingMergeTailCount--;
            if (pendingMergeTailCount <= 0)
                currentState = SpawnState.Normal;
        }
    }

    private void SpawnMonster()
    {
        if (FeverModeManager.Instance != null && FeverModeManager.Instance.IsFeverActive())
            return;

        int emptyIndex = FindEmptySlotFromFront();
        if (emptyIndex == -1) return;

        GameObject monster = MultiObjectPool.Instance.GetRandomByChance();
        if (monster == null) return;

        // 테마 적용
        foreach (var themeComponent in monster.GetComponentsInChildren<IThemeApplicable>(true))
        {
            themeComponent.ApplyTheme(BackgroundManager.Instance.IsNightTheme);
        }

        PositionMonsterAt12(monster, emptyIndex);

        // 머리면 병합 모드 전환 및 꼬리 예약
        if (monster.TryGetComponent(out NoteTypeHandler handler) && handler.noteType == NoteType.MergeHead)
        {

            currentState = SpawnState.Merged;
            pendingMergeTailCount = Random.Range(3, 7); // 3~6개
            currentHeadNote = monster;

        }
    }

    private void SpawnMergeTail()
    {
        int emptyIndex = FindEmptySlotFromFront();
        if (emptyIndex == -1) return;

        List<NoteType> merged = new List<NoteType> { NoteType.MergeTail };
        GameObject tail = MultiObjectPool.Instance.GetRandomFromTypes(merged);
        if (tail == null) return;

        PositionMonsterAt12(tail, emptyIndex);

        if (currentHeadNote != null)
        {
            tail.transform.SetParent(currentHeadNote.transform, true); // 월드 좌표 유지

        }
        else
        {

        }
    }

    /// <summary>
    /// 현재 회전각을 기준으로 "지금 12시 중앙에 와 있는 슬롯"부터 빈 슬롯을 탐색.
    /// 🔧 최적화: 최대 8개 슬롯만 확인하여 성능 향상
    /// </summary>
    private int FindEmptySlotFromFront()
    {
        if (occupiedSlots == null || occupiedSlots.Length != totalSlots)
            occupiedSlots = new bool[Mathf.Max(1, totalSlots)];

        int start = GetFrontSlotIndex(); // 지금 12시에 온 슬롯
        int count = 0;
        
        // 🔧 최적화: 최대 8개 슬롯만 확인 (성능 향상)
        while (count < Mathf.Min(MAX_SLOT_SEARCH_COUNT, totalSlots))
        {
            int i = (start + count) % totalSlots;
            if (!occupiedSlots[i]) return i;
            count++;
        }
        return -1; // 빈 슬롯이 없음
    }

    /// <summary>
    /// 결정적 front index 계산:
    /// - 슬롯 중심을 경계로 하여 floor만 사용 (경계 토글/반칸 현상 방지)
    /// 🔧 최적화: 캐시된 값 사용
    /// </summary>
    private int GetFrontSlotIndex()
    {
        // 🔧 최적화: 캐시된 값 사용
        // float anglePer = 360f / totalSlots; // 제거됨

        // 슬롯 중심 기준으로 정렬: +0.5칸 만큼 미리 이동시킨 뒤 floor
        //  → [center - 0.5칸, center + 0.5칸) 구간이 해당 슬롯으로 귀속
        float curr = Mathf.Repeat(rotator.CurrentRotation + cachedAnglePerSlot * 0.5f, 360f);

        int idx = Mathf.FloorToInt(curr / cachedAnglePerSlot);
        if (idx >= totalSlots) idx = 0;
        if (idx < 0) idx = (idx % totalSlots + totalSlots) % totalSlots;
        return idx;
    }

    /// <summary>
    /// 스폰 위치는 항상 "12시 중앙" 고정. (휠의 현재 회전과 무관)
    /// 이후 OrbitWalkingMonster가 slotIndex의 현재 위치로 주행/정렬.
    /// 🔧 최적화: 캐시된 값 사용
    /// </summary>
    private void PositionMonsterAt12(GameObject obj, int slotIndex)
    {
        // 🔧 최적화: 캐시된 값 사용
        // float anglePerSlot = 360f / totalSlots; // 제거됨

        // 12시(90°)에서 반 칸 보정 → 정확히 "칸 중앙"
        float correctedAngleDeg = 90f - (cachedAnglePerSlot / 2f);
        float angleRad = correctedAngleDeg * Mathf.Deg2Rad;

        float spawnRadius = wheelRadius + spawnOffset;
        Vector3 offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0) * spawnRadius;

        obj.transform.position = wheelCenter.position + offset;
        obj.transform.rotation = Quaternion.identity;

        var orbit = obj.GetComponent<OrbitWalkingMonster>();
        if (orbit != null)
        {
            orbit.Initialize(wheelCenter, wheelRadius, rotator);
            orbit.SetSlotIndex(slotIndex, occupiedSlots);
        }

        occupiedSlots[slotIndex] = true;
    }

    // ===== 외부(피버/테마 전환/씬 입장)에서 호출하는 동기화 유틸 =====

    /// <summary>
    /// rotator 각도를 강제로 스냅(예: -8도 되돌리기)하거나,
    /// 씬 입장/로딩 직후에 스포너의 누적/기준각을 현재각으로 동기화.
    /// </summary>
    public void SyncToRotatorNow()
    {
        if (rotator == null) rotator = FindAnyObjectByType<SplineRotator>();
        angleAccumulator = 0f;
        prevAngleDeg = (rotator != null) ? rotator.CurrentRotation : 0f; // 다음 LateUpdate에서 Δ=0
        // 결정적 front 계산은 현각에서 항상 파생되므로 별도 상태 필요 없음
    }

    // ===== Fever/연출 연동용 제어 메서드들 =====

    public void PauseSpawning()
    {
        spawnEnabled = false;
    }

    public void PauseSpawningAndClearAll()
    {
        spawnEnabled = false;

        var pool = MultiObjectPool.Instance;
        if (pool != null)
        {
            foreach (var obj in pool.ActiveObjects.ToArray())
                pool.Return(obj);
        }

        if (occupiedSlots == null || occupiedSlots.Length != totalSlots)
            occupiedSlots = new bool[Mathf.Max(1, totalSlots)];
        for (int i = 0; i < occupiedSlots.Length; i++)
            occupiedSlots[i] = false;

        currentState = SpawnState.Normal;
        pendingMergeTailCount = 0;
        currentHeadNote = null;

        angleAccumulator = 0f;
        prevAngleDeg = rotator != null ? rotator.CurrentRotation : 0f;
    }

    /// <summary>
    /// 피버 종료 후 재개: 상태 동기화 + 재개 즉시 1회 스폰
    /// </summary>
    public void ResumeAfterFever()
    {
        angleAccumulator = 0f;
        prevAngleDeg = rotator != null ? rotator.CurrentRotation : 0f;
        spawnEnabled = true;

        // 재개 즉시 1회 스폰(피버 규칙 준수)
        if (currentState == SpawnState.Merged && pendingMergeTailCount > 0)
        {
            SpawnMergeTail();
            pendingMergeTailCount--;
            if (pendingMergeTailCount <= 0)
                currentState = SpawnState.Normal;
        }
        else
        {
            SpawnMonster();
        }
    }

    /// <summary>
    /// 스폰을 즉시 중단 (피버 모드 진입 시 호출)
    /// </summary>
    public void StopSpawning()
    {
        spawnEnabled = false; // 즉시 스폰 비활성화
        currentState = SpawnState.Normal;
        pendingMergeTailCount = 0;
        currentHeadNote = null;
        nextSpawnTime = Time.time + spawnInterval * 2f; // 스폰 간격의 2배만큼 대기
        
        // 🔧 디버깅: 스폰 중단 로그
        Debug.Log("[ObjectSpawner] 일반 노트 스폰 중단됨");
    }

    /// <summary>
    /// 스폰을 재개 (피버 모드 종료 시 호출)
    /// </summary>
    public void ResumeSpawning()
    {
        spawnEnabled = true; // 스폰 재활성화
        currentState = SpawnState.Normal;
        pendingMergeTailCount = 0;
        currentHeadNote = null;
        nextSpawnTime = Time.time; // 스폰 타이머 리셋하여 즉시 스폰 가능하도록 설정
        
        // 🔧 디버깅: 스폰 재개 로그
        Debug.Log("[ObjectSpawner] 일반 노트 스폰 재개됨");
    }

    public void SetFeverSpawnInterval(float interval)
    {
        feverSpawnInterval = interval;
    }

    public void SetRailWidth(float width)
    {
        railWidth = width;
    }

    public void SetFeverDuration(float duration)
    {
        feverDuration = duration;
    }

    public void SetWheelRadius(float radius)
    {
        wheelRadius = radius;
    }

    public void SetSpawnOffset(float offset)
    {
        spawnOffset = offset;
    }

    public void SetTotalSlots(int slots)
    {
        totalSlots = Mathf.Max(1, slots);
        if (occupiedSlots == null || occupiedSlots.Length != totalSlots)
            occupiedSlots = new bool[totalSlots];
        
        // 🔧 최적화: 슬롯 수 변경 시 캐시된 값들 재계산
        cachedAnglePerSlot = 360f / totalSlots;
        cachedDiscontinuityThreshold = cachedAnglePerSlot * 2.0f;
    }
}
