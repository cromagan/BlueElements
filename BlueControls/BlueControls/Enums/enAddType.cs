using System;

namespace BlueControls.Enums
{
    [Flags]
    public enum enAddType
    {
        None = 0,
        Text = 1,
        Images = 2,
        // Binary = 4
        BinaryAndImages = 6,
        OnlySuggests = 8,
        CellDecide = 16,

        OnlyInternalCoded = 32

    }
}