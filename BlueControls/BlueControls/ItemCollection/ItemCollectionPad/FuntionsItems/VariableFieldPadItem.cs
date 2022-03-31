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

    public class VariableFieldPadItem : CustomizableShowPadItem, IReadableText, IAcceptAndSends, IContentHolder, IItemToControl {

        #region Fields

        private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld;
        private ÜberschriftAnordnung _überschiftanordung = ÜberschriftAnordnung.Über_dem_Feld;
        private string _Überschrift = string.Empty;
        private string _variable = string.Empty;

        #endregion

        #region Constructors

        public VariableFieldPadItem(string internalname) : base(internalname) { }

        #endregion

        #region Properties

        public ÜberschriftAnordnung CaptionPosition {
            get => _überschiftanordung;
            set {
                if (_überschiftanordung == value) { return; }
                _überschiftanordung = value;
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

        public string Überschrift {
            get => _Überschrift;
            set {
                if (_Überschrift == value) { return; }
                _Überschrift = value;
                OnChanged();
            }
        }

        [Description("Es können Variablen aus dem Skript benutzt werden.\r\nDiese müssen im Format ~variable~ angegeben werden.")]
        public string Variable {
            get => _variable;
            set {
                if (_variable == value) { return; }
                _variable = value;
                OnChanged();
            }
        }

        #endregion

        #region Methods

        public override System.Windows.Forms.Control? CreateControl(ConnectedFormulaView parent) {
            var c = new FlexiControl();
            c.Caption = Überschrift + ":";
            c.EditType = EditType;
            c.CaptionPosition = CaptionPosition;
            c.Tag = Internal;
            c.OriginalText = _variable;
            if (GetRowFrom is ICalculateRowsItemLevel rfw2 && rfw2.Genau_eine_Zeile) {
                var ff = parent.SearchOrGenerate((BasicPadItem)rfw2);

                if (ff is ICalculateRowsControlLevel cc) { cc.Childs.Add(c); }
                c.DisabledReason = "Dieser Wert ist nur eine Anzeige.";
            } else {
                c.DisabledReason = "Keine gültige Verknüpfung";
            }

            return c;
        }

        public override List<GenericControl> GetStyleOptions() {
            List<GenericControl> l = new();
            l.AddRange(base.GetStyleOptions());

            l.Add(new FlexiControlForProperty<String>(() => Überschrift));
            l.Add(new FlexiControlForProperty<String>(() => Variable));

            var u = new ItemCollection.ItemCollectionList.ItemCollectionList();
            u.AddRange(typeof(ÜberschriftAnordnung));
            l.Add(new FlexiControlForProperty<ÜberschriftAnordnung>(() => CaptionPosition, u));
            var b = new ItemCollection.ItemCollectionList.ItemCollectionList();
            b.AddRange(typeof(EditTypeFormula));
            l.Add(new FlexiControlForProperty<EditTypeFormula>(() => EditType, b));
            l.Add(new FlexiControl());

            return l;
        }

        public bool IsRecursiveWith(IAcceptAndSends obj) {
            if (obj == this) { return true; }

            if (GetRowFrom is IAcceptAndSends i) { return i.IsRecursiveWith(obj); }
            return false;
        }

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }

            switch (tag) {
                case "edittype":
                    _bearbeitung = (EditTypeFormula)IntParse(value);
                    return true;

                case "caption":
                    _überschiftanordung = (ÜberschriftAnordnung)IntParse(value);
                    return true;

                case "captiontext":
                    _Überschrift = value.FromNonCritical();
                    return true;

                case "variable":
                    _variable = value.FromNonCritical();
                    return true;
            }
            return false;
        }

        public string ReadableText() {
            return "Wert aus: " + Überschrift;
        }

        public QuickImage? SymbolForReadableText() {
            if (GetRowFrom == null) { return null; }

            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IDColor(GetRowFrom.Id));
        }

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            t = t + "CaptionText=" + _Überschrift.ToNonCritical() + ", ";
            t = t + "Variable=" + _variable.ToNonCritical() + ", ";

            t = t + "EditType=" + ((int)_bearbeitung).ToString() + ", ";
            t = t + "Caption=" + ((int)_überschiftanordung).ToString() + ", ";

            return t.Trim(", ") + "}";
        }

        protected override string ClassId() => "FI-VariableField";

        protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
            var id = -1; if (GetRowFrom != null) { id = GetRowFrom.Id; }

            if (!forPrinting) {
                DrawColorScheme(gr, positionModified, zoom, id);
            }

            if (GetRowFrom == null) {
                Skin.Draw_FormatedText(gr, "Datenquelle fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFNT.Scale(zoom), true);
            } else {
                base.DrawFakeControl(positionModified, zoom, CaptionPosition, gr, Überschrift);
            }

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
                return new VariableFieldPadItem(name);
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
        //            if (MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
        //        }
        //        Bitmap = ScreenShot.GrabArea(null, 2000, 2000).Pic;
        //        return;
        //    }
        //    if (Tags.TagGet("Datei laden").FromPlusMinus())
        //    {
        //        CloseMenu = false;
        //        if (Bitmap != null)
        //        {
        //            if (MessageBox.Show("Vorhandenes Bild überschreiben?", ImageCode.Warnung, "Ja", "Nein") != 0) { return; }
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
        //        Bitmap = (Bitmap)Generic.Image_FromFile(e.FileName);
        //        return;
        //    }
        //    if (Tags.TagGet("Skalieren").FromPlusMinus())
        //    {
        //        CloseMenu = false;
        //        var t = InputBox.Show("Skalierfaktor oder Formel eingeben:", "1", DataFormat.Text);
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