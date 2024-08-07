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
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Dieses Element kann Filter empfangen, und gibt dem Nutzer die Möglichkeit, aus dem daraus resultierenden Zeilen EINE zu wählen.
/// Per Tabellenansicht
/// </summary>

public class TableViewPadItem : ReciverSenderControlPadItem, IItemToControl, IReadableText, IItemSendFilter, IAutosizable {

    #region Fields

    private readonly ItemSendFilter _itemSends;
    private string _defaultArrangement = string.Empty;
    private Filterausgabe _filterOutputType = Filterausgabe.Gewähle_Zeile;

    #endregion

    #region Constructors

    public TableViewPadItem(string keyName, string toParse) : this(keyName) => this.Parse(toParse);

    public TableViewPadItem(Database? db) : this(string.Empty, db, null) { }

    public TableViewPadItem(string keyName, Database? db, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) {
        _itemSends = new();

        DatabaseOutput = db;
    }

    public TableViewPadItem(string keyName) : this(keyName, null, null) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-TableView";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.More;
    public bool AutoSizeableHeight => true;

    public ReadOnlyCollection<string> ChildIds {
        get => _itemSends.ChildIdsGet();
        set => _itemSends.ChildIdsSet(value, this);
    }

    public override bool DatabaseInputMustMatchOutputDatabase => true;

    public Database? DatabaseOutput {
        get => _itemSends.DatabaseOutputGet(this);
        set => _itemSends.DatabaseOutputSet(value, this);
    }

    public override string Description => "Darstellung einer Datenbank in Tabellen-Form.";

    public Filterausgabe FilterOutputType {
        get => _filterOutputType;
        set {
            if (IsDisposed) { return; }
            if (_filterOutputType == value) { return; }
            _filterOutputType = value;
            OnPropertyChanged();
        }
    }

    public override bool InputMustBeOneRow => false;
    public override bool MustBeInDrawingArea => true;
    public override string MyClassId => ClassId;

    public int OutputColorId {
        get => _itemSends.OutputColorIdGet();
        set => _itemSends.OutputColorIdSet(value, this);
    }

    [DefaultValue("")]
    public string Standard_Ansicht {
        get => _defaultArrangement;
        set {
            if (IsDisposed) { return; }
            if (_defaultArrangement == value) { return; }
            _defaultArrangement = value;
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add) => _itemSends.AddChild(this, add);

    public override void AddedToCollection() {
        base.AddedToCollection();
        _itemSends.DoCreativePadAddedToCollection(this);
    }

    public System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new Table();
        con.DatabaseSet(DatabaseOutput, string.Empty);
        con.DoDefaultSettings(parent, this, mode);
        con.Arrangement = _defaultArrangement;
        con.EditButton = string.Equals(Generic.UserGroup, Constants.Administrator, StringComparison.OrdinalIgnoreCase);
        return con;
    }

    public override string ErrorReason() {
        var b = base.ErrorReason();
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = base.ErrorReason();
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemSends.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        return string.Empty;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> l = [];

        l.AddRange(base.GetProperties(widthOfControl));

        l.Add(new FlexiControl("Eigenschaften:", widthOfControl, true));

        if (DatabaseOutput is Database db && !db.IsDisposed) {
            var u2 = new List<AbstractListItem>();
            foreach (var thisC in db.ColumnArrangements) {
                u2.Add(ItemOf(thisC));
            }
            l.Add(new FlexiControlForProperty<string>(() => Standard_Ansicht, u2));
        }

        l.AddRange(_itemSends.GetProperties(this, widthOfControl));

        if (DatabaseOutput is Database db2 && !db2.IsDisposed) {
            var u = new List<AbstractListItem>();
            u.AddRange(ItemsOf(typeof(Filterausgabe)));
            l.Add(new FlexiControlForProperty<Filterausgabe>(() => FilterOutputType, u));
        }

        l.Add(new FlexiControl());
        l.AddRange(base.GetProperties(widthOfControl));

        return l;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        _itemSends.ParseFinished(this);
    }

    public override bool ParseThis(string key, string value) {
        if (base.ParseThis(key, value)) { return true; }
        if (_itemSends.ParseThis(key, value)) { return true; }
        switch (key) {
            case "id":
                return true;

            case "defaultarrangement":
                _defaultArrangement = value.FromNonCritical();
                return true;
        }
        return false;
    }

    public override string ReadableText() {
        const string txt = "Tabellenansicht: ";

        if (this.IsOk() && DatabaseOutput != null) {
            return txt + DatabaseOutput.Caption;
        }

        return txt + ErrorReason();
    }

    public void RemoveChild(ReciverControlPadItem remove) => _itemSends.RemoveChild(remove, this);

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));

            //return QuickImage.Get(ImageCode.Datenbank, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemSends.ParsableTags(this)];
        result.ParseableAdd("DefaultArrangement", _defaultArrangement);
        return result.Parseable(base.ToParseableString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawArrowOutput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, OutputColorId);
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        DrawArrorInput(gr, positionModified, zoom, forPrinting, InputColorId);
    }

    #endregion
}