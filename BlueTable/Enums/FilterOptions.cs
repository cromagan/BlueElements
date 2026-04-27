// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;

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