# Data - 데이터 구조 및 설정 시스템

이 폴더는 게임의 데이터 구조, 설정, 그리고 오브젝트 풀링 시스템을 관리하는 컴포넌트들을 포함합니다.

## 📁 포함된 파일

### 🎯 난이도 설정

#### DifficultySetting.cs
- **역할**: 난이도 설정을 위한 ScriptableObject
- **주요 데이터**:
  - **점수 임계값**: 해당 난이도가 적용되는 최소 점수
  - **테마 타입**: 난이도별 테마 (Day, Night1-4, BurningNight, Ending)
  - **휠 회전 속도**: 난이도별 휠 회전 속도
  - **허용 노트 타입**: 해당 난이도에서 출현 가능한 노트 타입들
  - **구미호 이미지 세트**: 난이도별 구미호 스프라이트 (낮/밤)
  - **노트 스폰 확률**: 각 노트 타입별 출현 확률
  - **판정 이미지**: Wow/Nice/Bad 판정 결과 이미지

#### DifficultyDatabase.cs
- **역할**: 모든 난이도 설정을 관리하는 데이터베이스
- **주요 기능**:
  - 난이도 설정들의 리스트 관리
  - 점수 오름차순으로 정렬된 설정 제공
  - Inspector에서 쉽게 설정 가능

#### ScoreBasedDifficultyManager.cs
- **역할**: 점수에 따른 난이도 자동 조절
- **주요 기능**:
  - 점수 변경 시 자동 난이도 적용
  - 휠 회전 속도 자동 조절
  - 노트 타입 및 확률 동적 변경
  - 테마 자동 전환
  - 피버 상태에서 테마 변경 제한

### 🎲 노트 시스템

#### NoteTypeHandler.cs
- **역할**: 노트의 타입을 정의하는 간단한 컴포넌트
- **노트 타입**: WeaponNote, ManualNote, BonusNote, FeverNote, MergeHead, MergeTail

#### NoteSpawnChance.cs
- **역할**: 노트 스폰 확률을 정의하는 데이터 구조
- **주요 기능**:
  - 노트 타입별 스폰 확률 설정 (0~1 사이)
  - 확률 기반 랜덤 스폰 시스템과 연동

### 🗃️ 오브젝트 풀링

#### MultiObjectPool.cs
- **역할**: 노트 오브젝트의 풀링 및 스폰 확률 관리
- **주요 기능**:
  - 다중 프리팹 풀링 시스템
  - 점수 기반 노트 타입 제한
  - 확률 기반 랜덤 스폰
  - 활성 오브젝트 추적
  - 메모리 효율적인 오브젝트 재사용

## 🔗 의존성

- **DifficultySetting** → (독립적, ScriptableObject)
- **DifficultyDatabase** → DifficultySetting
- **ScoreBasedDifficultyManager** → DifficultyDatabase, ThemeManager, FeverModeManager
- **NoteTypeHandler** → (독립적, 열거형만 정의)
- **NoteSpawnChance** → NoteType
- **MultiObjectPool** → NoteTypeHandler, NoteSpawnChance

## 📋 사용법

### 🎯 난이도 설정 생성하기

#### 1. DifficultySetting 생성
```
Assets → Create → Game → Difficulty Setting
```

#### 2. 기본 설정 예시
```csharp
// 0점 (초급)
scoreThreshold: 0
theme: Day
rotationSpeed: 15f
allowedNoteTypes: [WeaponNote, ManualNote, BonusNote]
noteSpawnChances: [WeaponNote: 0.4, ManualNote: 0.4, BonusNote: 0.2]

// 100점 (중급)
scoreThreshold: 100
theme: Night1
rotationSpeed: 20f
allowedNoteTypes: [WeaponNote, ManualNote, BonusNote, MergeHead]
noteSpawnChances: [WeaponNote: 0.3, ManualNote: 0.3, BonusNote: 0.2, MergeHead: 0.2]

// 500점 (고급)
scoreThreshold: 500
theme: BurningNight
rotationSpeed: 25f
allowedNoteTypes: [WeaponNote, ManualNote, BonusNote, MergeHead, FeverNote]
noteSpawnChances: [WeaponNote: 0.25, ManualNote: 0.25, BonusNote: 0.15, MergeHead: 0.2, FeverNote: 0.15]
```

### 🎲 노트 타입 설정

#### 1. 노트에 타입 할당
```csharp
// Inspector에서 설정
public class NoteTypeHandler : MonoBehaviour
{
    public NoteType noteType = NoteType.WeaponNote;
}
```

#### 2. 스폰 확률 설정
```csharp
// Inspector에서 설정
public List<NoteSpawnChance> noteSpawnChances = new()
{
    new NoteSpawnChance { noteType = NoteType.WeaponNote, spawnChance = 0.4f },
    new NoteSpawnChance { noteType = NoteType.ManualNote, spawnChance = 0.4f },
    new NoteSpawnChance { noteType = NoteType.BonusNote, spawnChance = 0.2f }
};
```

### 🗃️ 오브젝트 풀 사용

#### 1. 풀에서 오브젝트 가져오기
```csharp
// 특정 프리팹에서 가져오기
GameObject note = MultiObjectPool.Instance.Get(prefab);

// 확률 기반 랜덤 가져오기
GameObject randomNote = MultiObjectPool.Instance.GetRandomByChance();

// 특정 타입에서 랜덤 가져오기
List<NoteType> types = new() { NoteType.MergeHead, NoteType.MergeTail };
GameObject mergeNote = MultiObjectPool.Instance.GetRandomFromTypes(types);
```

#### 2. 풀로 오브젝트 반환
```csharp
// 사용 완료된 오브젝트를 풀로 반환
MultiObjectPool.Instance.Return(note);
```

## 💡 최적화 팁

1. **ScriptableObject 활용**: DifficultySetting을 ScriptableObject로 만들어 에디터에서 쉽게 설정
2. **확률 정규화**: NoteSpawnChance의 확률 합계가 1이 되도록 설정하면 더 예측 가능한 스폰
3. **오브젝트 풀링**: 자주 생성/파괴되는 노트들을 풀링하여 메모리 할당 최소화
4. **점수 기반 난이도**: 점수에 따라 자동으로 난이도가 조절되어 플레이어 경험 향상

## 🔄 데이터 흐름

1. **게임 시작**: DifficultyDatabase에서 초기 난이도 설정 로드
2. **점수 증가**: ScoreBasedDifficultyManager가 점수에 맞는 난이도 자동 적용
3. **테마 변경**: ThemeManager가 새로운 테마 적용
4. **노트 스폰**: MultiObjectPool이 현재 난이도의 설정에 따라 노트 스폰
5. **실시간 조절**: 휠 속도, 노트 타입, 스폰 확률이 실시간으로 조절

## 🗑️ 제거된 파일들

다음 파일들은 사용되지 않거나 불필요하여 제거되었습니다:
- **JointNoteComponent.cs**: `IsHead` 플래그만 가지고 있지만 실제로는 사용되지 않음

## 🔧 주요 변경사항

1. **코드 단순화**: 불필요한 중간 계층 제거로 의존성 감소
2. **일관성 개선**: 노트 타입 구분을 `NoteTypeHandler`로 통일
3. **메모리 효율성**: 불필요한 컴포넌트 제거로 메모리 사용량 감소
