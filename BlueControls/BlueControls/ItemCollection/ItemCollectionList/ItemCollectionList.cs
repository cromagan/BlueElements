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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueDatabase;
using BlueControls.Enums;
using static BlueBasics.FileOperations;
using BlueDatabase.Enums;

namespace BlueControls.ItemCollection
{
    public class ItemCollectionList : ListExt<BasicListItem>, ICloneable
    {

        #region  Variablen-Deklarationen 

        private enCheckBehavior _CheckBehavior;
        private bool _CellposCorrect = true;
        private bool _Validating;

        private enBlueListBoxAppearance _Appearance;
        private enDesign _ControlDesign;
        private enDesign _ItemDesign;

        private SizeF ComputeAllItemPositions_lastF = Size.Empty;


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


                DesignOrStyleChanged();
                OnNeedRefresh();
            }
        }

        #endregion

        #region  Construktor + Initialize 



        public ItemCollectionList()
        {
            Initialize();
        }

        public ItemCollectionList(enBlueListBoxAppearance Design)
        {
            Initialize();
            _Appearance = Design;
            GetDesigns();
        }


        private void Initialize()
        {
            _CellposCorrect = true;
            _Appearance = enBlueListBoxAppearance.Listbox;
            _ItemDesign = enDesign.Undefiniert;
            _ControlDesign = enDesign.Undefiniert;
            _CheckBehavior = enCheckBehavior.SingleSelection;
        }

        #endregion

        #region  Event-Deklarationen + Delegaten 
        public event EventHandler ItemCheckedChanged;
        public event EventHandler NeedRefresh;
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
            OnNeedRefresh();
        }



        private void OnItemCheckedChanged()
        {
            ItemCheckedChanged?.Invoke(this, System.EventArgs.Empty);
        }

        private void OnNeedRefresh()
        {
            NeedRefresh?.Invoke(this, System.EventArgs.Empty);
        }

        protected override void OnListOrItemChanged()
        {
            base.OnListOrItemChanged();
            _CellposCorrect = false;
            OnNeedRefresh();
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


        internal float HeightOfBiggestItem(int MaxHeight)
        {
            float h = 16;

            foreach (var ThisItem in this)
            {
                if (ThisItem != null)
                {
                    h = Math.Max(h, ThisItem.QuickAndDirtySize(0).Height);
                    if (h > MaxHeight) { return MaxHeight; }
                }
            }

            return h;
        }

        internal float WidthOfBiggestItem(int MaxWidth)
        {
            float w = 16;

            foreach (var ThisItem in this)
            {
                if (ThisItem != null)
                {
                    w = Math.Max(w, ThisItem.QuickAndDirtySize(0).Width);
                    if (w > MaxWidth) { return MaxWidth; }
                }
            }

            return w;
        }

        internal float HeigthOfAllItemsAdded(int MaxWidth)
        {
            float w = 0;

            var maxh = (int)(MaxWidth * 0.8);

            foreach (var ThisItem in this)
            {
                if (ThisItem != null)
                {
                    w += ThisItem.QuickAndDirtySize(maxh).Height;
                }

            }

            return w;
        }

        internal void ComputeAllItemPositions(SizeF Max, bool CanChangeSize, bool MustBeOneColumn, enBlueListBoxAppearance GalleryStyle, System.Windows.Forms.Control InControl, Slider SliderY)
        {
            if (_ItemDesign == enDesign.Undefiniert) { GetDesigns(); }



            if (Math.Abs(ComputeAllItemPositions_lastF.Width - Max.Width) > 0.01 || Math.Abs(ComputeAllItemPositions_lastF.Height - Max.Height) > 0.01)
            {
                ComputeAllItemPositions_lastF = Max;
                _CellposCorrect = false;
            }
            if (_CellposCorrect) { return; }

  


            var InControlWidth = int.MaxValue;
            if (InControl != null) { InControlWidth = InControl.Width; }


            var BiggestWidth = WidthOfBiggestItem(InControlWidth);
            var Bigy = HeigthOfAllItemsAdded(InControlWidth);
            var Sp = 1;
            var SliderWidth = 0;
            float MultiX = 0;
            float CX = 0;
            float CY = 0;


            var WouldBeGood = -1;

            if (Bigy < 1)
            {
                _CellposCorrect = true;
                return;
            }


            if (GalleryStyle != enBlueListBoxAppearance.FileSystem && GalleryStyle != enBlueListBoxAppearance.Gallery && Count < 18) { MustBeOneColumn = true; }


            switch (GalleryStyle)
            {
                case enBlueListBoxAppearance.Gallery:
                    Sp = (int)Math.Truncate(Max.Width / 350.0);
                    if (Sp > 10) { Sp = 10; }
                    if (Sp < 1) { Sp = 1; }

                    if (SliderY != null)
                    {
                        MultiX = (int)Math.Truncate((Max.Width - SliderY.Width) / (double)Sp);
                    }
                    else
                    {
                        MultiX = (int)Math.Truncate(Max.Width / (double)Sp);
                    }
                    break;

                case enBlueListBoxAppearance.FileSystem:

                    Sp = (int)Math.Truncate(Max.Width / 110.0);
                    if (Sp < 1) { Sp = 1; }
                    if (SliderY != null)
                    {
                        MultiX = (int)Math.Truncate((Max.Width - SliderY.Width) / (double)Sp);
                    }
                    else
                    {
                        MultiX = (int)Math.Truncate(Max.Width / (double)Sp);
                    }
                    break;

                default:
                    if (CanChangeSize)
                    {
                        if (MustBeOneColumn)
                        {
                            Max = new SizeF(BiggestWidth, Bigy);
                        }
                        else
                        {
                            var TestSP = 0;
                            for (TestSP = 10; TestSP >= 1; TestSP--)
                            {
                                Max = new SizeF(BiggestWidth * TestSP + TestSP * Skin.PaddingSmal, Bigy / TestSP);

                                // Wenn die MindestWidth nicht abgefragt wird, wird die Width nachher erhöht erhöhe sich die Spalten. Und dann ist die Height falsch
                                if (Max.Width > 150 && Max.Width < 500 && Max.Height < 500 && Count / (double)TestSP > TestSP)
                                {
                                    WouldBeGood = TestSP;
                                    if (Max.Width / (double)Max.Height < 0.6F)
                                    {
                                        break;
                                    }
                                }
                            }

                            if (TestSP < 2 && WouldBeGood > 0)
                            {
                                TestSP = WouldBeGood;
                                Max = new SizeF(BiggestWidth * TestSP + TestSP * Skin.PaddingSmal, Bigy / TestSP);
                            }
                        }

                        Max = new SizeF((int)(Max.Width + BiggestWidth / 2), Max.Height + 100);
                    }


                    // Wenn die Maximale Höhe aller Items Größer als der Draw-Bereich ist, versuche, es auf mehrere Spalten aufzuteilen.
                    if (Bigy > Max.Height)
                    {
                        Sp = (int)Math.Truncate(Max.Width / (BiggestWidth + Skin.PaddingSmal));
                        if (Sp > 1 && !MustBeOneColumn)
                        {
                            if (Bigy / Sp > Max.Height)
                            {
                                Sp = 1;
                            }
                        }
                        else
                        {
                            Sp = 1;
                        }


                        do
                        {
                            MultiX = (int)Math.Truncate(Max.Width / (double)Sp);
                            if (Sp == 1)
                            {
                                break;
                            }

                            if (MultiX < BiggestWidth + Skin.PaddingSmal)
                            {
                                Sp -= 1;
                            }
                            else
                            {
                                break;
                            }
                        } while (true);


                        if (!CanChangeSize)
                        {
                            // Ok, wir habe nun die Spalten und die Wunschbreite.
                            // Wenn nun aber untenrum viel zu viel Platz ist, sieht es scheiße aus. Lieber nur eine Spalte machen!
                            if (Bigy / Sp < Max.Height - 30)
                            {
                                Sp = 1;
                                MultiX = Max.Width;
                            }
                        }


                    }
                    else
                    {

                        MultiX = BiggestWidth; // Max.Width
                    }
                    break;
            }


            if (SliderY != null)
            {
                if (Sp == 1 && Bigy > Max.Height || GalleryStyle == enBlueListBoxAppearance.Gallery || GalleryStyle == enBlueListBoxAppearance.FileSystem)
                {
                    SliderWidth = SliderY.Width;
                }
                else
                {
                    SliderWidth = 0;
                }
            }

            if (Sp == 1)
            {
                if (!CanChangeSize)
                {
                    MultiX = Max.Width;
                }
                else
                {
                    MultiX = BiggestWidth + SliderWidth;
                }
            }


            var IsZ = 0;
            var MinX = int.MaxValue;
            var Miny = int.MaxValue;
            var MaxX = int.MinValue;
            var Maxy = int.MinValue;




            //if (_AutoSort)
            //{
            //    Sort();
            //}



            foreach (var ThisItem in this)
            {
                // PaintmodX kann immer angezogen werden, da es eh nur bei einspaltigen Listboxen verändert wird!
                if (ThisItem != null)
                {


                    ThisItem.ComputePositionForListBox(_Appearance, CX, CY, MultiX, SliderWidth, InControlWidth);
                    var YVal = ThisItem.Pos.Height;


                    IsZ += 1;

                    if (GalleryStyle == enBlueListBoxAppearance.Gallery || GalleryStyle == enBlueListBoxAppearance.FileSystem)
                    {
                        // Links nach Rechts, bevorzugt für bildergallerien
                        CX += MultiX;

                        if (IsZ >= Sp)
                        {
                            IsZ = 0;
                            CY += YVal;
                            CX = 0;
                        }

                    }
                    else
                    {
                        // Oben nach Unten, Texte und alles andere
                        CY += YVal;
                        if (IsZ > (int)Math.Truncate((Count - 1) / (double)Sp))
                        {
                            IsZ = 0;
                            CY = 0;
                            CX = CX + MultiX;
                        }
                    }

                    MaxX = Math.Max(ThisItem.Pos.Right, MaxX);
                    Maxy = Math.Max(ThisItem.Pos.Bottom, Maxy);
                    MinX = Math.Min(ThisItem.Pos.Left, MinX);
                    Miny = Math.Min(ThisItem.Pos.Top, Miny);
                }

            }


            if (SliderY != null)
            {

                var SetTo0 = false;

                if (SliderWidth > 0)
                {
                    if (Maxy - Max.Height <= Miny)
                    {
                        SliderY.Enabled = false;
                        SetTo0 = true;
                    }
                    else
                    {
                        SliderY.Enabled = true;
                        SliderY.Minimum = Miny;
                        SliderY.SmallChange = 16;
                        SliderY.LargeChange = Max.Height;
                        SliderY.Maximum = Maxy - Max.Height;
                        SliderY.Height = (int)Max.Height;
                        SetTo0 = false;
                    }

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

            _CellposCorrect = true;
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
                        Add(new TextListItem(th.ToString(), te));
                    }
                }
            }

        }

        #region  Add / AddRange / AddBasicLevel 




        public void Add(clsNamedBinary ThisBin)
        {
            if (ThisBin.Picture != null)
            {
                Add(new BitmapListItem(ThisBin.Picture, ThisBin.Name));
            }
            else
            {
                Add(new TextListItem(ThisBin.Binary, ThisBin.Name));
            }
        }

        public void AddRange(ColumnCollection Columns, bool OnlyExportableTextformatForLayout, bool NoCritical)
        {
            foreach (var ThisColumnItem in Columns)
            {
                if (ThisColumnItem != null)
                {
                    var addx = true;
                    if (OnlyExportableTextformatForLayout && !ThisColumnItem.ExportableTextformatForLayout()) { addx = false; }
                    if (NoCritical && !ThisColumnItem.Format.CanBeCheckedByRules()) { addx = false; }

                    if (addx) { if (this[ThisColumnItem.Name] == null) { Add(ThisColumnItem); } }

                }

            }

        }


        public void AddRange(Type type)
        {
            foreach (int z1 in Enum.GetValues(type))
            {
                if (this[z1.ToString()] == null) { Add(new TextListItem(z1.ToString(), Enum.GetName(type, z1).Replace("_", " "))); }
            }

            this.Sort();

        }

        public void AddRange(string[] Values)
        {
            if (Values == null) { return; }

            foreach (var thisstring in Values)
            {
                if (this[thisstring] == null) { Add(thisstring); }
            }

        }


        public void Add(string Value)
        {
            Add(new TextListItem(Value, Value));
        }


        public void AddRange(ListExt<string> Values)
        {

            if (Values == null) { return; }

            foreach (var thisstring in Values)
            {
                if (this[thisstring] == null) { Add(new TextListItem(thisstring, thisstring)); }
            }


        }


        public void AddRange(List<string> Values, ColumnItem ColumnStyle, enShortenStyle Style)
        {

            if (Values == null) { return; }


            if (Values.Count > 10000)
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Values > 100000");
                return;
            }


            foreach (var thisstring in Values)
            {
                Add(thisstring, ColumnStyle, Style); // If Item(thisstring) Is Nothing Then Add(New CellLikeItem(thisstring, ColumnStyle))
            }

        }

        public void Add(string Value, ColumnItem ColumnStyle, enShortenStyle Style)
        {


            if (this[Value] == null)
            {
                if (ColumnStyle.Format == enDataFormat.Link_To_Filesystem && Value.FileType() == enFileFormat.Image)
                {
                    Add(new BitmapListItem(Value, Value, ColumnStyle.BestFile(Value, false), ColumnStyle.Database.FileEncryptionKey));
                }
                else
                {
                    Add(new CellLikeListItem(Value, ColumnStyle, Style, true));

                }
            }

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
                    if (this[thisstring] == null) { Add(new TextListItem(thisstring, thisstring)); }
                }
            }

        }



        public void AddRange(List<BasicListItem> Vars)
        {
            if (Vars == null) { return; }

            foreach (var thisItem in Vars)
            {
                if (IndexOf(thisItem) < 0)
                {
                    switch (thisItem)
                    {
                        case TextListItem TI: Add(TI); break;
                        case BitmapListItem BI: Add(BI); break;
                        case LineListItem LI: Add(LI); break;
                        case ObjectListItem OI: Add(OI); break;
                        case CellLikeListItem CI: Add(CI); break;
                        default:
                            Develop.DebugPrint_NichtImplementiert();
                            break;
                    }
                }
            }
        }

        public void AddLayoutsOf(Database vLayoutDatabase, bool vDoDiscLayouts, string vAdditionalLayoutPath)
        {

            for (var z = 0; z < vLayoutDatabase.Layouts.Count; z++)
            {
                using (var p = new CreativePad())
                {
                    p.ParseData(vLayoutDatabase.Layouts[z], false, string.Empty);
                    Add(new TextListItem(p.ID, p.Caption, enImageCode.Stern));
                }
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

                        if (this[ThisFile] == null) { Add(new TextListItem(ThisFile, ThisFile.FileNameWithSuffix(), QuickImage.Get(ThisFile.FileType(), 16))); }
                    }
                }

                if (vLayoutDatabase == null) { break; }

                du += 1;
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


        //private void InvalidateView()
        //{
        //    _CellposCorrect = false;
        //    OnListOrItemChanged();
        //}




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


        public void Swap(ref BasicListItem Nr1, ref BasicListItem Nr2)
        {
            // Der Swap geht so, und nicht anders! Es müssen die Items im Original-Array geswapt werden!
            Swap(IndexOf(Nr1), IndexOf(Nr2));
            _CellposCorrect = false;
        }


        public void Add(ColumnItem Column)
        {
            Add(new ObjectListItem(Column));
        }

        public void Add(enContextMenuComands Comand, bool vEnabled = true)
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
                default:
                    Develop.DebugPrint(Comand);
                    _ReadableText = _Internal;
                    _Symbol = QuickImage.Get(enImageCode.Fragezeichen);
                    break;
            }

            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben:" + Comand); }

            Add(new TextListItem(_Internal, _ReadableText, _Symbol, vEnabled));


        }

        public new void Add(BasicListItem cItem)
        {
            cItem.Parent = this;
            base.Add(cItem);
            OnNeedRefresh();
        }


        public void DesignOrStyleChanged()
        {

            foreach (var thisItem in this)
            {
                thisItem?.DesignOrStyleChanged();
            }
        }



        public void Remove(List<string> Internals)
        {

            if (Internals == null || Internals.Count == 0) { return; }

            foreach (var thisstring in Internals)
            {
                Remove(this[thisstring]);
            }
        }



        public void Remove(string Internal)
        {
            Remove(this[Internal]);
        }

        public new void Remove(BasicListItem cItem)
        {
            if (cItem == null) { return; }
            base.Remove(cItem);
            _CellposCorrect = false;
            OnNeedRefresh();
        }


        public new void Clear()
        {
            if (Count == 0) { return; }
            base.Clear();
            _CellposCorrect = false;
            OnNeedRefresh();
        }


        public Rectangle MaximumBounds()
        {
            var x1 = int.MaxValue;
            var y1 = int.MaxValue;
            var x2 = int.MinValue;
            var y2 = int.MinValue;

            var Done = false;


            foreach (var ThisItem in this) // As Integer = 0 To _Items.GetUpperBound(0)
            {
                if (ThisItem != null)
                {

                    x1 = Math.Min(x1, ThisItem.Pos.Left);
                    y1 = Math.Min(y1, ThisItem.Pos.Top);

                    x2 = Math.Max(x2, ThisItem.Pos.Right);
                    y2 = Math.Max(y2, ThisItem.Pos.Bottom);
                    Done = true;
                }
            }


            if (!Done) { return Rectangle.Empty; }

            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
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

            if (SomethingDonex) { OnNeedRefresh(); }
        }

        public object Clone()
        {

            var x = new ItemCollectionList(_Appearance);
            x.CheckBehavior = _CheckBehavior;


            foreach (var ThisItem in this)
            {
                x.Add((BasicListItem)ThisItem.Clone());
            }
            return x;
        }




        public static void GetItemCollection(ItemCollectionList e, ColumnItem column, RowItem checkedItemsAtRow, enShortenStyle style, int maxItems)
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
                    var F = new FilterCollection(column.Database);
                    F.Add(new FilterItem(cc, enFilterType.Istgleich_GroßKleinEgal, checkedItemsAtRow.CellGetString(cc)));
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

            e.AddRange(l, column, style);


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
