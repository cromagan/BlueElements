// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using MessageBox = BlueControls.Forms.MessageBox;
using static BlueBasics.Converter;
using BlueDatabase;
using BlueDatabase.Enums;

using BlueBasics.Interfaces;
using BlueControls.Interfaces;
using BlueControls.ConnectedFormula;

namespace BlueControls.ItemCollection {

    public class EditFieldPadItem : RectanglePadItem, IReadableText, IRecursiveCheck, IContentHolder, IItemToControl {

        #region Fields

        public static BlueFont? CaptionFNT = Skin.GetBlueFont(Design.Caption, States.Standard);

        public ColumnItem? Column = null;
        private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld;
        private ICalculateRows? _GetValueFrom = null;
        private ÜberschriftAnordnung _überschiftanordung = ÜberschriftAnordnung.Über_dem_Feld;

        #endregion

        #region Constructors

        public EditFieldPadItem(string internalname) : base(internalname) {
            SetCoordinates(new RectangleF(0, 0, 50, 30), true);
        }

        #endregion

        #region Properties

        public string Breite_Berechnen {
            get => string.Empty;
            set {
                var li = new ItemCollectionList.ItemCollectionList();
                for (var br = 1; br <= 20; br++) {
                    li.Add(br + " Spalte(n)", br.ToString(), true, string.Empty);

                    for (var pos = 1; pos <= br; pos++) {
                        li.Add(br + " Spalte(n) - Position: " + pos, br + ";" + pos);
                    }
                }

                var x2 = BlueControls.Forms.InputBoxListBoxStyle.Show("Bitte Breite und Position wählen:", li, AddType.None, true);

                if (x2 == null || x2.Count != 1) { return; }

                var doit = x2[0].SplitBy(";");

                var anzbr = IntParse(doit[0]);
                var npos = IntParse(doit[1]);
                var x = UsedArea;
                x.Width = (Parent.SheetSizeInPix.Width - (MmToPixel(0.5f, 300) * (anzbr - 1))) / anzbr;
                x.X = x.Width * (npos - 1) + MmToPixel(0.5f, 300) * (npos - 1);

                SetCoordinates(x, true);

                //OnChanged();
            }
        }

        //public EditFieldPadItem(string internalname, Bitmap? bmp, Size size) : base(internalname) {
        //    //Bitmap = bmp;
        //    //SetCoordinates(new RectangleF(0, 0, size.Width, size.Height), true);
        //    //Overlays = new List<QuickImage>();
        //    //Hintergrund_Weiß_Füllen = true;
        //    //Padding = 0;
        //    //Bild_Modus = enSizeModes.EmptySpace;
        //    //Stil = PadStyles.Undefiniert; // Kein Rahmen
        //}
        public ÜberschriftAnordnung CaptionPosition {
            get => _überschiftanordung;
            set {
                if (_überschiftanordung == value) { return; }
                _überschiftanordung = value;
                OnChanged();
            }
        }

        public string Datenbank {
            get {
                if (Column?.Database == null) { return "?"; }
                return Column.Database.Filename.FileNameWithSuffix();
            }
        }

        [Description("Wählt ein Zeilen-Objekt, aus der die Werte kommen.")]
        public string Datenquelle_wählen {
            get => string.Empty;
            set {
                var x = new ItemCollectionList.ItemCollectionList();
                foreach (var thisR in Parent) {
                    if (thisR is ICalculateRows rfp) {
                        if (!rfp.IsRecursiveWith(this)) {
                            x.Add(rfp, thisR.Internal);
                        }
                    }
                }

                x.Add("<Keine Quelle>");

                var it = Forms.InputBoxListBoxStyle.Show("Quelle wählen:", x, AddType.None, true);

                if (it == null || it.Count != 1) { return; }

                var t = Parent[it[0]];

                if (t is ICalculateRows rfp2) {
                    if (rfp2 != GetRowFrom) {
                        GetRowFrom = rfp2;
                        Column = null;
                    }
                } else {
                    GetRowFrom = null;
                    Column = null;
                }

                OnChanged();
            }
        }

        public EditTypeFormula EditType {
            get => _bearbeitung;
            set {
                if (_bearbeitung == value) { return; }
                _bearbeitung = value;
                OnChanged();
            }
        }

        public ICalculateRows? GetRowFrom {
            get => _GetValueFrom;

            set {
                if (value == _GetValueFrom) { return; }
                _GetValueFrom = value;
                RepairConnections();
                OnChanged();
            }
        }

        public string Interner_Name {
            get {
                if (Column == null) { return "?"; }
                return Column.Name;
            }
        }

        public string Spalte_bearbeiten {
            get => string.Empty;
            set {
                if (Column == null) { return; }
                Forms.TableView.OpenColumnEditor(Column, null, null);

                OnChanged();
            }
        }

        //public bool Hintergrund_Weiß_Füllen { get; set; }
        [Description("Wählt die Spalte, die angezeigt werden soll.\r\nDiese bestimmt maßgeblich die Eigenschaften")]
        public string Spalte_wählen {
            get => string.Empty;
            set {
                if (GetRowFrom == null) {
                    MessageBox.Show("Zuerst Datenquelle wählen.");
                    return;
                }

                if (GetRowFrom.Database == null) {
                    MessageBox.Show("Quelle fehlerhaft!");
                    return;
                }

                var lst = new ItemCollectionList.ItemCollectionList();
                lst.AddRange(GetRowFrom.Database.Column, false);

                var sho = Forms.InputBoxListBoxStyle.Show("Spalte wählen:", lst, AddType.None, true);

                if (sho == null || sho.Count != 1) { return; }

                var k = IntParse(sho[0]);

                var col = GetRowFrom.Database.Column.SearchByKey(k);

                if (col == Column) { return; }
                Column = col;

                OnChanged();
            }
        }

        //public Bitmap? Bitmap { get; set; }
        public string Spalten_AdminInfo {
            get {
                if (Column != null) { return Column.AdminInfo; }
                return string.Empty;
            }
            set {
                if (Column != null) { Column.AdminInfo = value; }
            }
        }

        //public enSizeModes Bild_Modus { get; set; }
        public string Spalten_QuickInfo {
            get {
                if (Column != null) { return Column.Quickinfo; }
                return string.Empty;
            }
            set {
                if (Column != null) { Column.Quickinfo = value; }
            }
        }

        public string Standardhöhe {
            get => string.Empty;
            set {
                var x = UsedArea;
                x.Height = Converter.MmToPixel(ConnectedFormula.ConnectedFormula.StandardHöhe, 300);
                SetCoordinates(x, true);

                //OnChanged();
            }
        }

        protected override int SaveOrder => 2;

        #endregion

        #region Methods

        public System.Windows.Forms.Control GenerateControl(ConnectedFormulaView parent) {
            if (GetRowFrom is RowWithFilterPaditem rfw2) {
                var ff = parent.SearchOrGenerate(rfw2);

                if (rfw2.Genau_eine_Zeile) {
                    var cx = new FlexiControlForCell();
                    cx.ColumnKey = Column.Key;
                    cx.EditType = EditType;
                    cx.CaptionPosition = CaptionPosition;
                    cx.Tag = Internal;
                    if (ff is Connector cc) { cc.Childs.Add(cx); }
                    return cx;
                } else {
                    var c = new FlexiControl();
                    c.Caption = Column.ReadableText() + ":";
                    c.EditType = EditType;
                    c.CaptionPosition = CaptionPosition;
                    c.Tag = Internal;
                    if (ff is Connector cc) { cc.Childs.Add(c); }
                    return c;
                }
            }

            var cy = new FlexiControl();
            cy.Caption = Column.ReadableText() + ":";
            cy.EditType = EditType;
            cy.CaptionPosition = CaptionPosition;
            cy.DisabledReason = "Keine Verknüpfung vorhanden.";
            cy.Tag = Internal;
            return cy;
        }

        //private string _quickinfo;
        //private string _adminInfo;
        //public EditFieldPadItem(string internalname, string fileToLoad) : this(internalname, (Bitmap)BitmapExt.Image_FromFile(fileToLoad), Size.Empty) { }

        //public EditFieldPadItem(string internalname, Bitmap? bmp) : this(internalname, bmp, Size.Empty) { }

        //public EditFieldPadItem(Bitmap? bmp, Size size) : this(string.Empty, bmp, size) { }

        //public EditFieldPadItem(Bitmap? bmp) : this(string.Empty, bmp, Size.Empty) { }
        public override List<GenericControl> GetStyleOptions() {
            List<GenericControl> l = new();

            l.Add(new FlexiControlForProperty<string>(() => Datenquelle_wählen, ImageCode.Pfeil_Rechts));
            l.Add(new FlexiControlForProperty<string>(() => Datenbank));
            l.Add(new FlexiControlForProperty<string>(() => Spalte_wählen, ImageCode.Pfeil_Rechts));
            l.Add(new FlexiControlForProperty<string>(() => Interner_Name));
            l.Add(new FlexiControlForProperty<string>(() => Spalte_bearbeiten, ImageCode.Spalte));
            l.Add(new FlexiControl());

            var u = new ItemCollection.ItemCollectionList.ItemCollectionList();
            u.AddRange(typeof(ÜberschriftAnordnung));
            l.Add(new FlexiControlForProperty<ÜberschriftAnordnung>(() => CaptionPosition, u));
            var b = new ItemCollection.ItemCollectionList.ItemCollectionList();
            b.AddRange(typeof(EditTypeFormula));
            l.Add(new FlexiControlForProperty<EditTypeFormula>(() => EditType, b));
            l.Add(new FlexiControlForProperty<string>(() => Standardhöhe, ImageCode.GrößeÄndern));
            l.Add(new FlexiControlForProperty<string>(() => Breite_Berechnen, ImageCode.GrößeÄndern));
            l.Add(new FlexiControl());
            l.Add(new FlexiControlForProperty<string>(() => Spalten_QuickInfo, 5));
            l.Add(new FlexiControlForProperty<string>(() => Spalten_AdminInfo, 5));
            l.Add(new FlexiControl());

            //if(_getValueFrom != null && _getValueFrom.Database is Database db) {
            //      foreach(var thisC in db) {
            //          s
            //      }

            //  }

            //{
            //    new FlexiControlForProperty(()=> this.Bildschirmbereich_wählen", enImageCode.Bild),
            //    new FlexiControlForProperty(()=> this.Datei_laden", enImageCode.Ordner),
            //    new FlexiControl(),
            //    new FlexiControlForProperty(()=> this.Platzhalter_für_Layout", 2),
            //    new FlexiControl()
            //};
            //ItemCollectionList.ItemCollectionList comms = new()
            //{
            //    { "Abschneiden", ((int)enSizeModes.BildAbschneiden).ToString(), QuickImage.Get("BildmodusAbschneiden|32") },
            //    { "Verzerren", ((int)enSizeModes.Verzerren).ToString(), QuickImage.Get("BildmodusVerzerren|32") },
            //    { "Einpassen", ((int)enSizeModes.EmptySpace).ToString(), QuickImage.Get("BildmodusEinpassen|32") }
            //};
            //l.Add(new FlexiControlForProperty(()=> this.Bild-Modus", comms));
            //l.Add(new FlexiControl());
            //AddLineStyleOption(l);
            //l.Add(new FlexiControlForProperty(()=> this.Hintergrund_weiß_füllen"));
            //l.AddRange(base.GetStyleOptions());
            return l;
        }

        public bool IsRecursiveWith(IRecursiveCheck obj) {
            if (obj == this) { return true; }

            if (GetRowFrom is IRecursiveCheck i) { return i.IsRecursiveWith(obj); }
            return false;
        }

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "getvaluefrom":
                    GetRowFrom = (ICalculateRows)Parent[value.FromNonCritical()];
                    return true;

                case "column":
                    Column = GetRowFrom.Database.Column.SearchByKey(IntParse(value));
                    return true;

                case "edittype":
                    _bearbeitung = (EditTypeFormula)IntParse(value);
                    return true;

                case "caption":
                    _überschiftanordung = (ÜberschriftAnordnung)IntParse(value);
                    return true;
            }
            return false;
        }

        public string ReadableText() {
            if (Column != null) {
                return "Wert aus: " + Column.ReadableText();

                //if (Genau_eine_Zeile) {
                //    return "(eine) Zeile aus: " + Database.Caption;
                //} else {
                //    return "Zeilen aus: " + Database.Caption;
                //}
            }

            return "Wert einer Spalte";
        }

        public QuickImage? SymbolForReadableText() {
            if (GetRowFrom == null) { return null; }

            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IDColor(GetRowFrom.Id));
        }

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            if (GetRowFrom != null) {
                t = t + "GetValueFrom=" + GetRowFrom.Internal.ToNonCritical() + ", ";
            }

            if (Column != null) {
                t = t + "Column=" + Column.Key + ", ";
            }

            t = t + "EditType=" + ((int)_bearbeitung).ToString() + ", ";
            t = t + "Caption=" + ((int)_überschiftanordung).ToString() + ", ";

            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "EditField";

        //    return false;
        //}
        protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
            var id = -1; if (GetRowFrom != null) { id = GetRowFrom.Id; }

            if (!forPrinting) {
                DrawColorScheme(gr, positionModified, zoom, id);
            }

            if (GetRowFrom == null) {
                Skin.Draw_FormatedText(gr, "Datenquelle fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFNT.Scale(zoom), true);
            } else if (Column == null) {
                Skin.Draw_FormatedText(gr, "Spalte fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFNT.Scale(zoom), true);
            } else {
                Point cap;
                var uc = positionModified.ToRect();

                switch (CaptionPosition) {
                    case ÜberschriftAnordnung.ohne:
                        cap = new Point(-1, -1);
                        //uc = positionModified.ToRect();
                        break;

                    case ÜberschriftAnordnung.Links_neben_Dem_Feld:
                        cap = new Point(0, 0);
                        uc.X += (int)(100 * zoom);
                        uc.Width -= (int)(100 * zoom);
                        break;

                    case ÜberschriftAnordnung.Ohne_mit_Abstand:
                        cap = new Point(-1, -1);
                        uc.Y += (int)(19 * zoom);
                        uc.Height -= (int)(19 * zoom);
                        break;

                    case ÜberschriftAnordnung.Über_dem_Feld:
                    default:
                        cap = new Point(0, 0);
                        uc.Y += (int)(19 * zoom);
                        uc.Height -= (int)(19 * zoom);
                        break;
                }

                if (cap.X >= 0) {
                    var e = new RectangleF(positionModified.Left + cap.X * zoom, positionModified.Top + cap.Y * zoom, positionModified.Width, 16 * zoom);
                    Skin.Draw_FormatedText(gr, Column.ReadableText() + ":", null, Alignment.Top_Left, e.ToRect(), CaptionFNT.Scale(zoom), true);
                }

                if (uc.Width > 0 && uc.Height > 0) {
                    gr.DrawRectangle(new Pen(Color.Black, zoom), uc);
                }
            }

            //drawingCoordinates.Inflate(-Padding, -Padding);
            //RectangleF r1 = new(drawingCoordinates.Left + Padding, drawingCoordinates.Top + Padding,
            //    drawingCoordinates.Width - (Padding * 2), drawingCoordinates.Height - (Padding * 2));
            //RectangleF r2 = new();
            //RectangleF r3 = new();
            //if (Bitmap != null) {
            //    r3 = new RectangleF(0, 0, Bitmap.Width, Bitmap.Height);
            //    switch (Bild_Modus) {
            //        case enSizeModes.Verzerren: {
            //                r2 = r1;
            //                break;
            //            }

            //        case enSizeModes.BildAbschneiden: {
            //                var scale = Math.Max((drawingCoordinates.Width - (Padding * 2)) / Bitmap.Width, (drawingCoordinates.Height - (Padding * 2)) / Bitmap.Height);
            //                var tmpw = (drawingCoordinates.Width - (Padding * 2)) / scale;
            //                var tmph = (drawingCoordinates.Height - (Padding * 2)) / scale;
            //                r3 = new RectangleF((Bitmap.Width - tmpw) / 2, (Bitmap.Height - tmph) / 2, tmpw, tmph);
            //                r2 = r1;
            //                break;
            //            }
            //        default: // Is = enSizeModes.WeißerRand
            //            {
            //                var scale = Math.Min((drawingCoordinates.Width - (Padding * 2)) / Bitmap.Width, (drawingCoordinates.Height - (Padding * 2)) / Bitmap.Height);
            //                r2 = new RectangleF(((drawingCoordinates.Width - (Bitmap.Width * scale)) / 2) + drawingCoordinates.Left, ((drawingCoordinates.Height - (Bitmap.Height * scale)) / 2) + drawingCoordinates.Top, Bitmap.Width * scale, Bitmap.Height * scale);
            //                break;
            //            }
            //    }
            //}
            //var trp = drawingCoordinates.PointOf(enAlignment.Horizontal_Vertical_Center);
            //gr.TranslateTransform(trp.X, trp.Y);
            //gr.RotateTransform(-Drehwinkel);
            //r1 = new RectangleF(r1.Left - trp.X, r1.Top - trp.Y, r1.Width, r1.Height);
            //r2 = new RectangleF(r2.Left - trp.X, r2.Top - trp.Y, r2.Width, r2.Height);
            //if (Hintergrund_Weiß_Füllen) {
            //    gr.FillRectangle(Brushes.White, r1);
            //}
            //try {
            //    if (Bitmap != null) {
            //        if (forPrinting) {
            //            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
            //        } else {
            //            gr.InterpolationMode = InterpolationMode.Low;
            //            gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            //        }
            //        gr.DrawImage(Bitmap, r2, r3, GraphicsUnit.Pixel);
            //    }
            //} catch {
            //    Generic.CollectGarbage();
            //}
            //if (Stil != PadStyles.Undefiniert) {
            //    if (Parent.SheetStyleScale > 0 && Parent.SheetStyle != null) {
            //        gr.DrawRectangle(Skin.GetBlueFont(Stil, Parent.SheetStyle).Pen(zoom * Parent.SheetStyleScale), r1);
            //    }
            //}
            //foreach (var thisQi in Overlays) {
            //    gr.DrawImage(thisQi, r2.Left + 8, r2.Top + 8);
            //}
            //gr.TranslateTransform(-trp.X, -trp.Y);
            //gr.ResetTransform();
            //if (!forPrinting) {
            //    if (!string.IsNullOrEmpty(Platzhalter_Für_Layout)) {
            //        Font f = new("Arial", 8);
            //        BlueFont.DrawString(gr, Platzhalter_Für_Layout, f, Brushes.Black, drawingCoordinates.Left, drawingCoordinates.Top);
            //    }
            //}
            base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        }

        //public bool ReplaceVariable(Variable variable) {
        //    if (string.IsNullOrEmpty(Platzhalter_Für_Layout)) { return false; }
        //    if ("~" + variable.Name.ToLower() + "~" != Platzhalter_Für_Layout.ToLower()) { return false; }
        //    if (variable is not VariableBitmap vbmp) { return false; }
        //    var ot = vbmp.ValueBitmap;
        //    if (ot is Bitmap bmp) {
        //        Bitmap = bmp;
        //        OnChanged();
        //        return true;
        //    }
        protected override BasicPadItem? TryCreate(string id, string name) {
            if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
                return new EditFieldPadItem(name);
            }
            return null;
        }

        private void RepairConnections() {
            ConnectsTo.Clear();

            if (GetRowFrom != null) {
                ConnectsTo.Add(new ItemConnection(ConnectionType.Top, true, (BasicPadItem)GetRowFrom, ConnectionType.Bottom, false));
            }
        }

        #endregion

        // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        // ~BitmapPadItem()
        // {
        //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        //     Dispose(disposing: false);
        // }
        //public override void DoStyleCommands(object sender, List<string> Tags, ref bool CloseMenu)
        //{
        //    base.DoStyleCommands(sender, Tags, ref CloseMenu);
        //    if (Tags.TagGet("Bildschirmbereich wählen").FromPlusMinus())
        //    {
        //        CloseMenu = false;
        //        if (Bitmap != null)
        //        {
        //            if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        //        }
        //        Bitmap = ScreenShot.GrabArea(null, 2000, 2000).Pic;
        //        return;
        //    }
        //    if (Tags.TagGet("Datei laden").FromPlusMinus())
        //    {
        //        CloseMenu = false;
        //        if (Bitmap != null)
        //        {
        //            if (MessageBox.Show("Vorhandenes Bild überschreiben?", enImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        //        }
        //        var e = new System.Windows.Forms.OpenFileDialog();
        //        e.CheckFileExists = true;
        //        e.Multiselect = false;
        //        e.Title = "Bild wählen:";
        //        e.Filter = "PNG Portable Network Graphics|*.png|JPG Jpeg Interchange|*.jpg|Bmp Windows Bitmap|*.bmp";
        //        e.ShowDialog();
        //        if (!FileExists(e.FileName))
        //        {
        //            return;
        //        }
        //        Bitmap = (Bitmap)modAllgemein.Image_FromFile(e.FileName);
        //        return;
        //    }
        //    if (Tags.TagGet("Skalieren").FromPlusMinus())
        //    {
        //        CloseMenu = false;
        //        var t = InputBox.Show("Skalierfaktor oder Formel eingeben:", "1", enDataFormat.Text);
        //        var sc = modErgebnis.Ergebnis(t);
        //        if (sc == null || sc == 1)
        //        {
        //            Notification.Show("Keine Änderung vorgenommen.");
        //            return;
        //        }
        //        var x = p_RU.X - p_LO.X;
        //        var y = p_RU.Y - p_LO.Y;
        //        p_RU.X = (float)((float)p_LO.X + (float)x * sc);
        //        p_RU.Y = (float)((float)p_LO.Y + (float)y * sc);
        //        KeepInternalLogic();
        //        return;
        //    }
        //    Hintergrund_weiß_füllen = Tags.TagGet("Hintergrund weiß füllen").FromPlusMinus();
        //    Bild_Modus = (enSizeModes)IntParse(Tags.TagGet("Bild-Modus"));
        //    Stil = (PadStyles)IntParse(Tags.TagGet("Umrandung"));
        //    Platzhalter_für_Layout = Tags.TagGet("Platzhalter für Layout").FromNonCritical();
        //}
    }
}