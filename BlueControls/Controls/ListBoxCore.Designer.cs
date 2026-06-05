// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Enums;
using BlueTable.Interfaces;
using System.Collections.ObjectModel;

namespace BlueControls.Controls;

/// <summary>
/// Kern-Listbox: enthält ausschließlich die Listbox-Logik und den Slider.
/// Wird von <see cref="ListBox"/> als untergeordnetes Control gehostet.
/// </summary>
public sealed partial class ListBoxCore :  IContextMenu, ITranslateable {

    #region Fields

    private readonly List<AbstractListItem> _item = [];
    private readonly object _itemLock = new();
    private ListBoxAppearance _appearance = ListBoxAppearance.Listbox;
    private CheckBehavior _checkBehavior = CheckBehavior.SingleSelection;
    private List<AbstractListItem> _checked = [];
    private Design _controlDesign = Design.Undefined;
    private SizeF _lastCheckedMaxSize = Size.Empty;
    private Size _maxNeededItemSize = Size.Empty;
    private AbstractListItem? _mouseOverItem;
    private bool _sorted;

    #endregion

    #region Constructors

    public ListBoxCore() {
        ItemDesign = Design.Undefined;
        HotItemForClick = null;
        InvalidateItemOrder();
        GetDesigns();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Aktualisiert die Design-Zuweisung basierend auf der Appearance.
    /// </summary>
    public void GetDesigns() {
        _controlDesign = (Design)_appearance;
        switch (_appearance) {
            case ListBoxAppearance.Autofilter:
                ItemDesign = Design.Item_AutoFilter;
                break;

            case ListBoxAppearance.DropdownSelectbox:
                ItemDesign = Design.Item_DropdownMenu;
                break;

            case ListBoxAppearance.Gallery:
            case ListBoxAppearance.FileSystem:
            case ListBoxAppearance.Listbox_Boxes:
            case ListBoxAppearance.Listbox:
                ItemDesign = Design.Item_ListBox;
                _controlDesign = Design.ListBox;
                break;

            case ListBoxAppearance.KontextMenu:
                ItemDesign = Design.Item_ContextMenu;
                break;

            case ListBoxAppearance.ComboBox_Textbox:
                ItemDesign = Design.ComboBox_TextBox;
                break;

            case ListBoxAppearance.ButtonList:
                ItemDesign = Design.Button;
                _controlDesign = Design.GroupBox;
                break;

            default:
                Develop.DebugError("Unbekanntes Design: " + _appearance);
                break;
        }
    }

    private void InvalidateItemOrder() { _maxNeededItemSize = Size.Empty; _sorted = false; Invalidate_MaxBounds(); }

    private bool IsAppearanceClickable() => _appearance is ListBoxAppearance.Listbox or ListBoxAppearance.Listbox_Boxes or ListBoxAppearance.Autofilter or ListBoxAppearance.Gallery or ListBoxAppearance.FileSystem or ListBoxAppearance.ButtonList;

    #endregion

}
