// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Forms;

public sealed partial class ItemSelect : DialogWithOkAndCancel {

    #region Fields

    private ListItem? _giveBack;

    #endregion

    #region Constructors

    private ItemSelect(List<ListItem> items) : base(true, true) {
        InitializeComponent();

        List.ItemClear();
        List.ItemAddRange(items);

        Setup(400, List.Bottom);
    }

    #endregion

    #region Methods

    public static RowItem? Show(List<RowItem> rows, string layoutId) {
        try {
            var items = rows.Select(thisRow => new RowLayoutListItem(thisRow, layoutId)).Cast<ListItem>().ToList();

            var x = Show(items);
            return (x as RowLayoutListItem)?.Row;
        } catch (Exception ex) {
            Develop.DebugPrint("Fehler beim Generieren des _internal: " + layoutId, ex);
            return null;
        }
    }

    public static string Show(List<string> files) {
        var items = new List<ListItem>();

        foreach (var thisString in files) {
            if (thisString.FileType() == FileFormat.Image) {
                items.Add(new BitmapListItem(thisString, thisString, thisString.FileNameWithoutSuffix(), string.Empty));
            }
        }
        var x = Show(items);
        return x?.KeyName ?? string.Empty;
    }

    public static ListItem? Show(List<ListItem>? items) {
        if (items is not { Count: not 0 }) { return null; }

        var x = new ItemSelect(items);
        x.ShowDialog();

        return x._giveBack;
    }

    protected override bool SetValue() {
        _giveBack = null;
        if (Canceled) { return true; }

        var l = List.Checked;
        if (l.Count != 1) {
            Canceled = true;
            return true;
        }

        _giveBack = List[l[0]];

        return true;
    }

    #endregion
}