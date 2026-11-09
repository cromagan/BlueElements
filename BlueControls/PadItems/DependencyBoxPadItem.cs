// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.PadItems.Abstract;

namespace BlueControls.PadItems;

/// <summary>
/// Eine Box des Abhängigkeits-Graphen: Formular, Formular-Element, Tabelle, Skript oder Spalte.
/// </summary>
public sealed class DependencyBoxPadItem : PadItem {

    #region Fields

    private static readonly BlueFont BoxFont = Skin.GetBlueFont(Design.Caption, States.Standard);
    private readonly PointM _punktObenLinks;

    private readonly PointM _punktUntenRechts;

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt die Box für den Knoten an der angegebenen Position.
    /// </summary>
    public DependencyBoxPadItem(DependencyOverviewForm.Node node, RectangleF position) : base(node.Key) {
        BeginInit();

        try {
            Node = node;
            _punktObenLinks = new PointM(this, "Oben Links", position.Left, position.Top);
            _punktUntenRechts = new PointM(this, "Unten Rechts", position.Right, position.Bottom);
            MovablePoint.Add(_punktObenLinks);
            MovablePoint.Add(_punktUntenRechts);
            PointsForSuccessfullyMove.AddRange(MovablePoint);
            CalculateJointMiddle(_punktObenLinks, _punktUntenRechts);
        } finally {
            EndInit();
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Der dargestellte Knoten des Abhängigkeits-Graphen.
    /// </summary>
    public DependencyOverviewForm.Node Node { get; }

    protected override int SaveOrder => 50;

    #endregion

    #region Methods

    public override void InitialPosition(int x, int y, int width, int height) {
        _punktObenLinks.SetTo(x, y, false);
        _punktUntenRechts.SetTo(x + width, y + height, false);
    }

    public override string ReadableText() => Node.Caption;

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(SymbolFür(Node), 16);

    protected override RectangleF CalculateCanvasUsedArea() => new((float)_punktObenLinks.X, (float)_punktObenLinks.Y, (float)(_punktUntenRechts.X - _punktObenLinks.X), (float)(_punktUntenRechts.Y - _punktObenLinks.Y));

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        if (!Node.Geladen) {
            var penStrich = new Pen(Color.FromArgb(150, 150, 150), 1f) { DashStyle = DashStyle.Dash };
            gr.FillRectangle(Brushes.WhiteSmoke, positionControl);
            gr.DrawRectangle(penStrich, positionControl);
        } else {
            var pen = new Pen(KnotenFarbe(Node.Art), Math.Max(1f, 2f * zoom));
            gr.FillRectangle(Brushes.White, positionControl);
            gr.DrawRectangle(pen, positionControl);
        }

        if (zoom < 0.3) { return; }

        var q = QuickImage.Get(SymbolFür(Node), 16);
        var qBmp = (Bitmap)q;
        lock (qBmp) {
            gr.DrawImage(qBmp, positionControl.Left + 8, positionControl.Top + (positionControl.Height - 16) / 2);
        }

        var f = BoxFont.Scale(zoom);
        var fKlein = BoxFont.Scale(zoom * 0.8f);
        var tx = positionControl.Left + 30;
        gr.FillRectangle(Brushes.White, new RectangleF(tx, positionControl.Top + 4, positionControl.Width - 34, positionControl.Height - 8));
        f.DrawString(gr, Node.Caption, 1f, tx, positionControl.Top + 6);
        fKlein.DrawString(gr, Node.Info, 1f, tx, positionControl.Top + 6 + f.Size * 1.2f);
    }

    /// <summary>
    /// Die Randfarbe einer Box je Knoten-Art.
    /// </summary>
    private static Color KnotenFarbe(DependencyOverviewForm.NodeArt art) => art switch {
        DependencyOverviewForm.NodeArt.Formular => Color.FromArgb(70, 124, 60),
        DependencyOverviewForm.NodeArt.Element => Color.FromArgb(126, 84, 168),
        DependencyOverviewForm.NodeArt.Tabelle => Color.FromArgb(52, 96, 176),
        DependencyOverviewForm.NodeArt.Skript => Color.FromArgb(110, 110, 110),
        _ => Color.FromArgb(64, 110, 190)
    };

    /// <summary>
    /// Das Symbol einer Box je Knoten-Art; Elemente bekommen das Symbol ihres Typs.
    /// </summary>
    private static ImageCode SymbolFür(DependencyOverviewForm.Node knoten) {
        switch (knoten.Art) {
            case DependencyOverviewForm.NodeArt.Formular:
                return ImageCode.Anwendung;

            case DependencyOverviewForm.NodeArt.Tabelle:
                return ImageCode.Tabelle;

            case DependencyOverviewForm.NodeArt.Skript:
                return ImageCode.Skript;

            case DependencyOverviewForm.NodeArt.Spalte:
                return ImageCode.Spalte;
        }

        switch (knoten.Info) {
            case "EditField":
                return ImageCode.Spalte;

            case "TableView":
                return ImageCode.Tabelle;

            case "ScriptButton":
                return ImageCode.Skript;

            case "Timer":
                return ImageCode.Uhr;

            case "RowAdder":
                return ImageCode.Zeile;

            case "FilterConverter":
            case "OutputFilter":
                return ImageCode.Trichter;

            case "TabFormula":
            case "DropDownSelectRow":
                return ImageCode.Register;

            case "Monitor":
                return ImageCode.Monitor;

            case "EasyText":
                return ImageCode.Textfeld;

            case "EasyPic":
                return ImageCode.Bild;

            case "FileExplorer":
                return ImageCode.Ordner;

            case "RegionFormula":
                return ImageCode.Groupbox;

            default:
                return ImageCode.Anwendung;
        }
    }

    #endregion
}
