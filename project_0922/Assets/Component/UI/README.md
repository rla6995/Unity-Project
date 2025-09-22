# UI - 사용자 인터페이스 시스템

이 폴더는 게임의 모든 사용자 인터페이스와 관련된 컴포넌트들을 포함합니다.

## 📁 포함된 파일

### 🎮 씬 관리

#### TitleSceneController.cs
- **역할**: 타이틀 씬의 입력 처리 및 씬 전환
- **주요 기능**:
  - 터치/마우스 입력 처리
  - Tap to Start 텍스트 깜빡임
  - 옵션 메뉴 및 종료 확인
  - ESC 키 지원
  - 게임 씬으로 전환

#### PauseManager.cs
- **역할**: 게임 일시정지 및 메뉴 시스템
- **주요 기능**:
  - ESC 키 기반 일시정지
  - 홈/옵션/종료 메뉴
  - 게임오버 상태에서의 메뉴 처리
  - 패널 간 전환 관리

### 🎯 무기 UI 시스템

#### WeaponSwapUI.cs
- **역할**: 무기 스왑 UI 및 버튼 이미지 관리
- **주요 기능**:
  - 좌우 무기 위치 스왑
  - 테마별 버튼 이미지 변경
  - 피버 모드 전용 스프라이트
  - 옵션 패널 아이콘 동기화

#### GameSceneWeaponUISetter.cs
- **역할**: 게임 씬의 무기 UI 설정
- **주요 기능**:
  - 게임 씬 버튼 상태 적용
  - 피버 모드 버튼 스프라이트 적용
  - 테마별 버튼 이미지 변경

#### WeaponSwapManager.cs
- **역할**: 무기 스왑 상태 관리
- **주요 기능**:
  - 좌우 무기 위치 토글
  - 씬 간 상태 유지 (DontDestroyOnLoad)
  - 싱글톤 패턴으로 전역 상태 관리

### ⚙️ 옵션 및 설정

#### OptionUIController.cs
- **역할**: 옵션 UI 제어
- **주요 기능**:
  - 옵션 메뉴 열기/닫기
  - ESC 키로 옵션 메뉴 제어
  - 사운드 효과 재생

#### OptionSoundButton.cs
- **역할**: 사운드 옵션 버튼
- **주요 기능**:
  - BGM/SE 개별 ON/OFF
  - PlayerPrefs 기반 설정 저장
  - 아이콘 상태 자동 변경
  - AudioManager와 연동

## 🔗 의존성

- **TitleSceneController** → AudioManager, SceneManager
- **PauseManager** → WeaponSwapUI, GameManager
- **WeaponSwapUI** → WeaponSwapManager, BackgroundManager
- **GameSceneWeaponUISetter** → WeaponSwapManager, BackgroundManager
- **WeaponSwapManager** → (독립적, 다른 시스템에서 참조)
- **OptionUIController** → AudioManager
- **OptionSoundButton** → AudioManager

## 📋 사용법

### 🎮 기본 UI 사용법

1. **타이틀 씬**: 터치/클릭으로 게임 씬 진입
2. **게임 중**: ESC 키로 일시정지 메뉴
3. **무기 스왑**: 옵션 메뉴에서 무기 위치 변경

### 🎯 무기 UI 시스템

1. **WeaponSwapManager**는 씬 전환 시에도 상태를 유지합니다
2. **WeaponSwapUI**는 옵션 패널에서 무기 위치를 변경합니다
3. **GameSceneWeaponUISetter**는 게임 씬의 버튼 상태를 동기화합니다

### ⚙️ 옵션 시스템

1. **OptionSoundButton**은 BGM과 SE를 개별적으로 제어합니다
2. 설정은 PlayerPrefs에 자동 저장되어 게임 재시작 시에도 유지됩니다
3. **OptionUIController**는 ESC 키로 옵션 메뉴를 제어합니다

## 🎨 UI 테마 시스템

모든 UI 컴포넌트는 테마 시스템과 연동되어 있습니다:
- 낮/밤 테마에 따라 버튼 이미지 자동 변경
- 피버 모드 시 전용 스프라이트 적용
- 테마 변경 시 자동으로 UI 상태 업데이트
