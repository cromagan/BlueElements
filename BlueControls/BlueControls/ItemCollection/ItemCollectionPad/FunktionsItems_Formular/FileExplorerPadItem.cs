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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.ItemCollection;

public class FileExplorerPadItem : FakeControlPadItem, IItemAcceptRow, IAutosizable {

    #region Fields

    private readonly ItemAcceptRow _itemAccepts;
    private bool _bei_Bedarf_Erzeugen;
    private bool _leere_Ordner_Löschen;
    private string _pfad = string.Empty;

    #endregion

    #region Constructors

    public FileExplorerPadItem(string internalname) : base(internalname) {
        _itemAccepts = new();
        SetCoordinates(new RectangleF(0, 0, 50, 30), true);
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-FileExplorer";
    public bool AutoSizeableHeight => true;

    [Description("Ob das Verzeichniss bei Bedarf erzeugt werden soll.")]
    public bool Bei_Bedarf_erzeugen {
        get => _bei_Bedarf_Erzeugen;

        set {
            if (value == _bei_Bedarf_Erzeugen) { return; }
            _bei_Bedarf_Erzeugen = value;
            this.RaiseVersion();
            OnChanged();
        }
    }

    public override string Description => "Dieses Element erzeugt eine File-Explorer-Steuerelement,\r\nwmit welchem interagiert werden kann.";

    public IItemSendRow? GetRowFrom {
        get => _itemAccepts.GetRowFromGet(this);
        set => _itemAccepts.GetRowFromSet(value, this);
    }

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);

    public DatabaseAbstract? InputDatabase => _itemAccepts.InputDatabase(this);

    [Description("Wenn angewählt, wird bei einer Änderung des Pfades geprüft, ob das Vereichniss leer ist.\r\nIst das der Fall, wird es gelöscht.")]
    public bool Leere_Ordner_löschen {
        get => _leere_Ordner_Löschen;

        set {
            if (value == _leere_Ordner_Löschen) { return; }
            _leere_Ordner_Löschen = value;
            this.RaiseVersion();
            OnChanged();
        }
    }

    [Description("Der Dateipfad, dessen Dateien angezeigt werden sollen.\r\nEs können Variablen aus dem Skript benutzt werden.\r\nDiese müssen im Format ~variable~ angegeben werden.")]
    public string Pfad {
        get => _pfad;

        set {
            if (value == _pfad) { return; }
            _pfad = value;
            this.RaiseVersion();
            OnChanged();
        }
    }

    protected override int SaveOrder => 4;

    #endregion

    #region Methods

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override Control CreateControl(ConnectedFormulaView parent) {
        var con = new FileBrowser {
            OriginalText = Pfad,
            CreateDir = _bei_Bedarf_Erzeugen,
            DeleteDir = _leere_Ordner_Löschen
        };
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

        l.Add(new FlexiControlForProperty<string>(() => Pfad));
        l.Add(new FlexiControlForProperty<bool>(() => Bei_Bedarf_erzeugen));
        l.Add(new FlexiControlForProperty<bool>(() => Leere_Ordner_löschen));

        l.Add(new FlexiControl());
        l.AddRange(base.GetStyleOptions());
        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        if (_itemAccepts.ParseThis(tag, value)) { return true; }

        switch (tag) {
            case "pfad":
                _pfad = value.FromNonCritical();
                return true;

            case "createdir":
                _bei_Bedarf_Erzeugen = value.FromPlusMinus();
                return true;

            case "deletedir":
                _leere_Ordner_Löschen = value.FromPlusMinus();
                return true;
        }
        return false;
    }

    public override string ReadableText() {
        var txt = "Dateisystem: ";

        if (IsOk() && InputDatabase != null) {
            return txt + InputDatabase.Caption;
        }

        return txt + ErrorReason();
    }

    public override QuickImage? SymbolForReadableText() {
        if (IsOk()) {
            return QuickImage.Get(ImageCode.Ordner, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        var result = new List<string>();

        result.AddRange(_itemAccepts.ParsableTags());

        result.ParseableAdd("Pfad", _pfad);
        result.ParseableAdd("CreateDir", _bei_Bedarf_Erzeugen);
        result.ParseableAdd("DeleteDir", _leere_Ordner_Löschen);
        return result.Parseable(base.ToString());
    }

    internal override void AddedToCollection() {
        base.AddedToCollection();
        //_itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
        //RepairConnections();
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        //var id = GetRowFrom?.OutputColorId ?? -1;

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        }

        DrawFakeControl(gr, positionModified, zoom, ÜberschriftAnordnung.Über_dem_Feld, "C:\\");

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        DrawArrorInput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", InputColorId);
    }

    protected override void ParseFinished() {
        base.ParseFinished();
        //_itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    #endregion

    //protected override BasicPadItem? TryCreate(string id, string name) {
    //    if (id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new FileExplorerPadItem(name);
    //    }
    //    return null;
    //}
}