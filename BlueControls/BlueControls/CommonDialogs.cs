using BlueDatabase;

#nullable enable

namespace BlueControls.Forms;

internal static class CommonDialogs {

    #region Methods

    public static DatabaseAbstract? ChooseKnownDatabase() {
        var l = DatabaseAbstract.AllAvailableTables();

        var l2 = new ItemCollection.ItemCollectionList.ItemCollectionList(true);

        foreach (var thisd in l) {
            _ = l2.Add(thisd);
        }

        var x = InputBoxListBoxStyle.Show("Datenbank wählen:", l2, Enums.AddType.None, true);

        if (x == null || x.Count != 1) { return null; }

        return DatabaseAbstract.GetById(new ConnectionInfo(x[0], null), Controls.Table.Database_NeedPassword);
    }

    #endregion
}