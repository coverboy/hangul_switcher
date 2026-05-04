using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace HangulSwitcher;

internal static class Program
{
    private static readonly Mutex SingleInstance = new(true, "{HangulSwitcher-A1B2C3}");

    [STAThread]
    private static void Main()
    {
        if (!SingleInstance.WaitOne(TimeSpan.Zero, true))
        {
            MessageBox.Show("이미 실행 중입니다.", "HangulSwitcher");
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new TrayContext());
        SingleInstance.ReleaseMutex();
    }
}

public sealed class TrayContext : ApplicationContext
{
    private const string AutoStartKey = "HangulSwitcher";
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    private readonly NotifyIcon _tray;
    private readonly KeyboardHook _hook;

    public TrayContext()
    {
        var startupItem = new ToolStripMenuItem("Windows 시작 시 실행")
        {
            Checked = IsAutoStartEnabled(),
            CheckOnClick = true
        };
        startupItem.CheckedChanged += (_, _) => SetAutoStart(startupItem.Checked);

        var aboutItem = new ToolStripMenuItem("정보(About)");
        aboutItem.Click += (_, _) => ShowAbout();

        var menu = new ContextMenuStrip();
        menu.Items.Add(startupItem);
        menu.Items.Add(aboutItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("종료", null, (_, _) => ExitApp());

        _tray = new NotifyIcon
        {
            Icon = LoadAppIcon(),
            Text = "한/영 전환 (Shift+Space)",
            ContextMenuStrip = menu,
            Visible = true
        };

        _hook = new KeyboardHook();
        _hook.ShiftSpacePressed += SendHangulToggle;
        _hook.Start();
    }

    private static void ShowAbout()
    {
        const string projectUrl = "https://github.com/coverboy/hangul_switcher";
        const string contactEmail = "coverboy@1234.co.kr";

        using var form = new Form
        {
            Text = "HangulSwitcher 정보",
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterScreen,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(460, 220)
        };

        var title = new Label
        {
            Text = "Windows 에서 Shift+Space 로 한/영 전환하는 system tray 유틸리티.",
            AutoSize = false,
            Location = new Point(20, 20),
            Size = new Size(420, 50)
        };

        var urlLabel = new Label { Text = "프로젝트:", Location = new Point(20, 85), AutoSize = true };
        var urlLink = new LinkLabel
        {
            Text = projectUrl,
            Location = new Point(95, 85),
            AutoSize = true
        };
        urlLink.LinkClicked += (_, _) => Process.Start(new ProcessStartInfo
        {
            FileName = projectUrl,
            UseShellExecute = true
        });

        var emailLabel = new Label { Text = "이메일:", Location = new Point(20, 115), AutoSize = true };
        var emailLink = new LinkLabel
        {
            Text = contactEmail,
            Location = new Point(95, 115),
            AutoSize = true
        };
        emailLink.LinkClicked += (_, _) => Process.Start(new ProcessStartInfo
        {
            FileName = $"mailto:{contactEmail}",
            UseShellExecute = true
        });

        var license = new Label
        {
            Text = "저작권: 완전 Free. 마음대로 가져다 쓰세요.",
            Location = new Point(20, 150),
            AutoSize = true
        };

        var ok = new Button { Text = "확인", Location = new Point(370, 180), Size = new Size(70, 28) };
        ok.Click += (_, _) => form.Close();
        form.AcceptButton = ok;

        form.Controls.AddRange(new Control[] { title, urlLabel, urlLink, emailLabel, emailLink, license, ok });
        form.ShowDialog();
    }

    private static void SendHangulToggle()
    {
        // VK_HANGUL = 0x15. IME 가 이 키를 한/영 토글로 처리.
        const byte VK_HANGUL = 0x15;
        const uint KEYEVENTF_KEYUP = 0x0002;
        NativeMethods.keybd_event(VK_HANGUL, 0, 0, UIntPtr.Zero);
        NativeMethods.keybd_event(VK_HANGUL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    private static bool IsAutoStartEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
        return key?.GetValue(AutoStartKey) != null;
    }

    private static void SetAutoStart(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)!;
        if (enable)
            key.SetValue(AutoStartKey, $"\"{Application.ExecutablePath}\"");
        else
            key.DeleteValue(AutoStartKey, throwOnMissingValue: false);
    }

    private static Icon LoadAppIcon()
    {
        // 1순위: exe 에 박혀있는 ApplicationIcon 추출 (단일 파일 배포에서도 동작)
        try
        {
            var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (icon != null) return icon;
        }
        catch { /* 폴백 */ }
        return SystemIcons.Application;
    }

    private void ExitApp()
    {
        _hook.Stop();
        _tray.Visible = false;
        _tray.Dispose();
        Application.Exit();
    }
}

public sealed class KeyboardHook
{
    public event Action? ShiftSpacePressed;

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN     = 0x0100;
    private const int WM_SYSKEYDOWN  = 0x0104;
    private const int VK_SPACE       = 0x20;
    private const int VK_LSHIFT      = 0xA0;
    private const int VK_RSHIFT      = 0xA1;

    private IntPtr _hookId = IntPtr.Zero;
    private NativeMethods.LowLevelKeyboardProc? _proc; // GC 방지용 필드 보관

    public void Start()
    {
        _proc = HookCallback;
        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule!;
        _hookId = NativeMethods.SetWindowsHookEx(
            WH_KEYBOARD_LL, _proc,
            NativeMethods.GetModuleHandle(module.ModuleName), 0);

        if (_hookId == IntPtr.Zero)
            throw new InvalidOperationException("키보드 훅 설치 실패: " + Marshal.GetLastWin32Error());
    }

    public void Stop()
    {
        if (_hookId != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
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
        (NativeMethods.GetAsyncKeyState(VK_LSHIFT) & 0x8000) != 0 ||
        (NativeMethods.GetAsyncKeyState(VK_RSHIFT) & 0x8000) != 0;
}

internal static class NativeMethods
{
    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
}
