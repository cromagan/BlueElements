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

    protected override System.Windows.Forms.Control? ControlCore => _control;

    public override string Description => "Zeigt eine Liste, deren Einträge ausgewählt, verschoben und entfernt werden können.";

    public override string KeyName => ClassId;

    public override bool SupportsSuggestions => true;

    #endregion

    #region Methods

    /// <summary>
    /// Berechnet die benötigte Größe anhand der Items: Die Breite wird so
    /// bemessen, dass das breiteste Item vollständig dargestellt wird (inkl.
    /// Reserve für einen eventuellen Scrollbalken). Die Höhe summiert alle
    /// sichtbaren Items, damit nichts abgeschnitten ist.
    /// </summary>
    public override Size CalculateRequiredSize(int minWidth, int minHeight) {
        if (_control is not { } c || c.Items is not { Count: > 0 } items) {
            return new Size(minWidth, minHeight);
        }

        var (biggestItemX, _, heightAdded, _) = items.CanvasItemData(c.ItemDesign);

        if (c.AddAllowed != AddType.None) { heightAdded += 26; }

        heightAdded++; // Reserve, damit kein vertikaler Slider entsteht.
        heightAdded = Math.Max(heightAdded, minHeight);
        biggestItemX = Math.Max(biggestItemX, minWidth);

        var primary = System.Windows.Forms.Screen.PrimaryScreen;
        var maxWi = (int)((primary?.Bounds.Width ?? 1920) * 0.7);
        var maxHe = (int)((primary?.Bounds.Height ?? 1080) * 0.7);

        if (biggestItemX > maxWi) { biggestItemX = maxWi; }
        if (heightAdded > maxHe) {
            heightAdded = maxHe;
            biggestItemX += 20; // Platz für den Scrollbalken.
        }

        return new Size(biggestItemX, heightAdded);
    }

    public override void CreateControl() {
        _control = new ListBox() { CheckBehavior = CheckBehavior.MultiSelection };
        _control.ItemClear();
    }

    public override string ReadableText() => "Listbox";

    public override void SubscribeEvents() {
        _control?.ItemCheckedChanged += ListBox_ItemCheckedChanged;
        _control?.RemoveClicked += ListBox_ItemRemoved;
        _control?.LostFocus += Control_LostFocus;
    }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(BlueBasics.Enums.ImageCode.Pfeil_Unten_Scrollbar);

    public override void UnsubscribeEvents() {
        _control?.ItemCheckedChanged -= ListBox_ItemCheckedChanged;
        _control?.RemoveClicked -= ListBox_ItemRemoved;
        _control?.LostFocus -= Control_LostFocus;
    }

    protected override void ApplyStyle() {
        if (_control is null) { return; }

        _control.CheckBehavior = CheckBehavior;
        _control.AutoSort = AutoSort;
        _control.Translate = true;
        _control.MoveAllowed = MoveAllowed;
        _control.RemoveAllowed = RemoveAllowed;

        if (AddAllowed == AddType.Suggestions) {
            // Suggestions-Modus: ListItems sind Vorschläge für das Hinzufügen-Menü,
            // keine direkten Listeneinträge. Die ausgewählten Werte bleiben als
            // Listeneinträge erhalten und werden mit den passenden Suggestions-Items aktualisiert.
            var currentKeys = _control.Items.Select(i => i.KeyName).ToList();

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
            _control.Check(currentKeys, true);
        } else {
            _control.ItemClear();
            _control.Suggestions.Clear();
            if (ListItems is not null) {
                var itemsToAdd = new List<ListItem>(ListItems);
                if (AutoSort) { itemsToAdd.Sort(); }
                _control.ItemAddRange(itemsToAdd);
            }
        }

        if (AddAllowed != AddType.None) {
            _control.AddAllowed = AddAllowed;
        } else {
            _control.AddAllowed = TextInputAllowed ? AddType.Text : AddType.None;
        }

        _control.CustomContextMenuItems = CustomContextMenuItems;
        _control.QuickInfo = QuickInfo;
        _control.Zoom = Zoom;
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
        SyncCheckedText();
    }

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    private void ListBox_ItemCheckedChanged(object? sender, System.EventArgs e) => OnValueChanged(SyncCheckedText());

    private void ListBox_ItemRemoved(object? sender, ListItemEventArgs e) {
        // RemoveAndUnRegister feuert kein ItemCheckedChanged, Text hier explizit syncen.
        SyncCheckedText();
        OnItemRemoved(e);
    }

    /// <summary>
    /// Hält Control.Text synchron mit den gecheckten Keys. Der Wert-Commit
    /// der TableView (Edit_Close) liest den Wert über Control.Text.
    /// </summary>
    private string SyncCheckedText() {
        if (_control is not { } c) { return string.Empty; }
        var value = string.Join('\r', c.Checked);
        c.Text = value;
        return value;
    }

    #endregion
}