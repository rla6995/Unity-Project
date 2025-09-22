# Effects - 특수 효과 및 애니메이션 시스템

이 폴더는 게임의 특수 효과, 애니메이션, 그리고 시각적 피드백을 담당하는 컴포넌트들을 포함합니다.

## 📁 포함된 파일

### ✨ 노트 효과

#### NoteGlowActivator.cs
- **역할**: 노트에 글로우 효과를 활성화하는 컴포넌트
- **주요 기능**:
  - 노트가 특정 영역에 진입 시 글로우 효과 자동 활성화
  - 0.2초 지연 후 글로우 애니메이션 트리거
  - "9-Sliced" 자식 오브젝트의 Animator와 연동
  - 노트 태그 기반 자동 감지

#### FeverCoinEffectManager.cs
- **역할**: 피버 노트 Wow 판정 시 코인 효과를 관리
- **주요 기능**:
  - 피버 코인 효과 오브젝트 활성화
  - "DropTrigger" 애니메이션 자동 실행
  - 0.2초 후 자동 비활성화
  - 싱글톤 패턴으로 전역 접근

#### OrbEffectController.cs
- **역할**: 구슬 오브젝트의 효과를 제어
- **주요 기능**:
  - 테마 변경 시 "Play" 애니메이션 트리거
  - 낮/밤 테마 전환 시 시각적 피드백
  - 디버그 로그로 효과 실행 상태 확인

## 🎭 효과 시스템 구조

### 🔍 효과 활성화 조건

#### NoteGlowActivator
- **트리거 조건**: 노트가 특정 콜라이더에 진입
- **대상**: "Note" 태그를 가진 오브젝트
- **효과**: 글로우 애니메이션 (0.2초 지연)

#### FeverCoinEffectManager
- **트리거 조건**: 피버 노트 Wow 판정
- **호출 위치**: WeaponJudgeSystem에서 자동 호출
- **효과**: 코인 드롭 애니메이션

#### OrbEffectController
- **트리거 조건**: 테마 변경 (BackgroundManager에서 호출)
- **효과**: 구슬 회전/빛남 애니메이션

### 🎨 애니메이션 연동

모든 효과 컴포넌트는 Unity의 Animator 시스템과 연동됩니다:
- **NoteGlowActivator**: "Glow" 트리거
- **FeverCoinEffectManager**: "DropTrigger" 트리거
- **OrbEffectController**: "Play" 트리거

## 🔗 의존성

- **NoteGlowActivator** → MultiObjectPool (노트 검색)
- **FeverCoinEffectManager** → (독립적, 외부에서 호출)
- **OrbEffectController** → (독립적, BackgroundManager에서 호출)

## 📋 사용법

### ✨ 노트 글로우 효과 추가하기

#### 1. 노트 프리팹 설정
```
노트 오브젝트
├── 9-Sliced (글로우 효과용)
│   ├── SpriteRenderer
│   └── Animator (Glow 애니메이션)
└── NoteGlowActivator 컴포넌트
```

#### 2. 글로우 애니메이션 설정
- Animator에 "Glow" 트리거 파라미터 추가
- 글로우 효과 애니메이션 클립 생성
- 트리거 시 글로우 효과 재생

#### 3. 콜라이더 설정
- 글로우를 활성화할 영역에 콜라이더 배치
- NoteGlowActivator 컴포넌트 연결

### 🪙 피버 코인 효과 설정하기

#### 1. 코인 효과 프리팹 생성
```
FeverCoinEffect
├── 코인 스프라이트
├── Animator (DropTrigger 애니메이션)
└── FeverCoinEffectManager 컴포넌트
```

#### 2. 애니메이션 설정
- "DropTrigger" 트리거 파라미터 추가
- 코인 드롭 애니메이션 클립 생성
- 효과 완료 후 자동 비활성화

#### 3. 자동 호출
```csharp
// WeaponJudgeSystem에서 자동으로 호출됨
if (type == NoteType.FeverNote && resultSingle == JudgeResult.Wow)
{
    FeverCoinEffectManager.Instance?.Play();
}
```

### 🔮 구슬 효과 설정하기

#### 1. 구슬 오브젝트 설정
```
Orb
├── SpriteRenderer (구슬 스프라이트)
├── Animator (Play 애니메이션)
└── OrbEffectController 컴포넌트
```

#### 2. 애니메이션 설정
- "Play" 트리거 파라미터 추가
- 구슬 회전/빛남 애니메이션 클립 생성
- 테마 변경 시 자동 실행

#### 3. 테마 연동
```csharp
// BackgroundManager에서 자동으로 호출됨
orbEffect?.TriggerEffect();
```

## 🎯 효과 최적화

### ⚡ 성능 고려사항

1. **애니메이션 길이**: 효과 애니메이션은 짧게 유지 (0.2~0.5초)
2. **자동 비활성화**: 효과 완료 후 자동으로 비활성화하여 메모리 절약
3. **트리거 기반**: 필요할 때만 효과 실행하여 불필요한 연산 방지

### 🔄 효과 재사용

1. **NoteGlowActivator**: 여러 노트에서 공통으로 사용 가능
2. **FeverCoinEffectManager**: 피버 모드 중 여러 번 재사용 가능
3. **OrbEffectController**: 테마 변경 시마다 재사용

## 💡 확장 팁

### 🆕 새로운 효과 추가하기

1. **효과 컴포넌트 생성**: MonoBehaviour를 상속받는 효과 클래스 작성
2. **애니메이션 연동**: Animator와 트리거 파라미터 연결
3. **자동 호출 설정**: 적절한 시점에 효과 실행되도록 연동
4. **성능 최적화**: 효과 완료 후 자동 정리 로직 추가

### 🎨 효과 커스터마이징

- **색상 변경**: 테마별로 다른 효과 색상 적용
- **크기 조절**: 효과의 강도에 따라 크기 동적 조절
- **사운드 연동**: 효과와 함께 적절한 효과음 재생
- **입체감**: 파티클 시스템과 연동하여 입체적인 효과 구현
