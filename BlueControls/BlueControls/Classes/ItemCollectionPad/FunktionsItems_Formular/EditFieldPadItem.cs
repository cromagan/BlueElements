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
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Erzeut ein FlexiControllForCell
/// Standard-Bearbeitungs-Feld
/// </summary>
public class EditFieldPadItem : ReciverControlPadItem, IItemToControl, IAutosizable {

    #region Fields

    private bool _autoX = true;
    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld;
    private string _columnName = string.Empty;
    private CaptionPosition _überschriftanordung = CaptionPosition.Über_dem_Feld;

    #endregion

    #region Constructors

    public EditFieldPadItem(string keyName) : this(keyName, null) { }

    public EditFieldPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-EditField";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;

    public bool AutoSizeableHeight {
        get {
            if (_bearbeitung == EditTypeFormula.nur_als_Text_anzeigen) {
                return (int)UsedArea.Height > AutosizableExtension.MinHeigthCaption;
            }

            if (_überschriftanordung is CaptionPosition.Links_neben_dem_Feld or CaptionPosition.ohne) {
                return (int)UsedArea.Height > AutosizableExtension.MinHeigthTextBox;
            }

            return (int)UsedArea.Height > AutosizableExtension.MinHeigthCapAndBox;
        }
    }

    [DefaultValue(true)]
    public bool AutoX {
        get => _autoX;
        set {
            if (IsDisposed) { return; }
            if (_autoX == value) { return; }
            _autoX = value;
            OnPropertyChanged();
        }
    }

    public CaptionPosition CaptionPosition {
        get => _überschriftanordung;
        set {
            if (IsDisposed) { return; }
            if (_überschriftanordung == value) { return; }
            _überschriftanordung = value;
            OnPropertyChanged();
        }
    }

    public ColumnItem? Column {
        get {
            var c = DatabaseInput?.Column[_columnName];
            return c is not { IsDisposed: not true } ? null : c;
        }
    }

    public string ColumnName {
        get => _columnName;
        set {
            if (IsDisposed) { return; }
            if (_columnName == value) { return; }
            _columnName = value;
            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
        }
    }

    public override bool DatabaseInputMustMatchOutputDatabase => false;
    public override string Description => "Standard Bearbeitungs-Steuerelement für Zellen.";

    public EditTypeFormula EditType {
        get => _bearbeitung;
        set {
            if (IsDisposed) { return; }
            if (_bearbeitung == value) { return; }
            _bearbeitung = value;
            OnPropertyChanged();
        }
    }

    public override bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;
    public override string MyClassId => ClassId;

    #endregion

    #region Methods

    //public Bitmap? Bitmap { get; set; }
    //public enSizeModes Bild_Modus { get; set; }
    public static List<AbstractListItem> GetAllowedEditTypes(ColumnItem? column) {
        var l = new List<AbstractListItem>();
        if (column is not { IsDisposed: not true }) { return l; }
        var t = typeof(EditTypeFormula);

        foreach (int z1 in Enum.GetValues(t)) {
            if (column.UserEditDialogTypeInFormula((EditTypeFormula)z1)) {
                l.Add(new TextListItem(Enum.GetName(t, z1).Replace("_", " "), z1.ToString(), null, false, true, string.Empty));
            }
        }
        return l;
    }

    public System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent, string mode) {
        //var ff = parent.SearchOrGenerate(rfw2);

        var con = new FlexiControlForCell {
            ColumnName = _columnName,
            EditType = EditType,
            CaptionPosition = CaptionPosition
        };

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        if (Column is not { IsDisposed: not true }) { return "Spalte fehlt"; }

        return base.ErrorReason();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [.. base.GetProperties(widthOfControl)];

        if (DatabaseInput is not { IsDisposed: false } db) { return result; }

        result.Add(new FlexiControl("Einstellungen:", widthOfControl, true));

        var lst = new List<AbstractListItem>();
        lst.AddRange(ItemsOf(db.Column, true));

        result.Add(new FlexiControlForProperty<string>(() => ColumnName, lst));

        if (Column is not { IsDisposed: not true }) { return result; }

        var u = new List<AbstractListItem>();
        u.AddRange(ItemsOf(typeof(CaptionPosition)));
        result.Add(new FlexiControlForProperty<CaptionPosition>(() => CaptionPosition, u));
        result.Add(new FlexiControlForProperty<bool>(() => AutoX));

        var b = new List<AbstractListItem>();
        b.AddRange(GetAllowedEditTypes(Column));
        result.Add(new FlexiControlForProperty<EditTypeFormula>(() => EditType, b));

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "column":
                //Column = GetRowFrom?.Database?.Column.SearchByKey(LongParse(value));
                return true;

            case "columnname":
                _columnName = value;
                return true;

            case "edittype":
                _bearbeitung = (EditTypeFormula)IntParse(value);
                return true;

            case "caption":
                _überschriftanordung = (CaptionPosition)IntParse(value);
                return true;

            case "autodistance":
                _autoX = value.FromPlusMinus();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Zelle: ";

    
            return txt + Column?.Caption;
  
    }

    public override QuickImage? SymbolForReadableText() {
        return QuickImage.Get(ImageCode.Stift, 16);
    }

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];

        result.ParseableAdd("ColumnName", _columnName);
        result.ParseableAdd("EditType", _bearbeitung);
        result.ParseableAdd("Caption", _überschriftanordung);
        result.ParseableAdd("AutoDistance", _autoX);
        return result.Parseable(base.ToParseableString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        }

        //if (Column  ==null || Column .IsDisposed) {
        //    Skin.Draw_FormatedText(gr, "Spalte fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFnt.Scale(zoom), true);
        //} else {
        DrawFakeControl(gr, positionModified, zoom, CaptionPosition, Column?.ReadableText() + ":", EditType);
        //}

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);

        DrawArrorInput(gr, positionModified, zoom, forPrinting, InputColorId);
    }

    #endregion

    //public bool ReplaceVariable(Variable variable) {
    //    if (string.IsNullOrEmpty(Platzhalter_Für_Layout)) { return false; }
    //    if ("~" + variable.Name.ToLowerInvariant() + "~" != Platzhalter_Für_Layout.ToLowerInvariant()) { return false; }
    //    if (variable is not VariableBitmap vbmp) { return false; }
    //    var ot = vbmp.ValueBitmap;
    //    if (ot is Bitmap bmp) {
    //        Bitmap = bmp;
    //        OnPropertyChanged();
    //        return true;
    //    }

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~BitmapPadItem()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
    //public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
    //{
    //    base.DoStyleCommands(sender, Tags, ref CloseMenu);
    //    if (Tags.TagGet("Bildschirmbereich wählen").FromPlusMinus())
    //    {
    //        CloseMenu = false;
    //        if (Bitmap != null)
    //        {
    //            if (MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
    //        }
    //        Bitmap = ScreenShot.GrabArea(null, 2000, 2000).Pic;
    //        return;
    //    }
    //    if (Tags.TagGet("Datei laden").FromPlusMinus())
    //    {
    //        CloseMenu = false;
    //        if (Bitmap != null)
    //        {
    //            if (MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
    //        }
    //        var e = new System.Windows.Forms.OpenFileDialog();
    //        e.CheckFileExists = true;
    //        e.Multiselect = false;
    //        e.Title = "Bild wählen:";
    //        e.Filter = "PNG Portable Network Graphics|*.png|JPG Jpeg Interchange|*.jpg|Bmp Windows Bitmap|*.bmp";
    //        e.ShowDialog();
    //        if (!FileExists(e.FileName))
    //        {
    //            return;
    //        }
    //        Bitmap = (Bitmap)Generic.Image_FromFile(e.FileName);
    //        return;
    //    }
    //    if (Tags.TagGet("Skalieren").FromPlusMinus())
    //    {
    //        CloseMenu = false;
    //        var t = InputBox.Show("Skalierfaktor oder Formel eingeben:", "1", DataFormat.Text);
    //        var sc = modErgebnis.Ergebnis(t);
    //        if (sc == null || sc == 1)
    //        {
    //            Notification.Show("Keine Änderung vorgenommen.");
    //            return;
    //        }
    //        var x = p_RU.X - p_LO.X;
    //        var y = p_RU.Y - p_LO.Y;
    //        p_RU.X = (float)((float)p_LO.X + (float)x * sc);
    //        p_RU.Y = (float)((float)p_LO.Y + (float)y * sc);
    //        KeepInternalLogic();
    //        return;
    //    }
    //    Hintergrund_weiß_füllen = Tags.TagGet("Hintergrund weiß füllen").FromPlusMinus();
    //    Bild_Modus = (enSizeModes)IntParse(Tags.TagGet("Bild-Modus"));
    //    Stil = (PadStyles)IntParse(Tags.TagGet("Umrandung"));
    //    Platzhalter_für_Layout = Tags.TagGet("Platzhalter für Layout").FromNonCritical();
    //}
}