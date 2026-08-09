// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueControls.Controls;
using System.Windows.Forms;

namespace BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;

public class EasyPicPadItem : ReciverControlPadItem, IItemToControl, IAutosizable {

    #region Constructors

    public EasyPicPadItem() : this(string.Empty, null) { }

    public EasyPicPadItem(string keyName, Controls.ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-EasyPic";

    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;
    public bool AutoSizeableHeight => true;

    public bool Bearbeitbar {
        get;

        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    [Description("Der Dateiname des Bildes, das angezeigt werden sollen.\r\nEs können Variablen aus dem Skript benutzt werden.\r\nDiese müssen im Format ~variable~ angegeben werden.")]
    public string Bild_Dateiname {
        get;

        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public override string Description => "Eine Bild-Anzeige,\r\nmit welchem der Benutzer interagieren kann.";
    public override bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;
    public override bool TableInputMustMatchOutputTable => false;
    protected override int SaveOrder => 4;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new EasyPic {
            OriginalText = Bild_Dateiname,
            Editable = Bearbeitbar
        };

        con.DoDefaultSettings(parent, this, mode);
        return con;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", widthOfControl, true),
            new FlexiControlForProperty<string>(() => Bild_Dateiname),
            new FlexiControlForProperty<bool>(() => Bearbeitbar),

        ];
        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("ImageName", Bild_Dateiname);
        result.ParseableAdd("Editable", Bearbeitbar);
        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("imagename", Bild_Dateiname);
        json.Set("editable", Bearbeitbar);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        BeginInit();
        try {
            Bild_Dateiname = json.GetString("imagename", Bild_Dateiname);
            Bearbeitbar = json.GetBool("editable", Bearbeitbar);
            base.ParseJson(json);
        } finally {
            EndInit();
        }
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "imagename":
                Bild_Dateiname = value.FromNonCritical();
                return true;

            case "editable":
                Bearbeitbar = value.FromPlusMinus();
                return true;
        }

        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Bild-Editor: ";

        return txt + TableInput?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Bild, 16, Color.Transparent, Skin.IdColor(InputColorId));

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        //var id = GetRowFrom?.OutputColorId ?? - 1;

        if (!forPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, true, true, false);
        }

        DrawFakeControl(gr, positionControl, zoom, CaptionPosition.Über_dem_Feld, "Bilddatei", EditTypeFormula.Listbox);

        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY, forPrinting);
        DrawArrorInput(gr, positionControl, zoom, forPrinting, InputColorId);
    }

    #endregion
}