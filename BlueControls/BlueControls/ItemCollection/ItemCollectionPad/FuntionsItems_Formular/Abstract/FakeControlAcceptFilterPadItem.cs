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

namespace BlueControls.ItemCollection;

public abstract class FakeControlAcceptFilterPadItem : FakeControlPadItem, IItemToControl, IItemAcceptFilter {

    #region Fields

    private List<IItemSendFilter> _getFilterFrom = new() { };

    #endregion

    #region Constructors

    protected FakeControlAcceptFilterPadItem(string internalname) : base(internalname) { }

    #endregion

    #region Properties

    [Description("Wählt ein Filter-Objekt, aus der die Werte kommen.")]
    public string Datenquelle_hinzufügen {
        get => string.Empty;
        set => FakeControlPadItem.Datenquelle_hinzufügen_Filter(this);
    }

    public ReadOnlyCollection<IItemSendFilter>? GetFilterFrom {
        get => new(_getFilterFrom);
        set => this.ChangeFilterTo(_getFilterFrom, value);
    }

    #endregion

    #region Methods

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new() {
            new FlexiControlForProperty<string>(() => Datenquelle_hinzufügen, ImageCode.Pfeil_Rechts),

            new FlexiControl(),
            new FlexiControlForProperty<string>(() => Sichtbarkeit, ImageCode.Schild),
            new FlexiControlForProperty<string>(() => Standardhöhe_setzen, ImageCode.GrößeÄndern),
            new FlexiControlForProperty<string>(() => Breite_berechnen, ImageCode.GrößeÄndern)
        };

        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
        }
        return false;
    }

    public override string ToString() {
        var result = new List<string>();

        return result.Parseable(base.ToString());
    }

    #endregion
}