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
        var hwnd = NativeMethods.GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return false;

        var threadId = NativeMethods.GetWindowThreadProcessId(hwnd, out _);
        var layout = NativeMethods.GetKeyboardLayout(threadId);
        var langId = (ushort)(layout.ToInt64() & 0xFFFF);
        if (langId != NativeMethods.LANG_KOREAN) return false;

        var imeWnd = NativeMethods.ImmGetDefaultIMEWnd(hwnd);
        if (imeWnd == IntPtr.Zero) return false;

        var openStatus = NativeMethods.SendMessage(
            imeWnd,
            NativeMethods.WM_IME_CONTROL,
            (IntPtr)NativeMethods.IMC_GETOPENSTATUS,
            IntPtr.Zero);
        return openStatus.ToInt64() != 0;
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}
