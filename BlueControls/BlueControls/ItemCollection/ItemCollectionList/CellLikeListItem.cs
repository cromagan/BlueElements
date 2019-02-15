﻿using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Drawing;

namespace BlueControls.ItemCollection.ItemCollectionList
{
    public class CellLikeListItem : BasicListItem
        {
            // Implements IReadableText

            //http://www.kurztutorial.info/programme/punkt-mm/rechner.html
            // Dim Ausgleich As Double = mmToPixel(1 / 72 * 25.4, 300)
            //   Dim FixZoom As Single = 3.07F

            #region  Variablen-Deklarationen 


            /// <summary>
            /// Nach welche Spalte sich der Stil richten muss.
            /// Wichtig, dass es ein Spalten-item ist, da bei neuen Datenbanken zwar die Spalte vorhnden ist, aber wenn keine Zeile Vorhanden ist, logischgerweise auch keine Zelle da ist.
            /// </summary>
            private ColumnItem _StyleLikeThis;

            private readonly enShortenStyle _style;

            #endregion


            #region  Event-Deklarationen + Delegaten 

            #endregion


            #region  Construktor + Initialize 


            //public CellLikeListItem() : base() { }


            public CellLikeListItem(string vInternalAndReadableText, ColumnItem vColumnStyle, enShortenStyle Style, bool vEnabled = true)
            {
                _Internal = vInternalAndReadableText;
                _StyleLikeThis = vColumnStyle;
                _style = Style;

                _Enabled = vEnabled;
                if (string.IsNullOrEmpty(_Internal))
                {
                    Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben.");
                }
            }




            protected override void InitializeLevel2()
            {
                _StyleLikeThis = null;
            }


            #endregion


            #region  Properties 

            #endregion

            public override string Internal()
            {
                return _Internal;
            }



            public override void DesignOrStyleChanged()
            {

            }



            public override SizeF SizeUntouchedForListBox()
            {
                return GenericControl.Skin.FormatedText_NeededSize(_StyleLikeThis, _Internal, null, GenericControl.Skin.GetBlueFont(Parent.ItemDesign, enStates.Standard), _style);
            }


            protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enStates vState, bool DrawBorderAndBack)
            {

                if (DrawBorderAndBack)
                {
                    GenericControl.Skin.Draw_Back(GR, Parent.ItemDesign, vState, PositionModified, null, false);
                }

                GenericControl.Skin.Draw_FormatedText(_StyleLikeThis, _Internal, null, GR, PositionModified, enAlignment.Top_Left, null, false, GenericControl.Skin.SkinRow(Parent.ItemDesign, vState), _style);

                if (DrawBorderAndBack)
                {
                    GenericControl.Skin.Draw_Border(GR, Parent.ItemDesign, vState, PositionModified);
                }
            }


            protected override string GetCompareKey()
            {
                // Die hauptklasse frägt nach diesem Kompare-Key
                var txt = ColumnItem.Draw_FormatedText_TextOf(_Internal, null, _StyleLikeThis, enShortenStyle.Unreplaced);
                return DataFormat.CompareKey(txt, _StyleLikeThis.Format) + "|" + _Internal;
            }


            public override bool IsClickable()
            {
                return true;
            }

            public override void ComputePositionForListBox(enBlueListBoxAppearance IsIn, float X, float Y, float MultiX, int SliderWidth, int MaxWidth)
            {
                SetCoordinates(new Rectangle((int)X, (int)Y, (int)(MultiX - SliderWidth), (int)SizeUntouchedForListBox().Height));
            }

            public override SizeF QuickAndDirtySize(int PreferedHeigth)
            {
                return SizeUntouchedForListBox();
            }

            protected override BasicListItem CloneLVL2()
            {
                return new CellLikeListItem(_Internal, _StyleLikeThis, _style, _Enabled);
            }

            protected override bool FilterMatchLVL2(string FilterText)
            {
                var txt = ColumnItem.Draw_FormatedText_TextOf(_Internal, null, _StyleLikeThis, enShortenStyle.Both);
                return txt.ToUpper().Contains(FilterText.ToUpper());
            }

        }
    }
