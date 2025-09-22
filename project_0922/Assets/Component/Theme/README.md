# Theme - 테마 시스템

이 폴더는 게임의 테마 및 시각적 요소를 관리하는 컴포넌트들을 포함합니다.

## 📁 포함된 파일

### 🎨 테마 관리

#### IThemeApplicable.cs
- **역할**: 테마 적용 가능한 컴포넌트를 위한 인터페이스
- **주요 기능**:
  - `ApplyTheme(bool isNight)` 메서드 정의
  - 모든 Themed 컴포넌트가 구현해야 하는 표준 인터페이스

#### ThemedSpriteRenderer.cs
- **역할**: SpriteRenderer의 테마별 스프라이트 변경
- **주요 기능**:
  - 낮/밤 테마별 스프라이트 자동 전환
  - OnEnable 시 자동 테마 적용
  - ThemeManager와 자동 연동

#### ThemedImage.cs
- **역할**: UI Image의 테마별 스프라이트 변경
- **주요 기능**:
  - 낮/밤 테마별 UI 이미지 자동 전환
  - OnEnable 시 자동 테마 적용
  - UI Toolkit과 호환

#### ThemedAnimator.cs
- **역할**: Animator의 테마별 컨트롤러 변경
- **주요 기능**:
  - 낮/밤 테마별 애니메이터 컨트롤러 전환
  - OnEnable 시 자동 테마 적용
  - 애니메이션 상태 유지

## 🎭 테마 시스템 구조

### 🌅 테마 타입
```csharp
public enum ThemeType
{
    Day,           // 낮 테마
    Night1,        // 밤 테마 1단계
    Night2,        // 밤 테마 2단계
    Night3,        // 밤 테마 3단계
    Night4,        // 밤 테마 4단계
    BurningNight,  // 각성 테마 (불타는 밤)
    Ending         // 엔딩 테마
}
```

### 🔄 테마 적용 흐름
1. **ScoreBasedDifficultyManager**가 점수에 따라 테마 결정
2. **ThemeManager**가 전체 테마 적용
3. **Themed 컴포넌트들**이 자동으로 테마에 맞는 스프라이트/애니메이션 적용
4. **BackgroundManager**가 배경 이미지 변경
5. **AudioManager**가 테마에 맞는 BGM 재생

## 🔗 의존성

- **IThemeApplicable** → (인터페이스, 의존성 없음)
- **ThemedSpriteRenderer** → ThemeManager, IThemeApplicable
- **ThemedImage** → ThemeManager, IThemeApplicable
- **ThemedAnimator** → ThemeManager, IThemeApplicable

## 📋 사용법

### 🎨 Themed 컴포넌트 추가하기

#### 1. SpriteRenderer에 테마 적용
```csharp
// Inspector에서 설정
public class ThemedSpriteRenderer : MonoBehaviour, IThemeApplicable
{
    public Sprite daySprite;      // 낮 테마 스프라이트
    public Sprite nightSprite;    // 밤 테마 스프라이트
}
```

#### 2. UI Image에 테마 적용
```csharp
// Inspector에서 설정
public class ThemedImage : MonoBehaviour, IThemeApplicable
{
    public Sprite daySprite;      // 낮 테마 이미지
    public Sprite nightSprite;    // 밤 테마 이미지
}
```

#### 3. Animator에 테마 적용
```csharp
// Inspector에서 설정
public class ThemedAnimator : MonoBehaviour, IThemeApplicable
{
    public RuntimeAnimatorController dayController;    // 낮 테마 애니메이터
    public RuntimeAnimatorController nightController;  // 밤 테마 애니메이터
}
```

### 🔧 프로그래밍 방식으로 테마 적용

```csharp
// 특정 컴포넌트에 직접 테마 적용
var themedComponent = GetComponent<IThemeApplicable>();
if (themedComponent != null)
{
    themedComponent.ApplyTheme(true);  // 밤 테마
    themedComponent.ApplyTheme(false); // 낮 테마
}

// 모든 Themed 컴포넌트에 일괄 적용
ThemeManager.Instance.ApplyTheme(ThemeType.Night1);
```

## 🎯 테마 전환 시나리오

### 📈 점수 기반 테마 전환
- **0점**: Day 테마
- **100점**: Night1 테마
- **200점**: Night2 테마
- **300점**: Night3 테마
- **400점**: Night4 테마
- **500점**: BurningNight 테마
- **1000점**: Ending 테마

### 🔥 피버 모드 테마
- 피버 모드 진입 시 자동으로 Day 테마로 강제 전환
- 피버 모드 종료 시 원래 테마로 복원

### 🌙 밤 테마 특징
- 텍스트 색상이 흰색으로 변경
- UI 버튼 이미지가 밤 테마용으로 전환
- 구슬 오브젝트가 밤 테마 스프라이트로 변경
- BGM이 밤 테마용으로 자동 변경

## 💡 최적화 팁

1. **OnEnable 시 자동 적용**: Themed 컴포넌트는 활성화될 때마다 자동으로 테마를 적용합니다
2. **비활성 오브젝트도 포함**: ThemeManager는 비활성 오브젝트도 포함하여 테마를 적용합니다
3. **메모리 효율성**: 테마 변경 시에만 스프라이트/애니메이터를 교체하여 메모리를 효율적으로 사용합니다
