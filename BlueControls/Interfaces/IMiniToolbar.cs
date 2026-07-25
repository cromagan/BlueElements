// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace BlueControls.Interfaces;

/// <summary>
/// Interface, das zur Generierung von Mini-Toolbars benötigt wird.
/// Die ganze Erstellung und Handling übernimmt dabei
/// <see cref="IMiniToolbar.MiniToolbarShow(Point, object?)"/>.
/// Dabei werden die hier angegebenen Routinen und Properties abgefragt.
/// Im Gegensatz zu <see cref="IContextMenu"/> erscheint die Mini-Toolbar
/// nicht an der Cursor-Position, sondern an der übergebenen Position
/// (z. B. neben einer angeklickten Zelle).
/// </summary>
public interface IMiniToolbar {

    #region Properties

    /// <summary>
    /// Definert, ob die Default-Mini-Toolbar angezeigt wird.
    /// </summary>
    public bool MiniToolbarDefault { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Diese Routine wird aufgerufen, um die internen Mini-Toolbar-Einträge zu erstellen.
    /// </summary>
    List<AbstractListItem>? GetMiniToolbarItems(object? hotItem);

    /// <summary>
    /// Zeigt die Mini-Toolbar an der übergebenen Bildschirm-Position an.
    /// <paramref name="screenPosition"/> ist die Position in Bildschirmkoordinaten
    /// (z. B. die rechte obere Ecke einer Zelle).
    /// </summary>
    public void MiniToolbarShow(Point screenPosition, object? hotItem) {
        FloatingForm.Close(Design.Form_MiniToolbar);

        var thisMiniToolbar = new List<AbstractListItem>();

        if (MiniToolbarDefault && GetMiniToolbarItems(hotItem) is { } mti && mti.Count > 0) {
            thisMiniToolbar.AddRange(mti);
        }

        if (thisMiniToolbar.Count > 0) {
            if (this is Control parentControl) {
                FloatingInputBoxListBoxStyle.ShowAtPosition(thisMiniToolbar, screenPosition, parentControl, hotItem);
            }
        }
    }

    #endregion
}