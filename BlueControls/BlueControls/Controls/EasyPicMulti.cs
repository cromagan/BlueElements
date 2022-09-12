using BlueBasics;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Controls;

public partial class EasyPicMulti : GenericControl, IBackgroundNone // System.Windows.Forms.UserControl //
{
    #region Fields

    private readonly List<BitmapExt?> _pic = new();
    private List<string> _files = new();
    private int _nr;

    #endregion

    #region Constructors

    public EasyPicMulti() {
        InitializeComponent();
    }

    #endregion

    #region Properties

    public List<string> Files {
        get => _files;
        set {
            _pic.Clear();
            _files = value;
            _nr = 0;
            if (_files == null) { _files = new List<string>(); }
            while (_pic.Count < _files.Count) { _pic.Add(null); }
            SetPic();
        }
    }

    #endregion

    // pic.ItemInternalChanged += Pic_ItemInternalChanged;

    #region Methods

    protected override void DrawControl(Graphics gr, States vState) {
        if (Convert.ToBoolean(vState & States.Standard_MouseOver)) { vState ^= States.Standard_MouseOver; }
        if (Convert.ToBoolean(vState & States.Standard_MousePressed)) { vState ^= States.Standard_MousePressed; }
        Skin.Draw_Back(gr, Design.EasyPic, vState, DisplayRectangle, this, true);
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
        Skin.Draw_Border(gr, Design.EasyPic, vState, DisplayRectangle);
    }

    private void btnLeft_Click(object sender, System.EventArgs e) {
        _nr--;
        if (_nr < 0) { _nr = 0; }
        SetPic();
    }

    //private void Pic_ItemInternalChanged(object sender, BlueBasics.EventArgs.ListEventArgs e) {
    //    _nr = 0;
    //    Invalidate();
    //}
    private void btnRight_Click(object sender, System.EventArgs e) {
        _nr++;
        if (_nr > _pic.Count - 1) { _nr = _pic.Count - 1; }
        SetPic();
    }

    private void btnSchnittView_Click(object sender, System.EventArgs e) {
        PictureView x = new(Files, false, string.Empty, 0);
        x.Show();
    }

    private void SetPic() {
        Bitmap? bitmap = null;
        if (_pic.Count > 0) {
            if (_pic[_nr] == null) {
                _pic[_nr] = new BitmapExt(_files[_nr], true);
            }
            bitmap = _pic[_nr];
        }
        zoompic.Bmp = bitmap;
    }

    #endregion
}