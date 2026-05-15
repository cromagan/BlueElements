// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueControls.Controls;
using System.Windows.Forms;

namespace BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;

public class MonitorPadItem : ReciverControlPadItem, IItemToControl, IAutosizable {

    #region Constructors

    public MonitorPadItem() : this(string.Empty, null) { }

    public MonitorPadItem(string keyName, Controls.ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) => SetCoordinates(new RectangleF(0, 0, 50, 30));

    #endregion

    #region Properties

    public static string ClassId => "FI-Monitor";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;
    public bool AutoSizeableHeight => true;

    public override string Description => "Zeigt Änderungen einer Zeile an.";
    public override bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;
    public override bool TableInputMustMatchOutputTable => false;
    protected override int SaveOrder => 5;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new Monitor();
        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        //result.ParseableAdd("Pfad", _pfad);
        //result.ParseableAdd("CreateDir", _bei_Bedarf_Erzeugen);
        //result.ParseableAdd("DeleteDir", _leere_Ordner_Löschen);
        return result;
    }

    public override string ReadableText() {
        const string txt = "Monitor: ";

        return txt + TableInput?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Monitor, 16);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        //var id = GetRowFrom?.OutputColorId ?? -1;

        if (!forPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, true, true, false);
        }

        //DrawFakeControl(gr, positionControl, zoom, CaptionPosition.Über_dem_Feld, "Monitor", EditTypeFormula.Listbox);

        if (!forPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY, forPrinting);
        DrawArrorInput(gr, positionControl, zoom, forPrinting, InputColorId);
    }

    #endregion
}