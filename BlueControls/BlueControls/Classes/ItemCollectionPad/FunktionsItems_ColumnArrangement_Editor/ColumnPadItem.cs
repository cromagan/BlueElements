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

using System;
using System.Collections.Generic;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.CellRenderer;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.ItemCollectionPad.Abstract;
using BlueDatabase;

namespace BlueControls.ItemCollectionPad.FunktionsItems_ColumnArrangement_Editor;

/// <summary>
/// Zum Darstellen einer Spalte. Im ViewEditpt benutzt
/// </summary>
public class ColumnPadItem : FixedRectangleBitmapPadItem {

    #region Fields

    public static readonly BlueFont? ColumnFont = Skin.GetBlueFont(Constants.Win11, PadStyles.Hervorgehoben, States.Standard, 1f);

    #endregion

    #region Constructors

    public ColumnPadItem(ColumnItem c, bool permanent, Renderer_Abstract? renderer) : base(c.Database?.TableName + "|" + c.KeyName) {
        Column = c;
        Permanent = permanent;
        Renderer = renderer;

        if (Column is { IsDisposed: false }) {
            Column.PropertyChanged += Column_PropertyChanged;
        }
    }

    #endregion

    #region Properties

    // ReSharper disable once UnusedMember.Global
    public static string ClassId => "FI-Column";

    public ColumnItem? Column { get; }

    public string Datenbank => IsDisposed || Column?.Database is not { IsDisposed: false } db ? "?" : db.TableName;

    public override string Description => string.Empty;

    //        _viewType = value ? ViewType.PermanentColumn : ViewType.Column;
    //    }
    //}
    public bool Permanent { get; set; }

    public Renderer_Abstract? Renderer { get; }

    /// <summary>
    /// Wird von Flexoptions aufgerufen
    /// </summary>
    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    //        if (value == Permanent) { return; }
    public override List<GenericControl> GetProperties(int widthOfControl) {
        if (Column?.Database is not { IsDisposed: false } db || Column.IsDisposed) { return []; }

        db.Editor = typeof(DatabaseHeadEditor);
        Column.Editor = typeof(ColumnEditor);

        List<GenericControl> result =
        [
            new FlexiControlForDelegate(db.Edit, "Tabelle: " + db.Caption, ImageCode.Datenbank),
            new FlexiControlForDelegate(Column.Edit, "Spalte: " + Column.Caption, ImageCode.Spalte),
            new FlexiControl(),
            new FlexiControlForProperty<bool>(() => Permanent),
            new FlexiControl(),
            new FlexiControl(),
            new FlexiControlForProperty<string>(() => Column.CaptionGroup1),
            new FlexiControlForProperty<string>(() => Column.CaptionGroup2),
            new FlexiControlForProperty<string>(() => Column.CaptionGroup3),
            new FlexiControl(),
            new FlexiControlForProperty<string>(() => Column.QuickInfo, 5),
            new FlexiControlForProperty<string>(() => Column.AdminInfo, 5)
        ];

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Database", Column?.Database);
        result.ParseableAdd("ColumnName", Column);

        return result;
    }

    public override string ReadableText() => "Spalte";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Spalte, 16);

    ///// <summary>
    ///// Für FlexOptions
    ///// </summary>
    //public bool Permanent {
    //    get => _viewType == ViewType.PermanentColumn;
    //    set {
    //        if (!PermanentPossible() && Permanent) { return; }
    //        if (!NonPermanentPossible() && !value) { return; }
    //public void Spalte_bearbeiten() {
    //    if (Column is not { IsDisposed: false }) { return; }
    //    TableView.OpenColumnEditor(Column, null, null);
    //}
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if (disposing) {
            if (Column is { IsDisposed: false }) {
                Column.PropertyChanged -= Column_PropertyChanged;
            }
            //Column = null;
        }
    }

    protected override void GeneratePic() {
        if (Column is not { IsDisposed: false }) {
            GeneratedBitmap = QuickImage.Get(ImageCode.Warnung, 128);
            return;
        }

        var wi = 300;// Math.Max(Column.Contentwidth ?? 0, 24);

        var bmp = new Bitmap(Math.Max((int)(wi * 0.7), 30), 300);
        var gr = Graphics.FromImage(bmp);

        gr.Clear(Column.BackColor);
        //gr.DrawString(Column.Caption, CellFont, )
        //Table.Draw_FormatedText(gr,)

        for (var z = 0; z < 3; z++) {
            var n = Column.CaptionGroup(z);
            if (!string.IsNullOrEmpty(n)) {
                Skin.Draw_FormatedText(gr, n, null, Alignment.Horizontal_Vertical_Center, new Rectangle(0, z * 16, bmp.Width, 61), null, false, ColumnFont, true);
            }
        }

        gr.TranslateTransform(bmp.Width / 2f, 50);
        gr.RotateTransform(-90);
        Skin.Draw_FormatedText(gr, Column.Caption, null, Alignment.VerticalCenter_Left, new Rectangle(-150, -150, 300, 300), null, false, ColumnFont, true);

        gr.TranslateTransform(-bmp.Width / 2f, -50);
        gr.ResetTransform();

        gr.DrawLine(Pens.Black, 0, 210, bmp.Width, 210);

        var r = Column.Database?.Row.First();
        if (r is { IsDisposed: false }) {
            Renderer?.Draw(gr, r.CellGetString(Column), new Rectangle(0, 210, bmp.Width, 90), Column.DoOpticalTranslation, (Alignment)Column.Align, 1f);
        }

        GeneratedBitmap = bmp;
    }

    private void Column_PropertyChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        RemovePic();
        OnPropertyChanged();
    }

    #endregion
}