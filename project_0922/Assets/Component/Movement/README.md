# Movement - 이동 및 물리 시스템

이 폴더는 게임의 모든 이동, 회전, 그리고 물리적 움직임을 담당하는 컴포넌트들을 포함합니다.

## 📁 포함된 파일

### 🎡 휠 시스템

#### SplineRotator.cs
- **역할**: 휠의 회전을 담당하는 핵심 컴포넌트
- **주요 기능**:
  - 반시계 방향 회전 (Z축 기준)
  - 외부에서 각도 강제 설정 가능
  - DefaultExecutionOrder(-200)로 우선순위 보장
  - 회전 속도 동적 조절 (난이도별)

#### OrbitWalkingMonster.cs
- **역할**: 휠 주변을 도는 노트의 이동
- **주요 기능**:
  - 원형 궤도 이동
  - 슬롯 인덱스 기반 위치 추적
  - 이동 일시정지/재개 (머지 시스템용)
  - 휠 회전 속도와 동기화

### 🚄 직선 레일 시스템

#### StraightRailScroller.cs
- **역할**: 피버 모드의 직선 레일 스크롤링
- **주요 기능**:
  - 무한 스크롤 레일 시스템
  - 화면 밖으로 나간 레일 재배치
  - 레일 너비 자동 계산
  - 스크롤 속도 동적 조절

#### FeverBonusNoteMover.cs
- **역할**: 피버 보너스 노트의 이동
- **주요 기능**:
  - 오른쪽으로 일정 속도 이동
  - 화면 밖으로 나가면 자동 풀 반환
  - 이동 속도 외부에서 설정 가능
  - 피버 레일과 동기화

## 🔄 이동 시스템 구조

### 🎡 휠 기반 이동

#### 회전 시스템
```
SplineRotator (회전 제어)
    ↓
휠 오브젝트 회전
    ↓
OrbitWalkingMonster (노트 이동)
    ↓
32개 슬롯 기반 위치 계산
```

#### 슬롯 시스템
- **총 슬롯**: 32개
- **각도 계산**: 360° ÷ 32 = 11.25° per slot
- **위치 정렬**: 12시 방향 기준으로 슬롯 정렬
- **스폰 위치**: 12시 중앙에 고정 스폰

### 🚄 직선 레일 시스템

#### 스크롤 메커니즘
```
레일 A, B (이중 레일)
    ↓
좌에서 우로 스크롤
    ↓
화면 밖 나가면 왼쪽으로 재배치
    ↓
무한 스크롤 효과
```

#### 노트 이동
- **스폰 위치**: 레일 왼쪽 끝
- **이동 방향**: 오른쪽으로 일정 속도
- **제거 조건**: 화면 오른쪽 끝 도달 시

## 🔗 의존성

- **SplineRotator** → (독립적, 다른 시스템에서 참조)
- **OrbitWalkingMonster** → SplineRotator, Transform
- **StraightRailScroller** → Transform, SpriteRenderer
- **FeverBonusNoteMover** → MultiObjectPool

## 📋 사용법

### 🎡 휠 회전 설정

#### 1. 기본 회전 설정
```csharp
// Inspector에서 설정
public class SplineRotator : MonoBehaviour
{
    [Tooltip("반시계(+) z축 회전 속도 (deg/sec)")]
    public float rotationSpeed = 20f;
}
```

#### 2. 동적 속도 조절
```csharp
// 난이도에 따른 회전 속도 변경
var rotator = FindObjectOfType<SplineRotator>();
if (rotator != null)
{
    rotator.rotationSpeed = newSpeed;
}
```

#### 3. 각도 강제 설정
```csharp
// 특정 각도로 강제 이동 (예: 피버 종료 시)
rotator.SetAngle(0f); // 0도로 스냅
```

### 🎯 궤도 이동 설정

#### 1. 노트 초기화
```csharp
// ObjectSpawner에서 자동 호출
var orbit = obj.GetComponent<OrbitWalkingMonster>();
if (orbit != null)
{
    orbit.Initialize(wheelCenter, wheelRadius, rotator);
    orbit.SetSlotIndex(slotIndex, occupiedSlots);
}
```

#### 2. 이동 제어
```csharp
// 머지 시스템에서 이동 일시정지
orbit.PauseMovement();

// 머지 완료 후 이동 재개
orbit.ResumeMovement();
```

### 🚄 직선 레일 설정

#### 1. 레일 배치
```csharp
// Inspector에서 연결
public Transform railA;  // 첫 번째 레일
public Transform railB;  // 두 번째 레일
```

#### 2. 스크롤 속도 조절
```csharp
// 피버 모드에 따른 속도 조절
public float scrollSpeed = 5f; // 기본 속도
```

#### 3. 레일 너비 자동 계산
```csharp
// 스프라이트 기반 자동 너비 계산
public void RecalculateRailWidthAndPosition()
{
    SpriteRenderer sr = railA.GetComponent<SpriteRenderer>();
    if (sr != null)
    {
        railWidth = sr.bounds.size.x;
        // 레일 위치 자동 조정
    }
}
```

### 🎲 피버 노트 이동

#### 1. 이동 속도 설정
```csharp
// 피버 레일과 동기화
var mover = note.GetComponent<FeverBonusNoteMover>();
if (mover != null)
{
    mover.SetSpeed(railScroller.scrollSpeed);
}
```

#### 2. 자동 풀 반환
```csharp
// 화면 밖으로 나가면 자동 반환
if (transform.position.x > 11f)
{
    MultiObjectPool.Instance?.Return(gameObject);
}
```

## ⚡ 성능 최적화

### 🎯 실행 순서 최적화

1. **SplineRotator**: DefaultExecutionOrder(-200)로 가장 먼저 실행
2. **ObjectSpawner**: DefaultExecutionOrder(-100)로 회전 후 스폰
3. **OrbitWalkingMonster**: 일반 Update로 이동 처리

### 🔄 메모리 효율성

1. **Transform 캐싱**: 자주 사용하는 Transform을 캐시하여 GetComponent 호출 최소화
2. **불필요한 계산 방지**: 이동이 일시정지된 상태에서는 계산 생략
3. **자동 정리**: 화면 밖으로 나간 오브젝트는 즉시 풀로 반환

## 💡 확장 팁

### 🆕 새로운 이동 패턴 추가

1. **이동 컴포넌트 생성**: MonoBehaviour를 상속받는 이동 클래스 작성
2. **인터페이스 구현**: 공통 이동 인터페이스 구현으로 일관성 유지
3. **풀링 연동**: MultiObjectPool과 연동하여 메모리 효율성 확보
4. **테마 연동**: 테마 변경 시 이동 패턴도 함께 변경

### 🎨 이동 패턴 커스터마이징

- **곡선 이동**: 베지어 곡선을 이용한 부드러운 이동
- **점프 이동**: 중력과 점프를 이용한 입체적 이동
- **추적 이동**: 플레이어를 향한 추적 이동
- **무작위 이동**: 확률 기반의 예측 불가능한 이동
