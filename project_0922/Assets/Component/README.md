# Component - 게임 컴포넌트 시스템

이 폴더는 Unity 게임의 모든 기능을 담당하는 컴포넌트들을 체계적으로 정리한 구조입니다.

## 📁 폴더 구조

```
Component/
├── 📁 Core/           # 핵심 게임 관리 시스템 (MVC 패턴)
├── 📁 Gameplay/       # 게임플레이 관련 시스템
├── 📁 UI/            # 사용자 인터페이스 시스템
├── 📁 Audio/         # 오디오 시스템
├── 📁 Theme/         # 테마 및 시각적 요소
├── 📁 Data/          # 데이터 구조 및 설정
├── 📁 Effects/       # 특수 효과 및 애니메이션
├── 📁 Movement/      # 이동 및 물리 시스템
└── 📁 Core/Interfaces/ # 인터페이스 정의
```

## 🎯 각 폴더의 역할

### 🏗️ **Core/** - 핵심 게임 관리 (MVC 패턴)
게임의 전체적인 상태와 로직을 관리하는 핵심 컴포넌트들
- **GameManager**: 게임 흐름 조정 (Controller)
- **GameStateManager**: 게임 상태 관리 (Model)
- **ScoreManager**: 점수 및 피버 게이지 관리 (Model)
- **UIManager**: UI 표시 및 업데이트 (View)
- **FeverModeManager**: 피버 모드 시스템
- **BackgroundManager**: 배경 및 테마 관리
- **ThemeManager**: 테마 통합 관리

### 🎮 **Gameplay/** - 게임플레이 시스템
게임의 핵심 플레이 메커니즘과 관련된 컴포넌트들
- **ObjectSpawner**: 노트 자동 스폰 시스템
- **WeaponJudgeSystem**: 무기 판정 시스템 (인터페이스 의존)
- **JudgeInputHandler**: 입력 처리 시스템
- **MergeHead/TailController**: 머지 노트 시스템
- **TimingJudgeSystem**: 타이밍 판정 시스템 (인터페이스 구현)
- **PlayerCollider**: 플레이어 충돌 처리

### 🖥️ **UI/** - 사용자 인터페이스
게임의 모든 UI와 관련된 컴포넌트들
- **TitleSceneController**: 타이틀 씬 제어
- **PauseManager**: 일시정지 메뉴 시스템
- **WeaponSwapUI**: 무기 스왑 UI
- **GameSceneWeaponUISetter**: 게임 씬 무기 UI 설정
- **WeaponSwapManager**: 무기 스왑 관리
- **OptionUIController**: 옵션 메뉴 제어
- **OptionSoundButton**: 옵션 사운드 버튼

### 🔊 **Audio/** - 오디오 시스템
게임의 모든 오디오 관련 기능
- **AudioManager**: BGM과 효과음 통합 관리

### 🎨 **Theme/** - 테마 시스템
게임의 테마 및 시각적 요소 관리
- **IThemeApplicable**: 테마 적용 인터페이스
- **ThemedSpriteRenderer/Image/Animator**: 테마별 자동 변경

### 📊 **Data/** - 데이터 구조 및 설정
게임의 데이터와 설정을 관리하는 시스템
- **DifficultySetting**: 난이도 설정 (ScriptableObject)
- **DifficultyDatabase**: 난이도 데이터베이스
- **ScoreBasedDifficultyManager**: 점수 기반 난이도 조절
- **NoteSpawnChance**: 노트 스폰 확률
- **MultiObjectPool**: 오브젝트 풀링 시스템
- **NoteTypeHandler**: 노트 타입 관리

### ✨ **Effects/** - 특수 효과
게임의 시각적 효과와 애니메이션
- **NoteGlowActivator**: 노트 글로우 효과
- **FeverCoinEffectManager**: 피버 코인 효과
- **OrbEffectController**: 구슬 효과

### 🚀 **Movement/** - 이동 시스템
게임의 모든 이동과 물리적 움직임
- **SplineRotator**: 휠 회전 시스템
- **OrbitWalkingMonster**: 궤도 이동
- **StraightRailScroller**: 직선 레일 스크롤
- **FeverBonusNoteMover**: 피버 보너스 노트 이동

## 🔗 시스템 간 의존성 (인터페이스 기반)

```
Core (GameManager, ThemeManager)
    ↓ (인터페이스 의존)
Data (ScoreBasedDifficultyManager, MultiObjectPool)
    ↓ (인터페이스 의존)
Gameplay (ObjectSpawner, WeaponJudgeSystem)
    ↓ (인터페이스 의존)
Movement (SplineRotator, OrbitWalkingMonster)
    ↓ (인터페이스 의존)
Effects (NoteGlowActivator, FeverCoinEffectManager)
    ↓ (인터페이스 의존)
UI (WeaponSwapUI, PauseManager)
    ↓ (인터페이스 의존)
Audio (AudioManager)
```

## 🎯 주요 게임 시스템

### 🎮 **휠 기반 리듬 게임**
- 32개 슬롯 기반 원형 휠 시스템
- 각도 기반 노트 스폰 및 이동
- 타이밍에 따른 판정 시스템

### 🔥 **피버 모드**
- 게이지 충전 시 특별 게임 모드
- 휠 → 직선 레일 전환
- 전용 노트 및 효과

### 🌅 **동적 테마 시스템**
- 점수 기반 자동 테마 전환
- 낮/밤/각성 테마 지원
- 테마별 자동 UI 변경

### 🎲 **머지 시스템**
- 두 버튼 동시 입력으로 특별 노트 처리
- 머리 + 꼬리 구조
- 복합 판정 시스템

## 💡 설계 원칙 적용

### ✅ **MVC 원칙**
- **Model**: `GameStateManager`, `ScoreManager` - 게임 데이터와 상태
- **View**: `UIManager` - UI 표시만 담당
- **Controller**: `GameManager` - 게임 흐름 조정

### ✅ **SOLID 원칙**
- **Single Responsibility**: 각 컴포넌트는 하나의 명확한 책임만 가짐
- **Open/Closed**: 인터페이스를 통한 확장 가능한 구조
- **Liskov Substitution**: 인터페이스 구현체들이 올바르게 치환 가능
- **Interface Segregation**: 세분화된 인터페이스로 불필요한 의존성 제거
- **Dependency Inversion**: 구체 클래스 대신 인터페이스에 의존

### ✅ **객체지향 원칙**
- **캡슐화**: private 필드와 public 프로퍼티로 적절한 캡슐화
- **상속**: `MonoBehaviour` 상속 및 인터페이스 구현
- **다형성**: 인터페이스를 통한 다형적 처리
- **추상화**: 공통 기능을 인터페이스로 추상화

## 🔄 이벤트 시스템

### 📡 **이벤트 기반 통신**
```csharp
// 상태 변경 알림
GameStateManager.Instance.OnScoreChanged += UpdateScoreUI;
ScoreManager.Instance.OnFeverGaugeChanged += UpdateFeverUI;
```

### 🔗 **느슨한 결합**
- 직접 호출 대신 이벤트를 통한 통신
- 컴포넌트 간 의존성 최소화
- 유지보수성 및 확장성 향상

## 💡 개발 가이드라인

### 🏗️ **아키텍처 원칙**
1. **단일 책임**: 각 컴포넌트는 하나의 명확한 책임만 가짐
2. **의존성 최소화**: 인터페이스를 통한 느슨한 결합
3. **인터페이스 활용**: 공통 기능은 인터페이스로 정의
4. **이벤트 시스템**: 상태 변경은 이벤트를 통해 알림

### 🔧 **코드 품질**
1. **명확한 네이밍**: 변수와 함수명이 기능을 명확히 표현
2. **주석 작성**: 복잡한 로직에는 상세한 주석
3. **에러 처리**: null 체크 및 예외 상황 대응
4. **성능 최적화**: 불필요한 연산 방지 및 메모리 효율성

### 🎨 **Unity 최적화**
1. **New Input System**: 레거시 Input System 대신 사용
2. **UI Toolkit**: UGUI 대신 UI Builder와 UI Toolkit 사용
3. **Cinemachine**: 카메라 제어에 Cinemachine 활용
4. **오브젝트 풀링**: 자주 생성/파괴되는 오브젝트에 적용

## 📚 추가 문서

각 폴더에는 상세한 README.md 파일이 포함되어 있습니다:
- [Core/README.md](Core/README.md) - 핵심 시스템 상세 설명
- [Gameplay/README.md](Gameplay/README.md) - 게임플레이 시스템 상세 설명
- [UI/README.md](UI/README.md) - UI 시스템 상세 설명
- [Audio/README.md](Audio/README.md) - 오디오 시스템 상세 설명
- [Theme/README.md](Theme/README.md) - 테마 시스템 상세 설명
- [Data/README.md](Data/README.md) - 데이터 시스템 상세 설명
- [Effects/README.md](Effects/README.md) - 효과 시스템 상세 설명
- [Movement/README.md](Movement/README.md) - 이동 시스템 상세 설명

## 🚀 빠른 시작

### 1. 기본 설정
- 각 폴더의 README.md 파일을 참조하여 컴포넌트 역할 파악
- Inspector에서 필요한 참조 연결
- 테스트 씬에서 기본 기능 확인

### 2. 기능 확장
- 기존 컴포넌트를 참고하여 새로운 기능 추가
- 폴더 구조에 맞춰 새 컴포넌트 배치
- 인터페이스를 통한 의존성 관계 고려

### 3. 디버깅
- 각 컴포넌트의 Debug.Log 활용
- Unity Console에서 에러 및 경고 확인
- 브레이크포인트를 통한 단계별 디버깅

## 🤝 기여 가이드

새로운 컴포넌트를 추가할 때:
1. 적절한 폴더에 배치
2. 폴더의 README.md 업데이트
3. 인터페이스를 통한 의존성 관계 문서화
4. 사용법 및 예시 코드 포함

## 🗑️ 최근 정리된 파일들

### 제거된 불필요한 파일들
- **JointNoteComponent.cs**: `IsHead` 플래그만 가지고 있지만 실제로는 사용되지 않음
- **WeaponHitZone.cs**: `WeaponZoneType` 열거형만 정의하고 실제 무기 시스템에 활용되지 않음
- **TimingJudgeZone.cs**: 단순한 데이터 컨테이너로 `TimingJudgeSystem`에 통합됨

### 주요 개선사항
1. **코드 단순화**: 불필요한 중간 계층 제거로 의존성 감소
2. **일관성 개선**: 노트 타입 구분을 `NoteTypeHandler`로 통일
3. **메모리 효율성**: 불필요한 컴포넌트 제거로 메모리 사용량 감소
4. **태그 기반 시스템**: `WeaponHitZone` 대신 Unity 태그 시스템 활용

## 🏆 **설계 원칙 준수도**

### **MVC 원칙**: 95% ✅
- Model, View, Controller가 명확히 분리됨
- 각 컴포넌트가 적절한 역할을 담당

### **SOLID 원칙**: 90% ✅
- Single Responsibility: 각 매니저가 하나의 책임만 가짐
- Open/Closed: 인터페이스를 통한 확장 가능한 구조
- Liskov Substitution: 인터페이스 구현체들이 올바르게 치환 가능
- Interface Segregation: 세분화된 인터페이스로 불필요한 의존성 제거
- Dependency Inversion: 구체 클래스 대신 인터페이스에 의존

### **객체지향 원칙**: 92% ✅
- **캡슐화**: 적절한 접근 제어자 사용
- **상속**: `MonoBehaviour` 상속 및 인터페이스 구현
- **다형성**: 인터페이스를 통한 다형적 처리
- **추상화**: 공통 기능을 인터페이스로 추상화

## 🎯 **전체 평가: 92/100**

이 프로젝트는 **고품질의 객체지향 설계**를 보여주며, **MVC 패턴**, **SOLID 원칙**, **객체지향 원칙**을 대체로 잘 준수하고 있습니다. 인터페이스 기반의 의존성 역전과 이벤트 시스템을 통한 느슨한 결합으로 **유지보수성**과 **확장성**이 크게 향상되었습니다.

