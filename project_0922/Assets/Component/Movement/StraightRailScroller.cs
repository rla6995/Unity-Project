using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class StraightRailScroller : MonoBehaviour
{
    [Header("Rail References")]
    public Transform railA;
    public Transform railB;

    [Header("Movement Settings")]
    public float scrollSpeed = 5f;
    
    [Header("Rail Configuration")]
    [Tooltip("레일의 총 칸 수 (에디터에서 설정)")]
    public int totalRailSegments = 25; // 🔧 피버 모드용 25칸으로 설정
    [Tooltip("한 칸의 너비 (자동 계산되거나 수동 설정)")]
    public float segmentWidth = 0f;
    
    [Header("Events")]
    [Tooltip("레일이 재배치될 때 발생하는 이벤트")]
    public UnityEvent<float> OnRailRepositioned;

    private float railWidth;
    public float GetRailWidth() => railWidth;
    public float GetSegmentWidth() => segmentWidth;
    public int GetTotalSegments() => totalRailSegments;
    
    private List<Transform> rails = new List<Transform>();
    private Vector3 lastRailAPosition;
    private bool isInitialized = false;
void Awake()
{
    rails = new List<Transform> { railA, railB };
}

    void Start()
    {
        StartCoroutine(InitWidthDelayed());
    }

    IEnumerator InitWidthDelayed()
    {
        yield return new WaitForEndOfFrame();
        RecalculateRailWidthAndPosition();
        
        // 초기 위치 저장
        if (railA != null)
        {
            lastRailAPosition = railA.position;
            isInitialized = true;
        }
    }

    public void RecalculateRailWidthAndPosition()
    {
        SpriteRenderer sr = railA.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            railWidth = sr.bounds.size.x;
            
            // 레일 한 칸의 너비 자동 계산
            if (totalRailSegments > 0)
            {
                segmentWidth = railWidth / totalRailSegments;

            }
            else
            {

            }

            railA.position = new Vector3(0f, -3.25f, 0f);
            railB.position = new Vector3(railA.position.x + railWidth, -3.25f, 0f);
        }
        else
        {

        }
    }

    void Update()
    {
        if (railWidth <= 0f || !isInitialized) return;

        Vector3 move = Vector3.right * scrollSpeed * Time.deltaTime;
        foreach (Transform rail in rails)
            rail.position += move;

        float screenRightX = Camera.main.ViewportToWorldPoint(Vector3.one).x;

        foreach (Transform rail in rails)
        {
            float leftEdge = rail.position.x - railWidth / 2f;

            if (leftEdge > screenRightX)
            {
                Transform leftMost = GetFarthestLeftRail();
                float newX = leftMost.position.x - railWidth;
                Vector3 oldPosition = rail.position;
                rail.position = new Vector3(newX, rail.position.y, rail.position.z);
                
                // 레일 재배치 이벤트 발생
                if (rail == railA && OnRailRepositioned != null)
                {
                    float repositionDistance = newX - oldPosition.x;
                    OnRailRepositioned.Invoke(repositionDistance);

                }
                

                break;
            }
        }
        
        // 레일 A의 위치 변화 추적
        if (railA != null && isInitialized)
        {
            Vector3 currentPosition = railA.position;
            float deltaX = currentPosition.x - lastRailAPosition.x;
            
            // 비정상적인 큰 점프 감지 (재배치)
            if (Mathf.Abs(deltaX) > railWidth * 0.5f)
            {
                if (OnRailRepositioned != null)
                {
                    OnRailRepositioned.Invoke(deltaX);

                }
            }
            
            lastRailAPosition = currentPosition;
        }
    }

    public Transform GetFarthestLeftRail()
    {
        Transform leftMost = rails[0];
        foreach (Transform rail in rails)
        {
            if (rail.position.x < leftMost.position.x)
                leftMost = rail;
        }
        return leftMost;
    }
    
}
