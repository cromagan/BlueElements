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
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace BlueControls.ItemCollection
{
    public class ChildPadItem : BasicPadItem, IMouseAndKeyHandle, ICanHaveColumnVariables
    {




        #region  Variablen-Deklarationen 

        internal PointDF p_LO;
        internal PointDF p_RO;
        internal PointDF p_RU;
        internal PointDF p_LU;

        public bool FixSize;

        private Bitmap _tmpBitmap;

        [AccessedThroughProperty(nameof(PadInternal))]
        private CreativePad _PadInternal;
        public CreativePad PadInternal
        {
            [DebuggerNonUserCode]
            get
            {
                return _PadInternal;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            [DebuggerNonUserCode]
            set
            {
                if (_PadInternal != null)
                {
                    _PadInternal.NeedRefresh -= _Pad_NeedRefresh;
                }

                _PadInternal = value;

                if (value != null)
                {
                    _PadInternal.NeedRefresh += _Pad_NeedRefresh;
                }
            }
        }

        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 



        protected override void Initialize()
        {
            base.Initialize();
            PadInternal = new CreativePad();
            p_LO = new PointDF(this, "LO", 0, 0, false, true, true);
            p_RO = new PointDF(this, "RO", 0, 0);
            p_RU = new PointDF(this, "RU", 0, 0);
            p_LU = new PointDF(this, "LU", 0, 0);
            FixSize = true;
            _tmpBitmap = null;
        }


        #endregion


        public override void DesignOrStyleChanged()
        {
            if (_tmpBitmap != null)
            {
                _tmpBitmap.Dispose();
                _tmpBitmap = null;
            }


            PadInternal.SheetStyle = Parent.SheetStyle.CellFirstString();
            PadInternal.SheetStyleScale = Parent.SheetStyleScale;
        }


        protected override string ClassId()
        {
            return "CHILDPAD";
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

            if (PadInternal != null)
            {

                PadInternal.SheetStyle = Parent.SheetStyle.CellFirstString();
                PadInternal.SheetStyleScale = Parent.SheetStyleScale;


                var r = UsedArea();

                while (r.Width * cZoom > 8000 || r.Height * cZoom > 8000)
                {
                    cZoom = cZoom * 0.8m; // Kann ruhig verändert werden, tut nix zur Sache, DKoordinates reichen
                }


                if (_tmpBitmap != null)
                {
                    if (_tmpBitmap.Width != (int)(r.Width * cZoom) || (int)(r.Height * cZoom) != _tmpBitmap.Height)
                    {
                        _tmpBitmap.Dispose();
                        _tmpBitmap = null;
                        modAllgemein.CollectGarbage();
                    }
                }

                if (_tmpBitmap == null)
                {
                    _tmpBitmap = new Bitmap(Math.Abs((int)(r.Width * cZoom)), Math.Abs((int)(r.Height * cZoom)));
                }
                PadInternal.Width = _tmpBitmap.Width;
                PadInternal.Height = _tmpBitmap.Height;
                PadInternal.ZoomFitWithoutSliders();

                PadInternal.ShowInPrintMode = ForPrinting;
                if (ForPrinting) { PadInternal.Unselect(); }

                PadInternal.DrawCreativePadToBitmap(Graphics.FromImage(_tmpBitmap), enStates.Standard);


            }

            //If ForPrinting AndAlso _Pad IsNot Nothing Then
            //    _tmpImage = CType(_Pad.ToImage(2), Bitmap)
            //End If


            if (_tmpBitmap != null)
            {
                //var scale = (float)Math.Min(DCoordinates.Width / (double)_tmpBitmap.Width, DCoordinates.Height / (double)_tmpBitmap.Height);
                //var r2 = new RectangleF((DCoordinates.Width - _tmpBitmap.Width * scale) / 2 + DCoordinates.Left, (DCoordinates.Height - _tmpBitmap.Height * scale) / 2 + DCoordinates.Top, _tmpBitmap.Width * scale, _tmpBitmap.Height * scale);

                //GR.DrawImage(_tmpBitmap, r2, new RectangleF(0, 0, _tmpBitmap.Width, _tmpBitmap.Height), GraphicsUnit.Pixel);
                GR.DrawImageInRectAspectRatio(_tmpBitmap, DCoordinates);
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
                case "fixsize":
                    FixSize = pair.Value.FromPlusMinus();
                    return true;
                case "modus": // Alt
                              // BildModus = CType(Integer.Parse(pair.Value), enSizeModes)
                    return true;
                case "data":
                    PadInternal = new CreativePad();
                    PadInternal.ParseData(pair.Value, false, string.Empty);
                    return true;
                case "checked":
                    return true;

            }

            return false;
        }


        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            if (FixSize)
            {
                t = t + "Fixsize=True, ";
            }
            else
            {
                t = t + "Fixsize=False, ";
            }

            if (PadInternal != null)
            {
                t = t + "Data=" + PadInternal.DataToString() + ", ";
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
        }


        public override void GenerateInternalRelation(List<clsPointRelation> relations)
        {


            p_LU.X = p_LO.X;
            p_RO.Y = p_LO.Y;
            p_RU.X = p_RO.X;
            p_RU.Y = p_LU.Y;

            if (FixSize)
            {
                relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_RO));
                relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_RU));
                relations.Add(new clsPointRelation(enRelationType.PositionZueinander, p_LO, p_LU));
            }
            else
            {
                relations.Add(new clsPointRelation(enRelationType.WaagerechtSenkrecht, p_LO, p_RO));
                relations.Add(new clsPointRelation(enRelationType.WaagerechtSenkrecht, p_RU, p_LU));

                relations.Add(new clsPointRelation(enRelationType.WaagerechtSenkrecht, p_LO, p_LU));
                relations.Add(new clsPointRelation(enRelationType.WaagerechtSenkrecht, p_RO, p_RU));
            }
        }

        private void _Pad_NeedRefresh(object sender, System.EventArgs e)
        {
            if (IsParsing) { return; }
            OnChanged();
        }

        public bool MouseDown(object sender, System.Windows.Forms.MouseEventArgs e, decimal cZoom, decimal MoveX, decimal MoveY)
        {
            if (PadInternal.Item.Count == 0) { return false; }


            var l1 = DrawingKoordinates(cZoom, MoveX, MoveY);
            var l2 = PadInternal.MaxBounds();
            var tZo = Math.Min(l1.Width / l2.Width, l1.Height / l2.Height);
            PadInternal.SetZoom(1);

            // Coordinaten auf Maßstab 1/1 scalieren
            var x = (e.X - l1.X) / tZo;
            var y = (e.Y - l1.Y) / tZo;

            // Nullpunkt verschiebung laut Maxbounds
            x = x + l2.X;
            y = y + l2.Y;

            // Und noch berücksichtigen, daß das Bild in den Rahmen eingepasst wurde
            x = x + (l2.Width - l1.Width / tZo) / 2;
            y = y + (l2.Height - l1.Height / tZo) / 2;

            x = Math.Min(x, int.MaxValue / 2.0m);
            y = Math.Min(y, int.MaxValue / 2.0m);
            x = Math.Max(x, int.MinValue / 2.0m);
            y = Math.Max(y, int.MinValue / 2.0m);


            var e2 = new System.Windows.Forms.MouseEventArgs(e.Button, e.Clicks, (int)x, (int)y, e.Delta);


            PadInternal.MouseDown(e2);

            return true;
        }

        public bool MouseMove(object sender, System.Windows.Forms.MouseEventArgs e, decimal cZoom, decimal MoveX, decimal MoveY)
        {
            if (PadInternal.Item.Count == 0) { return false; }


            var l1 = DrawingKoordinates(cZoom, MoveX, MoveY);
            var l2 = PadInternal.MaxBounds();
            decimal tZo = 1;
            if (l2.Width > 0 && l2.Height > 0) { tZo = Math.Min(l1.Width / l2.Width, l1.Height / l2.Height); }


            PadInternal.SetZoom(1);

            // Coordinaten auf Maßstab 1/1 scalieren
            var x = (e.X - l1.X) / tZo;
            var y = (e.Y - l1.Y) / tZo;

            // Nullpunkt verschiebung laut Maxbounds
            x = x + l2.X;
            y = y + l2.Y;

            // Und noch berücksichtigen, daß das Bild in den Rahmen eingepasst wurde
            x = x + (l2.Width - l1.Width / tZo) / 2;
            y = y + (l2.Height - l1.Height / tZo) / 2;

            x = Math.Min(x, int.MaxValue / 2.0m);
            y = Math.Min(y, int.MaxValue / 2.0m);
            x = Math.Max(x, int.MinValue / 2.0m);
            y = Math.Max(y, int.MinValue / 2.0m);


            var e2 = new System.Windows.Forms.MouseEventArgs(e.Button, e.Clicks, (int)x, (int)y, e.Delta);


            PadInternal.MouseMove(e2);

            return true;
        }

        public bool MouseUp(object sender, System.Windows.Forms.MouseEventArgs e, decimal cZoom, decimal MoveX, decimal MoveY)
        {

            if (PadInternal.Item.Count == 0) { return false; }


            var l1 = DrawingKoordinates(cZoom, MoveX, MoveY);
            var l2 = PadInternal.MaxBounds();
            var tZo = Math.Min(l1.Width / l2.Width, l1.Height / l2.Height);
            PadInternal.SetZoom(1);

            // Coordinaten auf Maßstab 1/1 scalieren
            var x = (e.X - l1.X) / tZo;
            var y = (e.Y - l1.Y) / tZo;

            // Nullpunkt verschiebung laut Maxbounds
            x = x + l2.X;
            y = y + l2.Y;

            // Und noch berücksichtigen, daß das Bild in den Rahmen eingepasst wurde
            x = x + (l2.Width - l1.Width / tZo) / 2;
            y = y + (l2.Height - l1.Height / tZo) / 2;

            x = Math.Min(x, int.MaxValue / 2.0m);
            y = Math.Min(y, int.MaxValue / 2.0m);
            x = Math.Max(x, int.MinValue / 2.0m);
            y = Math.Max(y, int.MinValue / 2.0m);


            var e2 = new System.Windows.Forms.MouseEventArgs(e.Button, e.Clicks, (int)x, (int)y, e.Delta);


            PadInternal.DoMouseUp(e2);

            return true;
        }

        public bool ParseVariable(string VariableName, enValueType ValueType, string Value)
        {
            if (PadInternal == null) { return false; }
            return PadInternal.ParseVariable(VariableName, ValueType, Value);
        }



        public bool ParseSpecialCodes()
        {
            if (PadInternal == null) { return false; }
            return PadInternal.ParseSpecialCodes();
        }


        public bool KeyUp(object sender, System.Windows.Forms.KeyEventArgs e, decimal cZoom, decimal MoveX, decimal MoveY)
        {
            if (PadInternal.Item.Count == 0) { return false; }
            PadInternal.KeyUp(e);
            return true;
        }



        public bool ResetVariables()
        {
            if (PadInternal == null) { return false; }
            return PadInternal.ResetVariables();
        }



        public bool RenameColumn(string oldName, ColumnItem cColumnItem)
        {
            if (PadInternal == null) { return false; }
            return PadInternal.RenameColumn(oldName, cColumnItem);
        }



        public override List<FlexiControl> GetStyleOptions(object sender, System.EventArgs e)
        {

            var l = new List<FlexiControl>();

            if (sender is CreativePad CP)
            {
                var Relations = CP.AllRelations();

                if (!FixSize && !p_LO.CanMove(Relations) && !p_RU.CanMove(Relations))
                {
                    l.Add(new FlexiControl("Objekt fest definiert,<br>Größe kann nicht fixiert werden"));
                }
                else
                {
                    l.Add(new FlexiControl("Größe fixiert", FixSize));
                }
            }
            return l;
        }

        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {
            var nFixSize = Tags.TagGet("Größe fixiert").FromPlusMinus();
            if (nFixSize != FixSize)
            {
                FixSize = nFixSize;
                ClearInternalRelations();
            }
        }
    }
}