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
using System;
using System.Drawing;

namespace BlueControls
{
    public class RectangleDF
    {

        public decimal X;
        public decimal Y;
        public decimal Width;
        public decimal Height;


        public RectangleDF()
        {
            X = 0M;
            Y = 0M;
            Width = 0M;
            Height = 0M;
        }

        public RectangleDF(decimal x, decimal y, decimal width, decimal height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public RectangleDF(Rectangle r)
        {
            X = r.X;
            Y = r.Y;
            Width = r.Width;
            Height = r.Height;
        }

        public RectangleDF(PointDF p1, PointDF p2)
        {

            X = Math.Min(p1.X, p2.X);
            Y = Math.Min(p1.Y, p2.Y);
            Width = Math.Abs(p1.X - p2.X);
            Height = Math.Abs(p1.Y - p2.Y);

        }

        public decimal Left
        {
            get { return X; }
        }

        public decimal Top
        {
            get { return Y; }
        }
        public decimal Right
        {
            get { return X + Width; }
        }
        public decimal Bottom
        {
            get { return Y + Height; }
        }

        /// <summary>
        /// Positive Werte verkleinern das Rechteck, negative vergrößern es.
        /// </summary>
        /// <param name="XVal"></param>
        /// <param name="YVal"></param>
        public void Inflate(int XVal, int YVal)
        {
            X += XVal;
            Y += YVal;
            Width -= XVal * 2;
            Height -= YVal * 2;

        }

        public PointDF PointOf(enAlignment P)
        {
            switch (P)
            {
                case enAlignment.Bottom_Left:
                    return new PointDF(Left, Bottom);
                case enAlignment.Bottom_Right:
                    return new PointDF(Right, Bottom);
                case enAlignment.Top_Left:
                    return new PointDF(Left, Top);
                case enAlignment.Top_Right:
                    return new PointDF(Right, Top);
                case enAlignment.Bottom_HorizontalCenter:
                    return new PointDF(Left + Width / 2m, Bottom);
                case enAlignment.Top_HorizontalCenter:
                    return new PointDF(Left + Width / 2m, Top);
                case enAlignment.VerticalCenter_Left:
                    return new PointDF(Left, Top + Height / 2m);
                case enAlignment.VerticalCenter_Right:
                    return new PointDF(Right, Top + Height / 2m);
                case enAlignment.Horizontal_Vertical_Center:
                    return new PointDF(Left + Width / 2m, Top + Height / 2m);
                default:
                    Develop.DebugPrint(P);
                    return new PointDF();

            }
        }



        public PointDF NearestCornerOF(PointDF P)
        {

            var LO = PointOf(enAlignment.Top_Left);
            var rO = PointOf(enAlignment.Top_Right);
            var ru = PointOf(enAlignment.Bottom_Right);
            var lu = PointOf(enAlignment.Bottom_Left);


            var llo = GeometryDF.Länge(P, LO);
            var lro = GeometryDF.Länge(P, rO);
            var llu = GeometryDF.Länge(P, lu);
            var lru = GeometryDF.Länge(P, ru);

            var Erg = Math.Min(Math.Min(llo, lro), Math.Min(llu, lru));

            if (Erg == llo) { return LO; }
            if (Erg == lro) { return rO; }
            if (Erg == llu) { return lu; }
            if (Erg == lru) { return ru; }

            return null;

        }


        public bool Contains(PointDF P)
        {
            return Contains(P.X, P.Y);
        }


        public bool Contains(decimal PX, decimal PY)
        {
            if (PX < X) { return false; }
            if (PY < Y) { return false; }
            if (PX > X + Width) { return false; }
            if (PY > Y + Height) { return false; }

            return true;
        }


        public RectangleF ZoomAndMoveRect(decimal cZoom, decimal MoveX, decimal MoveY)
        {
            return new RectangleF((int)(X * cZoom - MoveX), (int)(Y * cZoom - MoveY), (int)(Width * cZoom), (int)(Height * cZoom));
        }

        /// <summary>
        /// Erweitert das Rechteck, dass ein Kreis mit den angegebenen Parametern ebenfalls umschlossen wird.
        /// </summary>
        /// <param name="P"></param>
        /// <param name="maxrad"></param>
        public void ExpandTo(PointDF middle, decimal radius)
        {
            ExpandTo(new PointDF(middle.X, middle.Y + radius));
            ExpandTo(new PointDF(middle.X, middle.Y - radius));
            ExpandTo(new PointDF(middle.X + radius, middle.Y));
            ExpandTo(new PointDF(middle.X - radius, middle.Y));
        }




        /// <summary>
        /// Erweitert das Rechteck, dass der Angegebene Punkt ebenfalls umschlossen wird.
        /// </summary>
        /// <param name="P"></param>
        public void ExpandTo(PointDF P)
        {
            if (P.X < X)
            {
                Width = Right - P.X;
                X = P.X;
            }

            if (P.Y < Y)
            {
                Height = Bottom - P.Y;
                Y = P.Y;
            }


            if (P.X > Right)
            {
                Width = P.X - X;
            }

            if (P.Y > Bottom)
            {
                Height = P.Y - Y;
            }



        }
    }
}
