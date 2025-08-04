using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    public static ObjectSpawner Instance { get; private set; }

    [SerializeField] private Transform wheelCenter;
    [SerializeField] private float wheelRadius = 2f;
    [SerializeField] private float spawnOffset = 0f;
    [SerializeField] private SplineRotator rotator;
    [SerializeField] private int totalSlots = 32;
    private bool[] occupiedSlots;

    private MultiObjectPool pool;
    private float spawnTimer;
    public bool spawnEnabled = true;

    private enum SpawnState { Normal, Merged }
    private SpawnState currentState = SpawnState.Normal;

    private int pendingMergeTailCount = 0;
    private GameObject currentHeadNote;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        pool = FindAnyObjectByType<MultiObjectPool>();
        if (rotator == null) rotator = FindAnyObjectByType<SplineRotator>();
        occupiedSlots = new bool[totalSlots];
    }

    private void Update()
    {
        if (!spawnEnabled) return;

        float timePerSlot = 360f / (rotator.rotationSpeed * totalSlots);
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= timePerSlot)
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

            spawnTimer = 0f;
        }
    }

    private void SpawnMonster()
    {
        if (FeverModeManager.Instance != null && FeverModeManager.Instance.IsFeverActive())
            return;

        int emptyIndex = FindEmptySlot();
        if (emptyIndex == -1) return;

        GameObject monster = MultiObjectPool.Instance.GetRandomByChance(); // allowedNoteTypes 내부 사용

        if (monster == null) return;
        
        foreach (var themeComponent in monster.GetComponentsInChildren<IThemeApplicable>(true))
        {
            themeComponent.ApplyTheme(BackgroundManager.Instance.IsNightTheme);
        }

        PositionMonster(monster, emptyIndex);

        NoteTypeHandler handler = monster.GetComponent<NoteTypeHandler>();
        if (handler != null && handler.noteType == NoteType.MergeHead)
        {
            currentState = SpawnState.Merged;
            pendingMergeTailCount = Random.Range(3, 8); // 2~5개 꼬리 예약
            currentHeadNote = monster; // 꼬리들을 귀속시킬 헤드 저장
        }
    }

    private void SpawnMergeTail()
    {
        int emptyIndex = FindEmptySlot();
        if (emptyIndex == -1) return;

        List<NoteType> Merged = new List<NoteType> { NoteType.MergeTail };
        GameObject tail = MultiObjectPool.Instance.GetRandomFromTypes(Merged);
        if (tail == null) return;

        PositionMonster(tail, emptyIndex);

        // 마지막 HeadNote에 자식으로 추가
        if (currentHeadNote != null)
        {
            tail.transform.SetParent(currentHeadNote.transform, true); // worldPosition 유지
        }
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < totalSlots; i++)
        {
            if (!occupiedSlots[i])
                return i;
        }
        return -1;
    }

    private void PositionMonster(GameObject obj, int slotIndex)
    {
        float anglePerSlot = 360f / totalSlots;
        float correctedAngleDeg = 90f - (anglePerSlot / 2f);
        float angleRad = correctedAngleDeg * Mathf.Deg2Rad;

        float spawnRadius = wheelRadius + spawnOffset;
        Vector3 offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0) * spawnRadius;
        obj.transform.position = wheelCenter.position + offset;
        obj.transform.rotation = Quaternion.identity;

        OrbitWalkingMonster orbit = obj.GetComponent<OrbitWalkingMonster>();
        if (orbit != null)
        {
            orbit.Initialize(wheelCenter, wheelRadius, rotator);
            orbit.SetSlotIndex(slotIndex, occupiedSlots);
        }

        occupiedSlots[slotIndex] = true;
    }

    public void ResumeSpawning()
    {
        spawnEnabled = true;
    }
}
