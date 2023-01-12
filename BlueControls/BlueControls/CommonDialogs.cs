using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace BlueControls.Forms;

internal static class CommonDialogs {

    #region Methods

    public static BlueDatabase.DatabaseAbstract? ChooseKnownDatabase() {
        var l = DatabaseAbstract.AllAvailableTables(null);

        var l2 = new ItemCollection.ItemCollectionList.ItemCollectionList();

        foreach (var thisd in l) {
            l2.Add(thisd, thisd.UniqueID);
        }

        var x = BlueControls.Forms.InputBoxListBoxStyle.Show("Datenbank wählen:", l2, Enums.AddType.None, true);

        if (x == null || x.Count != 1) { return null; }

        return DatabaseAbstract.GetByID(new ConnectionInfo(x[0]), null);
    }

    #endregion
}