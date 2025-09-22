/// <summary>
/// 게임 상태 관리를 위한 인터페이스
/// 의존성 역전 원칙: 구체 클래스 대신 인터페이스에 의존
/// </summary>
public interface IGameState
{
    bool IsGameOver { get; }
    bool IsPaused { get; }
    int CurrentScore { get; }
    
    void SetGameOver(bool gameOver);
    void SetPaused(bool paused);
    void AddScore(int amount);
    void Restart();
}
