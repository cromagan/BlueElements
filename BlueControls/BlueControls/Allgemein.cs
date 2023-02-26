// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Forms;
using BlueBasics;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;

namespace BlueControls;

public static class Allgemein {

    #region Fields

    private static bool _serviceStarted;

    #endregion

    #region Methods

    public static T? ParentControl<T>(this object o) {
        if (o is not Control co) {
            return default;
        }

        do {
            co = co.Parent;
            switch (co) {
                case null:
                    return default;

                case T ctr:
                    return ctr;
            }
        } while (true);
    }

    public static IContextMenu? ParentControlWithCommands(this object o) {
        var par = o.ParentControl<IContextMenu>();
        if (par == null) { return null; }

        ItemCollectionList thisContextMenu = new(BlueListBoxAppearance.KontextMenu, false);
        ItemCollectionList userMenu = new(BlueListBoxAppearance.KontextMenu, false);
        List<string> tags = new();
        var cancel = false;
        var translate = true;
        par.GetContextMenuItems(null, thisContextMenu, out var hotItem, tags, ref cancel, ref translate);
        if (cancel) { return null; }

        ContextMenuInitEventArgs ec = new(hotItem, tags, userMenu);
        par.OnContextMenuInit(ec);
        if (ec.Cancel) { return null; }

        if (thisContextMenu.Count > 0 || userMenu.Count > 0) {
            return par;
        }

        return null;
    }

    public static void StartDatabaseService() {
        if (_serviceStarted) { return; }
        _serviceStarted = true;
        DatabaseAbstract.AllFiles.CollectionChanged += AllFiles_CollectionChanged;
    }

    private static void AllFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if (e.NewItems != null) {
            foreach (var thisit in e.NewItems) {
                if (thisit is DatabaseAbstract db) {
                    db.Loaded += TableView.CheckDatabase;
                    db.DropMessage += FormWithStatusBar.Db_DropMessage;
                }
            }
        }

        if (e.OldItems != null) {
            foreach (var thisit in e.OldItems) {
                if (thisit is DatabaseAbstract db) {
                    db.Loaded -= TableView.CheckDatabase;
                    db.DropMessage -= FormWithStatusBar.Db_DropMessage;
                }
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset) {
            Develop.DebugPrint_NichtImplementiert();
        }
    }

    #endregion

    //private static void DB_GenerateLayoutInternal(object sender, GenerateLayoutInternalEventArgs e) {
    //    if (e.Handled) { return; }
    //    e.Handled = true;
    //    if (e?.Row?.Database == null) { return; }

    //    var pad = new ItemCollectionPad(e.LayoutId, e.Row.Database, e.Row.Key);
    //    pad.SaveAsBitmap(e.Filename);
    //}
}