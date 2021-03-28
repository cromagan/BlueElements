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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.ItemCollection
{
    public class ChildPadItem : FormPadItemRectangle, IMouseAndKeyHandle, ICanHaveColumnVariables
    {




        #region  Variablen-Deklarationen 



        private Bitmap _tmpBMP;


        private CreativePad _PadInternal;
        public CreativePad PadInternal
        {
            get => _PadInternal;
            set
            {
                if (_PadInternal != null)
                {
                    _PadInternal.Item.DoInvalidate -= _Pad_DoInvalidate;
                }

                _PadInternal = value;

                if (value != null)
                {
                    _PadInternal.Item.DoInvalidate += _Pad_DoInvalidate;
                }
            }
        }
        private string _Name;

        public List<BasicPadItem> VisibleItems = null;
        public List<BasicPadItem> ZoomItems = null;

        public enAlignment Textlage { get; set; } = (enAlignment)(-1);
        public Color Randfarbe { get; set; } = Color.Transparent;

        [Description("Soll eine Umrandung einer anderen Ansicht hier angezeigt werden,<br>muss dessen Name hier eingegeben werden.")]
        public List<string> Eingebettete_Ansichten { get; set; } = new List<string>();



        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion

        [Description("Name und gleichzeitig eventuelle Beschriftung dieser Ansicht.")]
        public string Name
        {
            get => _Name;
            set
            {
                if (value == _Name) { return; }
                _Name = value;
                OnChanged();
            }
        }


        #region  Construktor  


        public ChildPadItem(ItemCollectionPad parent) : this(parent, string.Empty) { }

        public ChildPadItem(ItemCollectionPad parent, string internalname) : base(parent, internalname, false)
        {
            PadInternal = null; // new CreativePad();
            _tmpBMP = null;

            VisibleItems = null;
            ZoomItems = null;

            _Name = string.Empty;
            Textlage = (enAlignment)(-1);
            Randfarbe = Color.Transparent;
            //_ReadableText = string.Empty;
            //_VariableText = string.Empty;
            Eingebettete_Ansichten = new List<string>();
        }



        #endregion


        public override void DesignOrStyleChanged()
        {
            if (_tmpBMP != null)
            {
                _tmpBMP.Dispose();
                _tmpBMP = null;
            }


            PadInternal.Item.SheetStyle = Parent.SheetStyle;
            PadInternal.Item.SheetStyleScale = Parent.SheetStyleScale;
        }


        protected override string ClassId()
        {
            return "CHILDPAD";
        }


        protected override void DrawExplicit(Graphics GR, RectangleF DCoordinates, decimal cZoom, decimal shiftX, decimal shiftY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {
            try
            {

                var trp = DCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);
                GR.TranslateTransform(trp.X, trp.Y);
                GR.RotateTransform(-Drehwinkel);
                var font = new Font("Arial", (float)(30 * cZoom));


                if (PadInternal != null)
                {

                    PadInternal.Item.SheetStyle = Parent.SheetStyle;
                    PadInternal.Item.SheetStyleScale = Parent.SheetStyleScale;


                    if (_tmpBMP != null)
                    {
                        if (_tmpBMP.Width != DCoordinates.Width || DCoordinates.Height != _tmpBMP.Height)
                        {
                            _tmpBMP.Dispose();
                            _tmpBMP = null;
                            modAllgemein.CollectGarbage();
                        }
                    }


                    if (DCoordinates.Width < 1 || DCoordinates.Height < 1 || DCoordinates.Width > 20000 || DCoordinates.Height > 20000) { return; }

                    if (_tmpBMP == null)
                    {
                        _tmpBMP = new Bitmap((int)Math.Abs(DCoordinates.Width), (int)Math.Abs(DCoordinates.Height));
                    }

                    var mb = PadInternal.Item.MaxBounds(ZoomItems);
                    var zoomv = PadInternal.ZoomFitValue(mb, false, _tmpBMP.Size);
                    var centerpos = PadInternal.CenterPos(mb, false, _tmpBMP.Size, zoomv);
                    var slidervalues = PadInternal.SliderValues(mb, zoomv, centerpos);

                    PadInternal.ShowInPrintMode = ForPrinting;
                    if (ForPrinting) { PadInternal.Unselect(); }

                    PadInternal.DrawCreativePadToBitmap(_tmpBMP, enStates.Standard, zoomv, (decimal)slidervalues.X, (decimal)slidervalues.Y, VisibleItems);



                    if (_tmpBMP != null)
                    {

                        foreach (var thisA in Eingebettete_Ansichten)
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

                                var mb2 = Pad.PadInternal.Item.MaxBounds(Pad.ZoomItems);
                                mb2.Inflate(-1, -1);
                                var tmpG = Graphics.FromImage(_tmpBMP);
                                var p = new Pen(Pad.Randfarbe, (float)(8.7m * cZoom));
                                var p2 = new Pen(Color.White, (float)(8.7m * cZoom) + 2f);
                                p.DashPattern = new float[] { 5, 1, 1, 1 };
                                var DC2 = mb2.ZoomAndMoveRect(zoomv, (decimal)slidervalues.X, (decimal)slidervalues.Y);
                                tmpG.DrawRectangle(p2, DC2);
                                tmpG.DrawRectangle(p, DC2);



                                if (Pad.Textlage != (enAlignment)(-1))
                                {
                                    var s = tmpG.MeasureString(Pad.Name, font);

                                    tmpG.FillRectangle(Brushes.White, new RectangleF((float)DC2.Left, (float)(DC2.Top - s.Height - 9f * (float)cZoom), s.Width, s.Height));
                                    tmpG.DrawString(Pad.Name, font, new SolidBrush(Pad.Randfarbe), (float)DC2.Left, (float)(DC2.Top - s.Height - 9f * (float)cZoom));
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
                    GR.DrawString(Name, font, Brushes.Gray, (float)DCoordinates.Left, (float)DCoordinates.Top);

                }


                if (Textlage != (enAlignment)(-1))
                {
                    var p = new Pen(Randfarbe, (float)(8.7m * cZoom))
                    {
                        DashPattern = new float[] { 10, 2, 1, 2 }
                    };
                    GR.DrawRectangle(p, DCoordinates);

                    var s = GR.MeasureString(Name, font);
                    GR.DrawString(Name, font, new SolidBrush(Randfarbe), (float)DCoordinates.Left, (float)(DCoordinates.Top - s.Height - 9f * (float)cZoom));
                }


            }
            catch
            {
            }

            base.DrawExplicit(GR, DCoordinates, cZoom, shiftX, shiftY, vState, SizeOfParentControl, ForPrinting);


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
                    Größe_fixiert = value.FromPlusMinus();
                    return true;
                case "name":
                    _Name = value.FromNonCritical();
                    return true;
                case "data":
                    PadInternal = new CreativePad(new ItemCollectionPad(value, string.Empty));
                    return true;
                case "checked":
                    return true;
                case "embedded":
                    Eingebettete_Ansichten = value.FromNonCritical().SplitByCRToList();
                    return true;
                case "color":
                    Randfarbe = value.FromHTMLCode();
                    return true;
                case "pos":
                    Textlage = (enAlignment)int.Parse(value);
                    return true;
            }
            return false;
        }


        protected override void ParseFinished() { }

        public override string ToString()
        {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";


            if (!string.IsNullOrEmpty(_Name)) { t = t + "Name=" + _Name.ToNonCritical() + ", "; }

            //if (!string.IsNullOrEmpty(_ReadableText)) { t = t + "ReadableText=" + _ReadableText.ToNonCritical() + ", "; }

            if (Textlage != (enAlignment)(-1)) { t = t + "Pos=" + (int)Textlage + ", "; }

            if (Eingebettete_Ansichten.Count > 0) { t = t + "Embedded=" + Eingebettete_Ansichten.JoinWithCr().ToNonCritical() + ", "; }

            t = t + "Color=" + Randfarbe.ToHTMLCode() + ", ";


            if (PadInternal != null)
            {
                t = t + "Data=" + PadInternal.Item.ToString() + ", ";
            }

            return t.Trim(", ") + "}";
        }






        private void _Pad_DoInvalidate(object sender, System.EventArgs e)
        {
            if (IsParsing) { return; }
            OnChanged();
        }

        public bool MouseDown(object sender, System.Windows.Forms.MouseEventArgs e, decimal cZoom, decimal shiftX, decimal shiftY)
        {
            if (PadInternal == null || PadInternal.Item.Count == 0) { return false; }

            var l1 = UsedArea().ZoomAndMoveRect(cZoom, shiftX, shiftY);
            var l2 = PadInternal.Item.MaxBounds(ZoomItems);


            if (l1.Width <= 0 || l2.Height <= 0) { return false; }

            var tZo = Math.Min((decimal)l1.Width / l2.Width, (decimal)l1.Height / l2.Height);
            PadInternal.SetZoom(1);

            // Coordinaten auf Maßstab 1/1 scalieren
            var x = (e.X - (decimal)l1.X) / tZo;
            var y = (e.Y - (decimal)l1.Y) / tZo;

            // Nullpunkt verschiebung laut Maxbounds
            x += l2.X;
            y += l2.Y;

            // Und noch berücksichtigen, daß das Bild in den Rahmen eingepasst wurde
            x += (l2.Width - (decimal)l1.Width / tZo) / 2;
            y += (l2.Height - (decimal)l1.Height / tZo) / 2;

            x = Math.Min(x, int.MaxValue / 2.0m);
            y = Math.Min(y, int.MaxValue / 2.0m);
            x = Math.Max(x, int.MinValue / 2.0m);
            y = Math.Max(y, int.MinValue / 2.0m);


            var e2 = new System.Windows.Forms.MouseEventArgs(e.Button, e.Clicks, (int)x, (int)y, e.Delta);


            PadInternal.DoMouseDown(e2);

            return true;
        }

        public bool MouseMove(object sender, System.Windows.Forms.MouseEventArgs e, decimal cZoom, decimal shiftX, decimal shiftY)
        {
            if (PadInternal == null || PadInternal.Item.Count == 0) { return false; }

            var l1 = UsedArea().ZoomAndMoveRect(cZoom, shiftX, shiftY);
            var l2 = PadInternal.Item.MaxBounds(ZoomItems);

            if (l1.Width <= 0 || l2.Height <= 0) { return false; }

            decimal tZo = 1;
            if (l2.Width > 0 && l2.Height > 0) { tZo = Math.Min((decimal)l1.Width / l2.Width, (decimal)l1.Height / l2.Height); }


            PadInternal.SetZoom(1);

            // Coordinaten auf Maßstab 1/1 scalieren
            var x = (e.X - (decimal)l1.X) / tZo;
            var y = (e.Y - (decimal)l1.Y) / tZo;

            // Nullpunkt verschiebung laut Maxbounds
            x += l2.X;
            y += l2.Y;

            // Und noch berücksichtigen, daß das Bild in den Rahmen eingepasst wurde
            x += (l2.Width - (decimal)l1.Width / tZo) / 2;
            y += (l2.Height - (decimal)l1.Height / tZo) / 2;

            x = Math.Min(x, int.MaxValue / 2.0m);
            y = Math.Min(y, int.MaxValue / 2.0m);
            x = Math.Max(x, int.MinValue / 2.0m);
            y = Math.Max(y, int.MinValue / 2.0m);


            var e2 = new System.Windows.Forms.MouseEventArgs(e.Button, e.Clicks, (int)x, (int)y, e.Delta);


            PadInternal.DoMouseMove(e2);

            return true;
        }

        public bool MouseUp(object sender, System.Windows.Forms.MouseEventArgs e, decimal cZoom, decimal shiftX, decimal shiftY)
        {

            if (PadInternal.Item.Count == 0) { return false; }

            var l1 = UsedArea().ZoomAndMoveRect(cZoom, shiftX, shiftY);
            var l2 = PadInternal.Item.MaxBounds(ZoomItems);

            if (l1.Width <= 0 || l2.Height <= 0) { return false; }

            var tZo = Math.Min((decimal)l1.Width / l2.Width, (decimal)l1.Height / l2.Height);
            PadInternal.SetZoom(1);

            // Coordinaten auf Maßstab 1/1 scalieren
            var x = (e.X - (decimal)l1.X) / tZo;
            var y = (e.Y - (decimal)l1.Y) / tZo;

            // Nullpunkt verschiebung laut Maxbounds
            x += l2.X;
            y += l2.Y;

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

        public bool ReplaceVariable(string VariableName, object Value)
        {
            if (PadInternal == null) { return false; }
            var b = PadInternal.Item.ParseVariable(VariableName, Value);

            if (b) { OnChanged(); }
            return b;

        }



        public bool DoSpecialCodes()
        {
            if (PadInternal == null) { return false; }
            var b = PadInternal.Item.ParseSpecialCodes();

            if (b) { OnChanged(); }
            return b;
        }


        public bool KeyUp(object sender, System.Windows.Forms.KeyEventArgs e, decimal cZoom, decimal shiftX, decimal shiftY)
        {
            if (PadInternal.Item.Count == 0) { return false; }
            PadInternal.DoKeyUp(e, false);
            return true;
        }



        public bool ResetVariables()
        {
            if (PadInternal == null) { return false; }
            var b = PadInternal.Item.ResetVariables();
            if (b) { OnChanged(); }
            return b;
        }



        public bool RenameColumn(string oldName, ColumnItem cColumnItem)
        {
            if (PadInternal == null) { return false; }
            return PadInternal.Item.RenameColumn(oldName, cColumnItem);
        }



        public override List<FlexiControl> GetStyleOptions()
        {
            var l = new List<FlexiControl>
            {
                new FlexiControlForProperty(this, "Name"),

                new FlexiControlForProperty(this, "Randfarbe")
            };

            var Lage = new ItemCollectionList
            {
                { "ohne", "-1" },
                { "Links oben", ((int)enAlignment.Top_Left).ToString() }
            };

            l.Add(new FlexiControlForProperty(this, "Textlage", Lage));

            l.Add(new FlexiControlForProperty(this, "Eingebettete Ansichten", 5));
            l.AddRange(base.GetStyleOptions());
            return l;
        }

        //public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        //{
        //    _Name = Tags.TagGet("name").FromNonCritical();

        //    TextLage = (enAlignment)int.Parse(Tags.TagGet("Textlage"));

        //    AnsichtenVonMir = Tags.TagGet("Eingebettete Ansichten").FromNonCritical().SplitByCRToList();



        //    Randfarbe = Tags.TagGet("Randfarbe").FromHTMLCode();

        //    base.DoStyleCommands(sender, Tags, ref CloseMenu);

        //}
    }
}