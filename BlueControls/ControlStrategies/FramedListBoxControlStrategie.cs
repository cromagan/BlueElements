// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.EventArgs;

namespace BlueControls.ControlStrategies;

/// <summary>
/// Listen-Strategie mit zusätzlichem Rahmen (GroupBox) um die ListBox.
/// Einsatz als Inline-Editor in der TableView, wenn die Auswahl optisch
/// vom Zellinhalt abgegrenzt werden soll.
/// </summary>
public class FramedListBoxControlStrategie : ControlStrategie {

    #region Fields

    private GroupBox? _groupBox;
    private ListBox? _listBox;

    #endregion

    #region Properties

    public override System.Windows.Forms.Control? Control => _groupBox;

    public override bool SupportsSuggestions => true;

    #endregion

    #region Methods

    public override Size CalculateRequiredSize(int minWidth, int minHeight) {
        if (_listBox is not { } lb || lb.Items is not { Count: > 0 } items) {
            return new Size(minWidth, minHeight);
        }

        var (biggestItemX, _, heightAdded, _) = items.CanvasItemData(lb.ItemDesign);

        if (lb.AddAllowed != AddType.None) { heightAdded += 26; }

        heightAdded++;
        heightAdded = Math.Max(heightAdded, minHeight);
        biggestItemX = Math.Max(biggestItemX, minWidth);

        var primary = System.Windows.Forms.Screen.PrimaryScreen;
        var maxWi = (int)((primary?.Bounds.Width ?? 1920) * 0.7);
        var maxHe = (int)((primary?.Bounds.Height ?? 1080) * 0.7);

        if (biggestItemX > maxWi) { biggestItemX = maxWi; }
        if (heightAdded > maxHe) {
            heightAdded = maxHe;
            biggestItemX += 20;
        }

        return new Size(biggestItemX + 2 * Skin.Padding, heightAdded + 2 * Skin.Padding);
    }

    public override void CreateControl() {
        _groupBox = new GroupBox() {
            GroupBoxStyle = GroupBoxStyle.RoundRect,
            Text = string.Empty
        };

        _listBox = new ListBox() { CheckBehavior = CheckBehavior.MultiSelection };
        _listBox.ItemClear();

        _groupBox.Controls.Add(_listBox);
    }

    public override void SubscribeEvents() {
        _listBox?.ItemCheckedChanged += ListBox_ItemCheckedChanged;
        _listBox?.RemoveClicked += ListBox_ItemRemoved;
        _listBox?.LostFocus += Control_LostFocus;
        if (_groupBox is not null) { _groupBox.SizeChanged += GroupBox_SizeChanged; }
    }

    public override void UnsubscribeEvents() {
        _listBox?.ItemCheckedChanged -= ListBox_ItemCheckedChanged;
        _listBox?.RemoveClicked -= ListBox_ItemRemoved;
        _listBox?.LostFocus -= Control_LostFocus;
        if (_groupBox is not null) { _groupBox.SizeChanged -= GroupBox_SizeChanged; }
    }

    protected override void ApplyStyle() {
        if (_listBox is null) { return; }

        _listBox.CheckBehavior = CheckBehavior;
        _listBox.AutoSort = AutoSort;
        _listBox.Translate = true;
        _listBox.MoveAllowed = MoveAllowed;
        _listBox.RemoveAllowed = RemoveAllowed;

        if (AddAllowed == AddType.Suggestions) {
            var currentKeys = _listBox.Items.Select(i => i.KeyName).ToList();

            _listBox.Suggestions.Clear();
            if (ListItems is not null) {
                var suggestions = new List<ListItem>(ListItems);
                if (AutoSort) { suggestions.Sort(); }
                _listBox.Suggestions.AddRange(suggestions);
            }

            _listBox.ItemClear();
            foreach (var key in currentKeys) {
                _listBox.ItemAdd(_listBox.Suggestions.GetByKey(key) ?? ItemOf(key));
            }
            _listBox.Check(currentKeys, true);
        } else {
            _listBox.ItemClear();
            _listBox.Suggestions.Clear();
            if (ListItems is not null) {
                var itemsToAdd = new List<ListItem>(ListItems);
                if (AutoSort) { itemsToAdd.Sort(); }
                _listBox.ItemAddRange(itemsToAdd);
            }
        }

        _listBox.AddAllowed = AddAllowed != AddType.None
            ? AddAllowed
            : UserEditDialogType switch {
                EditTypeTable.Textfeld => AddType.Text,
                EditTypeTable.Textfeld_mit_Vorschlägen => AddType.Text,
                _ => AddType.None
            };
        _listBox.CustomContextMenuItems = CustomContextMenuItems;
        _listBox.QuickInfo = QuickInfo;
        _listBox.Zoom = Zoom;
    }

    protected override void SetValueToControlInternal(string value) {
        if (_listBox is null) { return; }

        var values = value.SplitAndCutByCr();

        foreach (var v in values) {
            if (!string.IsNullOrEmpty(v) && _listBox[v] is null) {
                _listBox.ItemAdd(_listBox.Suggestions.GetByKey(v) ?? ItemOf(v));
            }
        }
        _listBox.Check(values, true);
    }

    private void Control_LostFocus(object? sender, System.EventArgs e) => OnLostFocus();

    private void GroupBox_SizeChanged(object? sender, System.EventArgs e) {
        if (_groupBox is not { } gb || _listBox is not { } lb) { return; }
        var pad = Skin.Padding;
        lb.Location = new Point(pad, pad);
        lb.Size = new Size(Math.Max(gb.Width - 2 * pad, 1), Math.Max(gb.Height - 2 * pad, 1));
    }

    private void ListBox_ItemCheckedChanged(object? sender, System.EventArgs e) {
        if (_listBox is { } c) { OnValueChanged(string.Join('\r', c.Checked)); }
    }

    private void ListBox_ItemRemoved(object? sender, ListItemEventArgs e) => OnItemRemoved(e);

    #endregion
}