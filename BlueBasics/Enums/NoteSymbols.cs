// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

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