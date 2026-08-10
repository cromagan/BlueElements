// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
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

    #region Fields

    /// <summary>
    /// Größe der Icons in der Mini-Toolbar.
    /// </summary>
    public const int IconSize = 24;

    #endregion

    #region Properties

    /// <summary>
    /// Definert, ob die Default-Mini-Toolbar angezeigt wird.
    /// </summary>
    public bool MiniToolbarEnabled { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Diese Routine wird aufgerufen, um die internen Mini-Toolbar-Einträge zu erstellen.
    /// </summary>
    List<AbstractListItem>? GetMiniToolbarItems(object? hotItem);

    #endregion
}

public static class MiniToolbarExtension {

    #region Fields

    private static object? _lastHotItems = new();

    #endregion

    #region Methods

    /// <summary>
    /// Schließt eine eventuell sichtbare Mini-Toolbar. Das letzte HotItem
    /// bleibt dabei gespeichert — ein erneuter Aufruf von
    /// <see cref="IMiniToolbar.MiniToolbarShow"/> mit demselben HotItem zeigt
    /// die Toolbar nicht noch einmal an (gewünschtes „Ausgeschaltet-bleiben“
    /// bei wiederholten Klicks auf dieselbe Zelle).
    /// </summary>
    public static void HideMiniToolbar() {
        FloatingForm.Close(Design.Form_MiniToolbar);
    }

    /// <summary>
    /// Zeigt die Mini-Toolbar an der übergebenen Bildschirm-Position an.
    /// <paramref name="screenPosition"/> ist die Position in Bildschirmkoordinaten
    /// (z. B. die rechte obere Ecke einer Zelle).
    /// <para>
    /// Die Routine entscheidet intern, ob die Toolbar tatsächlich angezeigt wird:
    /// Ist <paramref name="hotItem"/> identisch mit dem HotItem des letzten Aufrufs,
    /// wird eine eventuell sichtbare Toolbar geschlossen und keine neue geöffnet.
    /// So wird ein wiederholter Klick auf dieselbe Zelle als „ausschalten“
    /// interpretiert — auch wenn die Zelle danach mehrfach angeklickt wird,
    /// bleibt die Toolbar ausgeblendet, bis eine andere Zelle getroffen wird.
    /// </para>
    /// <para>
    /// Zum Ausblenden der Toolbar (z. B. bei Mausbewegung oder Tastatur-Navigation)
    /// <see cref="HideMiniToolbar"/> verwenden. Das schließt
    /// nur das Fenster, merkt sich aber das letzte HotItem — so führt ein erneuter
    /// Klick auf dieselbe Zelle nicht zu einem erneuten Einblenden.
    /// </para>
    /// </summary>
    public static void MiniToolbarShow(this IMiniToolbar parent, Point screenPosition, object? hotItem) {
        HideMiniToolbar();

        if (!parent.MiniToolbarEnabled || Equals(_lastHotItems, hotItem)) {
            return;
        }

        var thisMiniToolbar = new List<AbstractListItem>();

        if (parent.GetMiniToolbarItems(hotItem) is { } mti && mti.Count > 0) {
            thisMiniToolbar.AddRange(mti);
        }

        if (thisMiniToolbar.Count > 0) {
            // HotItem erst speichern, wenn die Toolbar tatsächlich angezeigt
            // wird. Ein erneuter Aufruf mit demselben HotItem schaltet die
            // Toolbar damit aus (gewünschtes Toggle-Verhalten). Ein bloßes
            // HideMiniToolbar ändert _lastHotItems nicht — siehe XML-Doc oben.
            _lastHotItems = hotItem;
            FloatingInputBoxListBoxStyle.ShowAtPosition(thisMiniToolbar, screenPosition, parent as Control, hotItem);
        }
    }

    #endregion
}