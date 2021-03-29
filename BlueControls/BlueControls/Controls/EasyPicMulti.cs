using BlueBasics;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Controls {
    public partial class EasyPicMulti : GenericControl, IBackgroundNone // System.Windows.Forms.UserControl //
    {
        private int _nr = 0;
        private readonly List<BlueBasics.BitmapExt> pic = new();
        private List<string> files = new();

        public List<string> Files {
            get => files;


            set {
                pic.Clear();
                files = value;
                _nr = 0;
                if (files == null) { files = new List<string>(); }

                while (pic.Count < files.Count) { pic.Add(null); }
                SetPic();
            }
        }

        public EasyPicMulti() {
            InitializeComponent();

            // pic.ItemInternalChanged += Pic_ItemInternalChanged;

        }

        //private void Pic_ItemInternalChanged(object sender, BlueBasics.EventArgs.ListEventArgs e) {
        //    _nr = 0;
        //    Invalidate();
        //}

        private void btnRight_Click(object sender, System.EventArgs e) {

            _nr++;

            if (_nr > pic.Count - 1) { _nr = pic.Count - 1; }
            SetPic();


        }

        private void btnLeft_Click(object sender, System.EventArgs e) {
            _nr--;

            if (_nr < 0) { _nr = 0; }
            SetPic();

        }

        private void btnSchnittView_Click(object sender, System.EventArgs e) {
            var x = new PictureView(Files, false, string.Empty);
            x.Show();
        }


        private void SetPic() {
            Bitmap _Bitmap = null;





            if (pic.Count > 0) {

                if (pic[_nr] == null) {

                    pic[_nr] = new BitmapExt(files[_nr], true);
                }

                _Bitmap = pic[_nr].Bitmap;


            }

            zoompic.BMP = _Bitmap;

        }


        protected override void DrawControl(Graphics GR, enStates vState) {
            if (Convert.ToBoolean(vState & enStates.Standard_MouseOver)) { vState ^= enStates.Standard_MouseOver; }
            if (Convert.ToBoolean(vState & enStates.Standard_MousePressed)) { vState ^= enStates.Standard_MousePressed; }


            Skin.Draw_Back(GR, enDesign.EasyPic, vState, DisplayRectangle, this, true);

            //Bitmap _Bitmap = null;





            //if (pic.Count > 0) {

            //    if (pic[_nr] == null) {

            //        pic[_nr] = new BitmapExt(files[_nr], true);
            //    }

            //    _Bitmap = pic[_nr].Bitmap;


            //}


            //if (_Bitmap != null) {
            //    GR.DrawImageInRectAspectRatio(_Bitmap, 1, pnlControls.Height, Width - 2, Height - 2 - pnlControls.Height);

            //}

            Skin.Draw_Border(GR, enDesign.EasyPic, vState, DisplayRectangle);

        }
    }
}
