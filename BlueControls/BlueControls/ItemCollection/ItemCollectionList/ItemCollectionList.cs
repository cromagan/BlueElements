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
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using static BlueBasics.FileOperations;

namespace BlueControls.ItemCollection
{
    public class ItemCollectionList : ListExt<BasicListItem>, ICloneable
    {


        #region  Variablen-Deklarationen 

        private enCheckBehavior _CheckBehavior;
        private Size _CellposCorrect = Size.Empty;
        private bool _Validating;

        private enBlueListBoxAppearance _Appearance;
        private enDesign _ControlDesign;
        private enDesign _ItemDesign;

        private SizeF LastCheckedMaxSize = Size.Empty;


        #endregion


        #region  Properties 

        public enCheckBehavior CheckBehavior
        {
            get
            {
                return _CheckBehavior;
            }
            set
            {

                if (value == _CheckBehavior) { return; }
                _CheckBehavior = value;
                ValidateCheckStates(null);
            }
        }

        /// <summary>
        /// Itemdesign wird durch Appearance gesetzt
        /// </summary>
        /// <returns></returns>
        public enDesign ItemDesign //Implements IDesignAble.Design
        {
            get
            {
                if (_ItemDesign == enDesign.Undefiniert) { Develop.DebugPrint(enFehlerArt.Fehler, "ItemDesign undefiniert!"); }
                return _ItemDesign;
            }
        }

        /// <summary>
        /// ControlDesign wird durch Appearance gesetzt
        /// </summary>
        /// <returns></returns>
        public enDesign ControlDesign //Implements IDesignAble.Design
        {
            get
            {
                if (_ControlDesign == enDesign.Undefiniert) { Develop.DebugPrint(enFehlerArt.Fehler, "ControlDesign undefiniert!"); }
                return _ControlDesign;
            }
        }

        public enOrientation Orientation { get; private set; }

        public int BreakAfterItems { get; private set; }

        public enBlueListBoxAppearance Appearance
        {
            get
            {
                return _Appearance;
            }
            set
            {
                if (value == _Appearance && _ItemDesign != enDesign.Undefiniert) { return; }
                _Appearance = value;

                GetDesigns();

                //DesignOrStyleChanged();
                OnDoInvalidate();
            }
        }

        #endregion

        #region  Construktor 



        public ItemCollectionList() : this(enBlueListBoxAppearance.Listbox) { }

        public ItemCollectionList(enBlueListBoxAppearance design) : base()
        {
            _CellposCorrect = Size.Empty;
            _Appearance = enBlueListBoxAppearance.Listbox;
            _ItemDesign = enDesign.Undefiniert;
            _ControlDesign = enDesign.Undefiniert;
            _CheckBehavior = enCheckBehavior.SingleSelection;
            _Appearance = design;
            GetDesigns();
        }



        #endregion

        #region  Event-Deklarationen + Delegaten 
        public event EventHandler ItemCheckedChanged;
        public event EventHandler DoInvalidate;
        #endregion

        private void GetDesigns()
        {
            _ControlDesign = (enDesign)_Appearance;

            switch (_Appearance)
            {
                case enBlueListBoxAppearance.Autofilter:
                    _ItemDesign = enDesign.Item_Autofilter;
                    break;

                case enBlueListBoxAppearance.DropdownSelectbox:
                    _ItemDesign = enDesign.Item_DropdownMenu;
                    break;

                case enBlueListBoxAppearance.Gallery:
                    _ItemDesign = enDesign.Item_Listbox;
                    _ControlDesign = enDesign.ListBox;
                    break;

                case enBlueListBoxAppearance.FileSystem:
                    _ItemDesign = enDesign.Item_Listbox;
                    _ControlDesign = enDesign.ListBox;
                    break;

                case enBlueListBoxAppearance.Listbox:
                    _ItemDesign = enDesign.Item_Listbox;
                    _ControlDesign = enDesign.ListBox;
                    break;

                case enBlueListBoxAppearance.KontextMenu:
                    _ItemDesign = enDesign.Item_KontextMenu;
                    break;

                case enBlueListBoxAppearance.ComboBox_Textbox:
                    _ItemDesign = enDesign.ComboBox_Textbox;
                    break;

                default:
                    Develop.DebugPrint(enFehlerArt.Fehler, "Unbekanntes Design: " + _Appearance);
                    break;
            }
        }




        public void Check(ListExt<string> vItems, bool Checked)
        {
            Check(vItems.ToArray(), Checked);
        }

        public void Check(List<string> vItems, bool Checked)
        {
            Check(vItems.ToArray(), Checked);
        }

        public void Check(string[] vItems, bool Checked)
        {
            for (var z = 0; z <= vItems.GetUpperBound(0); z++)
            {
                if (this[vItems[z]] != null)
                {
                    this[vItems[z]].Checked = Checked;
                }
            }
        }

        public void UncheckAll()
        {
            foreach (var ThisItem in this)
            {
                if (ThisItem != null)
                {
                    ThisItem.Checked = false;
                }
            }
        }

        public void CheckAll()
        {
            foreach (var ThisItem in this)
            {
                if (ThisItem != null)
                {
                    ThisItem.Checked = true;
                }
            }
        }

        internal void SetNewCheckState(BasicListItem This, bool value, ref bool CheckVariable)
        {

            if (!This.IsClickable()) { value = false; }


            if (_CheckBehavior == enCheckBehavior.NoSelection)
            {
                value = false;
            }
            else if (CheckVariable && value == false && _CheckBehavior == enCheckBehavior.AlwaysSingleSelection)
            {
                if (Checked().Count == 1) { value = true; }
            }

            if (value == CheckVariable) { return; }

            CheckVariable = value;


            if (_Validating) { return; }


            ValidateCheckStates(This);

            OnItemCheckedChanged();
            OnDoInvalidate();
        }



        private void OnItemCheckedChanged()
        {
            ItemCheckedChanged?.Invoke(this, System.EventArgs.Empty);
        }

        public void OnDoInvalidate()
        {
            DoInvalidate?.Invoke(this, System.EventArgs.Empty);
        }

        public override void OnChanged()
        {
            _CellposCorrect = Size.Empty;
            base.OnChanged();
            OnDoInvalidate();
        }


        protected override void OnItemAdded(BasicListItem item)
        {
            if (string.IsNullOrEmpty(item.Internal))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Der Auflistung soll ein Item hinzugefügt werden, welches keinen Namen hat " + item.Internal);
            }

            item.SetParent(this);
            base.OnItemAdded(item);
            OnDoInvalidate();
        }

        public List<BasicListItem> Checked()
        {
            var p = new List<BasicListItem>();

            foreach (var ThisItem in this)
            {
                if (ThisItem != null && ThisItem.Checked) { p.Add(ThisItem); }
            }

            return p;
        }

        /// <summary>
        ///  BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed
        /// </summary>
        /// <returns></returns>
        internal Tuple<int, int, int, enOrientation> ItemData() // BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed
        {
            var w = 16;
            var h = 0;
            var hall = 0;

            var sameh = -1;
            var or = enOrientation.Senkrecht;

            foreach (var ThisItem in this)
            {
                if (ThisItem != null)
                {
                    var s = ThisItem.SizeUntouchedForListBox();

                    w = Math.Max(w, s.Width);
                    h = Math.Max(h, s.Height);
                    hall += s.Height;

                    if (sameh < 0)
                    {
                        sameh = ThisItem.SizeUntouchedForListBox().Height;
                    }
                    else
                    {
                        if (sameh != ThisItem.SizeUntouchedForListBox().Height) { or = enOrientation.Waagerecht; }
                        sameh = ThisItem.SizeUntouchedForListBox().Height;
                    }


                    if (!(ThisItem is TextListItem) && !(ThisItem is CellLikeListItem)) { or = enOrientation.Waagerecht; }

                }
            }


            return new Tuple<int, int, int, enOrientation>(w, h, hall, or);
        }




        public Size CalculateColumnAndSize()
        {
            var data = ItemData(); /// BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed

            if (data.Item4 == enOrientation.Waagerecht) { return ComputeAllItemPositions(new Size(300, 300), null, null, data); }



            BreakAfterItems = CalculateColumnCount(data.Item1, data.Item3, data.Item4);

            return ComputeAllItemPositions(new Size(1, 30), null, null, data);




        }

        internal Size ComputeAllItemPositions(Size ControlDrawingArea, System.Windows.Forms.Control InControl, Slider SliderY, Tuple<int, int, int, enOrientation> data)
        {


            if (Math.Abs(LastCheckedMaxSize.Width - ControlDrawingArea.Width) > 0.1 || Math.Abs(LastCheckedMaxSize.Height - ControlDrawingArea.Height) > 0.1)
            {
                LastCheckedMaxSize = ControlDrawingArea;
                _CellposCorrect = Size.Empty;
            }
            if (!_CellposCorrect.IsEmpty) { return _CellposCorrect; }
            if (Count == 0)
            {
                _CellposCorrect = Size.Empty;
                return Size.Empty;
            }



            if (_ItemDesign == enDesign.Undefiniert) { GetDesigns(); }

            // var data = ItemData(); // BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed


            var ori = data.Item4;

            if (BreakAfterItems < 1) { ori = enOrientation.Waagerecht; }


            var SliderWidth = 0;
            if (SliderY != null)
            {
                if (BreakAfterItems < 1 && data.Item3 > ControlDrawingArea.Height)
                {
                    SliderWidth = SliderY.Width;
                }
            }


            var colWidth = 0;
            var colCount = 1;

            switch (_Appearance)
            {
                case enBlueListBoxAppearance.Gallery:
                    colWidth = 200;
                    break;

                case enBlueListBoxAppearance.FileSystem:
                    colWidth = 110;
                    break;

                default:
                    // u.a. Autofilter
                    if (BreakAfterItems < 1)
                    {
                        colWidth = ControlDrawingArea.Width - SliderWidth;
                    }
                    else
                    {
                        colCount = Count / BreakAfterItems;
                        var r = Count % colCount;
                        if (r != 0) { colCount++; }

                        if (ControlDrawingArea.Width < 5)
                        {
                            colWidth = data.Item1;
                        }
                        else
                        {
                            colWidth = (ControlDrawingArea.Width - SliderWidth) / colCount;
                        }


                    }
                    break;
            }



            var MaxX = int.MinValue;
            var Maxy = int.MinValue;

            var itenc = -1;


            BasicListItem previtem = null;
            foreach (var ThisItem in this)
            {
                // PaintmodX kann immer abgezogen werden, da es eh nur bei einspaltigen Listboxen verändert wird!
                if (ThisItem != null)
                {
                    var cx = 0;
                    var cy = 0;

                    var wi = colWidth;
                    var he = 0;
                    itenc++;


                    if (ori == enOrientation.Waagerecht)
                    {
                        if (ThisItem.IsCaption) { wi = ControlDrawingArea.Width - SliderWidth; }
                        he = ThisItem.HeightForListBox(_Appearance, wi);
                    }
                    else
                    {
                        he = ThisItem.HeightForListBox(_Appearance, wi);
                    }


                    if (previtem != null)
                    {
                        if (ori == enOrientation.Waagerecht)
                        {
                            if (previtem.Pos.Right + colWidth > ControlDrawingArea.Width || ThisItem.IsCaption)
                            {
                                cx = 0;
                                cy = previtem.Pos.Bottom;
                            }
                            else
                            {
                                cx = previtem.Pos.Right;
                                cy = previtem.Pos.Top;
                            }
                        }
                        else
                        {
                            if (itenc % BreakAfterItems == 0)
                            {
                                cx = previtem.Pos.Right;
                                cy = 0;
                            }
                            else
                            {
                                cx = previtem.Pos.Left;
                                cy = previtem.Pos.Bottom;
                            }
                        }
                    }


                    ThisItem.SetCoordinates(new Rectangle(cx, cy, wi, he));

                    MaxX = Math.Max(ThisItem.Pos.Right, MaxX);
                    Maxy = Math.Max(ThisItem.Pos.Bottom, Maxy);
                    previtem = ThisItem;
                }
            }


            if (SliderY != null)
            {

                var SetTo0 = false;

                if (SliderWidth > 0)
                {
                    if (Maxy - ControlDrawingArea.Height <= 0)
                    {
                        SliderY.Enabled = false;
                        SetTo0 = true;
                    }
                    else
                    {
                        SliderY.Enabled = true;
                        SliderY.Minimum = 0;
                        SliderY.SmallChange = 16;
                        SliderY.LargeChange = ControlDrawingArea.Height;
                        SliderY.Maximum = Maxy - ControlDrawingArea.Height;

                        SetTo0 = false;
                    }

                    SliderY.Height = ControlDrawingArea.Height;
                    SliderY.Visible = true;
                }
                else
                {
                    SetTo0 = true;
                    SliderY.Visible = false;
                }

                if (SetTo0)
                {
                    SliderY.Minimum = 0;
                    SliderY.Maximum = 0;
                    SliderY.Value = 0;
                }


            }


            _CellposCorrect = new Size(MaxX, Maxy);
            return _CellposCorrect;
        }

        private int CalculateColumnCount(int BiggestItemWidth, int AllItemsHeight, enOrientation orientation)
        {

            if (orientation != enOrientation.Senkrecht)
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Nur 'senkrecht' erlaubt mehrere Spalten");
            }

            if (Count < 12) { return -1; }  // <10 ergibt dividieb by zere, weil es da 0 einträge währen bei 10 Spalten


            var dithemh = AllItemsHeight / Count;


            //    Size f;
            //    var WouldBeGood = -1;
            //    var TestSP = 0;
            for (var TestSP = 10; TestSP >= 1; TestSP--)
            {

                var colc = Count / TestSP;
                var rest = Count % colc;

                var ok = true;

                if (rest > 0 && rest < colc / 2) { ok = false; }

                if (colc < 5) { ok = false; }
                if (colc > 20) { ok = false; }
                if (colc * dithemh > 600) { ok = false; }
                if (colc * dithemh < 150) { ok = false; }
                if (TestSP * BiggestItemWidth > 600) { ok = false; }

                if ((colc * (float)dithemh) / (TestSP * (float)BiggestItemWidth) < 0.5) { ok = false; }


                if (ok)
                {
                    return colc;
                }


            }

            return -1;
        }

        /// <summary>
        /// Füllt die Ersetzungen mittels eines Übergebenen Enums aus.
        /// </summary>
        /// <param name="t">Beispiel: GetType(enDesign)</param>
        /// <param name="ZumDropdownHinzuAb">Erster Wert der Enumeration, der Hinzugefügt werden soll. Inklusive deses Wertes</param>
        /// <param name="ZumDropdownHinzuBis">Letzter Wert der Enumeration, der nicht mehr hinzugefügt wird, also exklusives diese Wertes</param>
        public void GetValuesFromEnum(System.Type t, int ZumDropdownHinzuAb, int ZumDropdownHinzuBis)
        {

            var items = System.Enum.GetValues(t);

            Clear();

            foreach (var thisItem in items)
            {

                var te = System.Enum.GetName(t, thisItem);
                var th = (int)thisItem;

                if (!string.IsNullOrEmpty(te))
                {
                    //NewReplacer.Add(th.ToString() + "|" + te);
                    if (th >= ZumDropdownHinzuAb && th < ZumDropdownHinzuBis)
                    {
                        Add(th.ToString(), te);
                    }
                }
            }

        }

        #region  Add / AddRange / AddBasicLevel 



        #region TextListItem



        public TextListItem Add(string internalAndReadableText)
        {
            return Add(internalAndReadableText, internalAndReadableText, null, false, true, enDataFormat.Text, string.Empty);
        }


        /// <summary>
        /// Fügt das übergebende Object den Tags hinzu.
        /// </summary>
        /// <param name="obj"></param>
        public TextListItem Add(IReadableText obj)
        {
            var i = Add(string.Empty, obj, string.Empty);
            i.Tags = obj;
            return i;
        }

        /// <summary>
        /// Fügt das übergebende Object den Tags hinzu.
        /// </summary>
        /// <param name="internalname"></param>
        /// <param name="obj"></param>
        public TextListItem Add(string internalname, IReadableText obj)
        {
            var i = Add(internalname, obj, string.Empty);
            i.Tags = obj;
            return i;
        }

        /// <summary>
        /// Fügt das übergebende Object den Tags hinzu.
        /// </summary>
        /// <param name="internalname"></param>
        /// <param name="obj"></param>
        public TextListItem Add(string internalname, IReadableText obj, string userDefCompareKey)
        {
            var i = Add(internalname, obj.ReadableText(), obj.SymbolForReadableText(), true, userDefCompareKey);
            i.Tags = obj;
            return i;
        }


        public TextListItem Add(string internalname, string readableText, bool isCaption, string userDefCompareKey)

        {
            return Add(internalname, readableText, null, isCaption, true, enDataFormat.Text, userDefCompareKey);
        }



        public TextListItem Add(string internalAndReadableText, bool isCaption)
        {
            return Add(internalAndReadableText, internalAndReadableText, null, isCaption, true, enDataFormat.Text, string.Empty);
        }









        public TextListItem Add(string internalAndReadableText, enDataFormat format)
        {
            return Add(internalAndReadableText, internalAndReadableText, null, false, true, format, string.Empty);
        }


        public TextListItem Add(string internalAndReadableText, enImageCode symbol)
        {
            return Add(internalAndReadableText, internalAndReadableText, symbol, false, true, enDataFormat.Text, string.Empty);
        }



        public TextListItem Add(string internalname, string readableText, bool enabled)
        {
            return Add(internalname, readableText, null, false, enabled, enDataFormat.Text, string.Empty);
        }

        public TextListItem Add(string internalname, string readableText, enImageCode symbol, bool enabled)
        {
            return Add(internalname, readableText, symbol, false, enabled, enDataFormat.Text, string.Empty);
        }

        public TextListItem Add(string internalname, string readableText, enImageCode symbol, bool enabled, string userDefCompareKey)
        {
            return Add(internalname, readableText, symbol, false, enabled, enDataFormat.Text, userDefCompareKey);
        }

        public TextListItem Add(string internalname, string readableText, QuickImage symbol, bool enabled)
        {
            return Add(internalname, readableText, symbol, false, enabled, enDataFormat.Text, string.Empty);
        }

        public TextListItem Add(string internalname, string readableText, QuickImage symbol, bool enabled, string userDefCompareKey)
        {
            return Add(internalname, readableText, symbol, false, enabled, enDataFormat.Text, userDefCompareKey);
        }





        public TextListItem Add(string internalname, string readableText)
        {
            return Add(internalname, readableText, null, false, true, enDataFormat.Text, string.Empty);

        }

        public TextListItem Add(string internalname, string readableText, enImageCode symbol)
        {
            return Add(internalname, readableText, symbol, false, true, enDataFormat.Text, string.Empty);
        }


        public TextListItem Add(string internalname, string readableText, QuickImage symbol)
        {
            return Add(internalname, readableText, symbol, false, true, enDataFormat.Text, string.Empty);
        }




        public TextListItem Add(string internalname, string readableText, enImageCode symbol, bool isCaption, bool enabled, enDataFormat format, string userDefCompareKey)
        {
            return Add(internalname, readableText, QuickImage.Get(symbol, 16), isCaption, enabled, format, userDefCompareKey);
        }

        public TextListItem Add(string internalname, string readableText, QuickImage symbol, bool isCaption, bool enabled, enDataFormat format, string userDefCompareKey)
        {

            var x = new TextListItem(internalname, readableText, symbol, isCaption, enabled, format, userDefCompareKey);
            Add(x);
            return x;
        }




        #endregion


        #region  BitmapListItem

        //public BitmapListItem Add()
        //{
        //    return Add(string.Empty, string.Empty, string.Empty, null, string.Empty);
        //}


        //public BitmapListItem Add(string internalname, string caption)
        //{
        //    return Add(internalname, caption, string.Empty, null, string.Empty);
        //}



        public BitmapListItem Add(Bitmap BMP, string caption)
        {
            return Add(string.Empty, caption, string.Empty, BMP, string.Empty);
        }



        public BitmapListItem Add(string internalname, Bitmap BMP)
        {
            return Add(internalname, string.Empty, string.Empty, BMP, string.Empty);
        }





        public BitmapListItem Add(string internalname, string caption, string Filename, string EncryptionKey)
        {
            return Add(internalname, caption, Filename, null, EncryptionKey);
        }


        //public BitmapListItem Add(string internalname, string caption, QuickImage QI)
        //{
        //    return Add(internalname, caption, string.Empty, QI.BMP, string.Empty);
        //}


        private BitmapListItem Add(string internalname, string caption, string Filename, Bitmap bmp, string EncryptionKey)
        {

            var i = new BitmapListItem(internalname, caption, Filename, bmp, EncryptionKey);
            return i;
        }


        #endregion


        public BasicListItem Add(clsNamedBinary ThisBin)
        {
            if (ThisBin.Picture != null)
            {
                return Add(ThisBin.Picture, ThisBin.Name);
            }
            else
            {
                return Add(ThisBin.Binary, ThisBin.Name);
            }
        }

        public TextListItem Add(ColumnItem column, bool doCaptionSort)
        {
            if (doCaptionSort)
            {
                return Add(column.Name, column, column.Ueberschriften + Constants.SecondSortChar + column.Name);
            }
            else
            {
                return Add(column.Name, column, string.Empty);
            }

        }

        public TextListItem Add(enContextMenuComands Comand, bool vEnabled = true)
        {

            //if (ThisBin.Picture != null)
            //{
            //    Add(new BitmapListItem(ThisBin.Picture, ThisBin.Name));
            //}
            //else
            //{
            //    Add(new TextListItem(ThisBin.Binary, ThisBin.Name));
            //}

            //var _Enabled = vEnabled;
            var _Internal = Comand.ToString();
            QuickImage _Symbol = null;
            var _ReadableText = string.Empty;

            switch (Comand)
            {
                case enContextMenuComands.Abbruch: _ReadableText = "Abbrechen"; _Symbol = QuickImage.Get("TasteESC|16"); break;
                case enContextMenuComands.Bearbeiten: _ReadableText = "Bearbeiten"; _Symbol = QuickImage.Get(enImageCode.Stift); break;
                case enContextMenuComands.Kopieren: _ReadableText = "Kopieren"; _Symbol = QuickImage.Get(enImageCode.Kopieren); break;
                case enContextMenuComands.InhaltLöschen: _ReadableText = "Inhalt löschen"; _Symbol = QuickImage.Get(enImageCode.Radiergummi); break;
                case enContextMenuComands.ZeileLöschen: _ReadableText = "Zeile löschen"; _Symbol = QuickImage.Get("Zeile|16|||||||||Kreuz"); break;
                case enContextMenuComands.DateiÖffnen: _ReadableText = "Öffnen / Ausführen"; _Symbol = QuickImage.Get(enImageCode.Blitz); break;
                case enContextMenuComands.SpaltenSortierungAZ: _ReadableText = "Nach dieser Spalte aufsteigend sortieren"; _Symbol = QuickImage.Get("AZ|16|8"); break;
                case enContextMenuComands.SpaltenSortierungZA: _ReadableText = "Nach dieser Spalte absteigend sortieren"; _Symbol = QuickImage.Get("ZA|16|8"); break;
                case enContextMenuComands.Information: _ReadableText = "Informationen anzeigen"; _Symbol = QuickImage.Get(enImageCode.Frage); break;
                case enContextMenuComands.ZellenInhaltKopieren: _ReadableText = "Zelleninhalt kopieren"; _Symbol = QuickImage.Get(enImageCode.Kopieren); break;
                case enContextMenuComands.ZellenInhaltPaste: _ReadableText = "In Zelle einfügen"; _Symbol = QuickImage.Get(enImageCode.Clipboard); break;
                case enContextMenuComands.SpaltenEigenschaftenBearbeiten: _ReadableText = "Spalteneigenschaften bearbeiten"; _Symbol = QuickImage.Get("Spalte|16|||||||||Stift"); break;
                case enContextMenuComands.Speichern: _ReadableText = "Speichern"; _Symbol = QuickImage.Get(enImageCode.Diskette); break;
                case enContextMenuComands.Löschen: _ReadableText = "Löschen"; _Symbol = QuickImage.Get(enImageCode.Kreuz); break;
                case enContextMenuComands.Umbenennen: _ReadableText = "Umbenennen"; _Symbol = QuickImage.Get(enImageCode.Stift); break;
                case enContextMenuComands.SuchenUndErsetzen: _ReadableText = "Suchen und ersetzen"; _Symbol = QuickImage.Get(enImageCode.Fernglas); break;
                case enContextMenuComands.Einfügen: _ReadableText = "Einfügen"; _Symbol = QuickImage.Get(enImageCode.Clipboard); break;
                case enContextMenuComands.Ausschneiden: _ReadableText = "Ausschneiden"; _Symbol = QuickImage.Get(enImageCode.Schere); break;
                case enContextMenuComands.VorherigenInhaltWiederherstellen: _ReadableText = "Vorherigen Inhalt wieder herstellen"; _Symbol = QuickImage.Get(enImageCode.Undo); break;
                case enContextMenuComands.WeitereBefehle: _ReadableText = "Weitere Befehle"; _Symbol = QuickImage.Get(enImageCode.Hierarchie); break;
                default:
                    Develop.DebugPrint(Comand);
                    _ReadableText = _Internal;
                    _Symbol = QuickImage.Get(enImageCode.Fragezeichen);
                    break;
            }

            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben:" + Comand); }

            return Add(_Internal, _ReadableText, _Symbol, vEnabled);


        }





        public void AddRange(ColumnCollection Columns, bool OnlyExportableTextformatForLayout, bool NoCritical, bool DoCaptionSort)
        {

            foreach (var ThisColumnItem in Columns)
            {
                if (ThisColumnItem != null)
                {
                    var addx = true;
                    if (addx && OnlyExportableTextformatForLayout && !ThisColumnItem.ExportableTextformatForLayout()) { addx = false; }
                    if (addx && NoCritical && !ThisColumnItem.Format.CanBeCheckedByRules()) { addx = false; }
                    if (addx && this[ThisColumnItem.Name] != null) { addx = false; }

                    if (addx)
                    {
                        Add(ThisColumnItem, DoCaptionSort);

                        if (DoCaptionSort)
                        {
                            var capt = ThisColumnItem.Ueberschriften;


                            if (this[capt] == null)
                            {
                                Add(new TextListItem(capt, capt, null, true, true, enDataFormat.Text, capt + Constants.FirstSortChar));
                            }


                        }

                    }

                }

            }

        }


        public void AddRange(Type type)
        {
            foreach (int z1 in Enum.GetValues(type))
            {
                if (this[z1.ToString()] == null) { Add(z1.ToString(), Enum.GetName(type, z1).Replace("_", " ")); }
            }

            Sort();

        }

        public void AddRange(string[] Values)
        {
            if (Values == null) { return; }

            foreach (var thisstring in Values)
            {
                if (this[thisstring] == null) { Add(thisstring); }
            }

        }




        public void AddRange(ListExt<string> Values)
        {

            if (Values == null) { return; }

            foreach (var thisstring in Values)
            {
                if (this[thisstring] == null) { Add(thisstring, thisstring); }
            }


        }


        public void AddRange(List<string> Values, ColumnItem ColumnStyle, enShortenStyle Style, bool compact)
        {

            if (Values == null) { return; }


            if (Values.Count > 10000)
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Values > 100000");
                return;
            }


            foreach (var thisstring in Values)
            {
                Add(thisstring, ColumnStyle, Style, compact); // If Item(thisstring) Is Nothing Then Add(New CellLikeItem(thisstring, ColumnStyle))
            }

        }

        public BasicListItem Add(string Value, ColumnItem ColumnStyle, enShortenStyle Style, bool compact)
        {


            if (this[Value] == null)
            {
                if (ColumnStyle.Format == enDataFormat.Link_To_Filesystem && Value.FileType() == enFileFormat.Image)
                {
                    return Add(Value, Value, ColumnStyle.BestFile(Value, false), ColumnStyle.Database.FileEncryptionKey);
                }
                else
                {
                    var i = new CellLikeListItem(Value, ColumnStyle, Style, true, compact);
                    Add(i);
                    return i;

                }
            }

            return null;
        }


        /// <summary>
        /// Kann mit GetNamedBinaries zurückgeholt werden
        /// </summary>
        /// <param name="list"></param>
        public void AddRange(ListExt<clsNamedBinary> list)
        {


            if (list == null) { return; }

            foreach (var ThisBin in list)
            {
                Add(ThisBin);
            }
        }


        public void AddRange(List<string> list)
        {
            if (list == null) { return; }

            foreach (var thisstring in list)
            {
                if (!string.IsNullOrEmpty(thisstring))
                {
                    if (this[thisstring] == null) { Add(thisstring, thisstring); }
                }
            }

        }



        //public void AddRange(List<BasicListItem> Vars)
        //{
        //    if (Vars == null) { return; }

        //    foreach (var thisItem in Vars)
        //    {
        //        if (IndexOf(thisItem) < 0)
        //        {
        //            switch (thisItem)
        //            {
        //                case TextListItem TI: Add(TI); break;
        //                case BitmapListItem BI: Add(BI); break;
        //                case LineListItem LI: Add(LI); break;
        //                // case ObjectListItem OI: Add(OI); break;
        //                case CellLikeListItem CI: Add(CI); break;
        //                default:
        //                    Develop.DebugPrint_NichtImplementiert();
        //                    break;
        //            }
        //        }
        //    }
        //}

        public void AddLayoutsOf(Database vLayoutDatabase, bool vDoDiscLayouts, string vAdditionalLayoutPath)
        {

            for (var z = 0; z < vLayoutDatabase.Layouts.Count; z++)
            {
                var p = new ItemCollectionPad(vLayoutDatabase.Layouts[z], string.Empty);
                Add(p.ID, p.Caption, enImageCode.Stern);
            }

            if (!vDoDiscLayouts) { return; }

            var du = 0;

            do
            {
                if (PathExists(vAdditionalLayoutPath))
                {
                    var e = Directory.GetFiles(vAdditionalLayoutPath);
                    foreach (var ThisFile in e)
                    {


                        if (ThisFile.FilePath() == vLayoutDatabase.DefaultLayoutPath()) { ThisFile.TrimStart(vLayoutDatabase.DefaultLayoutPath()); }

                        if (this[ThisFile] == null) { Add(ThisFile, ThisFile.FileNameWithSuffix(), QuickImage.Get(ThisFile.FileType(), 16)); }
                    }
                }

                if (vLayoutDatabase == null) { break; }

                du++;
                if (du >= 2) { break; }
                vAdditionalLayoutPath = vLayoutDatabase.DefaultLayoutPath();

            } while (true);

        }


        #endregion


        public ListExt<clsNamedBinary> GetNamedBinaries()
        {
            var l = new ListExt<clsNamedBinary>();
            foreach (var thisItem in this)
            {
                switch (thisItem)
                {
                    case BitmapListItem BI:
                        l.Add(new clsNamedBinary(BI.Caption, BI.Bitmap));
                        break;
                    case TextListItem TI:
                        l.Add(new clsNamedBinary(TI.Text, TI.Internal));
                        break;
                }
            }
            return l;
        }





        #region  Standard-Such-Properties 


        public BasicListItem this[int X, int Y]
        {
            get
            {

                foreach (var ThisItem in this)
                {
                    if (ThisItem != null && ThisItem.Contains(X, Y)) { return ThisItem; }
                }


                return null;
            }
        }


        #endregion




        public void Remove(string Internal)
        {
            Remove(this[Internal]);
        }



        public void RemoveRange(List<string> Internal)
        {
            foreach (var item in Internal)
            {
                Remove(item);
            }
        }



        public BasicListItem this[string Internal]
        {
            get
            {
                if (string.IsNullOrEmpty(Internal)) { return null; }

                foreach (var ThisItem in this)
                {
                    if (ThisItem != null && Internal.ToUpper() == ThisItem.Internal.ToUpper()) { return ThisItem; }
                }
                return null;
            }
        }


        private void ValidateCheckStates(BasicListItem ThisMustBeChecked)
        {

            _Validating = true;
            var SomethingDonex = false;

            var Done = false;
            BasicListItem F = null;

            switch (_CheckBehavior)
            {
                case enCheckBehavior.NoSelection:
                    UncheckAll();
                    break;

                case enCheckBehavior.MultiSelection:
                    break;

                case enCheckBehavior.SingleSelection:
                case enCheckBehavior.AlwaysSingleSelection:
                    foreach (var ThisItem in this)
                    {
                        if (ThisItem != null)
                        {

                            if (ThisMustBeChecked == null)
                            {
                                if (F == null) { F = ThisItem; }

                                if (ThisItem.Checked)
                                {
                                    if (!Done)
                                    {
                                        Done = true;
                                    }
                                    else
                                    {
                                        if (ThisItem.Checked)
                                        {
                                            ThisItem.Checked = false;
                                            SomethingDonex = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Done = true;
                                if (ThisItem != ThisMustBeChecked && ThisItem.Checked)
                                {
                                    SomethingDonex = true;
                                    ThisItem.Checked = false;
                                }

                            }
                        }
                    }

                    if (_CheckBehavior == enCheckBehavior.AlwaysSingleSelection && !Done && F != null && !F.Checked)
                    {
                        F.Checked = true;
                        SomethingDonex = true;
                    }
                    break;

                default:
                    Develop.DebugPrint(_CheckBehavior);
                    break;
            }

            _Validating = false;

            if (SomethingDonex) { OnDoInvalidate(); }
        }

        public object Clone()
        {

            var x = new ItemCollectionList(_Appearance)
            {
                CheckBehavior = _CheckBehavior
            };


            foreach (var ThisItem in this)
            {
                ThisItem.CloneToNewCollection(x);
            }
            return x;
        }




        public static void GetItemCollection(ItemCollectionList e, ColumnItem column, RowItem checkedItemsAtRow, enShortenStyle style, int maxItems, bool compact)
        {

            var Marked = new List<string>();
            var l = new List<string>();

            e.Clear();

            e.CheckBehavior = enCheckBehavior.MultiSelection; // Es kann ja mehr als nur eines angewählt sein, auch wenn nicht erlaubt!

            l.AddRange(column.DropDownItems);
            if (column.DropdownWerteAndererZellenAnzeigen)
            {
                if (column.DropdownKey >= 0 && checkedItemsAtRow != null)
                {
                    var cc = column.Database.Column.SearchByKey(column.DropdownKey);
                    var F = new FilterCollection(column.Database)
                    {
                        new FilterItem(cc, enFilterType.Istgleich_GroßKleinEgal, checkedItemsAtRow.CellGetString(cc))
                    };
                    l.AddRange(column.Contents(F));
                }
                else
                {
                    l.AddRange(column.Contents(null));
                }
            }


            switch (column.Format)
            {
                case enDataFormat.Bit:
                    l.Add(true.ToPlusMinus());
                    l.Add(false.ToPlusMinus());
                    break;

                case enDataFormat.Columns_für_LinkedCellDropdown:
                    var DB = column.LinkedDatabase();
                    if (DB != null && !string.IsNullOrEmpty(column.LinkedKeyKennung))
                    {
                        foreach (var ThisColumn in DB.Column)
                        {
                            if (ThisColumn.Name.ToLower().StartsWith(column.LinkedKeyKennung.ToLower()))
                            {
                                l.Add(ThisColumn.Key.ToString());
                            }
                        }
                    }
                    if (l.Count == 0)
                    {
                        Notification.Show("Keine Spalten gefunden, die<br>mit '" + column.LinkedKeyKennung + "' beginnen.", enImageCode.Information);
                    }
                    break;

                case enDataFormat.Values_für_LinkedCellDropdown:
                    var DB2 = column.LinkedDatabase();
                    l.AddRange(DB2.Column[0].Contents(null));
                    if (l.Count == 0)
                    {
                        Notification.Show("Keine Zeilen in der Quell-Datenbank vorhanden.", enImageCode.Information);
                    }
                    break;
            }

            if (column.Database.Row.Count() > 0)
            {
                if (checkedItemsAtRow != null)
                {
                    if (!checkedItemsAtRow.CellIsNullOrEmpty(column))
                    {
                        if (column.MultiLine)
                        {
                            Marked = checkedItemsAtRow.CellGetList(column);
                        }
                        else
                        {
                            Marked.Add(checkedItemsAtRow.CellGetString(column));
                        }
                    }

                    l.AddRange(Marked);

                }

                l = l.SortedDistinctList();

            }


            if (maxItems > 0 && l.Count > maxItems) { return; }

            e.AddRange(l, column, style, compact);


            if (checkedItemsAtRow != null)
            {
                foreach (var t in Marked)
                {
                    if (e[t] is BasicListItem bli) { bli.Checked = true; }
                }
            }
            e.Sort();
        }
    }
}
