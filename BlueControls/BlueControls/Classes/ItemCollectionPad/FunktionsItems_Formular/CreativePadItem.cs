﻿// Authors:
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
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

// ReSharper disable once UnusedMember.Global
public class CreativePadItem : ReciverControlPadItem, IItemToControl, IAutosizable {

    #region Fields

    private int _autoRefresh = 1;
    private float _defaultCopyScale = 1f;
    private string _design = string.Empty;
    private string _formular = string.Empty;
    private float _scale = 1f;
    private string _script = string.Empty;
    private string _typ = "Load";

    #endregion

    #region Constructors

    public CreativePadItem() : this(string.Empty, null) { }

    public CreativePadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) { }

    #endregion

    #region Properties

    // ReSharper disable once UnusedMember.Global
    public static string ClassId => "FI-CreativePad";

    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;

    public int AutoRefresh {
        get => _autoRefresh;
        set {
            if (IsDisposed) { return; }
            value = Math.Max(value, 0);
            value = Math.Min(value, 600);

            if (_autoRefresh == value) { return; }
            _autoRefresh = value;
            OnPropertyChanged();
        }
    }

    public bool AutoSizeableHeight => true;

    public override bool DatabaseInputMustMatchOutputDatabase => false;

    public float DefaultCopyScale {
        get => _defaultCopyScale;
        set {
            if (IsDisposed) { return; }
            value = Math.Max(value, 0.3f);
            value = Math.Min(value, 20f);
            value = (float)Math.Round(value, 2, MidpointRounding.AwayFromZero);

            if (Math.Abs(_defaultCopyScale - value) < Constants.DefaultTolerance) { return; }
            _defaultCopyScale = value;
            OnPropertyChanged();
        }
    }

    public override string Description => "Ein Steuerelement, das ein generiertes optisches Dokument anzeigt.";

    public string Design {
        get => _design;
        set {
            if (IsDisposed) { return; }
            if (_design == value) { return; }
            _design = value;
            OnPropertyChanged();
        }
    }

    public string Formular {
        get => _formular;
        set {
            if (IsDisposed) { return; }
            if (_formular == value) { return; }
            _formular = value;
            OnPropertyChanged();
        }
    }

    public override bool InputMustBeOneRow => true;

    public override bool MustBeInDrawingArea => true;

    public float Scale {
        get => _scale;
        set {
            if (IsDisposed) { return; }
            value = Math.Max(value, 0.3f);
            value = Math.Min(value, 20f);
            value = (float)Math.Round(value, 2, MidpointRounding.AwayFromZero);

            if (Math.Abs(_scale - value) < Constants.DefaultTolerance) { return; }
            _scale = value;
            OnPropertyChanged();
        }
    }

    public string Script {
        get => _script;

        set {
            if (value == _script) { return; }
            _script = value;
            OnPropertyChanged();
        }
    }

    public string Typ {
        get => _typ;
        set {
            if (IsDisposed) { return; }
            if (_typ == value) { return; }
            _typ = value;
            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
        }
    }

    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new ConnectedCreativePad();

        switch (_typ.ToLower()) {
            case "load":
                con.LoadAtRowChange = _formular;
                break;

            case "script":
                con.ExecuteScriptAtRowChange = _script;
                con.DefaultDesign = _design;
                break;
        }

        con.DefaultCopyScale = DefaultCopyScale;
        con.AutoRefresh = AutoRefresh;

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        switch (_typ.ToLower()) {
            case "load":

                if (string.IsNullOrEmpty(_formular)) { return "Kein Layout gewählt."; }
                break;

            case "script":

                if (string.IsNullOrEmpty(_script)) { return "Kein Skript angegeben."; }
                break;
        }

        return base.ErrorReason();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        var layouts = new List<AbstractListItem>();

        if (DatabaseInput is { IsDisposed: false } db) {

            #region Verfügbare Layouts ermitteln

            if (Directory.Exists(db.AdditionalFilesPathWhole())) {
                var f = Directory.GetFiles(db.AdditionalFilesPathWhole(), "*.bcr");

                layouts.AddRange(ItemsOf(f));
            }

            #endregion

            //#region Verfügbare Skripte ermitteln

            //foreach (var thise in db.EventScript) {
            //    if (!thise.ChangeValuesAllowed && thise.NeedRow) {
            //        scripte.Add(ItemOf(thise));
            //    }
            //}

            ////if (Directory.Exists(db.AdditionalFilesPathWhole())) {
            ////    var f = Directory.GetFiles(db.AdditionalFilesPathWhole(), "*.bcr");

            ////    layouts.AddRange(ItemsOf(f));
            ////}

            //#endregion
        }

        #region Verfügbare Typen ermitteln

        List<AbstractListItem> comms =
        [
            ItemOf("Layout Datei laden", "Load", QuickImage.Get("Ordner|24")),
            ItemOf("Per Skript erzeugen", "Script", QuickImage.Get("Skript|24"))
        ];

        #endregion

        List<GenericControl> result =
            [.. base.GetProperties(widthOfControl),
                new FlexiControl("Einstellungen:", widthOfControl, true),
                new FlexiControlForProperty<string>(() => Typ, comms),
                new FlexiControlForProperty<float>(() => DefaultCopyScale),
                new FlexiControlForProperty<int>(() => AutoRefresh)
            ];

        switch (_typ.ToLower()) {
            case "load":
                result.Add(new FlexiControl("Lädt folgendes Skript vom Dateisystem und ersetzt die Variablen", widthOfControl, false));
                result.Add(new FlexiControlForProperty<string>(() => Formular, layouts));
                result.Add(new FlexiControlForDelegate(Skripte_Bearbeiten, "Skripte bearbeiten", ImageCode.Skript));
                result.Add(new FlexiControl("Info: Es wird zuvor das Skript 'Export' ausgeführt. Auch die Variablen aus dem Skript können benutzt werden.", widthOfControl, false));
                break;

            case "script":
                result.Add(new FlexiControl("Erstellt ein leeres Blatt, das durch das folgende Skript befüllt wird.", widthOfControl, false));
                result.Add(new FlexiControlForDelegate(Skript_Bearbeiten, "Skript bearbeiten", ImageCode.Skript));
                var art = new List<AbstractListItem>();
                art.AddRange(ItemsOf(Skin.AllStyles()));
                result.Add(new FlexiControlForProperty<string>(() => Design, art));
                result.Add(new FlexiControlForProperty<float>(() => Scale));

                //result.Add(new FlexiControl("Info: Das Skript darf keine Werte ändern und muss sich auf eine Zeile beziehen. Außerdem muss die Variable PAD definiert werden: var PAD = ItemCollectionPadItem();", widthOfControl, false));
                break;
        }

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Typ", _typ);
        result.ParseableAdd("FormulaFile", _formular);
        result.ParseableAdd("Script", _script);
        result.ParseableAdd("Design", _design);
        result.ParseableAdd("Scale", _scale);
        result.ParseableAdd("CopyScale", _defaultCopyScale);
        result.ParseableAdd("AutoRefresh", _autoRefresh);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "typ":
                _typ = value.FromNonCritical();
                return true;

            case "formula":
            case "formulafile":
                _formular = value.FromNonCritical();
                return true;

            case "script":
                _script = value.FromNonCritical();
                return true;

            case "scale":
                _scale = FloatParse(value.FromNonCritical());
                return true;

            case "copyscale":
                _defaultCopyScale = FloatParse(value.FromNonCritical());
                return true;

            case "autorefresh":
                _autoRefresh = IntParse(value.FromNonCritical());
                return true;

            case "design":
                _design = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Layout-Generator";

    /// <summary>
    /// Internes Skript
    /// </summary>
    public void Skript_Bearbeiten() {
        var se = IUniqueWindowExtension.ShowOrCreate<CreativePadScriptEditor>(this);
        se.Database = DatabaseInput;
    }

    /// <summary>
    /// Skripte der Datenbank
    /// </summary>
    public void Skripte_Bearbeiten() {
        if (DatabaseInput is { IsDisposed: false } db) {
            TableView.OpenScriptEditor(db);
        }
        OnDoUpdateSideOptionMenu();
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Layout, 16, Skin.IdColor(InputColorId), Color.Transparent);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, float scale, float shiftX, float shiftY) {
        DrawColorScheme(gr, positionModified, scale, null, false, false, false);
        //var headh = 25 * zoom;
        //var headb = 70 * zoom;

        //var body = positionModified with { Y = positionModified.Y + headh, Height = positionModified.Height - headh };
        //var c = -1;
        //foreach (var thisC in _childs) {
        //    c++;
        //    var it = new RectangleF(positionModified.X + (c * headb), positionModified.Y, headb, headh);

        //    gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 200, 200, 200)), it);

        //    Skin.Draw_FormatedText(gr, thisC.FileNameWithoutSuffix(), null, Alignment.Horizontal_Vertical_Center, it.ToRect(), ColumnFont?.Scale(zoom), false);
        //    gr.DrawRectangle(new Pen(Color.Black, zoom), it);
        //}

        //gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 200, 200, 200)), body);
        //gr.DrawRectangle(new Pen(Color.Black, zoom), body);

        ////Skin.Draw_FormatedText(gr, _text, QuickImage.Get(ImageCode.Textfeld, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);
        ////Skin.Draw_FormatedText(gr, "Register-\r\nkarten", null, Alignment.Horizontal_Vertical_Center, body.ToRect(), ColumnFont?.Scale(zoom), false);

        //if (!ForPrinting) {
        //    DrawColorScheme(gr, positionModified, scale, InputColorId, true, true, true);
        //}

        base.DrawExplicit(gr, visibleArea, positionModified, scale, shiftX, shiftY);

        DrawArrorInput(gr, positionModified, scale, ForPrinting, InputColorId);
    }

    #endregion
}