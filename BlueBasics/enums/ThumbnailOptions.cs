// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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