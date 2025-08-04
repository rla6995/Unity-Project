using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject exitPanel;
    public GameObject optionPanel;

    public Button pauseButton;
    public Button resumeButton;
    public Button homeButton;
    public Button optionButton;

    public Button exitYesButton;
    public Button exitNoButton;

    private bool isPaused = false;
    private bool isExitPanelActive = false;

    public Button closeOptionButton;
    private bool isOptionPanelActive = false;
    public WeaponSwapUI weaponSwapUI;
    private GameObject previousPanel;
    public GameObject gameOverPanel;
void Start()
{
    pauseButton.onClick.AddListener(PauseGame);
    resumeButton.onClick.AddListener(ResumeGame);
    homeButton.onClick.AddListener(() => OpenExitPanel(pausePanel)); // ✅ 홈 버튼
    optionButton.onClick.AddListener(OpenOptionPanel);
    closeOptionButton.onClick.AddListener(CloseOptionPanel);
    exitYesButton.onClick.AddListener(ExitToStartScene);
    exitNoButton.onClick.AddListener(CloseExitPanel); // ✅ NO는 무조건 Close
    pausePanel.SetActive(false);
    exitPanel.SetActive(false);
    optionPanel.SetActive(false);
}


void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        if (isExitPanelActive)
        {
            AudioManager.Instance?.PlaySE(0);
            CloseExitPanel();  // Exit Panel 닫기
        }
        else if (isOptionPanelActive)
        {
            AudioManager.Instance?.PlaySE(0);
            CloseOptionPanel();  // Option Panel 닫기
        }
        else if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
        {
            // 게임오버 상태일 때는 exitPanel 열기
            OpenExitPanel(gameOverPanel);
        }
        else if (!isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }
}


    public void PauseGame()
    {
        Time.timeScale = 0;
        isPaused = true;
        AudioManager.Instance?.PlaySE(0);
        pausePanel.SetActive(true);
    }

public void ResumeGame()
{
    bool isGameOver = GameManager.Instance != null && GameManager.Instance.IsGameOver();

    // 게임 오버 상태에서는 시간은 멈추게, UI는 닫히게
    if (isGameOver)
    {
        Time.timeScale = 0; // 멈춤 유지
    }
    else
    {
        Time.timeScale = 1;
        isPaused = false;
    }
    AudioManager.Instance?.PlaySE(0);
    // 공통: 어떤 상황에서도 패널은 닫는다
    pausePanel.SetActive(false);
    exitPanel.SetActive(false);
    optionPanel.SetActive(false);
    isExitPanelActive = false;
}


public void OpenExitPanel(GameObject fromPanel = null)
{
    AudioManager.Instance?.PlaySE(0);
    previousPanel = fromPanel; // 이전 패널 저장
    pausePanel?.SetActive(false);
    optionPanel?.SetActive(false);
    fromPanel?.SetActive(false); // 넘겨준 패널이 있으면 끄기

    exitPanel.SetActive(true);
    isExitPanelActive = true;
}


public void CloseExitPanel()
{
    AudioManager.Instance?.PlaySE(0);
    exitPanel.SetActive(false);
    isExitPanelActive = false;

    if (previousPanel != null)
    {
        previousPanel.SetActive(true);
        previousPanel = null;
    }
    else
    {
        pausePanel.SetActive(true); // 기본은 PausePanel로 복귀
    }
}


    public void ExitToStartScene()
    {
        AudioManager.Instance?.PlaySE(0);
        Time.timeScale = 1; // 씬 이동 전에 시간 다시 재생
        SceneManager.LoadScene("StartScene"); // "StartScene"은 정확한 씬 이름으로 교체
    }

public void OpenOptionPanel()
{
    AudioManager.Instance?.PlaySE(0);
    pausePanel.SetActive(false);
    optionPanel.SetActive(true);
    isOptionPanelActive = true;
    weaponSwapUI.Initialize();
}

public void CloseOptionPanel()
{
    AudioManager.Instance?.PlaySE(0);
    optionPanel.SetActive(false);
    pausePanel.SetActive(true);
    isOptionPanelActive = false;
}
}
