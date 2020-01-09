#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
        private string _Name;

        public List<BasicPadItem> VisibleItems = null;
        public List<BasicPadItem> ZoomItems = null;


        public enAlignment TextLage = (enAlignment)(-1);
        public Color Farbe = Color.Transparent;
        public List<string> AnsichtenVonMir = new List<string>();



        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (value == _Name) { return; }
                _Name = value;
                OnChanged();
            }
        }

        //public string Text
        //{
        //    get
        //    {
        //        return _ReadableText;
        //    }
        //    set
        //    {
        //        if (value == _ReadableText) { return; }
        //        _ReadableText = value;
        //        OnChanged();
        //    }
        //}

        #region  Construktor  


        public ChildPadItem() : this(string.Empty) { }

        public ChildPadItem(string internalname) : base(internalname)
        {
            PadInternal = null; // new CreativePad();
            _tmpBMP = null;

            VisibleItems = null;
            ZoomItems = null;

            _Name = string.Empty;
            TextLage = (enAlignment)(-1);
            Farbe = Color.Transparent;
            //_ReadableText = string.Empty;
            //_VariableText = string.Empty;
            AnsichtenVonMir = new List<string>();
        }



        #endregion


        public override void DesignOrStyleChanged()
        {
            if (_tmpBMP != null)
            {
                _tmpBMP.Dispose();
                _tmpBMP = null;
            }


            PadInternal.SheetStyle = ((ItemCollectionPad)Parent).SheetStyle.CellFirstString();
            PadInternal.SheetStyleScale = ((ItemCollectionPad)Parent).SheetStyleScale;
        }


        protected override string ClassId()
        {
            return "CHILDPAD";
        }


        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {

            try
            {



                var trp = DCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);
                GR.TranslateTransform(trp.X, trp.Y);
                GR.RotateTransform(-Rotation);
                var font = new Font("Arial", (float)(30 * cZoom));


                if (PadInternal != null)
                {

                    PadInternal.SheetStyle = ((ItemCollectionPad)Parent).SheetStyle.CellFirstString();
                    PadInternal.SheetStyleScale = ((ItemCollectionPad)Parent).SheetStyleScale;


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
                        _tmpBMP = new Bitmap((int)Math.Abs(DCoordinates.Width), (int)Math.Abs(DCoordinates.Height));
                    }

                    var mb = PadInternal.MaxBounds(ZoomItems);

                    var zoomv = PadInternal.ZoomFitValue(mb, false, _tmpBMP.Size);
                    var centerpos = PadInternal.CenterPos(mb, false, _tmpBMP.Size, zoomv);
                    var slidervalues = PadInternal.SliderValues(mb, zoomv, centerpos);

                    PadInternal.ShowInPrintMode = ForPrinting;
                    if (ForPrinting) { PadInternal.Unselect(); }

                    PadInternal.DrawCreativePadToBitmap(_tmpBMP, enStates.Standard, zoomv, (decimal)slidervalues.X, (decimal)slidervalues.Y, VisibleItems);



                    if (_tmpBMP != null)
                    {

                        foreach (var thisA in AnsichtenVonMir)
                        {
                            ChildPadItem Pad = null;
                            foreach (var It in Parent)
                            {
                                if (It is ChildPadItem CP)
                                {
                                    if (CP.Name.ToUpper() == thisA.ToUpper())
                                    {
                                        Pad = CP;
                                        break;
                                    }
                                }
                            }


                            if (Pad != null)
                            {

                                var mb2 = Pad.PadInternal.MaxBounds(Pad.ZoomItems);
                                mb2.Inflate(-1, -1);
                                var tmpG = Graphics.FromImage(_tmpBMP);
                                var p = new Pen(Pad.Farbe, (float)(8.7m * cZoom));
                                var p2 = new Pen(Color.White, (float)(8.7m * cZoom) + 2f);
                                p.DashPattern = new float[] { 5, 1, 1, 1 };
                                var DC2 = mb2.ZoomAndMoveRect(zoomv, (decimal)slidervalues.X, (decimal)slidervalues.Y);
                                tmpG.DrawRectangle(p2, DC2);
                                tmpG.DrawRectangle(p, DC2);



                                if (Pad.TextLage != (enAlignment)(-1))
                                {
                                    var s = tmpG.MeasureString(Pad.Name, font);

                                    tmpG.FillRectangle(Brushes.White, new RectangleF((float)DC2.Left, (float)(DC2.Top - s.Height - 9f * (float)cZoom), s.Width, s.Height));
                                    tmpG.DrawString(Pad.Name, font, new SolidBrush(Pad.Farbe), (float)DC2.Left, (float)(DC2.Top - s.Height - 9f * (float)cZoom));
                                }
                            }
                        }


                        GR.DrawImage(_tmpBMP, new Rectangle((int)-DCoordinates.Width / 2, (int)-DCoordinates.Height / 2, (int)DCoordinates.Width, (int)DCoordinates.Height));
                    }
                }
                GR.TranslateTransform(-trp.X, -trp.Y);
                GR.ResetTransform();




                if (!ForPrinting)
                {
                    GR.DrawRectangle(CreativePad.PenGray, DCoordinates);
                }

                if (TextLage != (enAlignment)(-1))
                {
                    var p = new Pen(Farbe, (float)(8.7m * cZoom));
                    p.DashPattern = new float[] { 10, 2, 1, 2 };
                    GR.DrawRectangle(p, DCoordinates);

                    var s = GR.MeasureString(Name, font);
                    GR.DrawString(Name, font, new SolidBrush(Farbe), (float)DCoordinates.Left, (float)(DCoordinates.Top - s.Height - 9f * (float)cZoom));
                }


            }
            catch
            {
            }

        }


        public override bool ParseThis(string tag, string value)
        {
            if (base.ParseThis(tag, value)) { return true; }

            switch (tag)
            {
                //case "readabletext":
                //    _ReadableText = value.FromNonCritical();
                //    _VariableText = _ReadableText;
                //    return true;

                case "fixsize":
                    FixSize = value.FromPlusMinus();
                    return true;
                case "name":
                    _Name = value.FromNonCritical();
                    return true;
                case "data":
                    PadInternal = new CreativePad();
                    PadInternal.ParseData(value, false, string.Empty);
                    return true;
                case "checked":
                    return true;
                case "embedded":
                    AnsichtenVonMir = value.FromNonCritical().SplitByCRToList();
                    return true;
                case "color":
                    Farbe = value.FromHTMLCode();
                    return true;
                case "pos":
                    TextLage = (enAlignment)int.Parse(value);
                    return true;

            }
            return false;
        }


        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";


            if (!string.IsNullOrEmpty(_Name)) { t = t + "Name=" + _Name.ToNonCritical() + ", "; }

            //if (!string.IsNullOrEmpty(_ReadableText)) { t = t + "ReadableText=" + _ReadableText.ToNonCritical() + ", "; }

            if (TextLage != (enAlignment)(-1)) { t = t + "Pos=" + (int)TextLage + ", "; }

            if (AnsichtenVonMir.Count > 0) { t = t + "Embedded=" + AnsichtenVonMir.JoinWithCr().ToNonCritical() + ", "; }

            t = t + "Color=" + Farbe.ToHTMLCode() + ", ";


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


            if (l1.Width <= 0 || l2.Height <= 0) { return false; }

            var tZo = Math.Min((decimal)l1.Width / l2.Width, (decimal)l1.Height / l2.Height);
            PadInternal.SetZoom(1);

            // Coordinaten auf Maßstab 1/1 scalieren
            var x = (e.X - (decimal)l1.X) / tZo;
            var y = (e.Y - (decimal)l1.Y) / tZo;

            // Nullpunkt verschiebung laut Maxbounds
            x = x + l2.X;
            y = y + l2.Y;

            // Und noch berücksichtigen, daß das Bild in den Rahmen eingepasst wurde
            x = x + (l2.Width - (decimal)l1.Width / tZo) / 2;
            y = y + (l2.Height - (decimal)l1.Height / tZo) / 2;

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
            if (l2.Width > 0 && l2.Height > 0) { tZo = Math.Min((decimal)l1.Width / l2.Width, (decimal)l1.Height / l2.Height); }


            PadInternal.SetZoom(1);

            // Coordinaten auf Maßstab 1/1 scalieren
            var x = (e.X - (decimal)l1.X) / tZo;
            var y = (e.Y - (decimal)l1.Y) / tZo;

            // Nullpunkt verschiebung laut Maxbounds
            x = x + l2.X;
            y = y + l2.Y;

            // Und noch berücksichtigen, daß das Bild in den Rahmen eingepasst wurde
            x = x + (l2.Width - (decimal)l1.Width / tZo) / 2;
            y = y + (l2.Height - (decimal)l1.Height / tZo) / 2;

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

            var tZo = Math.Min((decimal)l1.Width / l2.Width, (decimal)l1.Height / l2.Height);
            PadInternal.SetZoom(1);

            // Coordinaten auf Maßstab 1/1 scalieren
            var x = (e.X - (decimal)l1.X) / tZo;
            var y = (e.Y - (decimal)l1.Y) / tZo;

            // Nullpunkt verschiebung laut Maxbounds
            x = x + l2.X;
            y = y + l2.Y;

            // Und noch berücksichtigen, daß das Bild in den Rahmen eingepasst wurde
            x = x + (l2.Width - (decimal)l1.Width / tZo) / 2;
            y = y + (l2.Height - (decimal)l1.Height / tZo) / 2;

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
            PadInternal.DoKeyUp(e, false);
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

            l.Add(new FlexiControl("Name", _Name, enDataFormat.Text, 1));
            //l.Add(new FlexiControl("Text", _VariableText, enDataFormat.Text, 5));

            l.Add(new FlexiControl("Randfarbe", Farbe.ToHTMLCode(), enDataFormat.Text, 1));


            var Lage = new ItemCollectionList();
            Lage.Add(new TextListItem("-1", "ohne"));
            Lage.Add(new TextListItem(((int)enAlignment.Top_Left).ToString(), "Links oben"));
            //Lage.Add(new TextListItem(((int)enAlignment.Bottom_Left).ToString(), "Links unten"));
            l.Add(new FlexiControl("Textlage", ((int)TextLage).ToString(), Lage));


            l.Add(new FlexiControl("Eingebettete Ansichten", AnsichtenVonMir.JoinWithCr(), enDataFormat.Text, 5));




            l.AddRange(base.GetStyleOptions(sender, e));
            return l;
        }

        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {
            _Name = Tags.TagGet("name").FromNonCritical();

            TextLage = (enAlignment)int.Parse(Tags.TagGet("Textlage"));

            AnsichtenVonMir = Tags.TagGet("Eingebettete Ansichten").FromNonCritical().SplitByCRToList();



            Farbe = Tags.TagGet("Randfarbe").FromHTMLCode();

            base.DoStyleCommands(sender, Tags, ref CloseMenu);

        }
    }
}