// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;

namespace BlueBasics.Enums;

[Flags]
public enum ImageCodeEffect {
    //Undefiniert = -1,
    None = 0,

    Durchgestrichen = 1,

    // SpiegelnX = 2,
    // SpiegelnY = 4,
    Graustufen = 8,

    WindowsMEDisabled = 16,

    // StdDarken = 32
    // StdLighten = 64
    WindowsXPDisabled = 128

    // NoTransparent = 256
}