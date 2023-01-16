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

using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.EventArgs;
using System;
using System.CodeDom;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace BlueControls;

public static class Allgemein {

    #region Fields

    private static bool _serviceStarted;

    #endregion

    #region Methods

    public static T? ParentControl<T>(this object o) {
        if (o is not System.Windows.Forms.Control co) {
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
        ItemCollectionList? thisContextMenu = new(BlueListBoxAppearance.KontextMenu);
        ItemCollectionList userMenu = new(BlueListBoxAppearance.KontextMenu);
        List<string> tags = new();
        var cancel = false;
        var translate = true;
        par.GetContextMenuItems(null, thisContextMenu, out var hotItem, tags, ref cancel, ref translate);
        if (cancel) { return null; }
        ContextMenuInitEventArgs ec = new(hotItem, tags, userMenu);
        par.OnContextMenuInit(ec);
        if (ec.Cancel) {
            return null;
        }

        if (thisContextMenu != null && thisContextMenu.Count > 0) {
            return par;
        }
        if (userMenu.Count > 0) {
            return par;
        }
        return null;
    }

    public static void StartDatabaseService() {
        if (_serviceStarted) { return; }
        _serviceStarted = true;
        DatabaseAbstract.AllFiles.ItemAdded += AllFiles_ItemAdded;
        DatabaseAbstract.AllFiles.ItemRemoving += AllFiles_ItemRemoving;
        //Database.DropConstructorMessage += Database_DropConstructorMessage;
    }

    public static void UpdateStatusBar(string jobname, string text) {
        if (string.IsNullOrEmpty(jobname) && string.IsNullOrEmpty(text)) {
            UpdateStatusBar(FehlerArt.Info, string.Empty, false);
        } else {
            UpdateStatusBar(FehlerArt.Info, "[" + jobname + " " + DateTime.Now.ToString("HH:mm:ss") + "] " + text, false);
        }
    }

    public static void UpdateStatusBar(FehlerArt type, string text, bool addtime) {
        if (addtime && !string.IsNullOrEmpty(text)) {
            text = DateTime.Now.ToString("HH:mm:ss") + " " + text;
        }

        var did = false;
        foreach (var thisf in FormManager.Forms) {
            if (thisf is IHasStatusbar fd) {
                var x = fd.UpdateStatus(type, text, did);
                if (x) { did = true; }
            }
        }
    }

    private static void AllFiles_ItemAdded(object sender, ListEventArgs e) {
        if (e.Item is DatabaseAbstract db) {
            db.GenerateLayoutInternal += DB_GenerateLayoutInternal;
            db.Loaded += TableView.CheckDatabase;
            db.DropMessage += Db_DropMessage;
        }
    }

    private static void AllFiles_ItemRemoving(object sender, ListEventArgs e) {
        if (e.Item is DatabaseAbstract db) {
            db.GenerateLayoutInternal -= DB_GenerateLayoutInternal;
            db.Loaded -= TableView.CheckDatabase;
            db.DropMessage -= Db_DropMessage;
        }
    }

    private static void Db_DropMessage(object sender, MessageEventArgs e) {
        UpdateStatusBar(e.Type, e.Message, true);
    }

    private static void DB_GenerateLayoutInternal(object sender, GenerateLayoutInternalEventArgs e) {
        if (e.Handled) { return; }
        e.Handled = true;
        if (e?.Row?.Database == null) { return; }

        var pad = new ItemCollectionPad(e.LayoutId, e.Row.Database, e.Row.Key);
        pad.SaveAsBitmap(e.Filename);
    }

    #endregion
}