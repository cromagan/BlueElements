// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#nullable enable

using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using System.Data.Common;
using System.Data.Design;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Editoren;

public partial class FilterCollectionEditor : EditorEasy, IHasDatabase {

    #region Constructors

    public FilterCollectionEditor() {
        InitializeComponent();
    }

    #endregion

    #region Properties

    public Database? Database {
        get {
            if (ToEdit is not FilterCollection fc || fc.IsDisposed) { return null; }
            return fc.Database;
        }
    }

    #endregion

    #region Methods

    public override void Clear() {
        lstFilter.ItemClear();
        capDatabase.Text = "Datenbank: ?";
    }

    public override bool Init(IEditable? toEdit) {
        if (toEdit is not FilterCollection fc || fc.IsDisposed) { return false; }

        if (fc.Database is not Database db || db.IsDisposed) { return false; }

        capDatabase.Text = "Datenbank: " + db.Caption;

        foreach (var item in fc) {
            lstFilter.ItemAdd(ItemOf(item));
            item.Editor = typeof(FilterEditor);
        }

        return true;
    }

    public AbstractListItem? NewChild() {
        if (ToEdit is not FilterCollection fc || fc.IsDisposed) { return null; }

        if (fc.Database is not Database db || db.IsDisposed) { return null; }

        var l = new FilterItem(db, FilterType.Istgleich_GroßKleinEgal, "?");
        l.Editor = typeof(FilterEditor);
        fc.Add(l);
        return ItemOf(l);
    }

    protected override void InitializeComponentDefaultValues() {
        lstFilter.AddMethod = NewChild;
    }

    private void lstFilter_RemoveClicked(object sender, EventArgs.AbstractListItemEventArgs e) {
        if (ToEdit is not FilterCollection fc || fc.IsDisposed) { return; }
        if (e.Item is ReadableListItem rli && rli.Item is FilterItem fi) {
            fc.Remove(fi);
        }
    }

    #endregion
}