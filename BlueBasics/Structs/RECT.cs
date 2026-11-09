// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Runtime.InteropServices;

namespace BlueBasics.Structs;

/// <summary>
/// Win32-Rechteck (Left, Top, Right, Bottom) für P/Invoke-Aufrufe.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct RECT {
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}
