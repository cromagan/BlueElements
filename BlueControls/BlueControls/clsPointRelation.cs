#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls {
    public sealed class clsPointRelation {

        private enRelationType _relationtype;
        public readonly ListExt<PointM> Points = new();

        public readonly ItemCollectionPad ParentCollection;
        public readonly object Parent;

        public readonly List<decimal> _Richtmaß = new();


        //internal bool Computed;
        //internal int Order;


        public event EventHandler Changed;

        public clsPointRelation(ItemCollectionPad parentCollection, object parent) {

            ParentCollection = parentCollection;
            _relationtype = enRelationType.None;

            Points.Clear();
            Points.Changed += Points_ListOrItemChanged;

            _Richtmaß.Clear();
            //Computed = false;
            //Order = -1;
            Parent = parent;
        }


        public clsPointRelation(ItemCollectionPad parentCollection, object parent, enRelationType enRelationType, PointM Point1, PointM Point2) : this(parentCollection, parent) {

            _relationtype = enRelationType;
            Points.ThrowEvents = false;

            Points.Add(Point1);
            Points.Add(Point2);

            Points.ThrowEvents = true;

            OverrideSavedRichtmaß(false, true);
        }


        public clsPointRelation(ItemCollectionPad parentCollection, object parent, string ToParse) : this(parentCollection, parent) {

            Points.ThrowEvents = false;

            if (ToParse.Contains("ParentType=BlueBasics.CreativePad,")) {
                ToParse = ToParse.Replace("ParentType=BlueBasics.CreativePad,", "ParentType=Main,");
            }
            if (ToParse.Contains("ParentType=BlueBasics.BlueCreativePad,")) {
                ToParse = ToParse.Replace("ParentType=BlueBasics.BlueCreativePad,", "ParentType=Main,");
            }
            if (ToParse.Contains("ParentType=BlueControls.ItemCollection.ItemCollectionPad,")) {
                ToParse = ToParse.Replace("ParentType=BlueControls.ItemCollection.ItemCollectionPad,", "ParentType=Main,");
            }
            _Richtmaß.Clear();


            foreach (var pair in ToParse.GetAllTags()) {
                switch (pair.Key) {
                    case "type":
                        _relationtype = (enRelationType)int.Parse(pair.Value);
                        break;

                    case "value":
                        var x = pair.Value.SplitBy(";");
                        foreach (var thisv in x) {
                            _Richtmaß.Add(modConverter.DecimalParse(thisv));
                        }
                        break;

                    case "point":
                        var added = false;
                        var m = pair.Value.IndexOf(", X=") + 4;
                        foreach (var ThisPoint in ParentCollection.AllPoints) {
                            if (ThisPoint != null) {

                                var nv = ThisPoint.ToString();

                                if (!string.IsNullOrEmpty(nv) && nv.Length >= m && nv.Substring(0, m) == pair.Value.Substring(0, m)) {
                                    added = true;
                                    Points.Add(ThisPoint);
                                    break;
                                }
                            }
                        }
                        if (!added) {
                            Develop.DebugPrint(enFehlerArt.Info, "Punkt nicht gefunden: " + pair.Value);
                        }

                        break;
                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }

            Points.ThrowEvents = true;


            if (_Richtmaß.Count != NeededRichtmaßData()) {
                _Richtmaß.Clear();
                OverrideSavedRichtmaß(true, true);
            }


            //_Richtmaß.Clear();
            //OverrideSavedRichtmaß(true, true);

            //if (!IsOk(true)) {
            //    _Richtmaß.Clear();
            //    OverrideSavedRichtmaß(true, true);
            //}
        }


        public enRelationType RelationType {
            get => _relationtype;

            set {
                if (_relationtype == value) { return; }
                _relationtype = value;
                OnChanged();
            }


        }



        //public int CompareTo(object obj)
        //{
        //    if (obj is clsPointRelation PRL)
        //    {
        //        // hierist es egal, ob es ein DoAlways ist oder nicht. Es sollen nur Bedingugen VOR Aktionen kommen
        //        return CompareKey().CompareTo(PRL.CompareKey());
        //    }
        //    else
        //    {
        //        Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Objecttyp!");
        //        return 0;
        //    }
        //}

        //public string CompareKey()
        //{
        //    return Order.ToString(Constants.Format_Integer5) + "|" + ((int)_relationtype).ToString(Constants.Format_Integer3);
        //}



        public override string ToString() {

            var t = "{Type=" + (int)_relationtype;

            // Richtmaß wird mitgespeichert. Falls sich die Vorlagen ändern, besteht die Chance, dass beziehungen erhalten bleiben
            foreach (var thisr in _Richtmaß) {
                t = t + ", Value=" + thisr.ToString().ToNonCritical();
            }

            foreach (var thispoint in Points) {
                t = t + ", Point=" + thispoint;
            }

            return t + "}";
        }


        public void Draw(Graphics GR, decimal cZoom, decimal shiftX, decimal shiftY, int OrderNr) {

            if (CreativePad.Debug_ShowRelationOrder) {
                var l1 = Points[0].ZoomAndMove(cZoom, shiftX, shiftY);
                var l2 = Points[1].ZoomAndMove(cZoom, shiftX, shiftY);
                GR.DrawLine(Pens.Orange, l1, l2);
                var mm1 = new PointF((l1.X + l2.X) / 2 - 5, (l1.Y + l2.Y) / 2 - 5);
                GR.DrawString(OrderNr.ToString(), new Font("Arial", 6), Brushes.Orange, mm1.X, mm1.Y);
            }


            if (Performs(true)) { return; }


            var c = Color.FromArgb(50, 255, 0, 0);
            var p = new Pen(c);


            foreach (var thispoint in Points) {
                thispoint.Draw(GR, cZoom, shiftX, shiftY, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard, string.Empty);


                if (thispoint.Parent is BasicPadItem tempVar) {
                    tempVar.Draw(GR, cZoom, shiftX, shiftY, enStates.Standard, Size.Empty, false);
                    tempVar.DrawOutline(GR, cZoom, shiftX, shiftY, c);
                }
            }



            switch (_relationtype) {
                case enRelationType.WaagerechtSenkrecht: {
                    var P1 = Points[0].ZoomAndMove(cZoom, shiftX, shiftY);
                    var P2 = Points[1].ZoomAndMove(cZoom, shiftX, shiftY);
                    var pb = new PointF((P1.X + P2.X) / 2 - 5, (P1.Y + P2.Y) / 2 - 5);
                    GR.DrawLine(p, P1, P2);


                    switch (_Richtmaß[0]) {
                        case 0:
                        case 180:
                            GR.DrawImage(QuickImage.Get(enImageCode.PfeilLinksRechts, 10).BMP, pb.X + 10, pb.Y);
                            break;
                        case 90:
                        case 270:
                            GR.DrawImage(QuickImage.Get(enImageCode.PfeilObenUnten, 10).BMP, pb.X, pb.Y + 10);
                            break;
                        default:
                            Develop.DebugPrint("Winkel unbekannt: " + _Richtmaß);
                            break;
                    }

                    break;
                }
                case enRelationType.PositionZueinander:
                case enRelationType.YPositionZueinander:
                case enRelationType.AbstandZueinander: {
                    var P1 = Points[0].ZoomAndMove(cZoom, shiftX, shiftY);
                    var P2 = Points[1].ZoomAndMove(cZoom, shiftX, shiftY);


                    decimal sX;
                    decimal sY;
                    decimal iX;
                    decimal iY;

                    var ist = GetRichtmaß();

                    if (_relationtype == enRelationType.PositionZueinander) {
                        sX = _Richtmaß[0];
                        sY = _Richtmaß[1];
                        iX = ist[0];
                        iY = ist[1];
                    } else {
                        sX = 0;
                        iX = 0;
                        sY = _Richtmaß[0];
                        iY = ist[0];
                    }


                    GR.DrawLine(p, P1, P2);


                    if (sX - iX > 0) {
                        //ok
                        GR.DrawImage(QuickImage.Get(enImageCode.Pfeil_Rechts, 16, "FFaaaa", "").BMP, P1.X - 8, P1.Y - 8);
                        GR.DrawImage(QuickImage.Get(enImageCode.Pfeil_Links, 16, "FFaaaa", "").BMP, P2.X - 8, P2.Y - 8);


                    } else if (sX - iX < 0) {
                        //ok
                        GR.DrawImage(QuickImage.Get(enImageCode.Pfeil_Links, 16, "FFaaaa", "").BMP, P1.X - 8, P1.Y - 8);
                        GR.DrawImage(QuickImage.Get(enImageCode.Pfeil_Rechts, 16, "FFaaaa", "").BMP, P2.X - 8, P2.Y - 8);

                    } else if (sY - iY < 0) {
                        //ok
                        GR.DrawImage(QuickImage.Get(enImageCode.Pfeil_Oben, 16, "FFaaaa", "").BMP, P1.X - 8, P1.Y - 8);
                        GR.DrawImage(QuickImage.Get(enImageCode.Pfeil_Unten, 16, "FFaaaa", "").BMP, P2.X - 8, P2.Y - 8);

                    } else if (sY - iY > 0) {
                        //ok
                        GR.DrawImage(QuickImage.Get(enImageCode.Pfeil_Unten, 16, "FFaaaa", "").BMP, P1.X - 8, P1.Y - 8);
                        GR.DrawImage(QuickImage.Get(enImageCode.Pfeil_Oben, 16, "FFaaaa", "").BMP, P2.X - 8, P2.Y - 8);
                    }

                    break;
                }

                default:
                    Develop.DebugPrint(_relationtype);
                    break;
            }
        }


        public bool Performs(bool StrongMode) {

            var Multi = 300;
            if (StrongMode) { Multi = 10; }

            if (!IsOk(false)) { return false; }


            switch (_relationtype) {
                case enRelationType.WaagerechtSenkrecht: {

                    if (Math.Abs(Points[0].X - Points[1].X) < 0.01m * Multi && Math.Abs(Points[0].Y - Points[1].Y) < 0.01m * Multi) { return true; }
                    var Dif = Math.Abs(_Richtmaß[0] - GetRichtmaß()[0]);

                    if (Dif <= 0.01m * Multi) { return true; }

                    if (Dif > 175M && Dif < 185M) { Dif -= 180M; }
                    if (Math.Abs(Dif) <= 0.01m * Multi) { return true; }

                    return false;

                }
                case enRelationType.PositionZueinander: {
                    var soll = GetRichtmaß();
                    if (Math.Abs(soll[0] - _Richtmaß[0]) > 0.01m * Multi) { return false; }
                    if (Math.Abs(soll[1] - _Richtmaß[1]) > 0.01m * Multi) { return false; }
                    return true;


                }
                default: {
                    return Math.Abs(_Richtmaß[0] - GetRichtmaß()[0]) <= 0.01m * Multi;

                }
            }
        }


        public void OverrideSavedRichtmaß(bool Parsing, bool IsCreating) {
            if (!IsOk(IsCreating)) { return; }

            if (!Parsing || _Richtmaß.Count == 0) { _Richtmaß.AddRange(GetRichtmaß()); }


            switch (_relationtype) {
                case enRelationType.WaagerechtSenkrecht:
                    _Richtmaß[0] = Math.Round(_Richtmaß[0]);

                    if (_Richtmaß[0] == 180 || _Richtmaß[0] == 360) { _Richtmaß[0] = 0; }
                    if (_Richtmaß[0] == 90 || _Richtmaß[0] == 270) { _Richtmaß[0] = 90; }

                    if (_Richtmaß[0] != 0 && _Richtmaß[0] != 90) { Develop.DebugPrint(enFehlerArt.Fehler, "Winkel nicht erlaubt: " + _Richtmaß[0]); }

                    break;

                case enRelationType.PositionZueinander:
                    break;

                case enRelationType.YPositionZueinander:
                    break;

                case enRelationType.Dummy:
                    break;

                case enRelationType.AbstandZueinander:
                    break;

                default:
                    Develop.DebugPrint(_relationtype);
                    break;
            }
        }


        private List<decimal> GetRichtmaß() {
            switch (_relationtype) {
                case enRelationType.None:
                    Develop.DebugPrint(enFehlerArt.Fehler, "Der Type None ist nicht erlaubt");
                    return new List<decimal>();

                case enRelationType.WaagerechtSenkrecht:
                    if (Math.Abs(Points[0].X) > 9999999 || Math.Abs(Points[0].Y) > 9999999) { return new List<decimal>() { -1 }; }

                    var tmp = Math.Round(Geometry.Winkel(Math.Round(Points[0].X, 2), Math.Round(Points[0].Y, 2), Math.Round(Points[1].X, 2), Math.Round(Points[1].Y, 2)), 2);
                    switch (tmp) {
                        case 0M:
                        case 180M:
                        case 360M:
                            return new List<decimal>() { 0 };
                        case 90M:
                        case 270M:
                            return new List<decimal>() { 90 };
                        default:
                            return new List<decimal>() { tmp };
                    }

                case enRelationType.PositionZueinander:

                    return new List<decimal>() { Math.Round(Points[0].X - Points[1].X, 1), Math.Round(Points[0].Y - Points[1].Y, 1) };



                case enRelationType.YPositionZueinander:
                    return new List<decimal>() { Math.Round(Points[0].Y - Points[1].Y, 1) };


                case enRelationType.Dummy:
                    return new List<decimal>();

                case enRelationType.AbstandZueinander:
                    tmp = Math.Round(GeometryDF.Länge(Points[0], Points[1]), 5);
                    return new List<decimal>() { tmp };

                default:
                    Develop.DebugPrint(_relationtype);
                    return new List<decimal>();
            }


        }


        public void Perform(List<PointM> pointOrder) {
            if (Points.Count != 2) { return; }

            PointM Fix, Flex;


            var p1 = pointOrder.IndexOf(Points[0]);
            var p2 = pointOrder.IndexOf(Points[1]);


            if (p1 > p2) {
                Fix = Points[1];
                Flex = Points[0];
            } else if (p1 < p2) {
                Fix = Points[0];
                Flex = Points[1];
            } else {
                return;
            }


            switch (_relationtype) {
                case enRelationType.WaagerechtSenkrecht:
                    if (_Richtmaß[0] == 90) {
                        Flex.SetTo(Fix.X, Flex.Y);
                    } else {
                        Flex.SetTo(Flex.X, Fix.Y);
                    }
                    break;

                case enRelationType.PositionZueinander:
                    if (Fix == Points[1]) {
                        Flex.SetTo(Fix.X + _Richtmaß[0], Fix.Y + _Richtmaß[1]);
                    } else {
                        Flex.SetTo(Fix.X - _Richtmaß[0], Fix.Y - _Richtmaß[1]);
                    }
                    break;

                case enRelationType.YPositionZueinander:
                    if (Fix == Points[1]) {
                        Flex.SetTo(Flex.X, Fix.Y + _Richtmaß[0]);
                    } else {
                        Flex.SetTo(Flex.X, Fix.Y - _Richtmaß[0]);
                    }
                    break;

                case enRelationType.AbstandZueinander:
                    var wi = GeometryDF.Winkel(Fix, Flex);
                    Flex.SetTo(Fix, _Richtmaß[0], wi);
                    break;

                default:
                    Develop.DebugPrint(_relationtype);
                    break;
            }
        }


        public enXY Connects() {
            switch (_relationtype) {
                case enRelationType.WaagerechtSenkrecht:
                    if (_Richtmaß[0] == 90) { return enXY.X; } // Senkrecht, dann müssen X gleich sein
                    if (_Richtmaß[0] == 0) { return enXY.Y; } // Waagerecht, dann müssen Y gleich sein
                    return enXY.none;

                case enRelationType.PositionZueinander:
                    return enXY.XY;

                case enRelationType.YPositionZueinander:
                    return enXY.Y;

                case enRelationType.Dummy:
                    return enXY.XY;

                case enRelationType.None:
                    return enXY.none;

                case enRelationType.AbstandZueinander:
                    return enXY.none;


                default:
                    Develop.DebugPrint(_relationtype);
                    return enXY.none;
            }


        }


        public bool SinngemäßIdenitisch(clsPointRelation R2) {
            return SinngemäßIdenitisch(this, R2);
        }




        public static bool SinngemäßIdenitisch(clsPointRelation R1, clsPointRelation R2) {

            if (R1 == null) { Develop.DebugPrint_Disposed(true); }
            if (R2 == null) { Develop.DebugPrint_Disposed(true); }


            if (R1._relationtype != R2._relationtype) { return false; }


            if (R1._Richtmaß.Count > 0 && R1._Richtmaß[0] != R2._Richtmaß[0]) { return false; }
            if (R1._Richtmaß.Count > 1 && R1._Richtmaß[1] != R2._Richtmaß[1]) { return false; }

            return UsesSamePoints(R1, R2);
        }

        public bool UsesSamePoints(clsPointRelation R2) {
            return UsesSamePoints(this, R2);
        }


        private static bool UsesSamePoints(clsPointRelation R1, clsPointRelation R2) {

            foreach (var thisPoint in R1.Points) {
                if (!R2.Points.Contains(thisPoint)) { return false; }
            }

            foreach (var thisPoint in R2.Points) {
                if (!R1.Points.Contains(thisPoint)) { return false; }
            }
            return true;
        }

        //public bool AllPointsHaveOrder()
        //{
        //    foreach (var Thispoint2 in Points)
        //    {
        //        if (Thispoint2.Order == int.MaxValue) { return false; }
        //    }

        //    return true;
        //}

        /// <summary>
        /// Gibt true zurück, wenn der zu testente Punkt nicht in allreadyUsed ist, aber einer der anderen der Punkte in dieser Beziehung bereits in allreadyUsed hat
        /// </summary>
        /// <param name="pointToCheck"></param>
        /// <returns></returns>
        public bool NeedCount(PointM pointToCheck, List<PointM> allreadyUsed) {
            if (!Points.Contains(pointToCheck)) { return false; }
            if (allreadyUsed.Contains(pointToCheck)) { return false; }


            foreach (var Thispoint2 in Points) {
                if (allreadyUsed.Contains(Thispoint2)) { return true; }
            }

            return false;
        }

        public bool IsInternal() {
            for (var z = 0; z <= Points.Count - 2; z++) {
                if (Points[z].Parent != Points[z + 1].Parent) { return false; }
            }
            return true;
        }

        public void OnChanged() {
            Changed?.Invoke(this, System.EventArgs.Empty);
        }





        private void Points_ListOrItemChanged(object sender, System.EventArgs e) {
            OnChanged();
        }


        private int NeededRichtmaßData() {

            if (_relationtype == enRelationType.None) { return 0; }
            if (_relationtype == enRelationType.PositionZueinander) { return 2; }

            return 1;
        }




        /// <summary>
        /// Prüft, ob die Beziehung einen Fehler enthält, der ein Löschen der Beziehung erfordert.
        /// Um zu prüfen, ob die Punkte zueinander stimmen, muss Performs benutzt werden.
        /// </summary>
        /// <returns></returns>
        public bool IsOk(bool IsCreating) {
            return string.IsNullOrEmpty(ErrorReason(IsCreating));
        }

        /// <summary>
        /// Prüft, ob die Beziehung einen Fehler enthält, der ein Löschen der Beziehung erfordert.
        /// Um zu prüfen, ob die Punkte zueinander stimmen, muss Performs benutzt werden.
        /// </summary>
        /// <returns></returns>
        public string ErrorReason(bool IsCreating) {


            if (Points == null) { return "Keine Punkte definiert."; }
            if (Points.Count < 2) { return "Zu wenige Punkte definiert."; }
            if (Points[0] == Points[1]) { return "Die Punkte verweisen auf sich selbst."; }
            if (_relationtype == enRelationType.None) { return "Der Type None ist nicht erlaubt."; }

            if (_relationtype == enRelationType.PositionZueinander && Points.Count < 2) { return "Zu wenige Punkte definiert."; }

            if (!IsCreating) {
                if (!ParentCollection.AllRelations.Contains(this)) { return "Beziehung nicht mehr vorhanden."; }
                if (_Richtmaß.Count < NeededRichtmaßData()) { return "Richtmaß hat zu wenig Angaben."; }

                if (Parent is BasicPadItem ba) {
                    if (!ParentCollection.Contains(ba)) { return "Objekt nicht mehr vorhanden"; }
                    if (!ba.Relations.Contains(this)) { return "Beziehung nicht mehr vorhanden."; }
                }

                foreach (var Thispoint in Points) {
                    if (Thispoint == null) { return "Einer der Punkte ist null."; }
                    if (!ParentCollection.AllPoints.Contains(Thispoint)) { return "Punkt nicht mehr vorhanden."; }
                }
            }
            return string.Empty;
        }


    }
}
