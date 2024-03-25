// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueBasics.MultiUserFile;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using static BlueBasics.IO;
using BlueControls.ItemCollectionList;

namespace BlueControls.ConnectedFormula;

public sealed class ConnectedFormula : IPropertyChangedFeedback, IDisposableExtended, IHasKeyName, ICanDropMessages {

    #region Fields

    public const float StandardHöhe = 1.75f;
    public const string Version = "0.50"; // 0.50 seit 08.03.2024

    public static readonly ObservableCollection<ConnectedFormula> AllFiles = [];
    private readonly List<string> _databaseFiles = [];

    private readonly List<string> _notAllowedChilds = [];

    private string _createDate;

    private string _creator;

    private int _id = -1;

    private string _loadedVersion = "0.00";

    private MultiUserFile? _muf;

    private ItemCollectionPad.ItemCollectionPad? _padData;

    private bool _saved;

    private bool _saving;

    #endregion

    #region Constructors

    public ConnectedFormula() : this(string.Empty) { }

    private ConnectedFormula(string filename) {
        AllFiles.Add(this);
        _muf = new MultiUserFile();

        //_muf.ConnectedControlsStopAllWorking += OnConnectedControlsStopAllWorking;
        _muf.Loaded += OnLoaded;
        _muf.Loading += OnLoading;
        _muf.SavedToDisk += OnSavedToDisk;
        _muf.DiscardPendingChanges += DiscardPendingChanges;
        _muf.HasPendingChanges += HasPendingChanges;
        _muf.ParseExternal += ParseExternal;
        _muf.ToListOfByte += ToListOfByte;
        _muf.Saving += _muf_Saving;
        _createDate = DateTime.UtcNow.ToString5();
        _creator = UserName;
        PadData = [];

        if (FileExists(filename)) {
            _muf.Load(filename, true);
        }

        if (_padData != null) {
            //_padData.SheetSizeInMm = new SizeF(PixelToMm(500, ItemCollectionPad.Dpi), PixelToMm(850, ItemCollectionPad.Dpi));
            _padData.GridShow = PixelToMm(AutosizableExtension.GridSize, ItemCollectionPad.ItemCollectionPad.Dpi);
            _padData.GridSnap = PixelToMm(AutosizableExtension.GridSize, ItemCollectionPad.ItemCollectionPad.Dpi);
        }
        Repair();
        _saved = true;
    }

    #endregion

    #region Destructors

    ~ConnectedFormula() { Dispose(disposing: false); }

    #endregion

    #region Events

    public event EventHandler<MessageEventArgs>? DropMessage;

    public event EventHandler<EditingEventArgs>? Editing;

    public event EventHandler? Loaded;

    public event EventHandler? Loading;

    public event EventHandler? NotAllowedChildsChanged;

    public event EventHandler? PropertyChanged;

    public event EventHandler? SavedToDisk;

    #endregion

    #region Properties

    [DefaultValue(true)]
    public bool DropMessages => true;

    public string Filename => _muf?.Filename ?? string.Empty;

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~ConnectedFormula()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
    public bool IsDisposed { get; private set; }

    public string KeyName => Filename;

    public ReadOnlyCollection<string> NotAllowedChilds {
        get => new(_notAllowedChilds);
        set {
            var l = new List<string>(value).SortedDistinctList();
            if (_notAllowedChilds.JoinWithCr() == l.JoinWithCr()) { return; }
            _notAllowedChilds.Clear();
            _notAllowedChilds.AddRange(l);
            _saved = false;
            OnNotAllowedChildsChanged();
        }
    }

    public ItemCollectionPad.ItemCollectionPad? PadData {
        get => _padData;
        private set {
            if (_padData == value) { return; }

            if (_padData != null) {
                _padData.PropertyChanged -= PadData_PropertyChanged;
            }
            _padData = value;
            if (_padData != null) {
                _padData.PropertyChanged += PadData_PropertyChanged;
            }

            if (_saving || (_muf?.IsLoading ?? false)) { return; }

            _saved = false;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gibt das Formular zurück.
    /// Zuerst wird geprüft, ob es bereits geladen ist. Falls nicht, wird es geladen.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static ConnectedFormula? GetByFilename(string filename) {
        if (string.IsNullOrEmpty(filename)) { return null; }

        foreach (var thisFile in AllFiles) {
            if (thisFile != null && string.Equals(thisFile.Filename, filename, StringComparison.OrdinalIgnoreCase)) {
                return thisFile;
            }
        }

        return !FileExists(filename) ? null : new ConnectedFormula(filename);
    }

    public static List<RectangleF> ResizeControls(List<IAutosizable> its, float newWidth, float newHeight, float currentWidth, float currentHeight) {
        var scaleY = newHeight / currentHeight;
        var scaleX = newWidth / currentWidth;

        #region Alle Items an die neue gedachte Y-Position schieben (newY), neue bevorzugte Höhe berechnen (newH), und auch newX und newW

        List<float> newX = [];
        List<float> newW = [];
        List<float> newY = [];
        List<float> newH = [];
        foreach (var thisIt in its) {

            #region  newY

            newY.Add(thisIt.UsedArea.Y * scaleY);

            #endregion

            #region  newX

            newX.Add(thisIt.UsedArea.X * scaleX);

            #endregion

            #region  newH

            var nh = thisIt.UsedArea.Height * scaleY;

            if (thisIt.AutoSizeableHeight) {
                if (!thisIt.CanChangeHeightTo(nh)) {
                    nh = AutosizableExtension.MinHeigthCapAndBox;
                }
            } else {
                nh = thisIt.UsedArea.Height;
            }

            newH.Add(nh);

            #endregion

            #region  newW

            newW.Add(thisIt.UsedArea.Width * scaleX);

            #endregion
        }

        #endregion

        #region  Alle Items von unten nach oben auf Überlappungen (auch dem Rand) prüfen.

        // Alle prüfen

        for (var tocheck = its.Count - 1; tocheck >= 0; tocheck--) {
            var pos = PositioOf(tocheck);

            #region Unterer Rand

            if (pos.Bottom > newHeight) {
                newY[tocheck] = newHeight - pos.Height;
                pos = PositioOf(tocheck);
            }

            #endregion

            for (var coll = its.Count - 1; coll > tocheck; coll--) {
                var poscoll = PositioOf(coll);
                if (pos.IntersectsWith(poscoll)) {
                    newY[tocheck] = poscoll.Top - pos.Height;
                    pos = PositioOf(tocheck);
                }
            }
        }

        #endregion

        #region  Alle UNveränderlichen Items von oben nach unten auf Überlappungen (auch dem Rand) prüfen.

        // Und von oben nach unten muss sein, weil man ja oben bündig haben will
        // Wichtig, das CanScaleHeightTo nochmal geprüft wird.
        // Nur so kann festgestellt werden, ob es eigentlich veränerlich wäre, aber durch die Mini-Größe doch als unveränderlich gilt

        for (var tocheck = 0; tocheck < its.Count; tocheck++) {
            if (!its[tocheck].CanScaleHeightTo(scaleY)) {
                var pos = PositioOf(tocheck);

                #region Oberer Rand

                if (pos.Y < 0) {
                    newY[tocheck] = 0;
                    pos = PositioOf(tocheck);
                }

                #endregion

                for (var coll = 0; coll < tocheck; coll++) {
                    if (!its[tocheck].CanScaleHeightTo(scaleY)) {
                        var poscoll = PositioOf(coll);
                        if (pos.IntersectsWith(poscoll)) {
                            newY[tocheck] = poscoll.Top + poscoll.Height;
                            pos = PositioOf(tocheck);
                        }
                    }
                }
            }
        }

        #endregion

        #region Alle Items, den Abstand stutzen, wenn der vorgänger unveränderlich ist - nur bei ScaleY >1

        if (scaleY > 1) {
            for (var tocheck = 0; tocheck < its.Count; tocheck++) {
                if (!its[tocheck].CanScaleHeightTo(scaleY)) {
                    //var pos = PositioOf(tocheck);

                    for (var coll = tocheck + 1; coll < its.Count; coll++) {
                        //var poscoll = PositioOf(coll);

                        if (its[coll].UsedArea.Y >= its[tocheck].UsedArea.Bottom && its[coll].UsedArea.IntersectsVericalyWith(its[tocheck].UsedArea)) {
                            newY[coll] = newY[tocheck] + newH[tocheck] + its[coll].UsedArea.Top - its[tocheck].UsedArea.Bottom;
                            //pos = PositioOf(tocheck);
                        }
                    }
                }
            }
        }

        #endregion

        #region  Alle veränderlichen Items von oben nach unten auf Überlappungen (auch dem Rand) prüfen - nur den Y-Wert.

        for (var tocheck = 0; tocheck < its.Count; tocheck++) {
            if (its[tocheck].CanScaleHeightTo(scaleY)) {
                var pos = PositioOf(tocheck);

                #region Oberer Rand

                if (pos.Y < 0) {
                    newY[tocheck] = 0;
                    pos = PositioOf(tocheck);
                }

                #endregion

                for (var coll = 0; coll < tocheck; coll++) {
                    var poscoll = PositioOf(coll);
                    if (pos.IntersectsWith(poscoll)) {
                        newY[tocheck] = poscoll.Top + poscoll.Height;
                        pos = PositioOf(tocheck);
                    }
                }
            }
        }

        #endregion

        #region  Alle veränderlichen Items von oben nach unten auf Überlappungen (auch dem Rand) prüfen - nur den Height-Wert stutzen.

        for (var tocheck = 0; tocheck < its.Count; tocheck++) {
            if (its[tocheck].CanScaleHeightTo(scaleY)) {
                var pos = PositioOf(tocheck);

                #region  Unterer Rand

                if (pos.Bottom > newHeight) {
                    newH[tocheck] = newHeight - pos.Y;
                    pos = PositioOf(tocheck);
                }

                #endregion

                #region  Alle Items stimmen mit dem Y-Wert, also ALLE prüfen, NACH dem Item

                for (var coll = tocheck + 1; coll < its.Count; coll++) {
                    var poscoll = PositioOf(coll);
                    if (pos.IntersectsWith(poscoll)) {
                        newH[tocheck] = poscoll.Top - pos.Top;
                        pos = PositioOf(tocheck);
                    }
                }

                #endregion
            }
        }

        #endregion

        #region Feedback-Liste erstellen (p)

        var p = new List<RectangleF>();
        for (var ite = 0; ite < its.Count; ite++) {
            p.Add(PositioOf(ite));
        }

        #endregion

        return p;

        RectangleF PositioOf(int no) => new(newX[no], newY[no], newW[no], newH[no]);
    }

    public static List<(IAutosizable item, RectangleF newpos)> ResizeControls(ItemCollectionPad.ItemCollectionPad padData, float newWidthPixel, float newhHeightPixel, string page, string mode) {

        #region Items und Daten in einer sortierene Liste ermitteln, die es betrifft (its)

        List<IAutosizable> its = [];

        foreach (var thisc in padData) {
            if (thisc is IAutosizable aas && aas.IsVisibleForMe(mode) && thisc.IsVisibleOnPage(page)) {
                its.Add(aas);
            }
        }

        its.Sort((it1, it2) => it1.UsedArea.Y.CompareTo(it2.UsedArea.Y));

        #endregion

        var p = ResizeControls(its, newWidthPixel, newhHeightPixel, padData.SheetSizeInPix.Width, padData.SheetSizeInPix.Height);

        var erg = new List<(IAutosizable item, RectangleF newpos)>();

        for (var x = 0; x < its.Count; x++) {
            erg.Add((its[x], p[x]));
        }

        return erg;
    }

    public static IEnumerable<string> VisibleFor_AllUsed() {
        var l = new List<string>();

        foreach (var thisCf in AllFiles) {
            if (!thisCf.IsDisposed && thisCf.PadData is ItemCollectionPad.ItemCollectionPad icp) {
                l.AddRange(icp.VisibleFor_AllUsed());
            }
        }

        return l.SortedDistinctList();
    }

    public void DiscardPendingChanges(object sender, System.EventArgs e) => _saved = true;

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void HasPendingChanges(object sender, MultiUserFileHasPendingChangesEventArgs e) {
        if (!_saved) { e.HasPendingChanges = true; return; }

        if (IntParse(_loadedVersion.Replace(".", string.Empty)) < IntParse(Version.Replace(".", string.Empty))) {
            e.HasPendingChanges = true;
        }
    }

    public bool IsAdministrator() {
        if (string.Equals(UserGroup, Constants.Administrator, StringComparison.OrdinalIgnoreCase)) { return true; }
        //if (_datenbankAdmin == null || _datenbankAdmin.Count == 0) { return false; }
        //if (_datenbankAdmin.Contains(Constants.Everybody, false)) { return true; }
        //if (!string.IsNullOrEmpty(UserName) && _datenbankAdmin.Contains("#User: " + UserName, false)) { return true; }
        //return !string.IsNullOrEmpty(UserGroup) && _datenbankAdmin.Contains(UserGroup, false);
        return false;
    }

    public void OnNotAllowedChildsChanged() => NotAllowedChildsChanged?.Invoke(this, System.EventArgs.Empty);

    public void OnPropertyChanged() => PropertyChanged?.Invoke(this, System.EventArgs.Empty);

    public void Repair() {
        // Reparatur-Routine

        PadData ??= [];

        PadData.BackColor = Skin.Color_Back(Design.Form_Standard, States.Standard);

        foreach (var thisCon in PadData.Connections) {
            thisCon.Bei_Export_sichtbar = false;
        }

        foreach (var thisIt in PadData) {
            if (string.IsNullOrEmpty(thisIt.Page)) {
                thisIt.Page = "Head";
            }

            if (thisIt is FakeControlPadItem itcf) {
                itcf.CFormula = this;
            }
        }

        var pg = PadData.AllPages();
        pg.AddIfNotExists("Head");

        foreach (var thisP in pg) {
            RowEntryPadItem? found = null;

            foreach (var thisit in PadData) {
                if (thisit is RowEntryPadItem repi) {
                    if (string.Equals(thisP, repi.Page, StringComparison.OrdinalIgnoreCase)) { found = repi; break; }
                }
            }
            if (found == null) {
                found = new RowEntryPadItem(string.Empty);

                PadData.Add(found);
            }

            found.SetCoordinates(new RectangleF((PadData.SheetSizeInPix.Width / 2) - 150, -30, 300, 30), true);
            found.Page = thisP;
            found.Bei_Export_sichtbar = false;
        }
    }

    public void Save() => _muf?.Save(true);

    //public void Variables_Add(VariableString va, bool isLoading) {
    //    _variables.Add(va);
    //    //ev.Changed += EventScript_Changed;
    //    if (!isLoading) { Variables_Changed(); }
    //}

    //public void Variables_RemoveAll(bool isLoading) {
    //    while (_variables.Count > 0) {
    //        //var va = _variables[_eventScript.Count - 1];
    //        //ev.Changed -= EventScript_Changed;

    //        _variables.RemoveAt(_variables.Count - 1);
    //    }

    //    if (!isLoading) { Variables_Changed(); }
    //}

    /// <summary>
    /// Leert die eingehende List und fügt alle bekannten Fomulare hinzu - außer die in notAllowedChilds
    /// </summary>
    /// <param name="list"></param>
    /// <param name="notAllowedChilds"></param>
    internal void AddChilds(List<AbstractListItem> list, ReadOnlyCollection<string> notAllowedChilds) {
        list.Clear();

        if (File.Exists(Filename)) {
            foreach (var thisf in Directory.GetFiles(Filename.FilePath(), "*.cfo")) {
                if (!notAllowedChilds.Contains(thisf)) {
                    list.Add(ItemOf(thisf.FileNameWithoutSuffix(), thisf, ImageCode.Diskette));
                }
            }
        }

        foreach (var thisf in AllFiles) {
            if (!notAllowedChilds.Contains(thisf.Filename)) {
                if (list.Get(thisf.Filename) == null) {
                    list.Add(ItemOf(thisf.Filename.FileNameWithoutSuffix(), thisf.Filename, ImageCode.Diskette));
                }
            }
        }

        //if (PadData != null) {
        //    foreach (var thisf in PadData.AllPages()) {
        //        if (!notAllowedChilds.Contains(thisf) && !string.Equals("Head", thisf, StringComparison.OrdinalIgnoreCase)) {
        //            _ = list.Add(thisf, ImageCode.Register);
        //        }
        //    }
        //}
    }

    /// <summary>
    /// Prüft, ob das Formular sichtbare Elemente hat.
    /// Zeilenselectionen werden dabei ignoriert.
    /// </summary>
    /// <param name="page">Wird dieser Wert leer gelassen, wird das komplette Formular geprüft</param>
    /// <returns></returns>
    internal bool HasVisibleItemsForMe(string page, string mode) {
        if (_padData == null) { return false; }

        foreach (var thisItem in _padData) {
            if (string.IsNullOrEmpty(page) ||
                string.IsNullOrEmpty(thisItem.Page) ||
                page.Equals(thisItem.Page, StringComparison.OrdinalIgnoreCase)) {
                if (thisItem is FakeControlPadItem cspi) {
                    if (cspi.MustBeInDrawingArea && cspi.IsVisibleForMe(mode)) { return true; }
                }
            }
        }

        return false;
    }

    internal bool IsEditing() {
        var e = new EditingEventArgs();

        OnEditing(e);

        return e.Editing;
    }

    internal void OnDropMessage(FehlerArt type, string message) {
        if (IsDisposed) { return; }
        if (!DropMessages) { return; }
        DropMessage?.Invoke(this, new MessageEventArgs(type, message));
    }

    internal void Resize(float newWidthPixel, float newhHeightPixel, bool changeControls, string mode) {
        if (PadData == null) { return; }

        if (changeControls) {
            //var newWidthPixel = MmToPixel(newwidthinmm, ItemCollectionPad.Dpi);
            //var newhHeightPixel = MmToPixel(newheightinmm, ItemCollectionPad.Dpi);

            foreach (var thisPage in PadData.AllPages()) {
                var x = ResizeControls(PadData, newWidthPixel, newhHeightPixel, thisPage, mode);

                #region Die neue Position in die Items schreiben

                foreach (var (item, newpos) in x) {
                    item.SetCoordinates(newpos, true);
                }

                #endregion
            }
        }

        PadData.SheetSizeInMm = new SizeF(PixelToMm(newWidthPixel, ItemCollectionPad.ItemCollectionPad.Dpi), PixelToMm(newhHeightPixel, ItemCollectionPad.ItemCollectionPad.Dpi));
    }

    internal void SaveAsAndChangeTo(string fileName) => _muf?.SaveAsAndChangeTo(fileName);

    private void _muf_Saving(object sender, CancelEventArgs e) {
        if (e.Cancel) { return; }

        e.Cancel = IntParse(_loadedVersion.Replace(".", string.Empty)) > IntParse(Version.Replace(".", string.Empty));

        //return IntParse(LoadedVersion.Replace(".", string.Empty)) > IntParse(DatabaseVersion.Replace(".", string.Empty))
        //    ? "Diese Programm kann nur Datenbanken bis Version " + DatabaseVersion + " speichern."
        //    : string.Empty;
    }

    private void DatabaseFiles_Changed() {
        if (_saving || (_muf?.IsLoading ?? true)) { return; }

        foreach (var thisfile in _databaseFiles) {
            _ = Database.GetById(new ConnectionInfo(thisfile, null, string.Empty), false, null, true);
        }

        _saved = false;
    }

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // Verwaltete Ressourcen (Instanzen von Klassen, Lists, Tasks,...)
                _ = _muf?.Save(true);
                _muf?.Dispose();
                _muf = null;
            }
            _ = AllFiles.Remove(this);

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    private void OnEditing(EditingEventArgs e) => Editing?.Invoke(this, e);

    //private void NotAllowedChilds_Changed(object sender, System.EventArgs e) {
    //    if (_saving || (_muf?.IsLoading ?? true)) { return; }
    //    _saved = false;
    //}
    private void OnLoaded(object sender, System.EventArgs e) {
        Repair();

        Loaded?.Invoke(this, e);
    }

    private void OnLoading(object sender, System.EventArgs e) => Loading?.Invoke(this, e);

    private void OnSavedToDisk(object sender, System.EventArgs e) {
        _saved = true;
        _loadedVersion = Version;
        SavedToDisk?.Invoke(this, e);
    }

    private void PadData_PropertyChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        if (_saving || (_muf?.IsLoading ?? true)) { return; }

        _saved = false;
        OnPropertyChanged();
    }

    private void ParseExternal(object sender, MultiUserParseEventArgs e) {
        var toParse = e.Data.ToStringWin1252();
        if (string.IsNullOrEmpty(toParse)) { return; }

        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key.ToLower()) {
                case "type":
                    break;

                case "version":
                    _loadedVersion = pair.Value;
                    break;

                //case "filepath":
                //    FilePath = pair.Value.FromNonCritical();
                //    break;

                case "databasefiles":
                    _databaseFiles.Clear();
                    _databaseFiles.AddRange(pair.Value.FromNonCritical().SplitByCrToList());
                    DatabaseFiles_Changed();
                    break;

                case "notallowedchilds":
                    _notAllowedChilds.Clear();
                    _notAllowedChilds.AddRange(pair.Value.FromNonCritical().SplitByCrToList());
                    OnNotAllowedChildsChanged();
                    break;

                case "createdate":
                    _createDate = pair.Value.FromNonCritical();
                    break;

                case "createname":
                    _creator = pair.Value.FromNonCritical();
                    break;

                case "paditemdata":
                    PadData = new ItemCollectionPad.ItemCollectionPad(pair.Value.FromNonCritical(), string.Empty);
                    break;

                case "lastusedid":
                    _id = IntParse(pair.Value);
                    break;

                case "events":
                    break;

                case "variables":
                    break;
            }
        }
    }

    private void ToListOfByte(object sender, MultiUserToListEventArgs e) {

        #region ein bischen aufräumen zuvor

        _saving = true;
        //PadData.Sort();

        //_id = -1;

        _databaseFiles.Clear();

        //foreach (var thisit in PadData) {
        //    if (thisit is IItemSendFilter rwf) {
        //        if (rwf.DatabaseOutput != null) {
        //            _ = _databaseFiles.AddIfNotExists(rwf.DatabaseOutput.ConnectionData.UniqueID);
        //            _id = Math.Max(_id, rwf.InputColorId);
        //        }
        //    }
        //}
        _saving = false;

        #endregion

        var t = new List<string>();

        t.ParseableAdd("Type", "ConnectedFormula");
        t.ParseableAdd("Version", Version);
        t.ParseableAdd("CreateDate", _createDate);
        t.ParseableAdd("CreateName", _creator);
        t.ParseableAdd("LastUsedID", _id);
        t.ParseableAdd("DatabaseFiles", _databaseFiles);
        t.ParseableAdd("NotAllowedChilds", _notAllowedChilds);

        if (PadData != null) {
            t.ParseableAdd("PadItemData", PadData.ToString());
        }

        e.Data = t.Parseable().WIN1252_toByte();
    }

    #endregion
}