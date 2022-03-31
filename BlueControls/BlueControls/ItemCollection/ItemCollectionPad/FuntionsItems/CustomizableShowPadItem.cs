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
using System.Windows.Forms;

namespace BlueControls.ItemCollection {

    public abstract class CustomizableShowPadItem : RectanglePadItem, IItemToControl {

        #region Fields

        public static BlueFont? CaptionFNT = Skin.GetBlueFont(Design.Caption, States.Standard);

        private ICalculateRowsItemLevel? _GetValueFrom = null;

        #endregion

        #region Constructors

        public CustomizableShowPadItem(string internalname) : base(internalname) {
            SetCoordinates(new RectangleF(0, 0, 50, 30), true);
        }

        #endregion

        #region Properties

        public ICalculateRowsItemLevel? GetRowFrom {
            get => _GetValueFrom;

            set {
                if (value == _GetValueFrom) { return; }
                _GetValueFrom = value;
                RepairConnections();
                OnChanged();
            }
        }

        protected override int SaveOrder => 3;

        private string Breite_Berechnen {
            get => string.Empty;
            set {
                var li = new ItemCollectionList.ItemCollectionList();
                for (var br = 1; br <= 20; br++) {
                    li.Add(br + " Spalte(n)", br.ToString(), true, string.Empty);

                    for (var pos = 1; pos <= br; pos++) {
                        li.Add(br + " Spalte(n) - Position: " + pos, br + ";" + pos);
                    }
                }

                var x2 = Forms.InputBoxListBoxStyle.Show("Bitte Breite und Position wählen:", li, AddType.None, true);

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

        private string Datenbank {
            get {
                if (GetRowFrom?.Database == null) { return "?"; }

                return GetRowFrom.Database.Filename.FileNameWithSuffix();
            }
        }

        [Description("Wählt ein Zeilen-Objekt, aus der die Werte kommen.")]
        private string Datenquelle_wählen {
            get => string.Empty;
            set {
                var x = new ItemCollectionList.ItemCollectionList();
                foreach (var thisR in Parent) {
                    if (thisR is ICalculateRowsItemLevel rfp) {
                        x.Add(rfp, thisR.Internal);
                    }
                }

                x.Add("<Keine Quelle>");

                var it = Forms.InputBoxListBoxStyle.Show("Quelle wählen:", x, AddType.None, true);

                if (it == null || it.Count != 1) { return; }

                var t = Parent[it[0]];

                if (t is ICalculateRowsItemLevel rfp2) {
                    if (rfp2 != GetRowFrom) {
                        GetRowFrom = rfp2;
                    }
                } else {
                    GetRowFrom = null;
                }

                OnChanged();
            }
        }

        private string Standardhöhe {
            get => string.Empty;
            set {
                var x = UsedArea;
                x.Height = MmToPixel(ConnectedFormula.ConnectedFormula.StandardHöhe, 300);
                SetCoordinates(x, true);

                //OnChanged();
            }
        }

        #endregion

        #region Methods

        public abstract Control? CreateControl(ConnectedFormulaView parent);

        public void DrawFakeControl(RectangleF positionModified, float zoom, ÜberschriftAnordnung CaptionPosition, Graphics gr, string captiontxt) {
            Point cap;
            var uc = positionModified.ToRect();

            switch (CaptionPosition) {
                case ÜberschriftAnordnung.ohne:
                    cap = new Point(-1, -1);
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
                Skin.Draw_FormatedText(gr, captiontxt + ":", null, Alignment.Top_Left, e.ToRect(), CaptionFNT.Scale(zoom), true);
            }

            if (uc.Width > 0 && uc.Height > 0) {
                gr.DrawRectangle(new Pen(Color.Black, zoom), uc);
            }
        }

        public override List<GenericControl> GetStyleOptions() {
            List<GenericControl> l = new();
            l.Add(new FlexiControlForProperty<string>(() => Datenquelle_wählen, ImageCode.Pfeil_Rechts));
            l.Add(new FlexiControlForProperty<string>(() => Datenbank));

            l.Add(new FlexiControl());

            l.Add(new FlexiControlForProperty<string>(() => Standardhöhe, ImageCode.GrößeÄndern));
            l.Add(new FlexiControlForProperty<string>(() => Breite_Berechnen, ImageCode.GrößeÄndern));

            return l;
        }

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "getvaluefrom":
                    GetRowFrom = (ICalculateRowsItemLevel)Parent[value.FromNonCritical()];
                    return true;
            }
            return false;
        }

        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            if (GetRowFrom != null) {
                t = t + "GetValueFrom=" + GetRowFrom.Internal.ToNonCritical() + ", ";
            }

            return t.Trim(", ") + "}";
        }

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
    }
}