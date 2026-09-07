// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Enums;

[Flags]
public enum FilterOptions {
    None = 0,
    Enabled = 1,
    TextFilterEnabled = 2,
    ExtendedFilterEnabled = 4,
    OnlyAndAllowed = 8,
    OnlyOrAllowed = 16,
    Enabled_OnlyAndAllowed = OnlyAndAllowed | Enabled,
    Enabled_OnlyOrAllowed = OnlyOrAllowed | Enabled
}