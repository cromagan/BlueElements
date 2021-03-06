﻿// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollection {

    public class RowFormulaPadItem : FormPadItemRectangle {

        #region Fields

        public Bitmap GeneratedBitmap;
        private Database _Database;
        private string _lastQuickInfo;
        private string _LayoutID;
        private int _RowKey;
        private string _tmpQuickInfo;

        #endregion

        #region Constructors

        public RowFormulaPadItem(ItemCollectionPad parent, string internalname) : this(parent, internalname, null, 0, string.Empty) {
        }

        public RowFormulaPadItem(ItemCollectionPad parent, Database database, int rowkey) : this(parent, string.Empty, database, rowkey, string.Empty) {
        }

        public RowFormulaPadItem(ItemCollectionPad parent, Database database, int rowkey, string layoutID) : this(parent, string.Empty, database, rowkey, layoutID) {
        }

        public RowFormulaPadItem(ItemCollectionPad parent, string internalname, Database database, int rowkey, string layoutID) : base(parent, internalname) {
            _Database = database;
            _Database.Disposing += _Database_Disposing;
            _RowKey = rowkey;
            if (_Database != null && string.IsNullOrEmpty(layoutID)) {
                ItemCollectionPad p = new(_Database.Layouts[0], string.Empty);
                layoutID = p.ID;
            }
            _LayoutID = layoutID;
            RemovePic();
            GeneratePic(true);
        }

        #endregion

        #region Properties

        // Namen so lassen, wegen Kontextmenu
        public string Layout_ID {
            get => _LayoutID;
            set {
                if (value == _LayoutID) { return; }
                _LayoutID = value;
                RemovePic();
                GeneratePic(true);
            }
        }

        public override string QuickInfo {
            get {
                var r = Row;
                if (r == null) { return string.Empty; }
                if (_lastQuickInfo == r.QuickInfo) { return _tmpQuickInfo; }
                _lastQuickInfo = r.QuickInfo;
                _tmpQuickInfo = _lastQuickInfo.Replace(r.CellFirstString(), "<b>[<imagecode=Stern|16>" + Row.CellFirstString() + "]</b>");
                return _tmpQuickInfo;
            }
            set {
                // Werte zurücksetzen
                _lastQuickInfo = string.Empty;
                _tmpQuickInfo = string.Empty;
            }
        }

        public RowItem Row => _Database?.Row.SearchByKey(_RowKey);

        #endregion

        #region Methods

        public void Datensatz_bearbeiten() {
            _tmpQuickInfo = string.Empty; // eigentlich unnötig, da RowChanged anschlagen müsste
            EditBoxRow.Show("Datensatz bearbeiten:", Row, true);
        }

        public override void DesignOrStyleChanged() => RemovePic();

        public override List<FlexiControl> GetStyleOptions() {
            List<FlexiControl> l = new()
            {
                new FlexiControlForProperty(this, "Datensatz bearbeiten", enImageCode.Stift),
                new FlexiControl()
            };
            ItemCollectionList Layouts = new();
            foreach (var thisLayouts in Row.Database.Layouts) {
                ItemCollectionPad p = new(thisLayouts, string.Empty);
                Layouts.Add(p.Caption, p.ID, enImageCode.Stern);
            }
            l.Add(new FlexiControlForProperty(this, "Layout-ID", Layouts));
            l.AddRange(base.GetStyleOptions());
            return l;
        }

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "layoutid":
                    _LayoutID = value.FromNonCritical();
                    return true;

                case "database":
                    _Database = Database.GetByFilename(value, false);
                    _Database.Disposing += _Database_Disposing;
                    return true;

                case "rowid": // TODO: alt
                case "rowkey":
                    _RowKey = int.Parse(value);
                    //Row = ParseExplicit_TMPDatabase.Row.SearchByKey(int.Parse(value));
                    //if (_Row != null) { ParseExplicit_TMPDatabase = null; }
                    return true;

                case "firstvalue":
                    var n = value.FromNonCritical();
                    if (Row != null) {
                        if (Row.CellFirstString().ToUpper() != n.ToUpper()) {
                            MessageBox.Show("<b><u>Eintrag hat sich geändert:</b></u><br><b>Von: </b> " + n + "<br><b>Nach: </b>" + Row.CellFirstString(), enImageCode.Information, "OK");
                        }
                        return true; // Alles beim Alten
                    }
                    var Rowtmp = _Database.Row[n];
                    if (Rowtmp == null) {
                        MessageBox.Show("<b><u>Eintrag nicht hinzugefügt</b></u><br>" + n, enImageCode.Warnung, "OK");
                    } else {
                        _RowKey = Rowtmp.Key;
                        MessageBox.Show("<b><u>Eintrag neu gefunden:</b></u><br>" + n, enImageCode.Warnung, "OK");
                    }
                    return true; // Alles beim Alten
            }
            return false;
        }

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";
            t = t + "LayoutID=" + _LayoutID.ToNonCritical() + ", ";
            if (_Database != null) { t = t + "Database=" + _Database.Filename.ToNonCritical() + ", "; }
            if (_RowKey != 0) { t = t + "RowKey=" + _RowKey + ", "; }
            if (Row is RowItem r) { t = t + "FirstValue=" + r.CellFirstString().ToNonCritical() + ", "; }
            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "ROW";

        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, double cZoom, double shiftX, double shiftY, enStates vState, Size SizeOfParentControl, bool ForPrinting) {
            if (GeneratedBitmap == null) { GeneratePic(false); }
            if (GeneratedBitmap != null) {
                var scale = (float)Math.Min(DCoordinates.Width / (double)GeneratedBitmap.Width, DCoordinates.Height / (double)GeneratedBitmap.Height);
                RectangleF r2 = new(DCoordinates.Left, DCoordinates.Top, GeneratedBitmap.Width * scale, GeneratedBitmap.Height * scale);
                if (ForPrinting) {
                    GR.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    GR.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                } else {
                    GR.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                    GR.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                }
                GR.DrawImage(GeneratedBitmap, r2, new RectangleF(0, 0, GeneratedBitmap.Width, GeneratedBitmap.Height), GraphicsUnit.Pixel);
            }
            base.DrawExplicit(GR, DCoordinates, cZoom, shiftX, shiftY, vState, SizeOfParentControl, ForPrinting);
        }

        //Public Overrides Function ContextMenuItemClicked(sender As Object, ClickedComand As ItemCollection.BasicListItem) As Boolean
        //    If ClickedComand.StartsWith("Layout;") Then
        //        _LayoutNr = Integer.Parse(ClickedComand.Substring(7))
        //        GeneratePic(True)
        //        If _tmpBMP Is Nothing Then Return True
        //        'p_RU.X = p_LO.X + _tmpImage.Width
        //        'p_RU.Y = p_LO.Y + _tmpImage.Height
        //        KeepInternalLogic()
        //        ' RecomputePointAndRelations()
        //        Return True
        //    End If
        //   switch (ClickedComand
        //        Case Is = "Bearbeiten"
        //            eInputBox("Datensatz bearbeiten:", _Row)
        //            GeneratePic(True)
        //            Return True
        //    End Select
        //    Return False
        //End Function
        protected override void ParseFinished() => GeneratePic(true);

        private void _Database_Disposing(object sender, System.EventArgs e) {
            _Database.Disposing -= _Database_Disposing;
            _Database = null;
            RemovePic();
        }

        private void GeneratePic(bool SizeChangeAllowed) {
            if (string.IsNullOrEmpty(_LayoutID) || !_LayoutID.StartsWith("#")) {
                GeneratedBitmap = (Bitmap)QuickImage.Get(enImageCode.Warnung, 128).BMP.Clone();
                PointMoved(null);
                if (SizeChangeAllowed) { p_RU.SetTo(p_LO.X + GeneratedBitmap.Width, p_LO.Y + GeneratedBitmap.Height); }
                return;
            }
            CreativePad _pad = new(new ItemCollectionPad(_LayoutID, _Database, _RowKey));
            var re = _pad.Item.MaxBounds(null);
            if (GeneratedBitmap != null) {
                if (GeneratedBitmap.Width != re.Width || GeneratedBitmap.Height != re.Height) {
                    RemovePic();
                }
            }
            if (GeneratedBitmap == null) { GeneratedBitmap = new Bitmap((int)re.Width, (int)re.Height); }
            var mb = _pad.Item.MaxBounds(null);
            var zoomv = _pad.ZoomFitValue(mb, false, GeneratedBitmap.Size);
            var centerpos = _pad.CenterPos(mb, false, GeneratedBitmap.Size, zoomv);
            var slidervalues = _pad.SliderValues(mb, zoomv, centerpos);
            _pad.ShowInPrintMode = true;
            _pad.Unselect();
            if (Parent.SheetStyle != null) { _pad.Item.SheetStyle = Parent.SheetStyle; }
            _pad.Item.DrawCreativePadToBitmap(GeneratedBitmap, enStates.Standard, zoomv, slidervalues.X, slidervalues.Y, null);
            if (SizeChangeAllowed) { p_RU.SetTo(p_LO.X + GeneratedBitmap.Width, p_LO.Y + GeneratedBitmap.Height); }
            PointMoved(null);
        }

        private void RemovePic() {
            if (GeneratedBitmap != null) { GeneratedBitmap.Dispose(); }
            GeneratedBitmap = null;
        }

        #endregion

        //public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        //{
        //    if (Tags.TagGet("Datensatz bearbeiten").FromPlusMinus())
        //    {
        //        CloseMenu = false;
        //        EditBoxRow.Show("Datensatz bearbeiten:", _Row, true);
        //        GeneratePic(true);
        //        return;
        //    }
        //    var newl = Tags.TagGet("LayoutId");
        //    if (newl != _LayoutID)
        //    {
        //        _LayoutID = newl;
        //        GeneratePic(true);
        //        if (_tmpBMP != null)
        //        {
        //            KeepInternalLogic();
        //        }
        //        KeepInternalLogic();
        //    }
        //}
    }
}