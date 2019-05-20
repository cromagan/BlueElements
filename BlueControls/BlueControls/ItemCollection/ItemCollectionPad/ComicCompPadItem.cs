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
using BlueControls.DialogBoxes;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using static BlueBasics.FileOperations;

namespace BlueControls.ItemCollection
{
    public class ComicCompPadItem : BasicPadItem, ICloneable
    {



        #region  Variablen-Deklarationen 


        /// <summary>
        /// Haupt Gelenkpunkt 1
        /// </summary>
        public PointDF P1;
        /// <summary>
        /// Winkel: LO-Eckpunktes des Images - Gelenkpunkt 1
        /// </summary>
        private decimal _W1;
        /// <summary>
        /// Länge: LO-Eckpunktes des Images - Gelenkpunkt 1
        /// </summary>
        private decimal _L1;


        /// <summary>
        /// Haupt Gelenkpunkt 2
        /// </summary>
        public PointDF P2;
        /// <summary>
        /// Winkel: RU-Eckpunktes des Images - Gelenkpunkt 2
        /// </summary>
        private decimal _W2;
        /// <summary>
        /// Länge: RU-Eckpunktes des Images - Gelenkpunkt 2
        /// </summary>
        private decimal _L2;




        public readonly List<PointDF> AdditionalPoints = new List<PointDF>();

        /// <summary>
        /// Winkel: Gelenkpunkt 1 - Gelenkpunkt 2
        /// </summary>
        private double _Winkel;

        /// <summary>
        /// Länge: Gelenkpukt 1 - Gelenkpunkt 2
        /// Wenn die Größe des Bildes Fest sein soll, ist hier eine Länge der zwei Punkte angegeben
        /// </summary>
        private decimal _Length;


        private Bitmap _Bitmap;

        private bool _EinpassModus;

        /// <summary>
        /// Diese Punkte bestimmen die gedrehten Eckpunkte des Bildes und werden von den Gelenkpunkten aus berechnet. Unskaliert und auch ohne Berücksichtigung der 'Move' Koordinaten
        /// </summary>
        private PointDF BerLO;
        private PointDF BerRO;
        private PointDF BerRU;
        private PointDF BerLU;

        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 



        public ComicCompPadItem()
        { }

        public ComicCompPadItem(string Internal, Bitmap cBitmap)
        {

            _Internal = Internal;
            _Bitmap = cBitmap;
            ImageChanged();
            if (string.IsNullOrEmpty(_Internal))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben.");
            }
        }


        protected override void InitializeLevel2()
        {
            P1 = new PointDF(this, "Punkt1", 0, 0, false, true, true);
            P2 = new PointDF(this, "Punkt2", 0, 0);
            _Bitmap = null;
            _Length = 0M;
            _EinpassModus = false;

            _Winkel = 90;
            _W1 = 0M;
            _L1 = 0M;
            _W2 = 0M;
            _L2 = 0M;

            AdditionalPoints.Clear();
        }


        #endregion


        #region  Properties 


        public Bitmap Bitmap
        {
            get
            {
                return _Bitmap;
            }
            set
            {
                _Bitmap = value;
                ImageChanged();
            }
        }


        #endregion


        public enum enMovePoints
        {
            LO_to_RU = 0,
            MO_to_MU = 1

        }

        public void ImageChanged()
        {

            P1.X = 0M;
            P1.Y = 0M;

            if (_Bitmap == null)
            {
                P2.X = 100M;
                P2.Y = 100M;
            }
            else
            {
                P2.X = _Bitmap.Width;
                P2.Y = _Bitmap.Height;
            }

            GetProportionsAndResetAngle();

            ClearInternalRelations();
        }

        public void GetProportionsAndResetAngle()
        {
            _EinpassModus = false;

            _Winkel = Convert.ToDouble(360 - GeometryDF.Winkelx(P2, P1));
            _W1 = 0M;
            _L1 = 0M;
            _W2 = 0M;
            _L2 = 0M;

            SetLengthToFix();

            CalculateCorners();
        }





        public override void DesignOrStyleChanged()
        {
            // Keine Variablen zum Reseten, ein Invalidate reicht
        }


        protected override string ClassId()
        {
            return "COMIC";
        }

        public override bool Contains(PointF value, decimal zoomfactor)
        {

            var ne = 5 / zoomfactor;

            if (value.DistanzZuStrecke(P1, P2) < ne)
            {
                return true;
            }

            foreach (var Thispoint in PointList())
            {
                if (Geometry.Länge(value, Thispoint.ToPointF()) < ne)
                {
                    return true;
                }
            }



            return false;
        }

        public PointDF MiddlePoint()
        {
            return new PointDF((P1.X + P2.X) / 2, (P1.Y + P2.Y) / 2);
        }

        public decimal AngelOfMiddleLine()
        {
            return GeometryDF.Winkelx(P1, P2);
        }


        private void CalculateCorners()
        {

            var Mitte = MiddlePoint();
            var WP12 = AngelOfMiddleLine();


            if (_EinpassModus)
            {
                // Einpassmodus bleiben die Eckpunkte so wie sie sind, _Winkel, _W1, _W2, _L1, _L2 und evtl. _Lenght passen sich an
                // Deswegen dürfen die BER Koordinaten nicht bewegt werden
                var TMP = GeometryDF.Winkelx(BerLO, BerRO);
                _Winkel = Convert.ToDouble(TMP - WP12 - 360);
                if (_Winkel <= -360)
                {
                    _Winkel += 360;
                }
                if (_Winkel >= 360)
                {
                    _Winkel -= 360;
                }

                _L1 = GeometryDF.Länge(P1, BerLO);
                _W1 = GeometryDF.Winkelx(P1, BerLO) + (360 - TMP);

                _L2 = GeometryDF.Länge(P2, BerRU);
                _W2 = GeometryDF.Winkelx(P2, BerRU) + (360 - TMP);

                if (_Length > 0M)
                {
                    SetLengthToFix();
                }

                foreach (var thispoint in AdditionalPoints)
                {
                    thispoint.Tag = GeometryDF.Länge(Mitte, thispoint) + ";" + (GeometryDF.Winkelx(Mitte, thispoint) - WP12);
                }

            }
            else
            {
                // hier ist es andersrum: _Winkel, _W1, _W2, _L1, _L2 und _Length bestimmen die BER Koordinaten
                var w = (decimal)(_Winkel + (double)WP12);

                BerLO = new PointDF(P1, _L1, _W1 + w);
                BerRU = new PointDF(P2, _L2, _W2 + w);

                var L = GeometryDF.Länge(BerLO, BerRU); // Hypothenuse
                var Alpha = Convert.ToDecimal(GeometryDF.Winkelx(BerLO, BerRU) - w);
                var ImLänge = Geometry.Cosinus((double)Alpha) * (double)L;

                BerRO = new PointDF(BerLO, (decimal)ImLänge, w);
                BerLU = new PointDF(BerRU, (decimal)ImLänge, 180 + w);
            }

        }


        protected override void DrawExplicit(Graphics GR, Rectangle DCoordinates, decimal cZoom, decimal MoveX, decimal MoveY, enStates vState, Size SizeOfParentControl, bool ForPrinting)
        {


            CalculateCorners();

            var LOt = BerLO.ZoomAndMove(cZoom, MoveX, MoveY);
            var rOt = BerRO.ZoomAndMove(cZoom, MoveX, MoveY);
            var RUt = BerRU.ZoomAndMove(cZoom, MoveX, MoveY);
            var lUt = BerLU.ZoomAndMove(cZoom, MoveX, MoveY);

            PointF[] destPara2 = {LOt, rOt, lUt};

            if (_Bitmap != null)
            {
                GR.DrawImage(_Bitmap, destPara2, new RectangleF(0, 0, _Bitmap.Width, _Bitmap.Height), GraphicsUnit.Pixel);
            }

            if (_EinpassModus || _Bitmap == null)
            {
                var Mitte = MiddlePoint();

                GR.DrawLine(CreativePad.PenGrayLarge, LOt, rOt);
                GR.DrawLine(CreativePad.PenGrayLarge, rOt, RUt);
                GR.DrawLine(CreativePad.PenGrayLarge, RUt, lUt);
                GR.DrawLine(CreativePad.PenGrayLarge, lUt, LOt);
                GR.DrawLine(CreativePad.PenGrayLarge, P1.ZoomAndMove(cZoom, MoveX, MoveY), P2.ZoomAndMove(cZoom, MoveX, MoveY));


                foreach (var thispoint in AdditionalPoints)
                {
                    GR.DrawLine(CreativePad.PenGray, Mitte.ZoomAndMove(cZoom, MoveX, MoveY), thispoint.ZoomAndMove(cZoom, MoveX, MoveY));
                    thispoint.Draw(GR, cZoom, MoveX, MoveY, enDesign.Button_EckpunktSchieber, enStates.Standard);
                }
            }


            if (!ForPrinting)
            {
                GR.DrawLine(CreativePad.PenGray, P1.ZoomAndMove(cZoom, MoveX, MoveY), P2.ZoomAndMove(cZoom, MoveX, MoveY));

                foreach (var thispoint in AdditionalPoints)
                {
                    thispoint.Draw(GR, cZoom, MoveX, MoveY, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard);
                }

                //  GR.DrawImage(GetTransformedImage, New Rectangle(0, 0, 300, 300))

                //Dim pss1 As PointF = UsedArea.PointOf(enAlignment.Top_Left).ZoomAndMove(cZoom, MoveX, MoveY)
                //Dim pss2 As PointF = UsedArea.PointOf(enAlignment.Bottom_Right).ZoomAndMove(cZoom, MoveX, MoveY)
                //GR.DrawRectangle(CreativePad.PenGrayLarge, New Rectangle(New Point(CInt(pss1.X), CInt(pss1.Y)), New Size(CInt(pss2.X - pss1.X), CInt(pss2.Y - pss1.Y))))
            }

        }



        public override List<PointDF> PointList()
        {
            var l = new List<PointDF>();

            l.Add(P1);
            l.Add(P2);


            l.AddRange(AdditionalPoints);

            return l;
        }





        public override RectangleDF UsedArea()
        {


            var x = new List<PointDF>();

            x.Add(P1);
            x.Add(P2);

            if (BerLO == null) { CalculateCorners(); }

            if (BerLO.X != 0M || BerRO.X != 0M)
            {
                x.Add(BerLO);
                x.Add(BerRO);
                x.Add(BerRU);
                x.Add(BerLU);
            }

            x.AddRange(PointList());


            var x1 = int.MaxValue;
            var y1 = int.MaxValue;
            var x2 = int.MinValue;
            var y2 = int.MinValue;


            foreach (var ThisPoint in x)
            {
                x1 = (int)(Math.Min(ThisPoint.X, x1));
                y1 = (int)(Math.Min(ThisPoint.Y, y1));

                x2 = (int)(Math.Max(ThisPoint.X, x2));
                y2 = (int)(Math.Max(ThisPoint.Y, y2));
            }

            return new RectangleDF(x1, y1, x2 - x1, y2 - y1);
        }

        public void SetLengthToFix()
        {
            _Length = GeometryDF.Länge(P1, P2);
        }


        protected override bool ParseLevel2(KeyValuePair<string, string> pair)
        {
            switch (pair.Key)
            {
                case "additionalpoints":
                    AdditionalPoints.Clear();
                    for (var z = 1 ; z <= int.Parse(pair.Value) ; z++)
                    {
                        var p = new PointDF(this, "Zusatz" + z, 0, 0, false, true);
                        AdditionalPoints.Add(p);
                    }
                    return true;

                case "angle1":
                    _W1 = decimal.Parse(pair.Value);
                    return true;

                case "angle2":
                    _W2 = decimal.Parse(pair.Value);
                    return true;

                case "angle3":
                    _Winkel = Convert.ToDouble(decimal.Parse(pair.Value));
                    return true;

                case "length1":
                    _L1 = decimal.Parse(pair.Value);
                    return true;

                case "length2":
                    _L2 = decimal.Parse(pair.Value);
                    return true;

                case "image":
                case "bitmap":
                    // TODO: image entfernen
                    _Bitmap = modConverter.Base64ToBitmap(pair.Value);
                    return true;

                case "fixlenght":
                    _Length = decimal.Parse(pair.Value);
                    return true;
            }

            return false;
        }


        protected override string ToStringLevel2()
        {
            var t = "";

            t = t + "AdditionalPoints=" + AdditionalPoints.Count + ", ";

            t = t + "Angle1=" + _W1 + ", ";
            t = t + "Length1=" + _L1 + ", ";
            t = t + "Angle2=" + _W2 + ", ";
            t = t + "Length2=" + _L2 + ", ";
            t = t + "Angle3=" + _Winkel + ", ";

            t = t + "FixLenght=" + _Length + ", ";


            if (_Bitmap != null) { t = t + "Bitmap=" + modConverter.BitmapToBase64(_Bitmap, ImageFormat.Png) + ", "; }
            return t.Trim(", ");
        }


        public override void SetCoordinates(RectangleDF r)
        {
            P1.SetTo(r.PointOf(enAlignment.Top_Left));
            P2.SetTo(r.PointOf(enAlignment.Bottom_Right));
            GetProportionsAndResetAngle();

            RecomputePointAndRelations();
        }


        public void Move(decimal x, decimal y)
        {
            P1.X += x;
            P2.X += x;
            P1.Y += y;
            P2.Y += y;

            RecomputePointAndRelations();
            CalculateCorners();
        }

        public void Move(PointDF P)
        {
            Move(P.X, P.Y);
        }



        public void SetCoordinates(Rectangle r, enMovePoints MovePoints)
        {
            P1.SetTo(r.PointOf(enAlignment.Top_Left));
            P2.SetTo(r.PointOf(enAlignment.Bottom_Right));

            GetProportionsAndResetAngle();

            switch (MovePoints)
            {
                case enMovePoints.LO_to_RU:
                    // Passt schon
                    break;
                case enMovePoints.MO_to_MU:
                    _EinpassModus = false;
                    _Winkel = 90;

                    _L1 = (P2.X - P1.X) / 2;
                    _L2 = _L1;

                    P1.X += _L1;
                    P2.X -= _L1;

                    _W1 = 180M;
                    _W2 = 0M;
                    SetLengthToFix();

                    break;

                default:
                    Develop.DebugPrint(MovePoints);
                    break;
            }



            RecomputePointAndRelations();
            CalculateCorners();
        }




        public object Clone()
        {
            ClearInternalRelations(); // Damit nix geclont wird

            var i = (ComicCompPadItem)MemberwiseClone();


            i.P1 = new PointDF(i, P1);
            i.P2 = new PointDF(i, P2);


            i.AdditionalPoints.Clear();

            foreach (var thispoint in AdditionalPoints)
            {
                i.AdditionalPoints.Add(new PointDF(i, thispoint));
            }

            i.BerLO = new PointDF(i, BerLO);
            i.BerRO = new PointDF(i, BerRO);
            i.BerRU = new PointDF(i, BerRU);
            i.BerLU = new PointDF(i, BerLU);
            i._EinpassModus = false;
            return i;
        }


        protected override void KeepInternalLogic()
        {
            var Mitte = MiddlePoint();
            var WP12 = AngelOfMiddleLine();

            if (_Length > 0M && !_EinpassModus)
            {
                if (Math.Abs(_Length - GeometryDF.Länge(P1, P2)) > 1)
                {
                    P2.SetTo(P1, _Length, WP12);
                    CalculateCorners();
                }
            }


            foreach (var Thispoint in AdditionalPoints)
            {
                if (!_EinpassModus)
                {
                    if (!string.IsNullOrEmpty(Thispoint.Tag))
                    {
                        var t = Thispoint.Tag.SplitBy(";");
                        Thispoint.SetTo(Mitte, decimal.Parse(t[0]), decimal.Parse(t[1]) + WP12);
                    }
                }

            }
        }


        public override void GenerateInternalRelation(List<clsPointRelation> relations)
        {
            if (!_EinpassModus)
            {
                if (_Length > 0M)
                {
                    relations.Add(new clsPointRelation(enRelationType.AbstandZueinander, P1, P2));
                }


                foreach (var Thispoint in AdditionalPoints)
                {
                    if (GeometryDF.Länge(Thispoint, P1) < GeometryDF.Länge(Thispoint, P2))
                    {
                        relations.Add(new clsPointRelation(enRelationType.PositionZueinander, P1, Thispoint));
                    }
                    else
                    {
                        relations.Add(new clsPointRelation(enRelationType.PositionZueinander, P2, Thispoint));
                    }
                }
            }
        }




        public void AddPointCartesian(string Name, double LenghtToMiddle, double AngleToMiddle)
        {
            ClearInternalRelations();
            var p = new PointDF(this, Name, 0, 0, false, true);
            p.Tag = LenghtToMiddle + ";" + AngleToMiddle;
            AdditionalPoints.Add(p);

        }

        public void AddPointCartesian(double LenghtToMiddle, double AngleToMiddle)
        {
            AddPointCartesian("ZusatzCartesian" + (AdditionalPoints.Count + 1), LenghtToMiddle, AngleToMiddle);
        }

        public void AddPointAbsolute(string Name, decimal x, decimal y)
        {
            var ps_Mitte = MiddlePoint();
            var ps_P = new PointDF(x, y);

            AddPointCartesian(Name, Convert.ToDouble(GeometryDF.Länge(ps_Mitte, ps_P)), Convert.ToDouble(GeometryDF.Winkelx(ps_Mitte, ps_P) - 270));
        }

        public void AddPointAbsolute(decimal x, decimal y)
        {
            AddPointAbsolute("ZusatzAbsolute" + (AdditionalPoints.Count + 1), x, y);
        }

        public void AddPointAbsolute(PointDF Vorlage)
        {
            var ps_Mitte = MiddlePoint();
            AddPointCartesian(Vorlage.Name, Convert.ToDouble(GeometryDF.Länge(ps_Mitte, Vorlage)), Convert.ToDouble(GeometryDF.Winkelx(ps_Mitte, Vorlage) - 270));
        }


        public Bitmap GetTransformedBitmap()
        {
            if (BerLO == null) { CalculateCorners(); }

            var r = UsedArea();
            var i = new Bitmap((int)r.Width, (int)r.Height);
            var gr = Graphics.FromImage(i);
            gr.Clear(Color.White);


            var p = new PointF[4];


            p[0] = BerLO.ToPointF();
            p[1] = BerRO.ToPointF();
            p[2] = BerLU.ToPointF();
            p[3] = BerRU.ToPointF();


            var MinX = float.MaxValue;
            var MinY = float.MaxValue;

            for (var z = 0 ; z <= 3 ; z++)
            {
                MinX = Math.Min(p[z].X, MinX);
                MinY = Math.Min(p[z].Y, MinY);
            }

            for (var z = 0 ; z <= 3 ; z++)
            {
                p[z].X -= MinX;
                p[z].Y -= MinY;
            }

            PointF[] destPara2 = {p[0], p[1], p[2]}; //LO,RO,RU
            if (_Bitmap != null)
            {
                gr.DrawImage(_Bitmap, destPara2, new RectangleF(0, 0, _Bitmap.Width, _Bitmap.Height), GraphicsUnit.Pixel);
            }

            gr.Dispose();

            return i;

        }





        public override List<FlexiControl> GetStyleOptions(object sender, System.EventArgs e)
        {
            var l = new List<FlexiControl>();
            l.Add(new FlexiControl("Bildschirmbereich wählen", enImageCode.Bild));
            l.Add(new FlexiControl("Datei laden", enImageCode.Ordner));

            if (_Bitmap != null)
            {
                l.Add(new FlexiControl());
                l.Add(new FlexiControl("Einpassmodus", _EinpassModus));
                l.Add(new FlexiControl());
                l.Add(new FlexiControl("Einen Punkt hinzufügen", enImageCode.PlusZeichen));
                l.Add(new FlexiControl("Letzten Punkt entfernen", enImageCode.MinusZeichen));
                l.Add(new FlexiControl());
                var Relations = ((CreativePad)sender).AllRelations();


                if (!(_Length > 0M) && P1.CanMove(Relations) && P2.CanMove(Relations))
                {
                    l.Add(new FlexiControl("Objekt fest definiert,<br>Größe kann nicht fixiert werden"));
                }
                else
                {
                    l.Add(new FlexiControl("Objektgröße fixiert", Convert.ToBoolean(_Length > 0M)));
                }

                l.Add(new FlexiControl());
                l.Add(new FlexiControl("Objekt zurücksetzen", enImageCode.Warnung));
            }

            return l;
        }

        public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        {


            if (Tags.TagGet("Bildschirmbereich wählen").FromPlusMinus())
            {
                CloseMenu = false;
                if (_Bitmap != null)
                {
                    if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
                }

                _Bitmap = ScreenShot.GrabArea(null, 2000, 2000).Pic;
                ImageChanged();
                return;
            }

            if (Tags.TagGet("Datei laden").FromPlusMinus())
            {
                CloseMenu = false;
                if (_Bitmap != null)
                {
                    if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
                }

                var e = new System.Windows.Forms.OpenFileDialog();
                e.CheckFileExists = true;
                e.Multiselect = false;
                e.Title = "Bild wählen:";
                e.Filter = "PNG Portable Network Graphics|*.png|JPG Jpeg Interchange|*.jpg|BMP Windows Bitmap|*.bmp";

                e.ShowDialog();

                if (!FileExists(e.FileName)) { return; }

                _Bitmap = (Bitmap)modAllgemein.Image_FromFile(e.FileName);
                ImageChanged();
                return;
            }

            if (Tags.TagGet("Einen Punkt hinzufügen").FromPlusMinus())
            {
                CloseMenu = false;
                _EinpassModus = true;
                AddPointCartesian(0, 0);

                return;
            }

            if (Tags.TagGet("Objekt zurücksetzen").FromPlusMinus())
            {
                CloseMenu = false;
                ImageChanged();
                return;
            }

            if (Tags.TagGet("Letzten Punkt entfernen").FromPlusMinus())
            {
                CloseMenu = false;
                if (AdditionalPoints.Count > 0)
                {
                    //AdditionalPoints[AdditionalPoints.Count - 1].Dispose();
                    AdditionalPoints.RemoveAt(AdditionalPoints.Count - 1);
                    ClearInternalRelations();
                }
                return;
            }

            _EinpassModus = Tags.TagGet("Einpassmodus").FromPlusMinus();

            if (Tags.TagGet("Objektgröße fixiert").FromPlusMinus())
            {
                SetLengthToFix();
            }
            else
            {
                _Length = 0M;
                ClearInternalRelations();
            }


        }
    }
}