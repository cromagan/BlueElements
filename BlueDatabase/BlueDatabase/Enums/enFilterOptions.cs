using System;

namespace BlueDatabase.Enums
{
    [Flags]
    public enum enFilterOptions
    {

        None = 0,
        Enabled = 1,
        TextFilterEnabled = 2,
        ExtendedFilterEnabled = 4,
        OnlyAndAllowed = 8,
        OnlyOrAllowed = 16,

        Enabled_OnlyAndAllowed = OnlyAndAllowed | Enabled,
        Enabled_OnlyOrAllowed = OnlyOrAllowed | Enabled


    }
}