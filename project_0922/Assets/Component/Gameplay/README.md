# Gameplay - 게임플레이 시스템

이 폴더는 게임의 핵심 플레이 메커니즘과 관련된 컴포넌트들을 포함합니다.

## 📁 포함된 파일

### 🎯 핵심 게임플레이

#### ObjectSpawner.cs
- **역할**: 휠 주변에 노트를 자동으로 스폰하는 시스템
- **주요 기능**:
  - 각도 기반 스폰 스케줄링
  - 32개 슬롯 기반 위치 계산
  - 머지 노트 시스템 (머리 + 꼬리)
  - 피버 모드 중 스폰 일시정지
  - 테마별 노트 스프라이트 적용

#### WeaponJudgeSystem.cs
- **역할**: 무기 입력에 따른 노트 판정 시스템
- **주요 기능**:
  - 4가지 입력 타입 지원 (Absorb, Swing, MergeHead, MergeTail)
  - 노트 타입별 유효성 검사
  - 타이밍 판정 (Wow/Nice/Bad)
  - 머지 꼬리 다중 판정
  - 피버 노트 전용 판정

#### JudgeInputHandler.cs
- **역할**: 키보드(Z/X) 및 UI 버튼 입력을 처리
- **주요 기능**:
  - 단독 입력 (흡수/스윙)
  - 합동 입력 (두 버튼 동시 누름)
  - 머지 모드 자동 전환
  - 꼬리 노트 주기적 판정 (0.05초 간격)

### 🎮 노트 시스템

#### NoteTypeHandler.cs
- **역할**: 노트의 타입을 정의하는 간단한 컴포넌트
- **노트 타입**: WeaponNote, ManualNote, BonusNote, FeverNote, MergeHead, MergeTail

### 🔄 머지 시스템

#### MergeHeadController.cs
- **역할**: 머지 노트의 머리 부분 제어
- **주요 기능**:
  - 스턴 상태 관리
  - 꼬리 노트 자동 감지
  - 꼬리 소멸 시 자동 풀 반환

#### MergeTailController.cs
- **역할**: 머지 노트의 꼬리 부분 제어
- **주요 기능**:
  - Wow 존 충돌 시 자동 제거 ("WowZone" 태그 사용)
  - 부모 머리에서 분리
  - 풀 자동 반환

### 🎯 판정 시스템

#### TimingJudgeSystem.cs
- **역할**: 타이밍 판정 시스템
- **주요 기능**: 
  - Wow/Nice/Bad 판정 결과 반환
  - 판정 중심점과 콜라이더 직접 관리
  - null 체크를 통한 안전한 판정

#### NoteHitDetector.cs
- **역할**: 노트 히트 감지
- **주요 기능**:
  - 가장 가까운 노트 검색
  - Nice 영역 내 모든 노트 검색

### 🎭 플레이어 상호작용

#### PlayerCollider.cs
- **역할**: 플레이어 충돌 처리
- **주요 기능**:
  - 노트 타입별 충돌 처리
  - 보너스 노트 자동 수집
  - 피버 노트 무시
  - 일반 노트 충돌 시 게임오버

## 🔗 의존성

- **ObjectSpawner** → SplineRotator, MultiObjectPool, FeverModeManager
- **WeaponJudgeSystem** → TimingJudgeSystem, NoteHitDetector, GameManager
- **JudgeInputHandler** → WeaponJudgeSystem, MultiObjectPool
- **MergeHeadController** → OrbitWalkingMonster, MultiObjectPool
- **MergeTailController** → MultiObjectPool (태그 기반 판정)
- **TimingJudgeSystem** → (독립적, 직접 콜라이더 관리)

## 📋 사용법

이 컴포넌트들은 게임의 핵심 플레이 메커니즘을 담당합니다:
1. **ObjectSpawner**는 게임 시작 시 자동으로 활성화됩니다
2. **WeaponJudgeSystem**은 입력 이벤트에 의해 호출됩니다
3. **JudgeInputHandler**는 UI 버튼과 키보드 입력을 모두 처리합니다
4. 머지 시스템은 두 버튼을 동시에 누를 때 자동으로 활성화됩니다

## 🗑️ 제거된 파일들

다음 파일들은 사용되지 않거나 불필요하여 제거되었습니다:
- **JointNoteComponent.cs**: `IsHead` 플래그만 가지고 있지만 실제로는 사용되지 않음
- **WeaponHitZone.cs**: `WeaponZoneType` 열거형만 정의하고 실제 무기 시스템에 활용되지 않음
- **TimingJudgeZone.cs**: 단순한 데이터 컨테이너로 `TimingJudgeSystem`에 통합됨

## 🔧 주요 변경사항

1. **TimingJudgeSystem**: `TimingJudgeZone` 의존성 제거, 직접 콜라이더 관리
2. **MergeTailController**: `WeaponHitZone` 대신 "WowZone" 태그 사용
3. **일관성 개선**: 불필요한 중간 계층 제거로 코드 단순화
