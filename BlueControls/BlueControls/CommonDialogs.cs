#nullable enable

using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;

namespace BlueControls.Forms;

internal static class CommonDialogs {

    #region Methods

    public static DatabaseAbstract? ChooseKnownDatabase() {
        var l = DatabaseAbstract.AllAvailableTables();

        var l2 = new ItemCollectionList(true);

        foreach (var thisd in l) {
            _ = l2.Add(thisd);
        }

        var x = InputBoxListBoxStyle.Show("Datenbank wählen:", l2, AddType.None, true);

        if (x == null || x.Count != 1) { return null; }

        return DatabaseAbstract.GetById(new ConnectionInfo(x[0], null), Table.Database_NeedPassword);
    }

    #endregion
}