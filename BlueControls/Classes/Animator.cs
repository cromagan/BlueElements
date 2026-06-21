// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Runtime.InteropServices;
using System.Threading;

namespace BlueControls.Classes;

/// <summary>
/// Zentrale Animations-Engine für Controls (Notification, QuickNote, ...).
/// Läuft auf einem eigenen Hintergrund-Thread und setzt Fensterposition und
/// -transparenz direkt via Win32 (SetWindowPos, SetLayeredWindowAttributes),
/// komplett am UI-Thread vorbei. Dadurch bleiben Animationen auch bei hoher
/// UI-Auslastung smooth, weil sie nicht durch BeginInvoke blockiert werden.
/// </summary>
public static class Animator {

    #region Fields

    private const int GwlExStyle = -20;
    private const int LwaAlpha = 0x2;
    private const int SwpNoActivate = 0x0010;
    private const int SwpNoOwnerZOrder = 0x0200;
    private const int SwpNoSize = 0x0001;
    private const int SwpNoZOrder = 0x0004;
    private const int SwpPosFlags = SwpNoSize | SwpNoZOrder | SwpNoActivate | SwpNoOwnerZOrder;
    private const int TargetFps = 125;
    private const int WsExLayered = 0x00080000;
    private static readonly Dictionary<IntPtr, Entry> _entries = [];
    private static readonly HashSet<IntPtr> _layeredEnsured = [];
    private static readonly object _lock = new();

    // Hält die Referenz auf den Animations-Thread. Die CLR rootet zwar
    // laufende Threads selbst, aber ohne Field wäre der Verweis im Debugger
    // schwer zu finden. Initialisiert über StartAnimationThread(), damit es
    // keinen expliziten statischen Konstruktor gibt (S3963).
    private static readonly Thread _thread = StartAnimationThread();

    private static readonly AutoResetEvent _wakeup = new(false);

    #endregion

    #region Properties

    /// <summary>
    /// True, wenn der Animations-Thread aktiv läuft. Diagnostisch, z.B. für
    /// Asserts, die sicherstellen wollen, dass die Engine initialisiert ist.
    /// </summary>
    internal static bool IsRunning => _thread.IsAlive;

    /// <summary>
    /// Zeit zwischen zwei Animations-Frames. Liegt bewusst deutlich unter der
    /// 60-fps-Schwelle (16,6 ms), damit auch bei leichtem Jitter kein Frame
    /// sichtbar übersprungen wird.
    /// </summary>
    private static TimeSpan FrameInterval => TimeSpan.FromMilliseconds(1000.0 / TargetFps);

    #endregion

    #region Methods

    /// <summary>
    /// Liefert die aktuelle Mausposition direkt via Win32 — thread-safe.
    /// </summary>
    public static Point GetCursorPos() {
        var pt = default(Point32);
        return GetCursorPos(out pt) ? new Point(pt.X, pt.Y) : Point.Empty;
    }

    /// <summary>
    /// Liefert das systemweite Vordergrundfenster — thread-safe via Win32.
    /// Nützlich, um aus dem Animations-Thread heraus festzustellen, ob der
    /// Nutzer das aktive Fenster gewechselt hat.
    /// </summary>
    public static IntPtr GetForegroundWindowHandle() => GetForegroundWindowNative();

    /// <summary>
    /// Liefert die aktuelle Y-Position (Bildschirmkoordinate) eines Fensters
    /// direkt via Win32 — thread-safe, ohne den UI-Thread zu berühren.
    /// </summary>
    public static int GetWindowY(IntPtr hwnd) {
        var rect = default(Rect);
        return GetWindowRect(hwnd, ref rect) ? rect.Top : 0;
    }

    public static bool IsHwndAlive(IntPtr hwnd) => hwnd != IntPtr.Zero && IsWindow(hwnd);

    public static bool IsHwndVisible(IntPtr hwnd) => hwnd != IntPtr.Zero && IsWindowVisible(hwnd);

    /// <summary>
    /// True, wenn das Fenster-handle noch gültig ist.
    /// </summary>
    /// <summary>
    /// True, wenn das Fenster laut Win32 sichtbar ist (WS_VISIBLE).
    /// </summary>
    /// <summary>
    /// Startet eine Animation für das Fenster <paramref name="hwnd" />.
    /// Der Delegate <paramref name="compute" /> wird im Animations-Thread
    /// aufgerufen und muss thread-safe sein (keine UI-Properties lesen,
    /// stattdessen <see cref="GetWindowY" /> / <see cref="IsHwndVisible" /> nutzen).
    /// <paramref name="onFinished" /> wird im Animations-Thread aufgerufen,
    /// sobald <see cref="AnimationFrame.Finished" /> true ist — UI-Aufrufe
    /// darin müssen selbst via BeginInvoke gemarshalled werden.
    /// </summary>
    public static void Start(IntPtr hwnd, Func<TimeSpan, AnimationFrame> compute, Action? onFinished = null) {
        if (hwnd == IntPtr.Zero) { return; }
        EnsureLayered(hwnd);

        var entry = new Entry(hwnd, compute, onFinished, DateTime.UtcNow);
        lock (_lock) {
            _entries[hwnd] = entry;
        }
        _wakeup.Set();
    }

    /// <summary>
    /// Startet eine Animation für <paramref name="target" />. Bequemlichkeits-
    /// Überladung, die <see cref="IAnimatable.Handle" />,
    /// <see cref="IAnimatable.Animate" /> und
    /// <see cref="IAnimatable.OnAnimationFinished" /> an die IntPtr-Überladung
    /// delegiert. Damit ist <see cref="Animator" /> typsicher nutzbar,
    /// ohne dass der Aufrufer Handle und Delegates selbst zusammenbauen muss.
    /// </summary>
    public static void Start(IAnimatable target) {
        if (target is null) { return; }
        Start(target.Handle, target.Animate, target.OnAnimationFinished);
    }

    /// <summary>
    /// Beendet die Animation für das Fenster <paramref name="hwnd" />,
    /// ohne <c>onFinished</c> aufzurufen.
    /// </summary>
    public static void Stop(IntPtr hwnd) {
        lock (_lock) {
            _entries.Remove(hwnd);
        }
    }

    private static void ApplyFrame(IntPtr hwnd, in AnimationFrame frame) {
        // Position — Win32 direkt, am UI-Thread vorbei.
        SetWindowPos(hwnd, IntPtr.Zero, frame.X, frame.Y, 0, 0, SwpPosFlags);

        // Opacity — Layered-Window-Alpha direkt setzen.
        var alpha = (byte)Math.Clamp((int)(frame.Opacity * 255), 0, 255);
        SetLayeredWindowAttributes(hwnd, 0, alpha, LwaAlpha);
    }

    private static void EnsureLayered(IntPtr hwnd) {
        lock (_lock) {
            if (_layeredEnsured.Contains(hwnd)) { return; }
        }

        var exStyle = GetWindowLongCompat(hwnd, GwlExStyle);
        if ((exStyle & WsExLayered) == 0) {
            SetWindowLongCompat(hwnd, GwlExStyle, exStyle | WsExLayered);
        }

        lock (_lock) {
            _layeredEnsured.Add(hwnd);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out Point32 lpPoint);

    [DllImport("user32.dll", EntryPoint = "GetForegroundWindow", SetLastError = true)]
    private static extern IntPtr GetForegroundWindowNative();

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    private static int GetWindowLongCompat(IntPtr hWnd, int nIndex) {
        return IntPtr.Size == 4 ? GetWindowLong32(hWnd, nIndex) : GetWindowLongPtr64(hWnd, nIndex).ToInt32();
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, ref Rect lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private static void Run() {
        while (!Generic.Ending) {
            var cycleStart = DateTime.UtcNow;

            List<Entry> snapshot;
            lock (_lock) {
                snapshot = [.. _entries.Values];
            }

            List<Entry>? finished = null;

            foreach (var e in snapshot) {
                if (!IsWindow(e.Hwnd)) {
                    (finished ??= []).Add(e);
                    continue;
                }

                AnimationFrame frame;
                try {
                    frame = e.Compute(DateTime.UtcNow - e.StartTime);
                } catch (Exception ex) {
                    Develop.DebugPrint("Fehler im Animation-Compute", ex);
                    (finished ??= []).Add(e);
                    continue;
                }

                ApplyFrame(e.Hwnd, frame);

                if (frame.Finished) {
                    (finished ??= []).Add(e);
                }
            }

            if (finished is not null) {
                List<Action?> callbacks;
                lock (_lock) {
                    callbacks = [];
                    foreach (var f in finished) {
                        _entries.Remove(f.Hwnd);
                        callbacks.Add(f.OnFinished);
                    }
                }

                foreach (var cb in callbacks) {
                    if (cb is null) { continue; }
                    try {
                        cb.Invoke();
                    } catch (Exception ex) {
                        Develop.DebugPrint("Fehler im OnFinished-Callback", ex);
                    }
                }
            }

            var elapsed = DateTime.UtcNow - cycleStart;
            var sleep = FrameInterval - elapsed;
            if (sleep > TimeSpan.Zero) {
                _wakeup.WaitOne(sleep);
            }
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    private static void SetWindowLongCompat(IntPtr hWnd, int nIndex, int value) {
        if (IntPtr.Size == 4) {
            _ = SetWindowLong32(hWnd, nIndex, value);
        } else {
            _ = SetWindowLongPtr64(hWnd, nIndex, new IntPtr(value));
        }
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

    /// <summary>
    /// Erzeugt den Animations-Thread, startet ihn und liefert die Referenz
    /// zurück. Als Initializer für <see cref="_thread" /> aufgerufen, damit
    /// die CLR das Feld vor dem ersten Zugriff anderer Static-Member hochfährt.
    /// </summary>
    private static Thread StartAnimationThread() {
        var t = new Thread(Run) {
            IsBackground = true,
            Name = "FormAnimator",
            Priority = ThreadPriority.AboveNormal
        };
        t.Start();
        return t;
    }

    #endregion

    #region Structs

    [StructLayout(LayoutKind.Sequential)]
    private struct Point32 {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion

    #region Classes

    private sealed class Entry(IntPtr hwnd, Func<TimeSpan, AnimationFrame> compute, Action? onFinished, DateTime startTime) {

        #region Properties

        public Func<TimeSpan, AnimationFrame> Compute { get; } = compute;
        public IntPtr Hwnd { get; } = hwnd;
        public Action? OnFinished { get; } = onFinished;

        public DateTime StartTime { get; } = startTime;

        #endregion
    }

    #endregion
}

/// <summary>
/// Bild eines Animations-Frame: Opacity (0..1) sowie die Bildschirmkoordinaten
/// X/Y. Wenn <see cref="Finished" /> true ist, beendet die Engine die Animation
/// und ruft den optionalen <c>onFinished</c>-Callback auf.
/// </summary>
public readonly struct AnimationFrame {

    #region Properties

    public bool Finished { get; init; }

    public double Opacity { get; init; }

    public int X { get; init; }

    public int Y { get; init; }

    #endregion
}