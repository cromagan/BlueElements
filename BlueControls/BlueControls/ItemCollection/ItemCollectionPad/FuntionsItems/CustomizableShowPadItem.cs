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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static BlueBasics.Converter;
using BlueDatabase.Enums;
using BlueControls.Interfaces;
using System.Windows.Forms;
using BlueControls.Forms;
using System;

namespace BlueControls.ItemCollection;

/// <summary>
/// Standard für Objekte, die einen Datenbank/Zeilenbezug haben.
/// Stellt auch alle Methode breit, zum Einrichten der Breite und Benutzer-Sichtbarkeiten.
/// Nur Tabs, die ein solches Objekt haben, werden als anzeigewürdig gewertet
/// </summary>
public abstract class CustomizableShowPadItem : RectanglePadItemWithVersion, IItemToControl {

    #region Fields

    public static BlueFont? CaptionFnt = Skin.GetBlueFont(Design.Caption, States.Standard);

    public ListExt<string> VisibleFor = new();
    private ICalculateOneRowItemLevel? _getValueFrom;

    #endregion

    #region Constructors

    protected CustomizableShowPadItem(string internalname) : base(internalname) {
        SetCoordinates(new RectangleF(0, 0, 50, 30), true);
    }

    #endregion

    #region Properties

    public string Breite_berechnen {
        get => string.Empty;
        set {
            var li = new ItemCollectionList.ItemCollectionList();
            for (var br = 1; br <= 20; br++) {
                li.Add(br + " Spalte(n)", br.ToString(), true, string.Empty);

                for (var pos = 1; pos <= br; pos++) {
                    li.Add(br + " Spalte(n) - Position: " + pos, br + ";" + pos);
                }
            }

            var x2 = InputBoxListBoxStyle.Show("Bitte Breite und Position wählen:", li, AddType.None, true);

            if (x2 == null || x2.Count != 1) { return; }

            var doit = x2[0].SplitBy(";");

            var anzbr = IntParse(doit[0]);
            var npos = IntParse(doit[1]);
            SetXPosition(anzbr, npos, 1);
            RaiseVersion();
        }
    }

    public string Datenbank {
        get {
            if (GetRowFrom?.Database == null) { return "?"; }

            return GetRowFrom.Database.Filename.FileNameWithSuffix();
        }
    }

    [Description("Wählt ein Zeilen-Objekt, aus der die Werte kommen.")]
    public string Datenquelle_wählen {
        get => string.Empty;
        set {
            var x = new ItemCollectionList.ItemCollectionList();
            foreach (var thisR in Parent) {
                if (thisR.IsVisibleOnPage(Page) && thisR is ICalculateOneRowItemLevel rfp) {
                    x.Add(rfp, thisR.Internal);
                }
            }

            x.Add("<Keine Quelle>");

            var it = InputBoxListBoxStyle.Show("Quelle wählen:", x, AddType.None, true);

            if (it == null || it.Count != 1) { return; }

            var t = Parent[it[0]];

            if (t is ICalculateOneRowItemLevel rfp2) {
                if (rfp2 != GetRowFrom) {
                    GetRowFrom = rfp2;
                }
            } else {
                GetRowFrom = null;
            }
            RaiseVersion();
            OnChanged();
        }
    }

    public ICalculateOneRowItemLevel? GetRowFrom {
        get => _getValueFrom;

        set {
            if (value == _getValueFrom) { return; }
            _getValueFrom = value;
            RepairConnections();
            RaiseVersion();
            OnChanged();
        }
    }

    public string Sichtbarkeit {
        get => string.Empty;
        set {
            ItemCollectionList.ItemCollectionList aa = new();
            aa.AddRange(Permission_AllUsed());

            if (aa["#Administrator"] == null) { aa.Add("#Administrator"); }
            aa.Sort();
            aa.CheckBehavior = CheckBehavior.MultiSelection;
            aa.Check(VisibleFor, true);
            var b = InputBoxListBoxStyle.Show("Wählen sie, wer anzeigeberechtigt ist:", aa, AddType.Text, true);
            if (b == null) { return; }
            VisibleFor.Clear();

            VisibleFor.AddRange(b.ToArray());

            if (VisibleFor.Count > 1 && VisibleFor.Contains("#Everybody", false)) {
                VisibleFor.Clear();
                VisibleFor.Add("#Everybody");
            }

            if (VisibleFor.Count == 0) { VisibleFor.Add("#Administrator"); }
            RaiseVersion();
            OnChanged();
        }
    }

    public string Standardhöhe_setzen {
        get => string.Empty;
        set {
            var x = UsedArea;

            var he = MmToPixel(ConnectedFormula.ConnectedFormula.StandardHöhe, 300);
            var he1 = MmToPixel(1, 300);
            x.Height = (int)(x.Height / he) * he;
            x.Height = (int)(x.Height / he1 + 0.99) * he1;

            if (x.Height < he) { x.Height = he; }
            RaiseVersion();
            SetCoordinates(x, true);
        }
    }

    protected override int SaveOrder => 3;

    #endregion

    #region Methods

    public static void DrawFakeControl(Graphics gr, RectangleF positionModified, float zoom, ÜberschriftAnordnung captionPosition, string captiontxt) {
        Point cap;
        var uc = positionModified.ToRect();

        switch (captionPosition) {
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
            Skin.Draw_FormatedText(gr, captiontxt, null, Alignment.Top_Left, e.ToRect(), CaptionFnt.Scale(zoom), true);
        }

        if (uc.Width > 0 && uc.Height > 0) {
            gr.DrawRectangle(new Pen(Color.Black, zoom), uc);
        }
    }

    public virtual Control? CreateControl(ConnectedFormulaView parent) => throw new NotImplementedException();

    public override List<FlexiControl> GetStyleOptions() {
        List<FlexiControl> l = new();
        l.Add(new FlexiControlForProperty<string>(() => Datenquelle_wählen, ImageCode.Pfeil_Rechts));
        l.Add(new FlexiControlForProperty<string>(() => Datenbank));

        l.Add(new FlexiControl());
        l.Add(new FlexiControlForProperty<string>(() => Sichtbarkeit, ImageCode.Schild));
        l.Add(new FlexiControlForProperty<string>(() => Standardhöhe_setzen, ImageCode.GrößeÄndern));
        l.Add(new FlexiControlForProperty<string>(() => Breite_berechnen, ImageCode.GrößeÄndern));

        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "version":
                Version = IntParse(value);
                return true;

            case "getvaluefrom":
                GetRowFrom = (ICalculateOneRowItemLevel)Parent[value.FromNonCritical()];
                return true;

            case "visiblefor":
                VisibleFor.Clear();
                VisibleFor.AddRange((value.FromNonCritical()).SplitAndCutByCr());
                if (VisibleFor.Count == 0) { VisibleFor.Add("#Everybody"); }
                return true;
        }
        return false;
    }

    public void SetXPosition(int anzahlSpaltenImFormular, int aufXPosition, int breiteInspalten) {
        var x = UsedArea;
        x.Width = (Parent.SheetSizeInPix.Width - (MmToPixel(0.5f, 300) * (anzahlSpaltenImFormular - 1))) / anzahlSpaltenImFormular;
        x.X = x.Width * (aufXPosition - 1) + MmToPixel(0.5f, 300) * (aufXPosition - 1);
        SetCoordinates(x, true);
    }

    public override string ToString() {
        var t = base.ToString();
        t = t.Substring(0, t.Length - 1) + ", ";

        t = t + "Version=" + Version + ", ";

        if (VisibleFor.Count == 0) { VisibleFor.Add("#Everybody"); }

        t = t + "VisibleFor=" + (VisibleFor.JoinWithCr()).ToNonCritical() + ", ";

        if (GetRowFrom != null) {
            t = t + "GetValueFrom=" + GetRowFrom.Internal.ToNonCritical() + ", ";
        }

        return t.Trim(", ") + "}";
    }

    internal bool IsVisibleForMe(string? myGroup, string? myName) {
        if (myGroup == null || myName == null) { return false; }

        if (VisibleFor == null || VisibleFor.Count == 0 || VisibleFor.Contains("#Everybody", false)) { return true; }

        if (myGroup.Equals("#Administrator", StringComparison.OrdinalIgnoreCase)) { return true; }

        if (VisibleFor.Contains(myGroup, false)) { return true; }

        if (VisibleFor.Contains("#USER: " + myName, false)) { return true; }
        if (VisibleFor.Contains("#USER:" + myName, false)) { return true; }
        return false;
    }

    private List<string> Permission_AllUsed() {
        var l = new List<string>();
        foreach (var thisIt in Parent) {
            if (thisIt is CustomizableShowPadItem csi) {
                l.AddRange(csi.VisibleFor);
            }
        }

        l.Add("#Everybody");
        l.Add("#User: " + Generic.UserName());

        return l.SortedDistinctList();
    }

    private void RepairConnections() {
        ConnectsTo.Clear();

        if (GetRowFrom != null) {
            ConnectsTo.Add(new ItemConnection(ConnectionType.Top, true, (BasicPadItem)GetRowFrom, ConnectionType.Bottom, false, false));
        }
    }

    #endregion
}