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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Extended_Text;
using BlueControls.Interfaces;
using BlueDatabase;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.AxHost;

namespace BlueControls.ItemCollection;

public class AddRowPaditem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptFilter {

    #region Fields

    private readonly ItemAcceptFilter _itemAccepts;
    private string _anzeige = string.Empty;
    private ExtText? _eTxt;

    #endregion

    #region Constructors

    public AddRowPaditem(string keyname, string toParse) : this(keyname) => Parse(toParse);

    public AddRowPaditem(string intern) : base(intern) => _itemAccepts = new();

    #endregion

    #region Properties

    public static string ClassId => "FI-AddRowButton";

    [Description("Nach welchem Format die Zeilen angezeigt werden sollen. Es können Variablen im Format ~Variable~ benutzt werden. Achtung, KEINE Skript-Variaben, nur Spaltennamen.")]
    public string Anzeige {
        get => _anzeige;
        set {
            if (_anzeige == value) { return; }
            _anzeige = value;
            OnChanged();
        }
    }

    public override string Description => "Dieses Element wird als Knopf mit einem Pluszeichen dargstellt.<br>Das Elemenz kann Filter empfangen und mit diesen Filtern eine neue Zeile anlegen";

    public ReadOnlyCollection<string>? GetFilterFrom {
        get => _itemAccepts.GetFilterFromKeysGet();
        set => _itemAccepts.GetFilterFromKeysSet(value, this);
    }

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);

    public DatabaseAbstract? InputDatabase => _itemAccepts.InputDatabase(this);

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override Control CreateControl(ConnectedFormulaView parent) {
        var con = new ButtonAddRow();

        con.DoInputSettings(parent, this);
        //con.DoOutputSettings(this);

        return con;
    }

    public override string ErrorReason() {
        if (InputDatabase == null || InputDatabase.IsDisposed) {
            return "Quelle fehlt";
        }
        //if (OutputDatabase == null || OutputDatabase.IsDisposed) {
        //    return "Ziel fehlt";
        //}
        return string.Empty;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();
        l.AddRange(_itemAccepts.GetStyleOptions(this));

        l.Add(new FlexiControl());
        l.AddRange(base.GetStyleOptions());
        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }

        if (_itemAccepts.ParseThis(tag, value)) { return true; }

        switch (tag) {
            case "showformat":
                _anzeige = value.FromNonCritical();
                return true;
        }
        return false;
    }

    public override string ReadableText() {
        var txt = "Neue Zeile anlegen: ";

        if (IsOk() && InputDatabase != null) {
            return txt + InputDatabase.Caption;
        }

        return txt + ErrorReason();
    }

    public override QuickImage? SymbolForReadableText() {
        if (IsOk()) {
            return QuickImage.Get(ImageCode.PlusZeichen, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        var result = new List<string>();

        result.AddRange(_itemAccepts.ParsableTags());

        result.ParseableAdd("ShowFormat", _anzeige);
        return result.Parseable(base.ToString());
    }

    internal override void AddedToCollection() {
        base.AddedToCollection();
        //_itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
        //RepairConnections();
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true);
        }

        _eTxt ??= new ExtText(Design.Button, States.Standard);
        Controls.Button.DrawButton(null, gr, Design.Button, States.Standard, QuickImage.Get(ImageCode.PlusZeichen), Alignment.Top_HorizontalCenter, false, _eTxt, "xxx", positionModified.ToRect(), false);

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    protected override void ParseFinished() {
        base.ParseFinished();
        //_itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    #endregion
}