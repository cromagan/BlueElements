// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.Enums;

[Flags]
public enum ThumbnailOptions {
    None = 0x00,
    BiggerSizeOk = 0x01,
    InMemoryOnly = 0x02,
    IconOnly = 0x04,
    ThumbnailOnly = 0x08,
    InCacheOnly = 0x10
}