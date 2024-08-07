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
using BlueDatabase;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Dieses Element ist in jedem Formular vorhanden und empfängt die Zeile aus einem anderen Element.
/// Hat NICHT IAcceptRowItem, da es nur von einer einzigen internen Routine befüllt werden darf.
/// Unsichtbares Element, wird nicht angezeigt.
/// </summary>
public class RowEntryPadItem : ReciverSenderControlPadItem, IReadableText, IItemSendFilter {

    #region Fields

    private readonly ItemSendFilter _itemSends;

    #endregion

    #region Constructors

    public RowEntryPadItem(string keyName, string toParse) : this(keyName, null, null) => this.Parse(toParse);

    public RowEntryPadItem(Database? db) : this(string.Empty, db, null) { }

    public RowEntryPadItem(string keyName, Database? db, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) {
        _itemSends = new();

        DatabaseOutput = db;
    }

    public RowEntryPadItem(string keyName) : this(keyName, null, null) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-RowEntryElement";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One | AllowedInputFilter.None;

    public ReadOnlyCollection<string> ChildIds {
        get => _itemSends.ChildIdsGet();
        set => _itemSends.ChildIdsSet(value, this);
    }

    public override bool DatabaseInputMustMatchOutputDatabase => true;

    public Database? DatabaseOutput {
        get => _itemSends.DatabaseOutputGet(this);
        set => _itemSends.DatabaseOutputSet(value, this);
    }

    public override string Description => "Dieses Element ist in jedem Formular vorhanden und kann\r\ndie Zeile aus einem übergerordneten Element empfangen uns weitergeben.\r\n\r\nUnsichtbares Element, wird nicht angezeigt.";
    public List<int> InputColorId => [OutputColorId];
    public override bool InputMustBeOneRow => false;
    public override bool MustBeInDrawingArea => false;
    public override string MyClassId => ClassId;

    public int OutputColorId {
        get => _itemSends.OutputColorIdGet();
        set => _itemSends.OutputColorIdSet(value, this);
    }

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add) => _itemSends.AddChild(this, add);

    public override void AddedToCollection() {
        base.AddedToCollection();
        _itemSends.DoCreativePadAddedToCollection(this);
    }

    public override string ErrorReason() {
        var b = base.ErrorReason();
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = base.ErrorReason();
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemSends.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        //if (DatabaseOutput is null || DatabaseOutput.IsDisposed) {
        //    return "Ausgangsdatenbank fehlt";
        //}

        //b = _itemSends.ErrorReason(this);
        //if (!string.IsNullOrEmpty(b)) { return b; }

        return string.Empty;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> l =
        [
            .. _itemSends.GetProperties(this, widthOfControl),
            //new FlexiControl(),
            .. base.GetProperties(widthOfControl),
        ];

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
            case "id": // TODO: 29.03.2023
                //Id = IntParse(value);
                return true;
        }
        return false;
    }

    public override string ReadableText() {
        const string txt = "Eingangs-Zeile: ";

        if (this.IsOk() && DatabaseOutput != null) {
            return txt + DatabaseOutput.Caption;
        }

        return txt + ErrorReason();
    }

    public void RemoveChild(ReciverControlPadItem remove) => _itemSends.RemoveChild(remove, this);

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemSends.ParsableTags(this)];
        return result.Parseable(base.ToParseableString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        // Die Eigangszeile ist immer vom übergeordenetem Formular und wird einfach weitergegeben.
        // Deswegen ist InputColorID nur Fake

        if (!forPrinting) {
            DrawArrowOutput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, OutputColorId);
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);

        DrawArrorInput(gr, positionModified, zoom, forPrinting, InputColorId);
    }

    #endregion
}