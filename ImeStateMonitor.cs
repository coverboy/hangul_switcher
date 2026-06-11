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
        // 한/영(conversion mode) 상태는 IME 컨텍스트마다 독립적이라,
        // 실제로 사용자가 타이핑하는 foreground window 기준으로 읽어야 한다.
        // shell window(바탕화면) 기준으로 읽으면 사용자가 한/영을 토글해도
        // 그 변화가 반영 안 되는 고정값이 나와 트레이 아이콘이 안 바뀐다.
        var hwnd = NativeMethods.GetForegroundWindow();
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
