﻿// Authors:
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
using BlueControls.Forms;
using BlueDatabase;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace BlueControls;

public static class Allgemein {

    #region Fields

    private static bool _serviceStarted;

    #endregion

    #region Methods

    public static void StartGlobalService() {
        if (_serviceStarted) { return; }
        _serviceStarted = true;
        Database.AllFiles.CollectionChanged += AllFiles_CollectionChanged;
        ConnectedFormula.ConnectedFormula.AllFiles.CollectionChanged += AllFiles_CollectionChanged;
    }

    private static void AllFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if (e.NewItems != null) {
            foreach (var thisit in e.NewItems) {
                if (thisit is ICanDropMessages dm) {
                    dm.DropMessage += FormWithStatusBar.GotMessageDropMessage;
                }
                if (thisit is Database db) {
                    db.AdditionalRepair += BlueControls.Controls.Table.Database_AdditionalRepair;
                }
            }
        }

        if (e.OldItems != null) {
            foreach (var thisit in e.OldItems) {
                if (thisit is ICanDropMessages dm) {
                    dm.DropMessage -= FormWithStatusBar.GotMessageDropMessage;
                }
                if (thisit is Database db) {
                    db.AdditionalRepair += BlueControls.Controls.Table.Database_AdditionalRepair;
                }
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset) {
            Develop.DebugPrint_NichtImplementiert(true);
        }
    }

    #endregion

    //private static void DB_GenerateLayoutInternal(object sender, GenerateLayoutInternalEventArgs e) {
    //    if (e.Handled) { return; }
    //    e.Handled = true;
    //    if (e?.Row?.Database is not Database db) { return; }

    //    var pad = new ItemCollectionPad(e.LayoutId, e.Row.Database, e.Row.Key);
    //    pad.SaveAsBitmap(e.Filename);
    //}
}