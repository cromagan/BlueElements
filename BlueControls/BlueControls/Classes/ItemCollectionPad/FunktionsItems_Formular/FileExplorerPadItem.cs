// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class FileExplorerPadItem : ReciverControlPadItem, IItemToControl, IAutosizable {

    #region Fields

    private bool _bei_Bedarf_Erzeugen;
    private string _filter = string.Empty;
    private bool _leere_Ordner_Löschen;
    private string _mindest_pfad = string.Empty;
    private string _pfad = string.Empty;

    #endregion

    #region Constructors

    public FileExplorerPadItem() : this(string.Empty, null) { }

    public FileExplorerPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) => SetCoordinates(new RectangleF(0, 0, 50, 30));

    #endregion

    #region Properties

    public static string ClassId => "FI-FileExplorer";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.One;
    public bool AutoSizeableHeight => true;

    [Description("Ob das Verzeichnis bei Bedarf erzeugt werden soll.")]
    public bool Bei_Bedarf_erzeugen {
        get => _bei_Bedarf_Erzeugen;

        set {
            if (IsDisposed) { return; }
            if (value == _bei_Bedarf_Erzeugen) { return; }
            _bei_Bedarf_Erzeugen = value;
            OnPropertyChanged();
        }
    }

    public override bool DatabaseInputMustMatchOutputDatabase => false;
    public override string Description => "Ein Datei-Browser,\r\nmit welchem der Benutzer interagieren kann.";

    public string Filter {
        get => _filter;

        set {
            if (IsDisposed) { return; }
            if (value == _filter) { return; }
            _filter = value;
            OnPropertyChanged();
        }
    }

    public override bool InputMustBeOneRow => true;

    [Description("Wenn angewählt, wird bei einer Änderung des Pfades geprüft, ob das Vereichniss leer ist.\r\nIst das der Fall, wird es gelöscht.")]
    public bool Leere_Ordner_löschen {
        get => _leere_Ordner_Löschen;

        set {
            if (IsDisposed) { return; }
            if (value == _leere_Ordner_Löschen) { return; }
            _leere_Ordner_Löschen = value;
            OnPropertyChanged();
        }
    }

    [Description("Bis zu diesem Pfad kann maximal zurück gegangen werden.\r\nEs können Variablen aus dem Skript benutzt werden.\r\nDiese müssen im Format ~variable~ angegeben werden.")]
    public string Mindest_Pfad {
        get => _mindest_pfad;

        set {
            if (IsDisposed) { return; }
            if (value == _mindest_pfad) { return; }
            _mindest_pfad = value;
            OnPropertyChanged();
        }
    }

    public override bool MustBeInDrawingArea => true;

    [Description("Der Dateipfad, dessen Dateien angezeigt werden sollen.\r\nEs können Variablen aus dem Skript benutzt werden.\r\nDiese müssen im Format ~variable~ angegeben werden.")]
    public string Pfad {
        get => _pfad;

        set {
            if (IsDisposed) { return; }
            if (value == _pfad) { return; }
            _pfad = value;
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 4;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new FileBrowser {
            Var_Directory = Pfad,
            Var_DirectoryMin = Mindest_Pfad,
            Filter = Filter,
            CreateDir = _bei_Bedarf_Erzeugen,
            DeleteDir = _leere_Ordner_Löschen
        };
        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", widthOfControl, true),
            new FlexiControlForProperty<string>(() => Pfad),
            new FlexiControlForProperty<string>(() => Mindest_Pfad),
            new FlexiControlForProperty<string>(() => Filter),
            new FlexiControlForProperty<bool>(() => Bei_Bedarf_erzeugen),
            new FlexiControlForProperty<bool>(() => Leere_Ordner_löschen),
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Path", _pfad);
        result.ParseableAdd("PathMin", _mindest_pfad);
        result.ParseableAdd("Filter", _filter);
        result.ParseableAdd("CreateDir", _bei_Bedarf_Erzeugen);
        result.ParseableAdd("DeleteDir", _leere_Ordner_Löschen);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "path":
            case "pfad":
                _pfad = value.FromNonCritical();
                return true;

            case "filter":
                _filter = value.FromNonCritical();
                return true;

            case "pathmin":
                _mindest_pfad = value.FromNonCritical();
                return true;

            case "createdir":
                _bei_Bedarf_Erzeugen = value.FromPlusMinus();
                return true;

            case "deletedir":
                _leere_Ordner_Löschen = value.FromPlusMinus();
                return true;
        }

        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Dateisystem: ";

        return txt + DatabaseInput?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Ordner, 16, Color.Transparent, Skin.IdColor(InputColorId));

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, float scale, float shiftX, float shiftY) {
        //var id = GetRowFrom?.OutputColorId ?? -1;

        if (!ForPrinting) {
            DrawColorScheme(gr, positionModified, scale, InputColorId, true, true, false);
        }

        DrawFakeControl(gr, positionModified, scale, CaptionPosition.Über_dem_Feld, "C:\\", EditTypeFormula.Listbox);

        if (!ForPrinting) {
            DrawColorScheme(gr, positionModified, scale, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, visibleArea, positionModified, scale, shiftX, shiftY);
        DrawArrorInput(gr, positionModified, scale, ForPrinting, InputColorId);
    }

    #endregion
}