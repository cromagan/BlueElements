﻿using System;
using BlueBasics;
using BlueBasics.Enums;

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

        public RectangleDF(decimal cX, decimal cY, decimal cWidth, decimal cHeight)
        {
            X = cX;
            Y = cY;
            Width = cWidth;
            Height = cHeight;
        }

        public decimal Left
        {
            get
            { return X; }


        }
        public decimal Top
        {
            get
            { return Y; }
        }
        public decimal Right
        {
            get
            { return X + Width; }
        }
        public decimal Bottom
        {
            get
            { return Y + Height; }
        }

        public void Inflate(int XVal, int YVal)
        {

            X += XVal;
            Y += YVal;
            Width += XVal * 2;
            Height += YVal * 2;



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

            if (Erg == llo)
            {
                return LO;
            }

            if (Erg == lro)
            {
                return rO;
            }

            if (Erg == llu)
            {
                return lu;
            }

            if (Erg == lru)
            {
                return ru;
            }

            return null;

        }


        internal bool Contains(PointDF P)
        {
            return Contains(P.X, P.Y);
        }


        internal bool Contains(decimal PX, decimal PY)
        {


            if (PX < X) { return false; }
            if (PY < Y) { return false; }
            if (PX > X + Width) { return false; }
            if (PY > Y + Height) { return false; }

            return true;
        }
    }
}
