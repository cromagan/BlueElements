﻿using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.DialogBoxes;
using BlueControls.Enums;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollection.ItemCollectionPad
{
    public class RowFormulaPadItem : BasicPadItem
    {


        private Database ParseLevel2_TMPDatabase;



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
        private int _LayoutNr;



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

        public RowFormulaPadItem(RowItem cRow, int LayoutNo)
        {
            _Row = cRow;
            _LayoutNr = LayoutNo;

        }



        protected override void InitializeLevel2()
        {
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


        public int LayoutNrx
        {
            get
            {
                return _LayoutNr;
            }
            set
            {

                if (value == _LayoutNr)
                {
                    return;
                }

                _LayoutNr = value;


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

        public override bool Contains(PointF P, decimal Zoomf)
        {
            return UsedArea().Contains(P.ToPointDF());
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
            return new RectangleDF(p_LO.X, p_LO.Y, p_RU.X - p_LO.X, p_RU.Y - p_LO.Y);
        }

        protected override bool ParseLevel2(KeyValuePair<string, string> pair)
        {

            switch (pair.Key)
            {
                case "checked":
                    return true;

                case "layout":
                    _LayoutNr = int.Parse(pair.Value);
                    return true;

                case "database":
                    ParseLevel2_TMPDatabase = Database.Load(pair.Value, false, Table.Database_NeedPassword, CreativePad.GenerateLayoutFromRow, CreativePad.RenameColumnInLayout);
                    return true;

                case "rowid": // TODO: alt
                case "rowkey":
                    _Row = ParseLevel2_TMPDatabase.Row.SearchByKey(int.Parse(pair.Value));
                    if (_Row != null) { ParseLevel2_TMPDatabase = null; }
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

                    _Row = ParseLevel2_TMPDatabase.Row[n];
                    ParseLevel2_TMPDatabase = null;

                    if (_Row == null)
                    {
                        MessageBox.Show("<b><u>Eintrag nicht hinzugefügt</b></u><br>" + n, enImageCode.Warnung, "OK");
                    }
                    else
                    {
                        MessageBox.Show("<b><u>Eintrag neu gefunde:</b></u><br>" + n, enImageCode.Warnung, "OK");
                    }


                    return true; // Alles beim Alten

            }

            return false;
        }


        protected override string ToStringLevel2()
        {
            var t = "";
            t = t + "Layout=" + _LayoutNr + ", ";

            if (_Row != null)
            {
                t = t + "Database=" + _Row.Database.Filename.ToNonCritical() + ", ";
                t = t + "RowKey=" + _Row.Key + ", ";
                t = t + "FirstValue=" + _Row.CellFirstString().ToNonCritical() + ", ";
            }

            return t.Trim(", ");
        }


        public override void SetCoordinates(RectangleDF r)
        {
            p_LO.SetTo(r.PointOf(enAlignment.Top_Left));
            p_RU.SetTo(r.PointOf(enAlignment.Bottom_Right));
            RecomputePointAndRelations();
        }


        internal override void KeepInternalLogic()
        {
            p_RO.SetTo(p_RU.X, p_LO.Y);
            p_LU.SetTo(p_LO.X, p_RU.Y);


            p_L.SetTo(p_LO.X, p_LO.Y + (p_LU.Y - p_LO.Y) / 2);
            p_R.SetTo(p_RO.X, p_L.Y);

            p_o.SetTo(p_LO.X + (p_RO.X - p_LO.X) / 2, p_LO.Y);
            p_u.SetTo(p_o.X, p_LU.Y);
        }


        public override void GenerateInternalRelation(List<clsPointRelation> Relations)
        {
            p_LU.X = p_LO.X;
            p_RO.Y = p_LO.Y;
            p_RU.X = p_RO.X;
            p_RU.Y = p_LU.Y;


            p_o.Y = p_LO.Y;
            p_u.Y = p_LU.Y;

            p_L.X = p_LO.X;
            p_R.X = p_RO.X;


            Relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_RO));
            Relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_RU));
            Relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_LU));

            Relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_R));
            Relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_L));
            Relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_u));
            Relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_o));
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

            if (Row == null || _LayoutNr < 0 || _LayoutNr > Row.Database.Layouts.Count - 1)
            {
                _tmpBMP = (Bitmap)QuickImage.Get(enImageCode.Warnung, 128).BMP.Clone();
                KeepInternalLogic();
                return;
            }


            var _pad = new CreativePad();

            _pad.GenerateFromRow(_LayoutNr, _Row, false);

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
            _pad.Width = _tmpBMP.Width;
            _pad.Height = _tmpBMP.Height;
            _pad.ZoomFitWithoutSliders();

            _pad.ShowInPrintMode = true;
            _pad.Unselect();


            if (Parent.SheetStyle != null) { _pad.SheetStyle = Parent.SheetStyle.CellFirstString(); }


            _pad.DrawCreativePadToBitmap(Graphics.FromImage(_tmpBMP), enStates.Standard);


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


            var Layouts = new ItemCollectionList.ItemCollectionList();
            for (var z = 0 ; z < Row.Database.Layouts.Count ; z++)
            {
                using (var p = new CreativePad())
                {
                    p.ParseData(Row.Database.Layouts[z], false);
                    Layouts.Add(new TextListItem(z.ToString(), p.Caption, enImageCode.Stern));
                }
            }

            l.Add(new FlexiControl("Layout", _LayoutNr.ToString(), Layouts));

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


            var newl = int.Parse(Tags.TagGet("Layout"));

            if (newl != _LayoutNr)
            {
                _LayoutNr = newl;
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