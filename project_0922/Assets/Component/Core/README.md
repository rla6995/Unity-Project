# Core - 핵심 게임 관리 시스템

이 폴더는 Unity 게임의 핵심적인 상태와 로직을 관리하는 컴포넌트들을 포함합니다.

## 📁 포함된 파일들

### 🎮 **GameManager.cs**
- **역할**: 게임의 전체적인 흐름을 조정하는 매니저
- **단일 책임**: 게임 흐름 조정만 담당
- **주요 기능**:
  - 피버 모드 진입 시퀀스 시작
  - 점수 입력 처리
  - 게임오버 트리거
- **의존성**: `GameStateManager`, `ScoreManager`, `UIManager`

### 🏗️ **GameStateManager.cs**
- **역할**: 게임의 전반적인 상태를 관리하는 매니저
- **단일 책임**: 게임 상태 관리만 담당
- **주요 기능**:
  - 게임오버 상태 관리
  - 일시정지 상태 관리
  - 점수 관리
  - 이벤트 시스템을 통한 상태 변경 알림
- **구현**: `IGameState` 인터페이스
- **의존성**: 없음 (독립적)

### 📊 **ScoreManager.cs**
- **역할**: 점수와 피버 게이지 관리를 담당하는 매니저
- **단일 책임**: 점수 관련 기능만 담당
- **주요 기능**:
  - 피버 게이지 관리
  - 판정 결과에 따른 점수 추가
  - 피버 모드 시 점수 배율 적용
- **구현**: `IScoreManager` 인터페이스
- **의존성**: `GameStateManager`, `FeverModeManager`

### 🖥️ **UIManager.cs**
- **역할**: 게임 UI 업데이트를 담당하는 매니저
- **단일 책임**: UI 표시만 담당
- **주요 기능**:
  - 점수 UI 업데이트
  - 피버 게이지 UI 업데이트
  - 판정 결과 표시
  - 여우 표정 변경
  - 피버 모드 UI 전환
- **구현**: `IUIManager` 인터페이스
- **의존성**: `GameStateManager`, `ScoreManager`, `FeverModeManager`

### 🔥 **FeverModeManager.cs**
- **역할**: 피버 모드 관리
- **단일 책임**: 피버 모드만 담당
- **주요 기능**:
  - 피버 모드 진입/종료
  - 휠 ↔ 직선 레일 전환
  - 플레이어 외형 변경
  - 피버 노트 스폰
- **의존성**: `ScoreManager`, `UIManager`, `ScoreBasedDifficultyManager`

### 🌅 **BackgroundManager.cs**
- **역할**: 배경 및 테마 관리
- **주요 기능**: 배경 이미지 변경, 테마별 배경 전환

### 🎨 **ThemeManager.cs**
- **역할**: 테마 통합 관리
- **주요 기능**: 테마별 시각적 요소 변경

## 🔗 **인터페이스 구조**

### 📋 **IGameState**
```csharp
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
```

### 📋 **IScoreManager**
```csharp
public interface IScoreManager
{
    float FeverGauge { get; }
    
    void IncreaseFever(float amount);
    void SetFeverGauge(float value);
    void ResetFeverGauge();
    void AddScore(JudgeResult result);
}
```

### 📋 **IUIManager**
```csharp
public interface IUIManager
{
    void ShowJudgeText(JudgeResult result);
    void UpdateScoreImageByCurrentSetting();
    void SetGumihoImageSet(Sprite daySprite, Sprite nightSprite);
    void ForceUpdateJudgeImage();
}
```

## 🎯 **설계 원칙 적용**

### ✅ **MVC 원칙**
- **Model**: `GameStateManager`, `ScoreManager` - 게임 데이터와 상태 관리
- **View**: `UIManager` - UI 표시만 담당
- **Controller**: `GameManager` - 게임 흐름 조정

### ✅ **SOLID 원칙**
- **Single Responsibility**: 각 매니저가 하나의 명확한 책임만 가짐
- **Open/Closed**: 인터페이스를 통한 확장 가능한 구조
- **Liskov Substitution**: 인터페이스 구현체들이 올바르게 치환 가능
- **Interface Segregation**: 세분화된 인터페이스로 불필요한 의존성 제거
- **Dependency Inversion**: 구체 클래스 대신 인터페이스에 의존

### ✅ **객체지향 원칙**
- **캡슐화**: private 필드와 public 프로퍼티로 적절한 캡슐화
- **상속**: `MonoBehaviour` 상속 및 인터페이스 구현
- **다형성**: 인터페이스를 통한 다형적 처리
- **추상화**: 공통 기능을 인터페이스로 추상화

## 🔄 **이벤트 시스템**

### 📡 **이벤트 기반 통신**
```csharp
// GameStateManager의 이벤트
public System.Action<bool> OnGameOverChanged;
public System.Action<bool> OnPauseChanged;
public System.Action<int> OnScoreChanged;

// ScoreManager의 이벤트
public System.Action<float> OnFeverGaugeChanged;
public System.Action<int> OnScoreAdded;
```

### 🔗 **이벤트 구독 패턴**
```csharp
// UIManager에서 이벤트 구독
GameStateManager.Instance.OnScoreChanged += UpdateScoreUI;
ScoreManager.Instance.OnFeverGaugeChanged += UpdateFeverUI;
```

## 🚀 **사용법**

### 1. **초기 설정**
```csharp
// 각 매니저는 자동으로 싱글톤 인스턴스 생성
// Inspector에서 필요한 참조 연결
```

### 2. **게임 상태 변경**
```csharp
// 게임오버 설정
GameStateManager.Instance.SetGameOver(true);

// 점수 추가
ScoreManager.Instance.AddScore(JudgeResult.Wow);

// UI 업데이트는 자동으로 이벤트를 통해 처리
```

### 3. **피버 모드 진입**
```csharp
// 피버 모드 시작
FeverModeManager.Instance.StartFeverEntrySequence();
```

## 💡 **개선 사항**

### 🔧 **최근 리팩토링**
1. **책임 분리**: `GameManager`의 과도한 책임을 여러 매니저로 분산
2. **인터페이스 도입**: 의존성 역전 원칙 적용으로 결합도 감소
3. **이벤트 시스템**: 직접 호출 대신 이벤트 기반 통신으로 느슨한 결합
4. **단일 책임**: 각 매니저가 하나의 명확한 책임만 담당

### 📈 **성능 최적화**
- 이벤트 구독/해제를 통한 메모리 누수 방지
- 불필요한 직접 참조 제거
- 인터페이스를 통한 느슨한 결합으로 유지보수성 향상

## 🤝 **의존성 관계**

```
GameManager (게임 흐름 조정)
    ↓
GameStateManager (게임 상태)
    ↓
ScoreManager (점수/피버)
    ↓
UIManager (UI 표시)
    ↓
FeverModeManager (피버 모드)
```

이 구조는 **의존성 역전 원칙**을 따르며, 각 컴포넌트가 인터페이스를 통해 통신하여 **느슨한 결합**을 유지합니다.
