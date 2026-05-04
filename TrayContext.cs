using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace HangulSwitcher;

internal sealed class TrayContext : ApplicationContext
{
    private const byte VK_HANGUL       = 0x15;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    private readonly NotifyIcon _tray;
    private readonly KeyboardHook _hook;

    public TrayContext()
    {
        _hook = new KeyboardHook();
        _hook.ShiftSpacePressed += SendHangulToggle;
        _hook.Install();

        _tray = new NotifyIcon
        {
            Icon = LoadAppIcon(),
            Text = $"{AppInfo.AppName} (Shift+Space)",
            ContextMenuStrip = BuildContextMenu(),
            Visible = true
        };
    }

    private ContextMenuStrip BuildContextMenu()
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
        return menu;
    }

    private static void ShowAbout()
    {
        using var form = new AboutForm();
        form.ShowDialog();
    }

    private static void SendHangulToggle()
    {
        // VK_HANGUL = 0x15. IME 가 이 키를 한/영 토글로 처리.
        NativeMethods.keybd_event(VK_HANGUL, 0, 0, UIntPtr.Zero);
        NativeMethods.keybd_event(VK_HANGUL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    private static bool IsAutoStartEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(AppInfo.RunKeyPath);
        return key?.GetValue(AppInfo.AutoStartRegistryName) != null;
    }

    private static void SetAutoStart(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(AppInfo.RunKeyPath, writable: true)!;
        if (enable)
            key.SetValue(AppInfo.AutoStartRegistryName, $"\"{Application.ExecutablePath}\"");
        else
            key.DeleteValue(AppInfo.AutoStartRegistryName, throwOnMissingValue: false);
    }

    private static Icon LoadAppIcon()
    {
        // 단일 파일 self-contained 배포에서도 동작하도록 exe 내장 아이콘 추출.
        // ExtractAssociatedIcon 은 손상된 exe / 권한 문제에서 IO 예외를 던질 수 있어 폴백 보장.
        try
        {
            var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (icon != null) return icon;
        }
        catch (System.IO.FileNotFoundException) { /* 폴백 */ }
        catch (System.ComponentModel.Win32Exception) { /* 폴백 */ }
        return SystemIcons.Application;
    }

    private void ExitApp()
    {
        Dispose();
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hook.Dispose();
            _tray.Visible = false;
            _tray.Dispose();
        }
        base.Dispose(disposing);
    }
}
