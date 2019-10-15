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
    public class ChildPadItem : FormPadItemRectangle, IMouseAndKeyHandle, ICanHaveColumnVariables
    {




        #region  Variablen-Deklarationen 



        private Bitmap _tmpBMP;

        [AccessedThroughProperty(nameof(PadInternal))]
        private CreativePad _PadInternal;
        public CreativePad PadInternal
        {
            get
            {
                return _PadInternal;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
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


        public List<BasicPadItem> VisibleItems = null;
        public List<BasicPadItem> ZoomItems = null;

        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 



        protected override void Initialize()
        {
            base.Initialize();
            PadInternal = null; // new CreativePad();
            _tmpBMP = null;
        }


        #endregion


        public override void DesignOrStyleChanged()
        {
            if (_tmpBMP != null)
            {
                _tmpBMP.Dispose();
                _tmpBMP = null;
            }


            PadInternal.SheetStyle = Parent.SheetStyle.CellFirstString();
            PadInternal.SheetStyleScale = Parent.SheetStyleScale;
        }


        protected override string ClassId()
        {
            return "CHILDPAD";
        }


        protected override void DrawExplicit(Graphics GR, Rectangle DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {

            var trp = DCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);
            GR.TranslateTransform(trp.X, trp.Y);
            GR.RotateTransform(-Rotation);

            if (PadInternal != null)
            {

                PadInternal.SheetStyle = Parent.SheetStyle.CellFirstString();
                PadInternal.SheetStyleScale = Parent.SheetStyleScale;

                //var r = UsedArea();

                //while (r.Width * cZoom > 8000 || r.Height * cZoom > 8000)
                //{
                //    cZoom = cZoom * 0.8m; // Kann ruhig verändert werden, tut nix zur Sache, DKoordinates reichen
                //}


                if (_tmpBMP != null)
                {
                    if (_tmpBMP.Width != DCoordinates.Width || DCoordinates.Height != _tmpBMP.Height)
                    {
                        _tmpBMP.Dispose();
                        _tmpBMP = null;
                        modAllgemein.CollectGarbage();
                    }
                }

                if (_tmpBMP == null)
                {
                    _tmpBMP = new Bitmap(Math.Abs(DCoordinates.Width), Math.Abs(DCoordinates.Height));
                }

                var mb = PadInternal.MaxBounds(ZoomItems);

                var zoomv = PadInternal.ZoomFitValue(mb, false, _tmpBMP.Size);
                var centerpos = PadInternal.CenterPos(mb, false, _tmpBMP.Size, zoomv);
                var slidervalues = PadInternal.SliderValues(mb, zoomv, centerpos);

                PadInternal.ShowInPrintMode = ForPrinting;
                if (ForPrinting) { PadInternal.Unselect(); }

                PadInternal.DrawCreativePadToBitmap(_tmpBMP, enStates.Standard, zoomv, (decimal)slidervalues.X, (decimal)slidervalues.Y, VisibleItems);
            }

            //If ForPrinting AndAlso _Pad IsNot Nothing Then
            //    _tmpImage = CType(_Pad.ToImage(2), Bitmap)
            //End If


            if (_tmpBMP != null)
            {
                //var scale = (float)Math.Min(DCoordinates.Width / (double)_tmpBitmap.Width, DCoordinates.Height / (double)_tmpBitmap.Height);
                //var r2 = new RectangleF((DCoordinates.Width - _tmpBitmap.Width * scale) / 2 + DCoordinates.Left, (DCoordinates.Height - _tmpBitmap.Height * scale) / 2 + DCoordinates.Top, _tmpBitmap.Width * scale, _tmpBitmap.Height * scale);

                //GR.DrawImage(_tmpBitmap, r2, new RectangleF(0, 0, _tmpBitmap.Width, _tmpBitmap.Height), GraphicsUnit.Pixel);
                GR.DrawImage(_tmpBMP, new Rectangle(-DCoordinates.Width / 2, -DCoordinates.Height / 2, DCoordinates.Width, DCoordinates.Height));
            }

            GR.TranslateTransform(-trp.X, -trp.Y);
            GR.ResetTransform();

            if (!ForPrinting)
            {
                GR.DrawRectangle(CreativePad.PenGray, DCoordinates);
            }
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
                default:
                    return base.ParseExplicit(pair);

            }
        }


        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            if (PadInternal != null)
            {
                t = t + "Data=" + PadInternal.DataToString() + ", ";
            }

            return t.Trim(", ") + "}";
        }






        private void _Pad_NeedRefresh(object sender, System.EventArgs e)
        {
            if (IsParsing) { return; }
            OnChanged();
        }

        public bool MouseDown(object sender, System.Windows.Forms.MouseEventArgs e, decimal cZoom, decimal MoveX, decimal MoveY)
        {
            if (PadInternal.Item.Count == 0) { return false; }

            var l1 = UsedArea().ZoomAndMoveRect(cZoom, MoveX, MoveY);
            var l2 = PadInternal.MaxBounds(ZoomItems);

            
            if (l1.Width <=0 || l2.Height <=0) { return false; }

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


            PadInternal.DoMouseDown(e2);

            return true;
        }

        public bool MouseMove(object sender, System.Windows.Forms.MouseEventArgs e, decimal cZoom, decimal MoveX, decimal MoveY)
        {
            if (PadInternal.Item.Count == 0) { return false; }

            var l1 = UsedArea().ZoomAndMoveRect(cZoom, MoveX, MoveY);
            var l2 = PadInternal.MaxBounds(ZoomItems);

            if (l1.Width <= 0 || l2.Height <= 0) { return false; }

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


            PadInternal.DoMouseMove(e2);

            return true;
        }

        public bool MouseUp(object sender, System.Windows.Forms.MouseEventArgs e, decimal cZoom, decimal MoveX, decimal MoveY)
        {

            if (PadInternal.Item.Count == 0) { return false; }

            var l1 = UsedArea().ZoomAndMoveRect(cZoom, MoveX, MoveY);
            var l2 = PadInternal.MaxBounds(ZoomItems);

            if (l1.Width <= 0 || l2.Height <= 0) { return false; }

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
            PadInternal.DoKeyUp(e);
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


            l.AddRange(base.GetStyleOptions(sender, e));
            return l;
        }

        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {
            base.DoStyleCommands(sender, Tags, ref CloseMenu);

        }
    }
}