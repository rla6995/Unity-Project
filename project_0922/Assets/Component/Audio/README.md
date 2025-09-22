# Audio - 오디오 시스템

이 폴더는 게임의 모든 오디오 관련 기능을 담당하는 컴포넌트를 포함합니다.

## 📁 포함된 파일

### AudioManager.cs
- **역할**: BGM과 효과음을 통합 관리하는 중앙 오디오 컨트롤러
- **주요 기능**:
  - 3개 AudioSource 분리 관리
    - **BGM Source**: 배경음악 (루프 재생)
    - **SE Source**: 일반 효과음
    - **Weapon SE Source**: 무기 관련 효과음
    - **Object SE Source**: 오브젝트 파괴 효과음
  - PlayerPrefs 기반 설정 저장
  - BGM/SE 개별 ON/OFF 제어
  - 씬 전환 시에도 유지 (DontDestroyOnLoad)

## 🎵 오디오 시스템 구조

### 🔊 AudioSource 분리 이유
1. **BGM Source**: 배경음악은 루프 재생이 필요
2. **SE Source**: 일반 효과음은 OneShot 재생
3. **Weapon SE Source**: 무기 효과음은 별도 볼륨 제어 가능
4. **Object SE Source**: 오브젝트 효과음은 별도 볼륨 제어 가능

### 🎶 BGM 시스템
- **인덱스 기반 재생**: `PlayBGM(int index)`
- **자동 루프**: 배경음악은 자동으로 반복 재생
- **테마별 BGM**: ThemeManager와 연동하여 자동 변경

### 🔊 효과음 시스템
- **일반 SE**: `PlaySE(int index)`
- **무기 SE**: `PlayWeaponSE(int index)`
- **오브젝트 SE**: `PlayObjectSE(int index)`
- **OneShot 재생**: 효과음은 한 번만 재생

## 🔗 의존성

- **AudioManager** → (독립적, 다른 시스템에서 참조)
- **ThemeManager** → AudioManager (BGM 자동 변경)
- **GameManager** → AudioManager (게임오버 효과음)
- **WeaponJudgeSystem** → AudioManager (판정 효과음)
- **JudgeInputHandler** → AudioManager (무기 효과음)

## 📋 사용법

### 🎵 기본 오디오 재생

```csharp
// BGM 재생 (루프)
AudioManager.Instance.PlayBGM(0); // 타이틀 BGM
AudioManager.Instance.PlayBGM(1); // 게임 BGM
AudioManager.Instance.PlayBGM(2); // 밤 테마 BGM
AudioManager.Instance.PlayBGM(3); // 피버 BGM
AudioManager.Instance.PlayBGM(5); // 각성 테마 BGM
AudioManager.Instance.PlayBGM(6); // 엔딩 BGM

// 효과음 재생
AudioManager.Instance.PlaySE(0); // UI 클릭음
AudioManager.Instance.PlaySE(1); // Wow 판정음
AudioManager.Instance.PlaySE(2); // Nice 판정음
AudioManager.Instance.PlaySE(3); // Bad 판정음
AudioManager.Instance.PlaySE(4); // 게임오버 효과음
AudioManager.Instance.PlaySE(5); // 피버 진입 효과음

// 무기 효과음
AudioManager.Instance.PlayWeaponSE(0); // 흡수 무기
AudioManager.Instance.PlayWeaponSE(1); // 스윙 무기
AudioManager.Instance.PlayWeaponSE(2); // 머지 무기

// 오브젝트 효과음
AudioManager.Instance.PlayObjectSE(0); // 수동 노트
AudioManager.Instance.PlayObjectSE(1); // 무기 노트
AudioManager.Instance.PlayObjectSE(2); // 머지 노트
AudioManager.Instance.PlayObjectSE(3); // 보너스 노트
AudioManager.Instance.PlayObjectSE(4); // 피버 노트
```

### ⚙️ 오디오 설정

```csharp
// BGM ON/OFF
AudioManager.Instance.SetBGMOn(true);   // BGM 켜기
AudioManager.Instance.SetBGMOn(false);  // BGM 끄기

// SE ON/OFF
AudioManager.Instance.SetSEOn(true);    // SE 켜기
AudioManager.Instance.SetSEOn(false);   // SE 끄기

// 현재 상태 확인
bool isBGMOn = AudioManager.Instance.IsBGMOn();
bool isSEOn = AudioManager.Instance.IsSEOn();
```

## 💾 설정 저장

오디오 설정은 자동으로 PlayerPrefs에 저장됩니다:
- **SoundBGM**: BGM ON/OFF 상태 (1=ON, 0=OFF)
- **SoundSE**: SE ON/OFF 상태 (1=ON, 0=OFF)

게임 재시작 시에도 설정이 유지됩니다.

## 🎯 테마 연동

AudioManager는 ThemeManager와 자동으로 연동됩니다:
- **Day 테마**: BGM 인덱스 1
- **Night 테마**: BGM 인덱스 2
- **Burning Night**: BGM 인덱스 5
- **Ending**: BGM 인덱스 6
- **피버 모드**: BGM 인덱스 3

테마 변경 시 자동으로 적절한 BGM이 재생됩니다.
