// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das Element sein Aussehen verändern kann - mittels StyleDB
/// Zusätzlich wenn es davon EINEN bestimmten Stil benutzt. Z.B. die Überschrift davon
/// </summary>
public interface IStyleableOne : IStyleable {

    #region Properties

    BlueFont? Font { get; set; }

    PadStyles Style { get; }

    #endregion
}

public static class StyleableOneExtension {

    #region Methods

    public static BlueFont GetFont(this IStyleableOne o, float additionalScale) => Math.Abs(1 - additionalScale) < Constants.DefaultTolerance ? GetFont(o) : GetFont(o).Scale(additionalScale);

    public static BlueFont GetFont(this IStyleableOne o) {
        o.Font ??= Skin.GetBlueFont(o.SheetStyle, o.Style);
        return o.Font;
    }

    public static void InvalidateFont(this IStyleableOne o) {
        if (o.Style != PadStyles.Undefined) {
            o.Font = null;
        }
    }

    #endregion
}