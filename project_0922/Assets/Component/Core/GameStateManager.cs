using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임의 전반적인 상태를 관리하는 매니저
/// 단일 책임 원칙: 게임 상태 관리만 담당
/// </summary>
public class GameStateManager : MonoBehaviour, IGameState
{
    public static GameStateManager Instance { get; private set; }

    private bool isGameOver = false;
    private bool isPaused = false;
    private int currentScore = 0;

    // 이벤트 시스템을 통한 상태 변경 알림
    public System.Action<bool> OnGameOverChanged;
    public System.Action<bool> OnPauseChanged;
    public System.Action<int> OnScoreChanged;

    public bool IsGameOver => isGameOver;
    public bool IsPaused => isPaused;
    public int CurrentScore => currentScore;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Time.timeScale = 1;
        currentScore = 0;
        isGameOver = false;
        isPaused = false;
    }

    public void SetGameOver(bool gameOver)
    {
        if (isGameOver == gameOver) return;
        
        isGameOver = gameOver;
        if (gameOver)
        {
            Time.timeScale = 0;
        }
        
        OnGameOverChanged?.Invoke(gameOver);
    }

    public void SetPaused(bool paused)
    {
        if (isPaused == paused) return;
        
        isPaused = paused;
        Time.timeScale = paused ? 0 : 1;
        
        OnPauseChanged?.Invoke(paused);
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;
        
        currentScore += amount;
        OnScoreChanged?.Invoke(currentScore);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
