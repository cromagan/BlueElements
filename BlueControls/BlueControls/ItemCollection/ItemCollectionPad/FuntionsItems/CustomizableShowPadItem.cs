﻿// Authors:
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;

using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;
using System;

using System.Collections.Generic;
using System.Drawing;

using BlueControls.ItemCollection.ItemCollectionList;

using static BlueBasics.Converter;

using BlueBasics.EventArgs;

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueDatabase.Enums;

using BlueDatabase.EventArgs;

using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static BlueBasics.FileOperations;

using static BlueBasics.Converter;

using BlueScript.Variables;

namespace BlueControls.ItemCollection;

/// <summary>
/// Standard für Objekte, die einen Datenbank/Zeilenbezug haben.
/// Stellt auch alle Methode breit, zum Einrichten der Breite und Benutzer-Sichtbarkeiten.
/// Nur Tabs, die ein solches Objekt habe, werden als anzeigewürdig gewertet
/// </summary>
public abstract class CustomizableShowPadItem : RectanglePadItem, IItemToControl {

    #region Fields

    public static BlueFont? CaptionFnt = Skin.GetBlueFont(Design.Caption, States.Standard);

    public ListExt<string> VisibleFor = new();
    private ICalculateOneRowItemLevel? _getValueFrom = null;

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

            var it = Forms.InputBoxListBoxStyle.Show("Quelle wählen:", x, AddType.None, true);

            if (it == null || it.Count != 1) { return; }

            var t = Parent[it[0]];

            if (t is ICalculateOneRowItemLevel rfp2) {
                if (rfp2 != GetRowFrom) {
                    GetRowFrom = rfp2;
                }
            } else {
                GetRowFrom = null;
            }

            OnChanged();
        }
    }

    public ICalculateOneRowItemLevel? GetRowFrom {
        get => _getValueFrom;

        set {
            if (value == _getValueFrom) { return; }
            _getValueFrom = value;
            RepairConnections();
            OnChanged();
        }
    }

    public string Sichtbarkeit {
        get => string.Empty;
        set {
            ItemCollectionList.ItemCollectionList aa = new();
            aa.AddRange(Permission_AllUsed());
            aa.Sort();
            aa.CheckBehavior = CheckBehavior.MultiSelection;
            aa.Check(VisibleFor, true);
            var b = InputBoxListBoxStyle.Show("Wählen sie, wer anzeigeberechtigt ist:", aa, AddType.Text, true);
            if (b == null) { return; }
            VisibleFor.Clear();
            VisibleFor.AddRange(b.ToArray());

            if (VisibleFor.Count > 1 && VisibleFor.Contains("#Everybody", false)) {
                VisibleFor.Clear();
            }

            if (VisibleFor.Count == 0) {
                VisibleFor.Add("#Everybody");
            }

            OnChanged();
        }
    }

    public string Standardhöhe_setzen {
        get => string.Empty;
        set {
            var x = UsedArea;
            var he = MmToPixel(ConnectedFormula.ConnectedFormula.StandardHöhe, 300);
            x.Height = x.Height % he;
            if (x.Height < he) { x.Height = he; }
            SetCoordinates(x, true);

            //OnChanged();
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

    public abstract Control? CreateControl(ConnectedFormulaView parent);

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();
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
            case "getvaluefrom":
                GetRowFrom = (ICalculateOneRowItemLevel)Parent[value.FromNonCritical()];
                return true;

            case "visiblefor":
                VisibleFor.Clear();
                VisibleFor.AddRange((value.FromNonCritical()).SplitAndCutByCr());
                return true;
        }
        return false;
    }

    public override string ToString() {
        var t = base.ToString();
        t = t.Substring(0, t.Length - 1) + ", ";

        if (VisibleFor.Count > 0) {
            t = t + "VisibleFor=" + (VisibleFor.JoinWithCr()).ToNonCritical() + ", ";
        }

        if (GetRowFrom != null) {
            t = t + "GetValueFrom=" + GetRowFrom.Internal.ToNonCritical() + ", ";
        }

        return t.Trim(", ") + "}";
    }

    private List<string> Permission_AllUsed() {
        var l = new List<string>();
        foreach (var thisIt in Parent) {
            if (thisIt is CustomizableShowPadItem csi) {
                l.AddRange(csi.VisibleFor);
            }
        }

        l.Add("#Everybody");
        l.Add("#User: " + BlueBasics.Generic.UserName());

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