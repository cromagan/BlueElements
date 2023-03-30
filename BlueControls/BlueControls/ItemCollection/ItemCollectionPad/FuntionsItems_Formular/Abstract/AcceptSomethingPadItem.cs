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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueBasics.Converter;
using static BlueControls.Interfaces.IItemSendSomethingExtensions;
using static BlueControls.Interfaces.IHasVersionExtensions;
using System.Collections.ObjectModel;

using static BlueControls.Interfaces.IItemSendSomethingExtensions;

namespace BlueControls.ItemCollection;

/// <summary>
/// Standard für Objekte, die einen Datenbank/Zeilenbezug haben.
/// Stellt auch alle Methode breit, zum Einrichten der Breite und Benutzer-Sichtbarkeiten.
/// Nur Tabs, die ein solches Objekt haben, werden als anzeigewürdig gewertet
/// </summary>
public abstract class AcceptSomethingPadItem : RectanglePadItemWithVersion, IItemToControl, IItemAcceptSomething {

    #region Fields

    protected int _inputColorId = -1;

    #endregion

    #region Constructors

    protected AcceptSomethingPadItem(string internalname) : base(internalname) {
        SetCoordinates(new RectangleF(0, 0, 50, 30), true);
        _inputColorId = -1;
    }

    #endregion

    #region Properties

    protected override int SaveOrder => 3;

    #endregion

    #region Methods

    public static void Datenquelle_hinzufügen_Filter(IItemAcceptFilter item) {
        if (item.Parent is null) { return; }

        var x = new ItemCollectionList.ItemCollectionList(true);
        foreach (var thisR in item.Parent) {
            if (thisR.IsVisibleOnPage(item.Page) && thisR is IItemSendFilter rfp) {
                _ = x.Add(rfp);
            }
        }

        _ = x.Add("<Abbruch>");

        var it = InputBoxListBoxStyle.Show("Quelle hinzufügen:", x, AddType.None, true);

        if (it == null || it.Count != 1) { return; }

        var t = item.Parent[it[0]];

        if (t is IItemSendFilter rfp2) {
            var l = new List<IItemSendFilter>();
            l.AddRange(item.GetFilterFrom);
            l.AddIfNotExists(rfp2);
            item.GetFilterFrom = new System.Collections.ObjectModel.ReadOnlyCollection<IItemSendFilter>(l);
        }
    }

    public static void Datenquelle_wählen_Zeile(IItemAcceptRow item) {
        if (item.Parent is null) { return; }

        var x = new ItemCollectionList.ItemCollectionList(true);
        foreach (var thisR in item.Parent) {
            if (thisR.IsVisibleOnPage(item.Page) && thisR is IItemSendRow rfp) {
                _ = x.Add(rfp);
            }
        }

        _ = x.Add("<Keine Quelle>");

        var it = InputBoxListBoxStyle.Show("Quelle wählen:", x, AddType.None, true);

        if (it == null || it.Count != 1) { return; }

        var newGetRowFrom = item.Parent[it[0]];

        if (newGetRowFrom is IItemSendRow rfp2) {
            item.GetRowFrom = rfp2;
        } else {
            item.GetRowFrom = null;
        }
        item.RaiseVersion();
        item.OnChanged();
    }

    public virtual Control? CreateControl(ConnectedFormulaView parent) => throw new NotImplementedException();

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
        }
        return false;
    }

    public void SetInputColorId(int inputColorId) {
        if (_inputColorId == inputColorId) { return; }

        _inputColorId = inputColorId;
        OnChanged();
    }

    public override string ToString() {
        var result = new List<string>();

        return result.Parseable(base.ToString());
    }

    #endregion
}