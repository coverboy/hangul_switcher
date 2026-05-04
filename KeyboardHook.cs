using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HangulSwitcher;

internal sealed class KeyboardHook : IDisposable
{
    public event Action? ShiftSpacePressed;

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN     = 0x0100;
    private const int WM_SYSKEYDOWN  = 0x0104;
    private const int VK_SPACE       = 0x20;
    private const int VK_LSHIFT      = 0xA0;
    private const int VK_RSHIFT      = 0xA1;
    private const int SHIFT_DOWN_MASK = 0x8000;

    private IntPtr _hookId = IntPtr.Zero;
    private NativeMethods.LowLevelKeyboardProc? _proc; // GC 방지용 필드 보관

    public void Install()
    {
        _proc = OnKeyboardEvent;
        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule!;
        _hookId = NativeMethods.SetWindowsHookEx(
            WH_KEYBOARD_LL, _proc,
            NativeMethods.GetModuleHandle(module.ModuleName), 0);

        if (_hookId == IntPtr.Zero)
            throw new InvalidOperationException("키보드 훅 설치 실패: " + Marshal.GetLastWin32Error());
    }

    public void Dispose()
    {
        if (_hookId == IntPtr.Zero) return;
        NativeMethods.UnhookWindowsHookEx(_hookId);
        _hookId = IntPtr.Zero;
    }

    private IntPtr OnKeyboardEvent(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
        {
            int vk = Marshal.ReadInt32(lParam);
            if (vk == VK_SPACE && IsShiftDown())
            {
                ShiftSpacePressed?.Invoke();
                return 1; // 이벤트 소비 → 공백 입력 차단
            }
        }
        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private static bool IsShiftDown() =>
        (NativeMethods.GetAsyncKeyState(VK_LSHIFT) & SHIFT_DOWN_MASK) != 0 ||
        (NativeMethods.GetAsyncKeyState(VK_RSHIFT) & SHIFT_DOWN_MASK) != 0;
}
