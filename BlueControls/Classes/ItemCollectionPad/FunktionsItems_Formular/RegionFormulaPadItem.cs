// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueControls.Controls;
using BlueControls.Controls.ConnectedFormula;
using System.Windows.Forms;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Erzeut ein Unter-Element von ConnectedFormulaView
/// </summary>
public class RegionFormulaPadItem : ReciverControlPadItem, IItemToControl, IAutosizable {

    #region Constructors

    public RegionFormulaPadItem() : this(string.Empty, null) { }

    public RegionFormulaPadItem(string keyName, ConnectedFormula? cformula) : base(keyName, cformula) {
        ParentFormula?.PropertyChanged += ParentFormula_PropertyChanged;
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-RegionFormula";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.More;

    [DefaultValue(false)]
    public bool Ausklappbar {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool AutoSizeableHeight => true;

    public string Child {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public override string Description => "Ein Steuerelement, mit dem ein untergeordnetes Formular angezeigt werden kann.";
    public override bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;

    [DefaultValue(GroupBoxStyle.Normal)]
    public GroupBoxStyle RahmenStil {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = GroupBoxStyle.Normal;

    public override bool TableInputMustMatchOutputTable => false;
    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var icpi = GetChild(Child);

        var con = new ConnectedFormulaView(mode, icpi) {
            GroupBoxStyle = RahmenStil,
            Detachable = Ausklappbar
        };

        if (RahmenStil != GroupBoxStyle.Nothing) {
            con.Text = icpi?.BestCaption() ?? "?";
        }

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        if (string.IsNullOrEmpty(Child)) {
            return "Keine Formular gewählt.";
        }

        return base.ErrorReason();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        var cl = ParentFormula?.AllKnownChilds(ParentFormula.NotAllowedChilds);

        List<GenericControl> result =
            [.. base.GetProperties(widthOfControl),
                new FlexiControl("Einstellungen:", widthOfControl, true),
                new FlexiControlForProperty<string>(() => Child, cl),

                new FlexiControlForProperty<GroupBoxStyle>(() => RahmenStil,ItemsOf(typeof(GroupBoxStyle)) ),
                new FlexiControlForProperty<bool>(() => Ausklappbar)
            ];

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Parent", ParentFormula?.Filename ?? string.Empty);
        result.ParseableAdd("Child", Child);
        result.ParseableAdd("BorderStyle", RahmenStil);
        result.ParseableAdd("Detachable", Ausklappbar);
        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("parent", ParentFormula?.Filename ?? string.Empty);
        json.Set("child", Child);
        json.Set("borderstyle", (int)RahmenStil);
        json.Set("detachable", Ausklappbar);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        var parent = json.GetString("parent");
        if (parent is { Length: > 0 }) {
            ParentFormula = LiveInstanceCacheHelper.GetLiveInstance<ConnectedFormula>(parent);
            ParentFormula?.PropertyChanged += ParentFormula_PropertyChanged;
        }
        Child = json.GetString("child", Child);
        RahmenStil = json.GetEnum("borderstyle", RahmenStil);
        Ausklappbar = json.GetBool("detachable", Ausklappbar);
        base.ParseJson(json);
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "parent":
                ParentFormula = LiveInstanceCacheHelper.GetLiveInstance<ConnectedFormula>(value.FromNonCritical());
                ParentFormula?.PropertyChanged += ParentFormula_PropertyChanged;
                return true;

            case "child":
                Child = value.FromNonCritical();
                return true;

            case "borderstyle":
                RahmenStil = (GroupBoxStyle)IntParse(value);
                return true;

            case "detachable":
                Ausklappbar = value.FromPlusMinus();
                return true;

            case "style":
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Unterformular: ";

        return txt + TableInput?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Groupbox);

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if (disposing) {
            ParentFormula?.PropertyChanged -= ParentFormula_PropertyChanged;
        }
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        DrawColorScheme(gr, positionControl, zoom, null, false, false, false);

        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY, forPrinting);

        DrawArrorInput(gr, positionControl, zoom, forPrinting, InputColorId);
    }

    private void ParentFormula_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (IsDisposed) { return; }
        if (ParentFormula is null) { return; }

        if (ParentFormula.NotAllowedChilds.Contains(Child)) {
            Child = string.Empty;
        }

        //OnPropertyChanged(string propertyname);
    }

    #endregion
}