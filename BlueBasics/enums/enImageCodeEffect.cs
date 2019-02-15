using System;

namespace BlueBasics.Enums
{
    [Flags]
    public enum enImageCodeEffect
    {
        Undefiniert = -1,

        Ohne = 0,

        Durchgestrichen = 1,

        //SpiegelnX = 2,
        //SpiegelnY = 4,

        Graustufen = 8,


        WindowsMEDisabled = 16,


        //StdDarken = 32
        //StdLighten = 64

        WindowsXPDisabled = 128

        // NoTransparent = 256
    }
}