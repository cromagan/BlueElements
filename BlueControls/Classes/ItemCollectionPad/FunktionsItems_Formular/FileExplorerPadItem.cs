// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueControls.Controls;
using System.Windows.Forms;

namespace BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;

public class FileExplorerPadItem : ReciverControlPadItem, IItemToControl, IAutosizable {

    #region Constructors

    public FileExplorerPadItem() : this(string.Empty, null) { }

    public FileExplorerPadItem(string keyName, Controls.ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-FileExplorer";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.One;
    public bool AutoSizeableHeight => true;

    [Description("Ob das Verzeichnis bei Bedarf erzeugt werden soll.")]
    public bool Bei_Bedarf_erzeugen {
        get;

        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public override string Description => "Ein Datei-Browser,\r\nmit welchem der Benutzer interagieren kann.";

    public string Filter {
        get;

        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public override bool InputMustBeOneRow => true;

    [Description("Wenn angewählt, wird bei einer Änderung des Pfades geprüft, ob das Vereichniss leer ist.\r\nIst das der Fall, wird es gelöscht.")]
    public bool Leere_Ordner_löschen {
        get;

        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    [Description("Bis zu diesem Pfad kann maximal zurück gegangen werden.\r\nEs können Variablen aus dem Skript benutzt werden.\r\nDiese müssen im Format ~variable~ angegeben werden.")]
    public string Mindest_Pfad {
        get;

        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public override bool MustBeInDrawingArea => true;

    [Description("Der Dateipfad, dessen Dateien angezeigt werden sollen.\r\nEs können Variablen aus dem Skript benutzt werden.\r\nDiese müssen im Format ~variable~ angegeben werden.")]
    public string Pfad {
        get;

        set {
            if (IsDisposed) { return; }
            if (value == field) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public override bool TableInputMustMatchOutputTable => false;
    protected override int SaveOrder => 4;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new FileBrowser {
            Var_Directory = Pfad,
            Var_DirectoryMin = Mindest_Pfad,
            Filter = Filter,
            CreateDir = Bei_Bedarf_erzeugen,
            DeleteDir = Leere_Ordner_löschen
        };
        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", widthOfControl, true),
            new FlexiControlForProperty<string>(() => Pfad),
            new FlexiControlForProperty<string>(() => Mindest_Pfad),
            new FlexiControlForProperty<string>(() => Filter),
            new FlexiControlForProperty<bool>(() => Bei_Bedarf_erzeugen),
            new FlexiControlForProperty<bool>(() => Leere_Ordner_löschen),
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Path", Pfad);
        result.ParseableAdd("PathMin", Mindest_Pfad);
        result.ParseableAdd("Filter", Filter);
        result.ParseableAdd("CreateDir", Bei_Bedarf_erzeugen);
        result.ParseableAdd("DeleteDir", Leere_Ordner_löschen);
        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("path", Pfad);
        json.Set("pathmin", Mindest_Pfad);
        json.Set("filter", Filter);
        json.Set("createdir", Bei_Bedarf_erzeugen);
        json.Set("deleteemptydir", Leere_Ordner_löschen);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        Pfad = json.GetString("path", Pfad);
        Mindest_Pfad = json.GetString("pathmin", Mindest_Pfad);
        Filter = json.GetString("filter", Filter);
        Bei_Bedarf_erzeugen = json.GetBool("createdir", Bei_Bedarf_erzeugen);
        Leere_Ordner_löschen = json.GetBool("deleteemptydir", Leere_Ordner_löschen);
        base.ParseJson(json);
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "path":
            case "pfad":
                Pfad = value.FromNonCritical();
                return true;

            case "filter":
                Filter = value.FromNonCritical();
                return true;

            case "pathmin":
                Mindest_Pfad = value.FromNonCritical();
                return true;

            case "createdir":
                Bei_Bedarf_erzeugen = value.FromPlusMinus();
                return true;

            case "deletedir":
                Leere_Ordner_löschen = value.FromPlusMinus();
                return true;
        }

        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Dateisystem: ";

        return txt + TableInput?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Ordner, 16, Color.Transparent, Skin.IdColor(InputColorId));

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        //var id = GetRowFrom?.OutputColorId ?? -1;

        if (!forPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, true, true, false);
        }

        DrawFakeControl(gr, positionControl, zoom, CaptionPosition.Über_dem_Feld, "C:\\", EditTypeFormula.Listbox);

        if (!forPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY, forPrinting);
        DrawArrorInput(gr, positionControl, zoom, forPrinting, InputColorId);
    }

    #endregion
}