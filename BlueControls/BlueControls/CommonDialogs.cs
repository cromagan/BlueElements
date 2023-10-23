#nullable enable

using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;

namespace BlueControls.Forms;

internal static class CommonDialogs {

    #region Methods

    public static DatabaseAbstract? ChooseKnownDatabase(string caption, string mustbefreezed) {
        var l = DatabaseAbstract.AllAvailableTables(mustbefreezed);

        var l2 = new ItemCollectionList.ItemCollectionList(true);

        foreach (var thisd in l) {
            _ = l2.Add(thisd);
        }

        var x = InputBoxListBoxStyle.Show(caption, l2, AddType.None, true);

        if (x == null || x.Count != 1) { return null; }

        return DatabaseAbstract.GetById(new ConnectionInfo(x[0], null, mustbefreezed), false, Table.Database_NeedPassword, true);
    }

    #endregion
}