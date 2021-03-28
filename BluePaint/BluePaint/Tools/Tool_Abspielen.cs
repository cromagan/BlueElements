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
using BlueControls.Forms;
using BluePaint.EventArgs;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.FileOperations;

namespace BluePaint
{
    public partial class Tool_Abspielen : GenericTool   // BlueControls.Forms.Form// 
    {
        private readonly List<string> _macro;
        private readonly List<GenericTool> _merker;


        public Tool_Abspielen(List<string> macro, List<GenericTool> merker) : base()
        {
            InitializeComponent();
            OnOverridePic(null);


            _macro = macro;
            _merker = merker;
        }




        public override void ToolFirstShown()
        {
            OnZoomFit();
        }

        private void optNeuerName_Click(object sender, System.EventArgs e)
        {

        }

        private void optUeberschreiben_Click(object sender, System.EventArgs e)
        {

        }

        private void optZielordner_Click(object sender, System.EventArgs e)
        {

        }

        private void txbZielordner_TextChanged(object sender, System.EventArgs e)
        {

        }

        private void txbQuelle_TextChanged(object sender, System.EventArgs e)
        {

        }

        private void btnAbspielen_Click(object sender, System.EventArgs e)
        {
            if (_macro == null || _macro.Count == 0)
            {
                MessageBox.Show("Keine Aufzeichnung vorhanden.");
                return;
            }


            if (!PathExists(txbQuelle.Text))
            {
                MessageBox.Show("Quellverzeichniss existiert nicht.");
                return;

            }

            var p = txbQuelle.Text;

            if (optZielordner.Checked)
            {

                if (!PathExists(txbZielordner.Text))
                {
                    MessageBox.Show("Zielverzeichniss existiert nicht.");
                    return;
                }
                p = txbZielordner.Text;
            }



            var f = System.IO.Directory.GetFiles(txbQuelle.Text, "*.PNG", System.IO.SearchOption.TopDirectoryOnly);
            if (f == null || f.GetUpperBound(0) < 0)
            {
                MessageBox.Show("Keine Dateien im Quellverzeichniss gefunden.");
                return;
            }


            foreach (var thisf in f)
            {
                OnOverridePic((Bitmap)BitmapExt.Image_FromFile(thisf));
                OnZoomFit();
                Develop.DoEvents();

                foreach (var thisS in _macro)
                {
                    DoMakro(thisS);
                    OnZoomFit();
                    Develop.DoEvents();
                }


                var newf = TempFile(p, thisf.FileNameWithoutSuffix(), "PNG");
                var B = OnNeedCurrentPic();

                B.Save(newf, System.Drawing.Imaging.ImageFormat.Png);
                B = null;
                modAllgemein.CollectGarbage();

            }

        }

        private void DoMakro(string thisS)
        {


            var t = thisS.SplitBy(";");

            foreach (var ThisTool in _merker)
            {

                if (ThisTool.MacroKennung() == t[0].FromNonCritical())
                {
                    ThisTool.OverridePic += ThisTool_OverridePic;
                    ThisTool.NeedCurrentPic += ThisTool_NeedCurrentPic;
                    ThisTool.ExcuteCommand(t[1].FromNonCritical());
                    ThisTool.OverridePic -= ThisTool_OverridePic;
                    ThisTool.NeedCurrentPic -= ThisTool_NeedCurrentPic;
                    return; // keine weiteren kennungen reagieren lassen
                }
            }
        }

        private void ThisTool_NeedCurrentPic(object sender, BitmapEventArgs e)
        {
            e.BMP = OnNeedCurrentPic();
        }

        private void ThisTool_OverridePic(object sender, BitmapEventArgs e)
        {
            OnOverridePic(e.BMP);
        }
    }
}