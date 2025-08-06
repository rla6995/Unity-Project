using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StraightRailScroller : MonoBehaviour
{
    public Transform railA;
    public Transform railB;

    public float scrollSpeed = 5f;

    private float railWidth;
    public float GetRailWidth() => railWidth;
    private List<Transform> rails = new List<Transform>();
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
    }

    public void RecalculateRailWidthAndPosition()
    {
        SpriteRenderer sr = railA.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            railWidth = sr.bounds.size.x;
            Debug.Log($"[StraightRailScroller] 측정된 railWidth = {railWidth:F3}");

            railA.position = new Vector3(0f, -3.25f, 0f);
            railB.position = new Vector3(railA.position.x + railWidth, -3.25f, 0f);
        }
        else
        {
            Debug.LogWarning("[RailScroller] railA에 SpriteRenderer가 없습니다.");
        }
    }

    void Update()
    {
        if (railWidth <= 0f) return;

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
                rail.position = new Vector3(newX, rail.position.y, rail.position.z);
                Debug.Log($"[RailScroller] {rail.name} 정확히 재배치: {newX}");
                break;
            }
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
