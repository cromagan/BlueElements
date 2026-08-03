// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.EventArgs;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using static BlueBasics.ClassesStatic.IO;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls.ConnectedFormula;

public sealed class ConnectedFormula : BlockableFile, ICreateByKey<ConnectedFormula>, IEditable, IReadableTextWithKey, IParseable, IJsonParseable, INotifyPropertyChanged {

    #region Fields

    private static readonly object _lock = new();

    private static List<string>? _visibleFor_AllUsed;

    private readonly List<string> _notAllowedChilds = [];

    private readonly List<ItemCollectionPadItem> _pages = [];

    private bool _finishingParse;

    #endregion

    #region Constructors

    internal ConnectedFormula(string filename) : base(filename) {
        Invalidate();
    }

    #endregion

    #region Events

    /// <summary>
    /// Ereignis, das beim Bearbeiten der Datei ausgelöst wird.
    /// </summary>
    public event EventHandler<EditingEventArgs>? Editing;

    /// <summary>
    /// Ereignis, das bei Eigenschaftsänderungen ausgelöst wird.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<JsonPathChangedEventArgs>? PropertyChangedExt;

    #endregion

    #region Properties

    public static string Type => "ConnectedFormula";

    /// <summary>
    /// 0.50 seit 08.03.2024
    /// </summary>
    public static string Version => "0.50";

    public string CaptionForEditor => "Formular";

    /// <summary>
    /// Das Erstellungsdatum der Datei.
    /// </summary>
    public string CreateDate { get; private set; } = string.Empty;

    /// <summary>
    /// Der Ersteller der Datei.
    /// </summary>
    public string Creator { get; private set; } = string.Empty;

    public override bool ExtendedSave => true;

    /// <summary>
    /// Gibt an, ob die Klasse die Rohdaten bereits verarbeitet hat.
    /// Wird automatisch auf false gesetzt, wenn die Datei veraltet ist (Invalidate).
    /// </summary>
    public bool IsParsed { get; private set; }

    public override bool MustZipped => false;

    public ReadOnlyCollection<string> NotAllowedChilds {
        get => new(_notAllowedChilds);
        set {
            if (IsDisposed) { return; }
            var l = new List<string>(value).SortedDistinctList();
            if (!_notAllowedChilds.IsDifferentTo(l)) { return; }

            _notAllowedChilds.Clear();
            _notAllowedChilds.AddRange(l);
            OnPropertyChanged();
            OnPropertyChangedExt("notAllowedChilds", _notAllowedChilds);
        }
    }

    public IReadOnlyList<ItemCollectionPadItem> Pages {
        get {
            if (!IsParsed) {
                this.Parse(Constants.Win1252.GetString(Content));
            }
            return _pages.AsReadOnly();
        }
    }

    public string QuickInfo => string.Empty;

    #endregion

    #region Methods

    /// <summary>
    /// Liefert einen stabilen Snapshot aller lebenden
    /// ConnectedFormula-Instanzen. Überschreibt (per Method-Hiding) die geerbte
    /// <see cref="LiveInstanceCache{T}.AllInstances"/> (T = BlockableFile), die
    /// alle BlockableFile-Instanzen zurückgibt — hier wird auf den konkreten
    /// Typ ConnectedFormula gefiltert und bereits disposed Instanzen
    /// herausgefiltert (das geerbte Register wird nur asynchron bereinigt).
    /// </summary>
    public new static List<ConnectedFormula> AllInstances() {
        List<ConnectedFormula> result = [];
        foreach (var bf in LiveInstances.Values) {
            if (bf is ConnectedFormula { IsDisposed: false } cf) { result.Add(cf); }
        }
        return result;
    }

    /// <summary>
    /// Factory für <see cref="LiveInstanceCache{T}.GetOrCreate{TDerived}" />.
    /// Erzeugt eine neue ConnectedFormula-Instanz für den angegebenen Dateipfad.
    /// </summary>
    public static ConnectedFormula Create(string key) => new(key);

    /// <summary>
    /// Holt eine bestehende oder erzeugt eine neue ConnectedFormula-Instanz für
    /// den angegebenen Dateipfad. Race-Safe über das geerbte
    /// <see cref="LiveInstanceCache{T}.GetOrCreate"/> (T = BlockableFile).
    /// </summary>
    public static ConnectedFormula? Get(string filename) => GetOrCreate<ConnectedFormula>(filename);

    public static void Invalidate_VisibleFor_AllUsed() {
        lock (_lock) {
            _visibleFor_AllUsed = null;
        }
    }

    public static List<string> VisibleFor_AllUsed() {
        // Erster Check ohne Lock für die Performance (Double-Check Locking Prinzip)
        if (_visibleFor_AllUsed is not null) { return _visibleFor_AllUsed; }

        lock (_lock) {
            // Zweiter Check innerhalb des Locks, falls ein anderer Thread gerade fertig geworden ist
            if (_visibleFor_AllUsed is not null) { return _visibleFor_AllUsed; }

            List<string> tempResult = []; // Lokale Liste, um den Cache erst am Ende zu füllen

            foreach (var bf in LiveInstances.Values) {
                // LiveInstances ist BlockableFile-typisch (geerbt über
                // BlockableFile : LiveInstanceCache<BlockableFile>). Nur
                // ConnectedFormula-Instanzen besitzen Pages.
                if (bf is not ConnectedFormula { IsDisposed: false } thisCf) { continue; }
                foreach (var icp in thisCf._pages) {
                    if (icp is { IsDisposed: false }) {
                        tempResult.AddRange(icp.VisibleFor_AllUsed());
                    }
                }
            }

            _visibleFor_AllUsed = tempResult.SortedDistinctList();
            return _visibleFor_AllUsed;
        }
    }

    // Das Schloss für die Threadsicherheit

    public ItemCollectionPadItem AddPage(string headname) {
        // Ein Gitterkästchen in mm - konsistent zu ParseFinished.
        var gridMm = PixelToMm(AutosizableExtension.GridSize, ItemCollectionPadItem.Dpi);

        var p = new ItemCollectionPadItem {
            Caption = headname,
            Breite = 100 * gridMm,
            Höhe = 100 * gridMm,
            GridShow = gridMm,
            GridSnap = gridMm
        };

        var it = new RowEntryPadItem();
        p.Add(it);

        RegisterPage(p);
        _pages.Add(p);
        OnPropertyChanged();

        return p;
    }

    public List<string> AllPages() {
        var p = new List<string>();

        foreach (var thisp in _pages) {
            if (thisp is { IsDisposed: false, HasItems: true }) {
                p.AddIfNotExists(thisp.Caption);
            }
        }

        return p;
    }

    public override void Dispose() {
        if (IsDisposed) { return; }

        // Austragen aus dem Live-Register übernimmt BlockableFile.Dispose
        // (mit Race-Safety: nur wenn noch diese Instanz hinterlegt ist).
        Editing = null;
        PropertyChanged = null;
        PropertyChangedExt = null;

        base.Dispose();

        ClearPages();
    }

    public ItemCollectionPadItem? GetPage(string keyOrCaption) {
        if (!IsParsed) { this.Parse(Constants.Win1252.GetString(Content)); }

        foreach (var icp in _pages) {
            if (icp is not { IsDisposed: false }) { continue; }
            if (string.Equals(icp.KeyName, keyOrCaption, StringComparison.OrdinalIgnoreCase)) { return icp; }
            if (string.Equals(icp.Caption, keyOrCaption, StringComparison.OrdinalIgnoreCase)) { return icp; }
        }

        return null;
    }

    /// <summary>
    /// ConnectedFormula ist die Wurzel des Baums. Pfad-Abstiege über
    /// <see cref="JsonParseableExtension.ApplyPartialJson" /> erreichen diese Methode
    /// nicht, weil nach oben kein Partial-Json gepusht wird. Einzige Ausnahme ist der
    /// Container <c>pages</c>, über den eine einzelne Page anhand ihres KeyNames
    /// aufgelöst wird.
    /// </summary>
    public IJsonParseable? GetSubItemByKey(string containerName, string key) {
        if (string.Equals(containerName, "pages", StringComparison.OrdinalIgnoreCase)) {
            return GetPage(key);
        }
        return null;
    }

    public override void Invalidate() {
        ClearPages();
        IsParsed = false;
        base.Invalidate();
    }

    /// <summary>
    /// Löst das <see cref="PropertyChangedExt" />-Event aus. ConnectedFormula ist
    /// die Wurzel des Baums; Sub-Item-Änderungen aus dem <see cref="Pages" />-Baum
    /// werden über <see cref="Pages_PropertyChangedExt" /> hierher durchgereicht
    /// und zusammen mit <see cref="OnPropertyChanged" /> (das <see cref="BlockableFile" />
    /// via <c>MarkDirty</c> als ungespeichert markiert) weitergegeben.
    /// </summary>
    public void OnPropertyChangedExt(string relativePath, object? value) {
        if (IsDisposed) { return; }
        PropertyChangedExt?.Invoke(this, this.BuildSubItemEventArgs(relativePath, value));
    }

    /// <summary>
    /// Gibt die serialisierbaren Elemente zurück.
    /// </summary>
    public List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [];

        result.ParseableAdd("Type", Type);
        result.ParseableAdd("Version", Version);
        result.ParseableAdd("CreateDate", CreateDate);
        result.ParseableAdd("CreateName", Creator);

        result.ParseableAdd("NotAllowedChilds", _notAllowedChilds, false);

        foreach (var p in _pages) {
            if (!p.IsDisposed) {
                result.ParseableAdd("Page", p as IStringable);
            }
        }

        return result;
    }

    /// <summary>
    /// Neue JSON-basierte Serialisierung. Implementiert <see cref="IJsonStringable.ParseableJson" />
    /// und löst das alte <see cref="ParseableItems" /> langfristig ab.
    /// Schema-Vereinheitlichung gegenüber dem alten Format:
    /// <list type="bullet">
    ///   <item><description><c>CreateName</c> → <c>creator</c></description></item>
    ///   <item><description><c>CreateDate</c> → <c>createdate</c></description></item>
    ///   <item><description><c>NotAllowedChilds</c> als JSON-Array statt \r-getrennter String</description></item>
    ///   <item><description>Pages als flaches JSON-Array <c>pages</c> statt verschachteltem <c>page</c>-Container</description></item>
    /// </list>
    /// </summary>
    public JsonObject ParseableJson() {
        var json = new JsonObject()
            .Set("type", Type)
            .Set("version", Version)
            .Set("createdate", CreateDate)
            .Set("creator", Creator);

        json.SetArrayIfNotEmpty("notallowedchilds", _notAllowedChilds);

        json.SetArrayIfNotEmpty("pages", _pages.Where(p => !p.IsDisposed));

        return json;
    }

    /// <summary>
    /// Wird aufgerufen, wenn die Analyse abgeschlossen ist. Die Pages wurden
    /// bereits in <see cref="ParseThis" /> oder <see cref="ParseJson" /> direkt
    /// in <c>_pages</c> eingetragen. Hier erfolgen nur noch Standard-Sicherungen
    /// (Default-Head, RowEntryItem, GridShow/GridSnap) und die Receiver-Reparatur.
    /// </summary>
    public void ParseFinished(string parsed) {
        _finishingParse = true;
        IsParsed = true;

        try {

            #region Default-Head sicherstellen

            if (_pages.Count == 0) {
                AddPage("Head");
            }

            #endregion

            #region Pro Page: RowEntryItem sicherstellen, GridShow/GridSnap setzen

            var gridMm = PixelToMm(AutosizableExtension.GridSize, ItemCollectionPadItem.Dpi);

            foreach (var icpi in _pages) {
                if (icpi is not { IsDisposed: false }) { continue; }

                if (icpi.IsHead() || icpi.Any()) {
                    var found = icpi.GetRowEntryItem();

                    if (found is null) {
                        found = new RowEntryPadItem();
                        icpi.Add(found);
                    }

                    found.SetCoordinates(new RectangleF(icpi.CanvasUsedArea.Width / 2 - 150, -30, 300, 30));
                    found.Bei_Export_sichtbar = false;
                }

                icpi.GridShow = gridMm;
                icpi.GridSnap = gridMm;
            }

            #endregion

            RepairRecivers();
        } finally {
            _finishingParse = false;
        }

        // KEIN SetLoadedContent mit re-serialisierten ParseableItems: das würde
        // das Lazy-Loaden aller referenzierten Tabellen auslösen. Der Content
        // wurde bereits vom Dateisystem geladen und ist in _content aktuell —
        // lediglich die Hashes werden als "gespeichert" markiert.
        MarkCurrentContentAsLoaded();
    }

    public void ParseFinishedJson(JsonObject parsed) => ParseFinished(parsed.ToJsonString());

    public void ParseJson(JsonObject json) {
        CreateDate = json.GetString("createdate", CreateDate);
        Creator = json.GetString("creator", Creator);

        if (json["notallowedchilds"] is JsonArray na) {
            _notAllowedChilds.Clear();
            _notAllowedChilds.AddRange(na.ToStringList());
        }

        if (json["pages"] is JsonArray pa) {
            foreach (var pageJson in pa) {
                if (pageJson is not JsonObject po) { continue; }
                var page = new ItemCollectionPadItem();
                page.ParseJson(po);
                page.ParseFinishedJson(po);
                RegisterPage(page);
                _pages.Add(page);
            }
        }
    }

    /// <summary>
    /// Verarbeitet ein Schlüssel-Wert-Paar während der Analyse.
    /// </summary>
    public bool ParseThis(string key, string value) {
        switch (key) {
            case "type":
                return true;

            case "version":
                return true;

            case "createdate":
                CreateDate = value.FromNonCritical();
                return true;

            case "createname":
                Creator = value.FromNonCritical();
                return true;

            case "notallowedchilds":
                _notAllowedChilds.Clear();
                _notAllowedChilds.AddRange(value.FromNonCritical().SplitByCr());
                return true;

            case "page":
            case "paditemdata":
                var container = new ItemCollectionPadItem();
                container.Parse(value.FromNonCritical());

                // Altes Container-Format: direkte Kinder sind ausschließlich
                // ItemCollectionPadItem → als einzelne Pages übernehmen.
                // Sonst den Container selbst als eine Page behandeln.
                if (container.Any() && container.All(it => it is ItemCollectionPadItem)) {
                    foreach (var child in container.ToList()) {
                        if (child is ItemCollectionPadItem { IsDisposed: false } page) {
                            container.Remove(child);
                            RegisterPage(page);
                            _pages.Add(page);
                        }
                    }
                } else {
                    RegisterPage(container);
                    _pages.Add(container);
                }
                return true;

            case "databasefiles":
            case "tablefiles":
            case "lastusedid":
            case "events":
            case "variables":
                return true;
        }

        return false;
    }

    public override string ReadableText() {
        if (!string.IsNullOrWhiteSpace(Filename)) { return Filename.FileNameWithoutSuffix(); }

        return string.Empty;
    }

    public override QuickImage? SymbolForReadableText() => !string.IsNullOrWhiteSpace(Filename) ? QuickImage.Get(ImageCode.Diskette, 16) : QuickImage.Get(ImageCode.Warnung, 16);

    /// <summary>
    /// Gibt alle bekannten Fomulare zurück - außer die in notAllowedChilds
    /// </summary>
    internal List<AbstractListItem> AllKnownChilds(ReadOnlyCollection<string> notAllowedChilds) {
        List<AbstractListItem> list = [];

        if (FileExists(Filename)) {
            foreach (var thisf in GetFiles(Filename.FilePath(), "*.cfo", System.IO.SearchOption.TopDirectoryOnly)) {
                if (!notAllowedChilds.Contains(thisf)) {
                    list.Add(ItemOf(thisf.FileNameWithoutSuffix(), thisf, ImageCode.Diskette));
                }
            }
        }

        foreach (var thisf in LiveInstances.Values) {
            if (!notAllowedChilds.Contains(thisf.Filename)) {
                if (list.GetByKey(thisf.Filename) is null) {
                    list.Add(ItemOf(thisf.Filename.FileNameWithoutSuffix(), thisf.Filename, ImageCode.Diskette));
                }
            }
        }

        foreach (var icpi in _pages) {
            if (icpi is { IsDisposed: false, HasItems: true } && !notAllowedChilds.Contains(icpi.KeyName) && !icpi.IsHead()) {
                list.Add(ItemOf(icpi));
            }
        }

        return list;
    }

    internal bool IsEditing() {
        var e = new EditingEventArgs();

        OnEditing(e);

        return e.Editing;
    }

    protected override byte[]? BuildContent() {
        if (!IsParsed || IsDisposed) { return null; }
        return Constants.Win1252.GetBytes(ParseableItems().FinishParseable());
    }

    private void ClearPages() {
        foreach (var page in _pages) {
            UnregisterPage(page);
            page.Dispose();
        }
        _pages.Clear();
    }

    /// <summary>
    /// Ruft das Editing-Ereignis auf.
    /// </summary>
    private void OnEditing(EditingEventArgs e) => Editing?.Invoke(this, e);

    /// <summary>
    /// Ruft das PropertyChanged-Ereignis auf und markiert die Datei als ungespeichert.
    /// Der Inhalt wird NICHT sofort neu serialisiert, sondern erst beim nächsten
    /// Speichern über <see cref="BuildContent"/>.
    /// Ist die Datei durch einen anderen Prozess gesperrt (<see cref="BlockerMessage"/>),
    /// werden die Änderungen im Speicher übernommen, aber NICHT als Dirty markiert
    /// und kein Schreibzugriff versucht. Standard-Reparaturen (z.B. ShowAlways im
    /// Editor) sind nicht speicherrelevant.
    /// </summary>
    private void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") {
        if (IsDisposed) { return; }
        if (IsSaving || IsLoading || _finishingParse || !IsParsed) { return; }
        if (BlockerMessage() is { Length: > 0 }) { return; }

        if (AcquireWriteAccess() is { Length: > 0 } f) {
            Develop.DebugError($"Keine Änderungen an der Datei '{Filename.FileNameWithoutSuffix()}' möglich ({propertyName})! {f}");
            return;
        }

        MarkDirty();

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void Pages_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (IsDisposed) { return; }
        OnPropertyChanged(e.PropertyName ?? "unknown");
    }

    private void Pages_PropertyChangedExt(object? sender, JsonPathChangedEventArgs e) {
        if (IsDisposed) { return; }
        OnPropertyChanged();
        OnPropertyChangedExt(e.RelativePath, e.Partial);
    }

    private void RegisterPage(ItemCollectionPadItem page) {
        page.Parent = this;
        page.PropertyChangedExt += Pages_PropertyChangedExt;
        page.PropertyChanged += Pages_PropertyChanged;
    }

    private void RepairReciver(ItemCollectionPadItem icpi) {
        foreach (var thisIt in icpi) {
            if (thisIt is ItemCollectionPadItem { IsDisposed: false } icp2) {
                RepairReciver(icp2);
            }

            if (thisIt is ReciverControlPadItem itcf) {
                itcf.ParentFormula = this;
            }
        }
    }

    private void RepairRecivers() {
        foreach (var page in _pages) {
            if (page is { IsDisposed: false }) {
                RepairReciver(page);
            }
        }
    }

    private void UnregisterPage(ItemCollectionPadItem page) {
        page.PropertyChangedExt -= Pages_PropertyChangedExt;
        page.PropertyChanged -= Pages_PropertyChanged;
    }

    #endregion
}