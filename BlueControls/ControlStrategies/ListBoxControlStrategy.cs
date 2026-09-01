// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.EventArgs;

namespace BlueControls.ControlStrategies;

public class ListBoxControlStrategy : ControlStrategy {

    #region Fields

    private ListBox? _control;

    #endregion

    #region Properties

    public static string ClassId => "Listbox";

    public override string Description => "Zeigt eine Liste, deren Einträge ausgewählt, verschoben und entfernt werden können.";
    public override string KeyName => ClassId;
    public override bool SupportsSuggestions => true;
    protected override System.Windows.Forms.Control? ControlCore => _control;

    #endregion

    #region Methods

    /// <summary>
    /// Berechnet den benötigten Bereich anhand der Items: Die Breite wird so
    /// bemessen, dass das breiteste Item vollständig dargestellt wird (inkl.
    /// Reserve für einen eventuellen Scrollbalken). Die Höhe summiert alle
    /// sichtbaren Items, damit nichts abgeschnitten ist — mindestens das
    /// ganze Feld, höchstens bis zum 2,5-Fachen der Zeilenhöhe.
    /// </summary>
    public override Rectangle CalculateRequiredBounds(Rectangle bounds) {
        if (_control is not { } c || c.Items is not { Count: > 0 } items) {
            return new Rectangle(bounds.Location, new Size(bounds.Width, Math.Min(bounds.Height, MaxSingleLineFillHeight)));
        }

        var (biggestItemX, _, heightAdded, _) = items.CanvasItemData(c.ItemDesign);

        if (c.AddAllowed != AddType.None) { heightAdded += 26; }

        heightAdded++; // Reserve, damit kein vertikaler Slider entsteht.
        // Optik: das ganze Feld ausfüllen, auch wenn die Liste nur einzeilig
        // ist — höchstens bis zum 2,5-Fachen der Zeilenhöhe.
        heightAdded = Math.Max(heightAdded, Math.Min(bounds.Height, MaxSingleLineFillHeight));
        biggestItemX = Math.Max(biggestItemX, bounds.Width);

        var primary = System.Windows.Forms.Screen.PrimaryScreen;
        var maxWi = (int)((primary?.Bounds.Width ?? 1920) * 0.7);
        var maxHe = (int)((primary?.Bounds.Height ?? 1080) * 0.7);

        if (biggestItemX > maxWi) { biggestItemX = maxWi; }
        if (heightAdded > maxHe) {
            heightAdded = maxHe;
            biggestItemX += 20; // Platz für den Scrollbalken.
        }

        return new Rectangle(bounds.Location, new Size(biggestItemX, heightAdded));
    }

    public override string ReadableText() => "Listbox";

    /// <summary>
    /// Setzt die Scroll-Position der Liste auf den Anfang zurück.
    /// </summary>
    public override void Reset() {
        base.Reset();
        if (_control is not { IsDisposed: false } c) { return; }
        c.ResetScroll();
    }

    public override void SubscribeEvents() {
        _control?.ItemCheckedChanged += ListBox_ItemCheckedChanged;
        _control?.RemoveClicked += ListBox_ItemRemoved;
        _control?.LostFocus += Control_LostFocus;
    }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Pfeil_Unten_Scrollbar);

    public override void UnsubscribeEvents() {
        _control?.ItemCheckedChanged -= ListBox_ItemCheckedChanged;
        _control?.RemoveClicked -= ListBox_ItemRemoved;
        _control?.LostFocus -= Control_LostFocus;
    }

    protected override void ApplyStyle() {
        if (_control is null) { return; }

        // Nur der Dropdown-Menü-Stil lässt die Liste wie ein geöffnetes Dropdown wirken.
        _control.Appearance = Border == GroupBoxStyle.DropdownMenu ? ListBoxAppearance.DropdownSelectbox : ListBoxAppearance.Listbox;

        _control.CheckBehavior = CheckBehavior;
        _control.AutoSort = AutoSort;
        _control.Translate = true;
        _control.MoveAllowed = MoveAllowed;
        _control.RemoveAllowed = RemoveAllowed;

        // Nur die ausgewählten Werte übernehmen — abgewählte Einträge dürfen
        // durch einen Style-Wechsel nicht wieder in den Wert zurückkommen.
        var currentKeys = _control.Checked.ToList();

        if (AddAllowed == AddType.Suggestions) {
            // Suggestions-Modus: ListItems sind Vorschläge für das Hinzufügen-Menü,
            // keine direkten Listeneinträge. Die ausgewählten Werte bleiben als
            // Listeneinträge erhalten und werden mit den passenden Suggestions-Items aktualisiert.
            _control.Suggestions.Clear();
            if (ListItems is not null) {
                var suggestions = new List<ListItem>(ListItems);
                if (AutoSort) { suggestions.Sort(); }
                _control.Suggestions.AddRange(suggestions);
            }

            _control.ItemClear();
            foreach (var key in currentKeys) {
                _control.ItemAdd(_control.Suggestions.GetByKey(key) ?? ItemOf(key));
            }
        } else {
            _control.ItemClear();
            _control.Suggestions.Clear();
            if (ListItems is not null) {
                var itemsToAdd = new List<ListItem>(ListItems);
                if (AutoSort) { itemsToAdd.Sort(); }
                _control.ItemAddRange(itemsToAdd);
            }
            // Ausgewählte Werte, die nicht mehr in ListItems stehen, als Eintrag
            // erhalten — sonst fielen sie beim nächsten Write-Back aus dem Wert.
            foreach (var key in currentKeys) {
                if (_control[key] is null) { _control.ItemAdd(ItemOf(key)); }
            }
        }

        _control.Check(currentKeys, true);

        if (AddAllowed != AddType.None) {
            _control.AddAllowed = AddAllowed;
        } else {
            _control.AddAllowed = TextInputAllowed ? AddType.Text : AddType.None;
        }

        _control.CustomContextMenuItems = CustomContextMenuItems;
        _control.QuickInfo = QuickInfo;
        _control.Zoom = Zoom;
    }

    protected override void CreateControlCore() {
        _control = new ListBox() { CheckBehavior = CheckBehavior.MultiSelection };
        _control.ItemClear();
    }

    protected override void ForceWriteBackValue() {
        if (_control is not { IsDisposed: false } c) { return; }
        Value = string.Join('\r', c.Checked);
    }

    protected override void SetValueToControlInternal(string value) {
        if (_control is null) { return; }

        var values = value.SplitAndCutByCr();

        foreach (var v in values) {
            if (!string.IsNullOrEmpty(v) && _control[v] is null) {
                _control.ItemAdd(_control.Suggestions.GetByKey(v) ?? ItemOf(v));
            }
        }
        _control.Check(values, true);
    }

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    private void ListBox_ItemCheckedChanged(object? sender, System.EventArgs e) => ForceWriteBackValue();

    private void ListBox_ItemRemoved(object? sender, ListItemEventArgs e) => OnItemRemoved(e);

    #endregion
}