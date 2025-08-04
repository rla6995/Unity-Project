using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TitleSceneController : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject optionMenu;                  // 옵션 메뉴 패널
    public CanvasGroup tapToStartCanvasGroup;      // Tap to Start 텍스트
    public GameObject exitConfirmPanel;            // 종료 확인 패널

    [Header("설정")]
    public float blinkSpeed = 1.5f;                // 반짝임 속도

    // 두 번 터치 및 슬라이드 방지 관련
    private bool isTouchReady = false;
    private float touchStartTime = 0f;
    private Vector2 touchStartPos;
    public float maxTapDuration = 0.3f;    // 탭이라고 간주할 최대 시간 (초)
    public float maxTapMovement = 20f;     // 탭이라고 간주할 최대 이동 거리 (픽셀)

    void Start()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM(0); // 타이틀 씬 전용 BGM
    }

    void Update()
    {
        // Tap to Start 텍스트 반짝이게
        if (tapToStartCanvasGroup != null)
        {
            float alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;
            tapToStartCanvasGroup.alpha = alpha;
        }

        // ESC 키 입력 처리 (PC/Android 모두)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionMenu != null && optionMenu.activeSelf)
            {
                optionMenu.SetActive(false);
                return;
            }

            if (exitConfirmPanel != null)
            {
                bool isActive = exitConfirmPanel.activeSelf;
                exitConfirmPanel.SetActive(!isActive);
            }

            return;
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            OnTouchStarted(Time.time, Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnTouchEnded(Time.time, Input.mousePosition);
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                OnTouchStarted(Time.time, touch.position);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                OnTouchEnded(Time.time, touch.position);
            }
        }
#endif
    }

    private void OnTouchStarted(float time, Vector2 pos)
    {
        touchStartTime = time;
        touchStartPos = pos;
    }

    private void OnTouchEnded(float time, Vector2 pos)
    {
        float duration = time - touchStartTime;
        float movement = Vector2.Distance(pos, touchStartPos);

        // 탭으로 간주되는 짧고 정적인 터치만 통과
        if (duration <= maxTapDuration && movement <= maxTapMovement)
        {
            if (!isTouchReady)
            {
                isTouchReady = true; // 첫 탭이면 준비 상태로 전환
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if ((optionMenu != null && optionMenu.activeSelf) ||
                (exitConfirmPanel != null && exitConfirmPanel.activeSelf))
                return;

            SceneManager.LoadScene("GameScene");
        }
    }

    // 종료 확인창의 Yes 버튼
    public void OnClickExitYes()
    {
        Application.Quit();
    }

    // 종료 확인창의 No 버튼
    public void OnClickExitNo()
    {
        AudioManager.Instance?.PlaySE(0);
        if (exitConfirmPanel != null)
            exitConfirmPanel.SetActive(false);
    }
}
