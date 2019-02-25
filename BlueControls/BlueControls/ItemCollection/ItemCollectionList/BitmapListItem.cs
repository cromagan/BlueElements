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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using static BlueBasics.FileOperations;

namespace BlueControls.ItemCollection
{
    public class BitmapListItem : BasicListItem
    {
        #region  Variablen-Deklarationen 


        private Bitmap _Bitmap;
        private string _caption;
        private List<string> _captiontmp = new List<string>();
        private int _padding;
        private readonly ListExt<QuickImage> _overlays = new ListExt<QuickImage>();

        private string _ImageFilename;

        private string _EncryptionKey;

        private int _captionlines = 2;

        private const int ConstMY = 15;

        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 



        public BitmapListItem()
        { }


        public BitmapListItem(string intern, string caption)
        {
            _Internal = intern;
            _caption = caption;
            _captiontmp.Clear();
            if (string.IsNullOrEmpty(_Internal))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben.");
            }
        }


        public BitmapListItem(Bitmap BMP, string caption)
        {
            _Bitmap = BMP;
            _caption = caption;
            _captiontmp.Clear();
            if (string.IsNullOrEmpty(_Internal))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben.");
            }
        }


        public BitmapListItem(string intern, Bitmap BMP)
        {
            _Internal = intern;
            _Bitmap = BMP;
            if (string.IsNullOrEmpty(_Internal))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben.");
            }
        }

        public BitmapListItem(Bitmap BMP)
        {
            _Bitmap = BMP;

            if (string.IsNullOrEmpty(_Internal))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben.");
            }
        }


        public BitmapListItem(string intern, string caption, string Filename, string EncryptionKey)
        {
            _Internal = intern;

            _caption = caption;
            _captiontmp.Clear();

            _ImageFilename = Filename;
            _EncryptionKey = EncryptionKey;


            if (string.IsNullOrEmpty(_Internal))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben.");
            }
        }


        public BitmapListItem(string intern, string caption, QuickImage QI)
        {
            _Internal = intern;

            _caption = caption;
            _captiontmp.Clear();

            _Bitmap = QI.BMP;
            if (string.IsNullOrEmpty(_Internal))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben.");
            }
        }





        protected override void InitializeLevel2()
        {
            _caption = string.Empty;
            _captiontmp.Clear();
            _Bitmap = null;

            _ImageFilename = string.Empty;
            //_ImageDatassystem = Nothing


            _overlays.Clear();
            _overlays.ListOrItemChanged += _overlays_ListOrItemChanged;


            _padding = 0;


            _EncryptionKey = string.Empty;
        }

        private void _overlays_ListOrItemChanged(object sender, System.EventArgs e)
        {
            OnChanged();
        }


        #endregion


        #region  Properties 


        public Bitmap Bitmap
        {
            get
            {
                GetImage();
                return _Bitmap;
            }
            set
            {
                _ImageFilename = string.Empty;
                _Bitmap = value;
                OnChanged();
            }
        }

        public string Caption
        {
            get
            {
                return _caption;
            }

            set
            {
                if (_caption == value) { return; }
                _caption = value;
                _captiontmp.Clear();
                OnChanged();
            }
        }

        public int CaptionLines
        {
            get
            {
                return _captionlines;
            }
            set
            {
                if (value < 1) { value = 1; }

                if (_captionlines == value) { return; }
                _captionlines = value;
                _captiontmp.Clear();
                OnChanged();
            }
        }

        public int Padding
        {
            get
            {
                return _padding;
            }

            set
            {
                if (_padding == value) { return; }
                _padding = value;
                OnChanged();
            }
        }

        public List<QuickImage> Overlays
        {
            get
            {
                return _overlays;
            }

        }


        #endregion



        public override string Internal()
        {
            return _Internal;
        }
        public override void DesignOrStyleChanged()
        {
            // Keine Variablen zum Reseten, ein Invalidate reicht
        }


        protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enStates vState, bool DrawBorderAndBack)
        {

            if (DrawBorderAndBack)
            {
                GenericControl.Skin.Draw_Back(GR, Parent.ItemDesign, vState, PositionModified, null, false);
            }
            var DCoordinates = PositionModified;
            DCoordinates.Inflate(-_padding, -_padding);
            var ScaledImagePosition = new RectangleF();
            var AreaOfWholeImage = new RectangleF();

            GetImage();

            if (!string.IsNullOrEmpty(_caption) && _captiontmp.Count == 0) { _captiontmp = _caption.SplitByWidth(DCoordinates.Width, _captionlines, enDesign.Item_Listbox, enStates.Standard); }





            if (_Bitmap != null)
            {
                AreaOfWholeImage = new RectangleF(0, 0, _Bitmap.Width, _Bitmap.Height);
                var scale = (float)Math.Min((DCoordinates.Width - _padding * 2) / (double)_Bitmap.Width,
                                              (DCoordinates.Height - _padding * 2 - _captionlines * ConstMY) / (double)_Bitmap.Height);

                ScaledImagePosition = new RectangleF((DCoordinates.Width - _Bitmap.Width * scale) / 2 + DCoordinates.Left,
                                                     (DCoordinates.Height - _Bitmap.Height * scale) / 2 + DCoordinates.Top - _captionlines * ConstMY / 2,
                                                    _Bitmap.Width * scale,
                                                    _Bitmap.Height * scale);
            }


            var trp = DCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);

            ScaledImagePosition = new RectangleF(ScaledImagePosition.Left - trp.X, ScaledImagePosition.Top - trp.Y, ScaledImagePosition.Width, ScaledImagePosition.Height);


            GR.TranslateTransform(trp.X, trp.Y);
            if (_Bitmap != null) { GR.DrawImage(_Bitmap, ScaledImagePosition, AreaOfWholeImage, GraphicsUnit.Pixel); }

            foreach (var thisQI in Overlays)
            {
                GR.DrawImage(thisQI.BMP, ScaledImagePosition.Left + 8, ScaledImagePosition.Top + 8);
            }

            if (!string.IsNullOrEmpty(_caption))
            {


                var c = _captiontmp.Count;

                var Ausgl = (c - _captionlines) * ConstMY / 2;


                foreach (var ThisCap in _captiontmp)
                {
                    c--;
                    SizeF s = GenericControl.Skin.FormatedText_NeededSize(ThisCap, null, GenericControl.Skin.GetBlueFont(enDesign.Item_Listbox, vState));
                    var r = new Rectangle((int)(DCoordinates.Left + (DCoordinates.Width - s.Width) / 2.0), (int)(DCoordinates.Bottom - s.Height) - 3, (int)s.Width, (int)s.Height);

                    r.X = r.X - trp.X;
                    r.Y = r.Y - trp.Y;

                    r.Y = r.Y - ConstMY * c + Ausgl;


                    //r = new Rectangle(r.Left - trp.X, r.Top - trp.Y, r.Width, r.Height);
                    //GenericControl.Skin.Draw_Back(GR, enDesign.Item_Listbox_Unterschrift, vState, r, null, false);
                    //GenericControl.Skin.Draw_Border(GR, enDesign.Item_Listbox_Unterschrift, vState, r);
                    GenericControl.Skin.Draw_FormatedText(GR, ThisCap, enDesign.Item_Listbox, vState, null, enAlignment.Horizontal_Vertical_Center, r, null, false);

                }
            }


            GR.TranslateTransform(-trp.X, -trp.Y);
            GR.ResetTransform();


            if (DrawBorderAndBack)
            {
                GenericControl.Skin.Draw_Border(GR, Parent.ItemDesign, vState, PositionModified);
            }

        }



        public override SizeF SizeUntouchedForListBox()
        {
            if (_Bitmap == null) { Debugger.Break(); }
            GetImage();
            if (_Bitmap == null) { return SizeF.Empty; }
            return _Bitmap.Size;
        }

        private void GetImage()
        {
            if (string.IsNullOrEmpty(_ImageFilename)) { return; }
            if (_Bitmap != null) { return; }

            try
            {

                if (FileExists(_ImageFilename))
                {
                    if (!string.IsNullOrEmpty(_EncryptionKey))
                    {
                        var b = modConverter.FileToByte(_ImageFilename);
                        b = modAllgemein.SimpleCrypt(b, _EncryptionKey, -1);
                        _Bitmap = modConverter.ByteToBitmap(b);
                    }
                    else
                    {
                        _Bitmap = (Bitmap)modAllgemein.Image_FromFile(_ImageFilename);
                    }

                }
            }
            catch (Exception ex)
            {
                Develop.DebugPrint(ex);
            }
        }


        public bool ImageLoaded()
        {
            return _Bitmap != null;
        }





        public override bool IsClickable()
        {
            return true;
        }

        public override void ComputePositionForListBox(enBlueListBoxAppearance IsIn, float X, float Y, float MultiX, int SliderWidth, int MaxWidth)
        {

            MaxWidth = Math.Min(MaxWidth, 800);


            switch (IsIn)
            {
                case enBlueListBoxAppearance.Gallery:
                    SetCoordinates(new Rectangle((int)X, (int)Y, (int)MultiX, (int)(MultiX * 0.8)));
                    break;

                case enBlueListBoxAppearance.FileSystem:
                    SetCoordinates(new Rectangle((int)X, (int)Y, 110, 110 + _captionlines * ConstMY));
                    break;

                default:
                    SetCoordinates(new Rectangle((int)X, (int)Y, (int)(MultiX - SliderWidth), (int)(MaxWidth * 0.8)));
                    break;

            }




        }

        public override SizeF QuickAndDirtySize(int PreferedHeigth)
        {


            if (ImageLoaded())
            {
                return SizeUntouchedForListBox();
            }

            return new SizeF(400, PreferedHeigth);
        }


        protected override string GetCompareKey()
        {
            return Internal();
        }


        protected override BasicListItem CloneLVL2()
        {
            Develop.DebugPrint_NichtImplementiert();
            return null;
        }

        protected override bool FilterMatchLVL2(string FilterText)
        {
            if (Caption.ToUpper().Contains(FilterText.ToUpper())) { return true; }
            if (_ImageFilename.ToUpper().Contains(FilterText.ToUpper())) { return true; }
            return false;
        }


    }
}
