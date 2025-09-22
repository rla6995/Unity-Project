using UnityEngine;

public class FeverBonusNoteMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("레일과의 동기화 여부")]
    public bool syncWithRail = true;
    
    [Tooltip("레일과 동기화하지 않을 때의 이동 속도")]
    private float moveSpeed = 5f; // StraightRailScroller와 동일한 속도

    private StraightRailScroller railScroller;
    private bool isInitialized = false;
    private int assignedSegment = -1; // 🔧 할당된 칸 인덱스
    
    // 🔧 목표 위치 설정
    private Vector3 targetPosition;
    private bool hasTargetPosition = false;

    void Start()
    {
        // StraightRailScroller 찾기
        railScroller = FindAnyObjectByType<StraightRailScroller>();
        if (railScroller == null)
        {

        }
        else
        {
            isInitialized = true;
        }
    }

    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    /// <summary>
    /// 할당된 칸 인덱스 설정
    /// </summary>
    public void SetAssignedSegment(int segment)
    {
        assignedSegment = segment;
    }
    
    /// <summary>
    /// 목표 위치 설정 (고정 스폰 위치에서 목표 칸으로 이동)
    /// </summary>
    public void SetTargetPosition(float targetX, float targetY)
    {
        targetPosition = new Vector3(targetX, targetY, transform.position.z);
        hasTargetPosition = true;
    }

    void Update()
    {
        if (syncWithRail && railScroller != null && isInitialized)
        {
            // 🔧 레일과 완전히 동일한 속도와 방향으로 이동
            float noteSpeed = railScroller.scrollSpeed;
            transform.position += Vector3.right * noteSpeed * Time.deltaTime;
        }
        else
        {
            // 기본 속도로 이동
            UpdateStandardMovement();
        }

        // 화면 밖으로 나간 노트 정리
        if (transform.position.x > 15f)
        {
            ReturnToPool();
        }
    }
    
    /// <summary>
    /// 표준 움직임 업데이트 (레일과 동기화하지 않는 경우)
    /// </summary>
    private void UpdateStandardMovement()
    {
        float deltaMovement = moveSpeed * Time.deltaTime;
        transform.position += Vector3.right * deltaMovement;
    }
    
    /// <summary>
    /// 오브젝트 풀로 반환
    /// </summary>
    public void ReturnToPool()
    {
        // 🔧 할당된 칸을 비우기
        if (assignedSegment >= 0 && FeverModeManager.Instance != null)
        {
            FeverModeManager.Instance.ReleaseSegment(assignedSegment);
        }
        
        MultiObjectPool pool = FindAnyObjectByType<MultiObjectPool>();
        if (pool != null)
        {
            pool.Return(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 칸만 해제 (풀 반환은 하지 않음)
    /// </summary>
    public void ReleaseSegmentOnly()
    {
        // 🔧 할당된 칸만 비우기
        if (assignedSegment >= 0 && FeverModeManager.Instance != null)
        {
            FeverModeManager.Instance.ReleaseSegment(assignedSegment);
        }
    }
}
