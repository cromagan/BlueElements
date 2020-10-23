﻿#region BlueElements - a collection of useful tools, database and controls
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
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls
{
    public sealed class clsPointRelation
    {

        private enRelationType _relationtype;
        public readonly ListExt<PointM> Points = new ListExt<PointM>();

        public readonly ItemCollectionPad ParentCollection;
        public readonly object Parent;

        private string _Richtmaß;


        //internal bool Computed;
        //internal int Order;


        public event EventHandler Changed;

        public clsPointRelation(ItemCollectionPad parentCollection, object parent)
        {

            ParentCollection = parentCollection;
            _relationtype = enRelationType.None;

            Points.Clear();
            Points.Changed += Points_ListOrItemChanged;

            _Richtmaß = string.Empty;
            //Computed = false;
            //Order = -1;
            Parent = parent;
        }


        public clsPointRelation(ItemCollectionPad parentCollection, object parent, enRelationType enRelationType, PointM Point1, PointM Point2) : this(parentCollection, parent)
        {

            _relationtype = enRelationType;
            Points.ThrowEvents = false;

            Points.Add(Point1);
            Points.Add(Point2);

            Points.ThrowEvents = true;

            OverrideSavedRichtmaß(false, true);
        }


        public clsPointRelation(ItemCollectionPad parentCollection, object parent, string ToParse) : this(parentCollection, parent)
        {

            Points.ThrowEvents = false;

            if (ToParse.Contains("ParentType=BlueBasics.CreativePad,"))
            {
                ToParse = ToParse.Replace("ParentType=BlueBasics.CreativePad,", "ParentType=Main,");
            }
            if (ToParse.Contains("ParentType=BlueBasics.BlueCreativePad,"))
            {
                ToParse = ToParse.Replace("ParentType=BlueBasics.BlueCreativePad,", "ParentType=Main,");
            }
            if (ToParse.Contains("ParentType=BlueControls.ItemCollection.ItemCollectionPad,"))
            {
                ToParse = ToParse.Replace("ParentType=BlueControls.ItemCollection.ItemCollectionPad,", "ParentType=Main,");
            }



            foreach (var pair in ToParse.GetAllTags())
            {
                switch (pair.Key)
                {
                    case "type":
                        _relationtype = (enRelationType)int.Parse(pair.Value);
                        break;
                    case "value":
                        _Richtmaß = pair.Value;
                        break;
                    case "point":
                        var added = false;
                        var m = pair.Value.IndexOf(", X=") + 4;
                        foreach (var ThisPoint in ParentCollection.AllPoints)
                        {
                            if (ThisPoint != null)
                            {

                                var nv = ThisPoint.ToString();

                                if (!string.IsNullOrEmpty(nv) && nv.Length >= m && nv.Substring(0, m) == pair.Value.Substring(0, m))
                                {
                                    added = true;
                                    Points.Add(ThisPoint);
                                    break;
                                }
                            }
                        }
                        if (!added)
                        {
                            Develop.DebugPrint(enFehlerArt.Warnung, "Punkt nicht gefunden: " + pair.Value);
                        }

                        break;
                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }

            Points.ThrowEvents = true;

            OverrideSavedRichtmaß(true, true);
        }




        public enRelationType RelationType
        {
            get
            {
                return _relationtype;
            }

            set
            {
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



        public override string ToString()
        {

            var t = "{Type=" + (int)_relationtype +
                     ", Value=" + _Richtmaß;


            foreach (var thispoint in Points)
            {
                t = t + ", Point=" + thispoint;
            }

            return t + "}";
        }


        public void Draw(Graphics GR, decimal cZoom, decimal MoveX, decimal MoveY, int OrderNr)
        {

            if (CreativePad.Debug_ShowRelationOrder)
            {
                var l1 = Points[0].ZoomAndMove(cZoom, MoveX, MoveY);
                var l2 = Points[1].ZoomAndMove(cZoom, MoveX, MoveY);
                GR.DrawLine(Pens.Orange, l1, l2);
                var mm1 = new PointF((l1.X + l2.X) / 2 - 5, (l1.Y + l2.Y) / 2 - 5);
                GR.DrawString(OrderNr.ToString(), new Font("Arial", 6), Brushes.Orange, mm1.X, mm1.Y);
            }


            if (Performs(true)) { return; }


            var c = Color.FromArgb(50, 255, 0, 0);
            var p = new Pen(c);


            foreach (var thispoint in Points)
            {
                thispoint.Draw(GR, cZoom, MoveX, MoveY, enDesign.Button_EckpunktSchieber_Phantom, enStates.Standard, false);


                if (thispoint.Parent is BasicPadItem tempVar)
                {
                    tempVar.Draw(GR, cZoom, MoveX, MoveY, enStates.Standard, Size.Empty, false);
                    tempVar.DrawOutline(GR, cZoom, MoveX, MoveY, c);
                }
            }



            switch (_relationtype)
            {
                case enRelationType.WaagerechtSenkrecht:
                    {
                        var P1 = Points[0].ZoomAndMove(cZoom, MoveX, MoveY);
                        var P2 = Points[1].ZoomAndMove(cZoom, MoveX, MoveY);
                        var pb = new PointF((P1.X + P2.X) / 2 - 5, (P1.Y + P2.Y) / 2 - 5);
                        GR.DrawLine(p, P1, P2);


                        switch (_Richtmaß)
                        {
                            case "0":
                            case "180":
                                GR.DrawImage(QuickImage.Get(enImageCode.PfeilLinksRechts, 10).BMP, pb.X + 10, pb.Y);
                                break;
                            case "90":
                            case "270":
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
                case enRelationType.AbstandZueinander:
                    {
                        var P1 = Points[0].ZoomAndMove(cZoom, MoveX, MoveY);
                        var P2 = Points[1].ZoomAndMove(cZoom, MoveX, MoveY);


                        double sX = 0;
                        double sY = 0;
                        double iX = 0;
                        double iY = 0;

                        if (_relationtype == enRelationType.PositionZueinander)
                        {
                            var soll = _Richtmaß.SplitBy(";");
                            var ist = GetRichtmaß().SplitBy(";");

                            sX = float.Parse(soll[0]);
                            sY = float.Parse(soll[1]);
                            iX = float.Parse(ist[0]);
                            iY = float.Parse(ist[1]);


                        }
                        else
                        {
                            sX = 0;
                            iX = 0;
                            sY = float.Parse(_Richtmaß);
                            iY = float.Parse(GetRichtmaß());


                        }


                        GR.DrawLine(p, P1, P2);


                        if (sX - iX > 0)
                        {
                            //ok
                            GR.DrawImage(QuickImage.Get(enImageCode.Pfeil_Rechts, 16, "FFaaaa", "").BMP, P1.X - 8, P1.Y - 8);
                            GR.DrawImage(QuickImage.Get(enImageCode.Pfeil_Links, 16, "FFaaaa", "").BMP, P2.X - 8, P2.Y - 8);


                        }
                        else if (sX - iX < 0)
                        {
                            //ok
                            GR.DrawImage(QuickImage.Get(enImageCode.Pfeil_Links, 16, "FFaaaa", "").BMP, P1.X - 8, P1.Y - 8);
                            GR.DrawImage(QuickImage.Get(enImageCode.Pfeil_Rechts, 16, "FFaaaa", "").BMP, P2.X - 8, P2.Y - 8);

                        }
                        else if (sY - iY < 0)
                        {
                            //ok
                            GR.DrawImage(QuickImage.Get(enImageCode.Pfeil_Oben, 16, "FFaaaa", "").BMP, P1.X - 8, P1.Y - 8);
                            GR.DrawImage(QuickImage.Get(enImageCode.Pfeil_Unten, 16, "FFaaaa", "").BMP, P2.X - 8, P2.Y - 8);

                        }
                        else if (sY - iY > 0)
                        {
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


        public bool Performs(bool StrongMode)
        {

            var Multi = 10;
            if (!StrongMode) { Multi = 300; }

            if (!IsOk(false)) { return false; }


            switch (_relationtype)
            {
                case enRelationType.WaagerechtSenkrecht:
                    {

                        if (Math.Abs(Points[0].X - Points[1].X) < 0.01m * Multi && Math.Abs(Points[0].Y - Points[1].Y) < 0.01m * Multi) { return true; }
                        var RMd = decimal.Parse(_Richtmaß);
                        var RMd2 = decimal.Parse(GetRichtmaß());
                        var Dif = Math.Abs(RMd - RMd2);

                        if (Dif <= 0.01m * Multi) { return true; }

                        if (Dif > 175M && Dif < 185M) { Dif -= 180M; }
                        if (Math.Abs(Dif) <= 0.01m * Multi) { return true; }

                        return false;

                    }
                case enRelationType.PositionZueinander:
                    {
                        var ist = _Richtmaß.SplitBy(";");
                        var soll = GetRichtmaß().SplitBy(";");
                        if (Math.Abs(decimal.Parse(ist[0]) - decimal.Parse(soll[0])) > 0.01m * Multi) { return false; }
                        if (Math.Abs(decimal.Parse(ist[1]) - decimal.Parse(soll[1])) > 0.01m * Multi) { return false; }
                        return true;


                    }
                default:
                    {

                        var RMd = decimal.Parse(_Richtmaß);
                        var RMd2 = decimal.Parse(GetRichtmaß());

                        return Math.Abs(RMd - RMd2) <= 0.01m * Multi;

                    }
            }
        }


        public void OverrideSavedRichtmaß(bool Parsing, bool IsCreating)
        {
            if (!IsOk(IsCreating)) { return; }

            if (!Parsing || string.IsNullOrEmpty(_Richtmaß)) { _Richtmaß = GetRichtmaß(); }


            switch (_relationtype)
            {
                case enRelationType.WaagerechtSenkrecht:
                    _Richtmaß = Math.Round(decimal.Parse(_Richtmaß), 0).ToString();

                    if (_Richtmaß == "180" || _Richtmaß == "360") { _Richtmaß = "0"; }
                    if (_Richtmaß == "90" || _Richtmaß == "270") { _Richtmaß = "90"; }

                    if (_Richtmaß != "0" && _Richtmaß != "90") { Develop.DebugPrint(enFehlerArt.Fehler, "Winkel nicht erlaubt: " + _Richtmaß); }

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


        private string GetRichtmaß()
        {
            switch (_relationtype)
            {
                case enRelationType.None:
                    Develop.DebugPrint(enFehlerArt.Fehler, "Der Type None ist nicht erlaubt");
                    break;

                case enRelationType.WaagerechtSenkrecht:
                    if (Math.Abs(Points[0].X) > 9999999 || Math.Abs(Points[0].Y) > 9999999) { return "-1"; }

                    var tmp = Math.Round(Geometry.Winkel(Math.Round(Points[0].X, 2), Math.Round(Points[0].Y, 2), Math.Round(Points[1].X, 2), Math.Round(Points[1].Y, 2)), 2);
                    switch (tmp)
                    {
                        case 0M:
                        case 180M:
                        case 360M:
                            return "0";
                        case 90M:
                        case 270M:
                            return "90";
                        default:
                            return tmp.ToString();
                    }

                case enRelationType.PositionZueinander:
                    return Math.Round(Points[0].X - Points[1].X, 1) + ";" + Math.Round(Points[0].Y - Points[1].Y, 1);

                case enRelationType.YPositionZueinander:
                    return Math.Round(Points[0].Y - Points[1].Y, 1).ToString();


                case enRelationType.Dummy:
                    return string.Empty;

                case enRelationType.AbstandZueinander:
                    tmp = Math.Round(GeometryDF.Länge(Points[0], Points[1]), 5);
                    return tmp.ToString();

                default:
                    Develop.DebugPrint(_relationtype);
                    break;
            }

            return string.Empty;
        }


        public void Perform(List<PointM> pointOrder)
        {
            if (Points.Count != 2) { return; }

            PointM Fix, Flex;


            var p1 = pointOrder.IndexOf(Points[0]);
            var p2 = pointOrder.IndexOf(Points[1]);


            if (p1 > p2)
            {
                Fix = Points[1];
                Flex = Points[0];
            }
            else if (p1 < p2)
            {
                Fix = Points[0];
                Flex = Points[1];
            }
            else
            {
                return;
            }

            //var OK = false;
            //foreach (var t in Points)
            //{
            //    if (t.Order > OrderNr) { OK = true; }
            //}
            //if (!OK) { return false; }

            //var StartPx = new List<PointM>();
            //foreach (var t in Points)
            //{
            //    StartPx.Add(new PointM(t));
            //}


            //PointM Fix, Flex;


            //if (Points[1].Order > Points[0].Order)
            //{
            //    Fix = Points[0];
            //    Flex = Points[1];
            //}
            //else
            //{
            //    Fix = Points[1];
            //    Flex = Points[0];
            //}



            switch (_relationtype)
            {
                case enRelationType.WaagerechtSenkrecht:
                    if (_Richtmaß == "90")
                    {
                        Flex.SetTo(Fix.X, Flex.Y);
                    }
                    else
                    {
                        Flex.SetTo(Flex.X, Fix.Y);
                    }
                    break;

                case enRelationType.PositionZueinander:
                    var w = _Richtmaß.SplitBy(";");

                    if (Fix == Points[1])
                    {
                        Flex.SetTo(Fix.X + decimal.Parse(w[0]), Fix.Y + decimal.Parse(w[1]));
                    }
                    else
                    {
                        Flex.SetTo(Fix.X - decimal.Parse(w[0]), Fix.Y - decimal.Parse(w[1]));
                    }
                    break;

                case enRelationType.YPositionZueinander:
                    if (Fix == Points[1])
                    {
                        Flex.SetTo(Flex.X, Fix.Y + decimal.Parse(_Richtmaß));
                    }
                    else
                    {
                        Flex.SetTo(Flex.X, Fix.Y - decimal.Parse(_Richtmaß));
                    }
                    break;

                case enRelationType.AbstandZueinander:
                    var wi = GeometryDF.Winkel(Fix, Flex);
                    Flex.SetTo(Fix, decimal.Parse(_Richtmaß), wi);
                    break;

                default:
                    Develop.DebugPrint(_relationtype);
                    break;
            }


            //var DidSomething = false;


            //for (var z = 0; z < Points.Count; z++)
            //{

            //    // ACHTUNG: Position-Fix und Fixpoints sind unterschiedlich!
            //    // Position-Fix: Der Punkt DARF nicht bewegt werden
            //    // FixPoints: Über Beziehungen kann er eigentlich nicht bewegt werden. Aber über des kontextmenü kann es ja sein, dass die Beziehungen ungültig geworden sind.
            //    if (Points[z].Moveable != enXY.none)
            //    {
            //        // Erst auf Änderungen prüfen, damit ein neuer Durchgang angestoßen wird.
            //        // Und dann die Fixen dinegns zurück setzen
            //        Points[z].X = StartPx[z].X;
            //        Points[z].Y = StartPx[z].Y;
            //    }

            //    if (!AllowBigChanges && GeometryDF.Länge(Points[z], StartPx[z]) > 100M)
            //    {
            //        Points[z].X = StartPx[z].X;
            //        Points[z].Y = StartPx[z].Y;
            //    }


            //    if (Math.Abs(StartPx[z].X - Points[z].X) > 0.01m || Math.Abs(StartPx[z].X - Points[z].X) > 0.01m)
            //    {
            //        DidSomething = true;
            //    }


            //}

            //return DidSomething;
        }


        public string Richtmaß()
        {
            return _Richtmaß;
        }

        public bool Connects(enXY toCheck)
        {
            switch (_relationtype)
            {
                case enRelationType.WaagerechtSenkrecht:
                    if (toCheck.HasFlag(enXY.X) && _Richtmaß != "90") { return false; }
                    if (toCheck.HasFlag(enXY.Y) && _Richtmaß != "0") { return false; }
                    break;

                case enRelationType.PositionZueinander:

                    //break; case Is = enRelationType.Mittig
                    //    Return False

                    //break; case Is = enRelationType.WinkelÜberMittelpunkt
                    //    Return False

                    break;
                case enRelationType.YPositionZueinander:
                    return toCheck.HasFlag(enXY.Y);

                case enRelationType.Dummy:
                    return true;

                case enRelationType.None:
                    return false;

                case enRelationType.AbstandZueinander:
                    return false;


                default:
                    Develop.DebugPrint(_relationtype);
                    return false;
            }

            return true;
        }


        public bool SinngemäßIdenitisch(clsPointRelation R2)
        {
            return SinngemäßIdenitisch(this, R2);
        }




        public static bool SinngemäßIdenitisch(clsPointRelation R1, clsPointRelation R2)
        {

            if (R1 == null) { Develop.DebugPrint_Disposed(true); }
            if (R2 == null) { Develop.DebugPrint_Disposed(true); }


            if (R1._relationtype != R2._relationtype) { return false; }


            if (R1.Richtmaß() != R2.Richtmaß()) { return false; }

            return UsesSamePoints(R1, R2);
        }

        public bool UsesSamePoints(clsPointRelation R2)
        {
            return UsesSamePoints(this, R2);
        }


        private static bool UsesSamePoints(clsPointRelation R1, clsPointRelation R2)
        {

            foreach (var thisPoint in R1.Points)
            {
                if (!R2.Points.Contains(thisPoint)) { return false; }
            }

            foreach (var thisPoint in R2.Points)
            {
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
        public bool NeedCount(PointM pointToCheck, List<PointM> allreadyUsed)
        {
            if (!Points.Contains(pointToCheck)) { return false; }
            if (allreadyUsed.Contains(pointToCheck)) { return false; }


            foreach (var Thispoint2 in Points)
            {
                if (allreadyUsed.Contains(Thispoint2)) { return true; }
            }

            return false;
        }

        public bool IsInternal()
        {
            for (var z = 0; z <= Points.Count - 2; z++)
            {
                if (Points[z].Parent != Points[z + 1].Parent) { return false; }
            }
            return true;
        }

        public void OnChanged()
        {
            Changed?.Invoke(this, System.EventArgs.Empty);
        }





        private void Points_ListOrItemChanged(object sender, System.EventArgs e)
        {
            OnChanged();
        }

        /// <summary>
        /// Prüft, ob die Beziehung einen Fehler enthält, der ein Löschen der Beziehung erfordert.
        /// Um zu prüfen, ob die Punkte zueinander stimmen, muss Performs benutzt werden.
        /// </summary>
        /// <returns></returns>
        public bool IsOk(bool IsCreating)
        {
            return string.IsNullOrEmpty(ErrorReason(IsCreating));
        }

        /// <summary>
        /// Prüft, ob die Beziehung einen Fehler enthält, der ein Löschen der Beziehung erfordert.
        /// Um zu prüfen, ob die Punkte zueinander stimmen, muss Performs benutzt werden.
        /// </summary>
        /// <returns></returns>
        public string ErrorReason(bool IsCreating)
        {


            if (Points == null) { return "Keine Punkte definiert."; }
            if (Points.Count < 2) { return "Zu wenige Punkte definiert."; }
            if (Points[0] == Points[1]) { return "Die Punkte verweisen auf sich selbst."; }
            if (_relationtype == enRelationType.None) { return "Der Type None ist nicht erlaubt"; }

            if (!IsCreating)
            {
                if (!ParentCollection.AllRelations.Contains(this)) { return "Beziehung nicht mehr vorhanden."; }


                if (Parent is BasicPadItem ba)
                {
                    if (!ParentCollection.Contains(ba)) { return "Objekt nicht mehr vorhanden"; }
                    if (!ba.Relations.Contains(this)) { return "Beziehung nicht mehr vorhanden."; }
                }

                foreach (var Thispoint in Points)
                {
                    if (Thispoint == null) { return "Einer der Punkte ist null."; }
                    if (!ParentCollection.AllPoints.Contains(Thispoint)) { return "Punkt nicht mehr vorhanden."; }
                }
            }
            return string.Empty;
        }


    }
}
