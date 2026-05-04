namespace HangulSwitcher;

internal sealed class ImeStateMonitor : IDisposable
{
    public event Action<bool>? StateChanged; // true = 한글 모드, false = 영문 모드

    private const int POLL_INTERVAL_MS = 100;

    private readonly System.Windows.Forms.Timer _timer;
    private bool? _lastState;

    public ImeStateMonitor()
    {
        _timer = new System.Windows.Forms.Timer { Interval = POLL_INTERVAL_MS };
        _timer.Tick += (_, _) => Poll();
    }

    public void Start() => _timer.Start();

    public void Poll()
    {
        var state = ReadIsHangulMode();
        if (state == _lastState) return;
        _lastState = state;
        StateChanged?.Invoke(state);
    }

    private static bool ReadIsHangulMode()
    {
        // foreground window 가 IME-aware 하지 않은 경우(WSL/Zed 등)에도
        // 시스템 IME 상태가 일관되게 반영되는 explorer shell window 기준으로 읽음.
        var hwnd = NativeMethods.GetShellWindow();
        if (hwnd == IntPtr.Zero) return false;

        var threadId = NativeMethods.GetWindowThreadProcessId(hwnd, out _);
        var layout = NativeMethods.GetKeyboardLayout(threadId);
        var langId = (ushort)(layout.ToInt64() & 0xFFFF);
        if (langId != NativeMethods.LANG_KOREAN) return false;

        var imeWnd = NativeMethods.ImmGetDefaultIMEWnd(hwnd);
        if (imeWnd == IntPtr.Zero) return false;

        // IMC_GETOPENSTATUS 는 IME 활성/비활성만 반환해서 한/영 토글을 못 잡음.
        // IMC_GETCONVERSIONMODE 로 conversion flags 받아서 NATIVE bit 검사.
        var convMode = NativeMethods.SendMessage(
            imeWnd,
            NativeMethods.WM_IME_CONTROL,
            (IntPtr)NativeMethods.IMC_GETCONVERSIONMODE,
            IntPtr.Zero);
        return (convMode.ToInt64() & NativeMethods.IME_CMODE_NATIVE) != 0;
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}
