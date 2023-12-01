﻿// Authors:
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

using BlueBasics;
using BlueDatabase;
using System.Collections.Generic;
using BlueControls.Interfaces;
using System.ComponentModel;
using BlueControls.Enums;
using System.Drawing;

#nullable enable

namespace BlueControls.Controls;

public partial class TextGenerator : GenericControl, IControlAcceptSomething {

    #region Fields

    private readonly List<ColumnItem> _auswahlc = new();
    private string _auswahlSpalte1 = string.Empty;

    private string _auswahlSpalte2 = string.Empty;

    private string _auswahlSpalte3 = string.Empty;

    private ColumnItem? _textc;
    private string _textSpalte = string.Empty;

    #endregion

    #region Constructors

    public TextGenerator() : base() => InitializeComponent();

    #endregion

    #region Properties

    [Description("Dieses Feld kann für den Forms-Editor verwendet werden.")]
    [DefaultValue("")]
    public string AuswahlSpalte1 {
        get => _auswahlSpalte1;
        set {
            if (_auswahlSpalte1 == value) { return; }
            _auswahlSpalte1 = value;
            GenerateColumns();
        }
    }

    [Description("Dieses Feld kann für den Forms-Editor verwendet werden.")]
    [DefaultValue("")]
    public string AuswahlSpalte2 {
        get => _auswahlSpalte2;
        set {
            if (_auswahlSpalte2 == value) { return; }
            _auswahlSpalte1 = value;
            GenerateColumns();
        }
    }

    [Description("Dieses Feld kann für den Forms-Editor verwendet werden.")]
    [DefaultValue("")]
    public string AuswahlSpalte3 {
        get => _auswahlSpalte3;
        set {
            if (_auswahlSpalte3 == value) { return; }
            _auswahlSpalte1 = value;
            GenerateColumns();
        }
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput { get; set; }

    public bool FilterManualSeted { get; set; } = false;

    public List<IControlSendSomething> Parents { get; } = new();

    [Description("Dieses Feld kann für den Forms-Editor verwendet werden.")]
    [DefaultValue("")]
    public string Text_Spalte {
        get => _textSpalte;
        set {
            if (_textSpalte == value) { return; }
            _textSpalte = value;
            GenerateColumns();
        }
    }

    #endregion

    #region Methods

    public void FilterInput_Changed(object sender, System.EventArgs e) {
        FilterInput = this.FilterOfSender();
        GenerateColumns();// Wegen der Datenbank
        GenerateItemsAndText();
    }

    public void FilterInput_Changing(object sender, System.EventArgs e) { }

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) { FilterInput_Changing(this, System.EventArgs.Empty); }

        if (disposing && (components != null)) {
            FilterInput?.Dispose();
            //FilterOutput.Dispose();
            FilterInput = null;
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override void DrawControl(Graphics gr, States vState) {
        //if (vState.HasFlag(States.Standard_MouseOver)) { vState ^= States.Standard_MouseOver; }
        //if (vState.HasFlag(States.Standard_MousePressed)) { vState ^= States.Standard_MousePressed; }

        //Skin.Draw_Back(gr, Design.EasyPic, vState, DisplayRectangle, this, true);

        ////if (_bitmap != null) {
        ////    gr.DrawImageInRectAspectRatio(_bitmap, 1, 1, Width - 2, Height - 2);
        ////}

        //Skin.Draw_Border(gr, Design.EasyPic, vState, DisplayRectangle);
    }

    private void GenerateColumns() {
        _textc = null;
        _auswahlc.Clear();
        if (FilterInput?.Database is not DatabaseAbstract db || !db.IsDisposed) { return; }

        _textc = db.Column.Exists(_textSpalte);

        if (db.Column.Exists(_auswahlSpalte1) is ColumnItem c1) { _auswahlc.Add(c1); }
        if (db.Column.Exists(_auswahlSpalte2) is ColumnItem c2) { _auswahlc.Add(c2); }
        if (db.Column.Exists(_auswahlSpalte3) is ColumnItem c3) { _auswahlc.Add(c3); }
    }

    private void GenerateItemsAndText() {
        if (DesignMode) { return; }

        if (FilterInput?.Database is not DatabaseAbstract db || !db.IsDisposed) { return; }

        if (_auswahlc.Count == 0) { return; }
        if (_textc == null) { return; }

        #region Zuerst die ungültigen Einträge löschen, es kann ja was abgewählt worden sein

        var stufe = 0;

        do {

            #region Ermitteln, mit welchen String die Items anfangen müssen - die Stufe mit einbezogen (allowed)

            var x = lstAuswahl.Item.Checked().ToListOfString();

            var allowed = new List<string>();

            foreach (var thisx in x) {
                if (thisx.CountString(";") == stufe) { allowed.Add(thisx); }
            }

            #endregion

            #region Nun alle löschen, die nicht damit beginnen

            bool again;
            do {
                again = false;
                foreach (var thisit in lstAuswahl.Item) {
                    if (thisit.Internal.CountString(";") >= stufe) {
                        var ok = false;
                        foreach (var thisall in allowed) {
                            if (thisit.Internal.StartsWith(thisall)) { ok = true; }
                        }
                        if (!ok) { lstAuswahl.Item.Remove(thisit); again = true; break; }
                    }
                }
            } while (again);

            #endregion

            stufe += 1;
        } while (stufe < _auswahlc.Count);

        #endregion

        #region Gut, nun steht fest, welche Items wirklich erlaubt sind. die gewählten ermitteln (chk)

        var chk = lstAuswahl.Item.Checked().ToListOfString();

        #endregion

        #region und nun die Datenbank durchforsten und fehlende Einträge erzeugen

        var allr = FilterInput.Rows;
        //allr.Sort(); // , _textDatabase.SortDefinition
        var txt = string.Empty;

        foreach (var thisRow in allr) {
            var r = RowString(thisRow);

            if (lstAuswahl.Item[r] == null) {
                var add = false;
                foreach (var thisChecked in chk) {
                    if (thisChecked.CountString(";") + 1 == r.CountString(";") && r.StartsWith(thisChecked)) { add = true; }
                }
                if (add) {
                    var rvis = string.Empty.PadLeft((r.CountString(";") - 2) * 5) + r.Substring(r.LastIndexOf(";") + 1);

                    _ = lstAuswahl.Item.Add(rvis, r);
                }
            }

            if (chk.Contains(r)) {
                txt = txt + "\r\n" + thisRow.CellGetString(_textc);
            }
        }

        //lstAuswahl.Item.Sort();

        #endregion

        textBox1.Text = txt;
    }

    private void lstAuswahl_ItemClicked(object sender, EventArgs.AbstractListItemEventArgs e) => GenerateItemsAndText();

    private string RowString(RowItem thisRow) {
        var stufe = 0;
        var s = string.Empty;

        do {
            if (stufe > _auswahlc.Count) { return s; }
            var co = _auswahlc[stufe];
            if (co == null) { return s; }
            if (thisRow.IsNullOrEmpty(co)) { return s; }

            if (!string.IsNullOrEmpty(s)) { s += ";"; }
            s += thisRow.CellGetString(co);
            stufe++;
        } while (true);
    }

    #endregion
}