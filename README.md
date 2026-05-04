# HangulSwitcher — Shift+Space 로 한/영 전환 (Korean / English IME toggle for Windows)

Windows 10/11 에서 **Shift+Space** 한 번으로 한국어(한글) ↔ 영어(English) 입력기를 전환하는 system tray 유틸리티. 키보드 종류 설정·재부팅 없이, 블루투스·노트북·외장 키보드 어디서든 동일하게 동작합니다. Korean keyboard hangul english IME toggle for Windows tray utility — AutoHotkey 대안.

## 만든 이유

Windows 의 한/영 전환은 본래 키보드 종류가 **"PC/AT 101키 호환 키보드 (종류 3)"** 으로 잡혀 있을 때 우측 Alt 키로 동작합니다. 그런데 이 키보드 종류 설정에는 실사용에서 두 가지 번거로움이 있습니다.

1. **종류 변경 시 재부팅 필수** — 장치 관리자에서 키보드 종류를 3 으로 바꾸면 즉시 반영되지 않고 재부팅이 필요해 작업 흐름이 끊깁니다.
2. **재부팅이 설정을 되돌리는 모순** — 블루투스 키보드, 노트북 내장 키보드, 일부 USB-HID 키보드는 부팅 시 OS 가 본래 드라이버(주로 종류 4 또는 영문 101키)를 자동 매핑하면서 사용자가 지정한 종류 3 을 덮어씁니다. 결국 종류 3 적용을 위해 재부팅을 했는데, 그 재부팅이 종류 3 을 다시 풀어버리는 상황이 반복됩니다.

본 유틸은 키보드 종류 설정과 무관하게 OS 레벨 글로벌 키 후크로 **Shift+Space** 를 가로채 IME 토글 메시지를 직접 송신합니다. 그래서:

- 키보드 종류를 굳이 변경할 필요 없음 (기본값 그대로 OK)
- 재부팅 없이 실행 즉시 적용
- 블루투스·노트북·USB·외장 키보드 어디서 입력하든 동일하게 동작
- AutoHotkey 같은 별도 매크로 엔진 불필요

## 동작

- 백그라운드 상주 → system tray 아이콘으로만 노출
- 전역 단축키 **Shift+Space** → 현재 포커스된 입력 필드의 IME 한/영 토글
- tray 아이콘 우클릭 메뉴:
  - **Windows 시작 시 실행** — 체크 한 번으로 자동 시작 등록 (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run` 레지스트리)
  - **정보(About)** — 프로젝트 URL · 이메일 · 라이선스 표시
  - **종료**

## 요구 사항

- Windows 10 / Windows 11
- .NET 8 Desktop Runtime (Standalone 빌드는 불필요)

## 다운로드

GitHub Releases 에서 두 가지 배포판 zip 을 받을 수 있습니다.

| 배포판 | zip 크기 | .NET 8 Desktop Runtime |
|--------|---------|------------------------|
| `HangulSwitcher-Standalone.zip` | ~63MB  | 불필요 (exe 안에 포함) |
| `HangulSwitcher-NeedsDotNet.zip` | ~80KB  | 별도 설치 필요 |

런타임 없는 PC 에 처음 배포하거나 대상 환경을 모르면 **Standalone**, 본인/팀 PC 에 .NET 8 Desktop Runtime 이 깔려 있으면 **NeedsDotNet** 권장.

> 런타임 설치: `winget install Microsoft.DotNet.DesktopRuntime.8`

## 실행

zip 풀고 `HangulSwitcher.exe` 더블클릭 → tray 에 아이콘 등장. **자동 시작은 tray 아이콘 우클릭 → "Windows 시작 시 실행" 체크** 하면 됩니다 (별도 시작 프로그램 폴더 등록 불필요).

## 빌드

### Windows 로컬

```powershell
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

산출물: `publish/HangulSwitcher.exe`

### Linux/macOS 에서 빌드

Windows-only 프로젝트라 non-Windows OS 에서는 직접 빌드 불가 (`NETSDK1100`).

→ GitHub Actions `windows-latest` runner 로 우회. `gh workflow run build-windows` 하면 빌드 + Releases 자동 등록.

## 구조

`Program.cs` 단일 파일. 글로벌 키 후킹 + tray 아이콘 + IME 메시지 송신만 담당.

## License

완전 Free. 마음대로 가져다 쓰세요.

## Keywords

hangul, korean, korean keyboard, korean ime, hangul english toggle, korean english switcher, 한영 전환, 한/영 키, 한영키, 한글 영문, 한글 영어 전환, windows ime utility, shift space korean, shift+space hangul, system tray utility, autohotkey alternative, windows 10, windows 11, IME toggle, keyboard layout switcher
