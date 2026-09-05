// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Enums;

public enum NoteSymbols {
    [Image(ImageCode.Stift, "Neutral")]
    Pencil,

    [Image(ImageCode.HäkchenDoppelt)]
    Ok,

    [Image(ImageCode.Warnung, "Warnung")]
    Warning,

    [Image(ImageCode.Kritisch, "Kritisch")]
    Critical
}