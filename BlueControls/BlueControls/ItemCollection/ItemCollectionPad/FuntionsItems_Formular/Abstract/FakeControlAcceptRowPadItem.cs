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

using static BlueControls.Interfaces.IHasVersionExtensions;

namespace BlueControls.ItemCollection;

/// <summary>
/// Standard für Objekte, die einen Datenbank/Zeilenbezug haben.
/// Stellt auch alle Methode breit, zum Einrichten der Breite und Benutzer-Sichtbarkeiten.
/// Nur Tabs, die ein solches Objekt haben, werden als anzeigewürdig gewertet
/// </summary>
public abstract class FakeControlAcceptRowPadItem : FakeControlPadItem, IItemToControl, IItemAcceptRow {

    #region Fields

    private string? _getValueFromkey;
    private IItemSendRow? _tmpgetValueFrom;

    #endregion

    #region Constructors

    protected FakeControlAcceptRowPadItem(string internalname) : base(internalname) { }

    #endregion

    #region Properties

    [Description("Wählt ein Zeilen-Objekt, aus der die Werte kommen.")]
    public string Datenquelle_wählen {
        get => string.Empty;
        set => FakeControlPadItem.Datenquelle_wählen_Zeile(this);
    }

    public IItemSendRow? GetRowFrom {
        get {
            if (Parent == null || _getValueFromkey == null) { return null; }

            _tmpgetValueFrom ??= Parent[_getValueFromkey] as IItemSendRow;

            return _tmpgetValueFrom;
        }
        set {
            var f = GetRowFrom;
            this.ChangeGetRowFrom(ref f, value);

            _tmpgetValueFrom = f;
            _getValueFromkey = null;

            _getValueFromkey = f?.KeyName ?? string.Empty;
        }
    }

    #endregion

    #region Methods

    public override Control? CreateControl(ConnectedFormulaView parent) => throw new NotImplementedException();

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new() {
            new FlexiControlForProperty<string>(() => Datenquelle_wählen, ImageCode.Pfeil_Rechts),

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
            case "getvaluefrom":
                _getValueFromkey = value.FromNonCritical();

                return true;
        }
        return false;
    }

    public override string ToString() {
        var result = new List<string>();

        result.ParseableAdd("GetValueFrom", _getValueFromkey);

        return result.Parseable(base.ToString());
    }

    #endregion
}