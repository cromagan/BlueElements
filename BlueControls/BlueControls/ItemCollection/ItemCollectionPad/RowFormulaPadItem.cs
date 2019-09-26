#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
#endregion


using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.Enums;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollection
{
    public class RowFormulaPadItem : BasicPadItem
    {


        private Database ParseExplicit_TMPDatabase;



        #region  Variablen-Deklarationen 

        internal PointDF p_LO;
        internal PointDF p_RO;
        internal PointDF p_RU;
        internal PointDF p_LU;

        internal PointDF p_L;
        internal PointDF p_R;
        internal PointDF p_o;
        internal PointDF p_u;


        private RowItem _Row;
        private Bitmap _tmpBMP;
        private string _LayoutID;



        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 

        public RowFormulaPadItem()
        { }

        public RowFormulaPadItem(RowItem cRow)
        {
            _Row = cRow;
        }

        public RowFormulaPadItem(RowItem cRow, string LayoutID)
        {
            _Row = cRow;
            _LayoutID = LayoutID;

        }



        protected override void Initialize()
        {
            base.Initialize();
            p_LO = new PointDF(this, "LO", 0, 0, false, true, true);
            p_RO = new PointDF(this, "RO", 0, 0);
            p_RU = new PointDF(this, "RU", 0, 0);
            p_LU = new PointDF(this, "LU", 0, 0);
            p_L = new PointDF(this, "L", 0, 0);
            p_R = new PointDF(this, "R", 0, 0);
            p_o = new PointDF(this, "O", 0, 0);
            p_u = new PointDF(this, "U", 0, 0);
            _Row = null;
            _tmpBMP = null;
        }


        #endregion


        public string LayoutID
        {
            get
            {
                return _LayoutID;
            }
            set
            {

                if (value == _LayoutID) { return; }

                _LayoutID = value;


                if (_tmpBMP != null)
                {
                    _tmpBMP.Dispose();
                    _tmpBMP = null;
                }
            }
        }


        public RowItem Row
        {
            get
            {
                return _Row;
            }
            set
            {

                if (_Row == value)
                {
                    return;
                }

                _Row = value;

                removePic();

                //GeneratePic()
            }
        }


        private void removePic()
        {
            if (_tmpBMP != null)
            {
                _tmpBMP.Dispose();
                _tmpBMP = null;
            }
        }

        public override void DesignOrStyleChanged()
        {
            removePic();
        }


        protected override string ClassId()
        {
            return "ROW";
        }

        public override bool Contains(PointF value, decimal zoomfactor)
        {
            var tmp = UsedArea();
            var ne = (int)(5 / zoomfactor);
            tmp.Inflate(-ne, -ne);
            return tmp.Contains(value.ToPointDF());
        }


        protected override void DrawExplicit(Graphics GR, Rectangle DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {

            if (_tmpBMP == null)
            {
                GeneratePic(false);
            }




            if (_tmpBMP != null)
            {
                var scale = (float)Math.Min(DCoordinates.Width / (double)_tmpBMP.Width, DCoordinates.Height / (double)_tmpBMP.Height);
                var r2 = new RectangleF((DCoordinates.Width - _tmpBMP.Width * scale) / 2 + DCoordinates.Left, (DCoordinates.Height - _tmpBMP.Height * scale) / 2 + DCoordinates.Top, _tmpBMP.Width * scale, _tmpBMP.Height * scale);

                GR.DrawImage(_tmpBMP, r2, new RectangleF(0, 0, _tmpBMP.Width, _tmpBMP.Height), GraphicsUnit.Pixel);
            }

            if (!ForPrinting)
            {
                GR.DrawRectangle(CreativePad.PenGray, DCoordinates);
            }
        }

        public override List<PointDF> PointList()
        {
            var l = new List<PointDF>();
            l.Add(p_LO);
            l.Add(p_RU);
            l.Add(p_LU);
            l.Add(p_RO);

            l.Add(p_L);
            l.Add(p_R);
            l.Add(p_u);
            l.Add(p_o);


            return l;
        }


        public override RectangleDF UsedArea()
        {
            if (p_LO == null || p_RU == null) { return new RectangleDF(); }
            return new RectangleDF(Math.Min(p_LO.X, p_RU.X), Math.Min(p_LO.Y, p_RU.Y), Math.Abs(p_RU.X - p_LO.X), Math.Abs(p_RU.Y - p_LO.Y));
        }

        protected override bool ParseExplicit(KeyValuePair<string, string> pair)
        {

            switch (pair.Key)
            {
                case "checked":
                    return true;

                case "layoutid":
                    _LayoutID = pair.Value.FromNonCritical();
                    return true;

                case "database":
                    ParseExplicit_TMPDatabase = Database.GetByFilename(pair.Value, false);
                    if (ParseExplicit_TMPDatabase == null)
                    {
                        ParseExplicit_TMPDatabase = new Database(false);
                        ParseExplicit_TMPDatabase.Load(pair.Value);
                    }
                    return true;

                case "rowid": // TODO: alt
                case "rowkey":
                    _Row = ParseExplicit_TMPDatabase.Row.SearchByKey(int.Parse(pair.Value));
                    if (_Row != null) { ParseExplicit_TMPDatabase = null; }
                    return true;

                case "firstvalue":
                    var n = pair.Value.FromNonCritical();

                    if (_Row != null)
                    {
                        if (Row.CellFirstString().ToUpper() != n.ToUpper())
                        {
                            MessageBox.Show("<b><u>Eintrag hat sich geändert:</b></u><br><b>Von: </b> " + n + "<br><b>Nach: </b>" + Row.CellFirstString(), enImageCode.Information, "OK");
                        }
                        return true; // Alles beim Alten
                    }

                    _Row = ParseExplicit_TMPDatabase.Row[n];
                    ParseExplicit_TMPDatabase = null;

                    if (_Row == null)
                    {
                        MessageBox.Show("<b><u>Eintrag nicht hinzugefügt</b></u><br>" + n, enImageCode.Warnung, "OK");
                    }
                    else
                    {
                        MessageBox.Show("<b><u>Eintrag neu gefunden:</b></u><br>" + n, enImageCode.Warnung, "OK");
                    }


                    return true; // Alles beim Alten

            }

            return false;
        }


        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            t = t + "LayoutID=" + _LayoutID.ToNonCritical() + ", ";

            if (_Row != null)
            {
                t = t + "Database=" + _Row.Database.Filename.ToNonCritical() + ", ";
                t = t + "RowKey=" + _Row.Key + ", ";
                t = t + "FirstValue=" + _Row.CellFirstString().ToNonCritical() + ", ";
            }

            return t.Trim(", ") + "}";
        }


        public override void SetCoordinates(RectangleDF r)
        {
            p_LO.SetTo(r.PointOf(enAlignment.Top_Left));
            p_RU.SetTo(r.PointOf(enAlignment.Bottom_Right));
            RecomputePointAndRelations();
        }


        protected override void KeepInternalLogic()
        {
            p_RO.SetTo(p_RU.X, p_LO.Y);
            p_LU.SetTo(p_LO.X, p_RU.Y);


            p_L.SetTo(p_LO.X, p_LO.Y + (p_LU.Y - p_LO.Y) / 2);
            p_R.SetTo(p_RO.X, p_L.Y);

            p_o.SetTo(p_LO.X + (p_RO.X - p_LO.X) / 2, p_LO.Y);
            p_u.SetTo(p_o.X, p_LU.Y);
        }


        public override void GenerateInternalRelation(List<clsPointRelation> relations)
        {
            p_LU.X = p_LO.X;
            p_RO.Y = p_LO.Y;
            p_RU.X = p_RO.X;
            p_RU.Y = p_LU.Y;


            p_o.Y = p_LO.Y;
            p_u.Y = p_LU.Y;

            p_L.X = p_LO.X;
            p_R.X = p_RO.X;


            relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_RO));
            relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_RU));
            relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_LU));

            relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_R));
            relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_L));
            relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_u));
            relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_o));
        }



        internal PointDF PointOf(enAlignment P)
        {

            switch (P)
            {
                case enAlignment.Bottom_Left: return p_LU;
                case enAlignment.Bottom_Right: return p_RU;
                case enAlignment.Top_Left: return p_LO;
                case enAlignment.Top_Right: return p_RO;
                case enAlignment.Bottom_HorizontalCenter: return p_u;
                case enAlignment.Top_HorizontalCenter: return p_o;
                case enAlignment.VerticalCenter_Left: return p_L;
                case enAlignment.VerticalCenter_Right: return p_R;
                default: return null;
            }
        }

        private void GeneratePic(bool SizeChangeAllowed)
        {

            if (Row == null || string.IsNullOrEmpty(_LayoutID) || !_LayoutID.StartsWith("#"))
            {
                _tmpBMP = (Bitmap)QuickImage.Get(enImageCode.Warnung, 128).BMP.Clone();
                KeepInternalLogic();
                return;
            }


            var _pad = new CreativePad();

            _pad.GenerateFromRow(_LayoutID, _Row, false);

            var re = _pad.MaxBounds();

            if (_tmpBMP != null)
            {
                if (_tmpBMP.Width != re.Width || _tmpBMP.Height != re.Height)
                {
                    _tmpBMP.Dispose();
                    _tmpBMP = null;
                }
            }

            if (_tmpBMP == null) { _tmpBMP = new Bitmap((int)re.Width, (int)re.Height); }


            var zoomv = _pad.ZoomFitValue(false, _tmpBMP.Size);
            var centerpos = _pad.CenterPos(false, _tmpBMP.Size, zoomv);
            var slidervalues = _pad.SliderValues(zoomv, centerpos);

            _pad.ShowInPrintMode = true;
            _pad.Unselect();


            if (Parent.SheetStyle != null) { _pad.SheetStyle = Parent.SheetStyle.CellFirstString(); }


            _pad.DrawCreativePadToBitmap(_tmpBMP, enStates.Standard, zoomv, (decimal)slidervalues.X, (decimal)slidervalues.Y);


            if (SizeChangeAllowed) { p_RU.SetTo(p_LO.X + _tmpBMP.Width, p_LO.Y + _tmpBMP.Height); }

            RecomputePointAndRelations();
        }




        //Public Overrides Function ContextMenuItemClicked(sender As Object, ClickedComand As ItemCollection.BasicListItem) As Boolean



        //    If ClickedComand.Internal.StartsWith("Layout;") Then
        //        _LayoutNr = Integer.Parse(ClickedComand.Internal.Substring(7))
        //        GeneratePic(True)

        //        If _tmpBMP Is Nothing Then Return True

        //        'p_RU.X = p_LO.X + _tmpImage.Width
        //        'p_RU.Y = p_LO.Y + _tmpImage.Height

        //        KeepInternalLogic()

        //        ' RecomputePointAndRelations()
        //        Return True

        //    End If


        //   switch (ClickedComand.Internal
        //        Case Is = "Bearbeiten"
        //            eInputBox("Datensatz bearbeiten:", _Row)
        //            GeneratePic(True)

        //            Return True
        //    End Select

        //    Return False
        //End Function



        public override List<FlexiControl> GetStyleOptions(object sender, System.EventArgs e)
        {
            var l = new List<FlexiControl>();

            l.Add(new FlexiControl("Datensatz bearbeiten", enImageCode.Stift));

            l.Add(new FlexiControl());


            var Layouts = new ItemCollectionList();
            for (var z = 0; z < Row.Database.Layouts.Count; z++)
            {
                using (var p = new CreativePad())
                {
                    p.ParseData(Row.Database.Layouts[z], false, string.Empty);
                    Layouts.Add(new TextListItem(z.ToString(), p.Caption, enImageCode.Stern));
                }
            }

            l.Add(new FlexiControl("LayoutId", _LayoutID, Layouts));

            return l;
        }

        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {

            if (Tags.TagGet("Datensatz bearbeiten").FromPlusMinus())
            {
                CloseMenu = false;
                EditBoxRow.Show("Datensatz bearbeiten:", _Row, true);
                GeneratePic(true);
                return;
            }


            var newl = Tags.TagGet("LayoutId");

            if (newl != _LayoutID)
            {
                _LayoutID = newl;
                GeneratePic(true);
                if (_tmpBMP != null)
                {
                    KeepInternalLogic();
                }
                KeepInternalLogic();
            }




        }
    }
}