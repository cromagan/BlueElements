// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueBasics.MultiUserFile;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using static BlueBasics.Converter;
using static BlueBasics.Develop;
using static BlueBasics.IO;

namespace BlueControls.ConnectedFormula;

public class ConnectedFormula : IChangedFeedback, IDisposableExtended, IHasKeyName {

    #region Fields

    public const string Version = "0.10";
    public static readonly List<ConnectedFormula> AllFiles = new();
    public static readonly float StandardHöhe = 1.75f;
    public static readonly float Umrechnungsfaktor2 = MmToPixel(StandardHöhe, 300) / 22;
    private readonly List<string> _databaseFiles = new();
    private readonly List<string> _notAllowedChilds = new();

    private string _createDate;

    private string _creator;

    private int _id = -1;

    private string _loadedVersion = "0.00";

    private MultiUserFile? _muf;

    private ItemCollectionPad? _padData;

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
        _createDate = DateTime.Now.ToString(Constants.Format_Date5);
        _creator = Generic.UserName;
        PadData = new ItemCollectionPad();

        if (FileExists(filename)) {
            _muf.Load(filename, true);
        }

        _saved = true;

        if (_padData != null) {
            //_padData.SheetSizeInMm = new SizeF(PixelToMm(500, 300), PixelToMm(850, 300));
            _padData.GridShow = 0.5f;
            _padData.GridSnap = 0.5f;
        }
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    public event EventHandler? Loaded;

    public event EventHandler? Loading;

    public event EventHandler? NotAllowedChildsChanged;

    public event EventHandler? SavedToDisk;

    #endregion

    #region Properties

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

    public ItemCollectionPad? PadData {
        get => _padData;
        private set {
            if (_padData == value) { return; }

            if (_padData != null) {
                _padData.Changed -= PadData_Changed;
            }
            _padData = value;
            if (_padData != null) {
                _padData.Changed += PadData_Changed;
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

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public void OnNotAllowedChildsChanged() => NotAllowedChildsChanged?.Invoke(this, System.EventArgs.Empty);

    public void Repair() {
        // Reparatur-Routine

        if (PadData == null) {
            PadData = new ItemCollectionPad();
        }

        foreach (var thisCon in PadData.Connections) {
            thisCon.Bei_Export_sichtbar = false;
        }

        foreach (var thisIt in PadData) {
            if (string.IsNullOrEmpty(thisIt.Page)) {
                thisIt.Page = "Head";
            }

            if (thisIt is IHasConnectedFormula itcf) {
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

            if (found != null) {
                found.SetCoordinates(new RectangleF((PadData.SheetSizeInPix.Width / 2) - 150, -30, 300, 30), true);
                found.Page = thisP;
                found.Bei_Export_sichtbar = false;
            }
        }
    }

    public void Save() => _muf?.Save(true);

    /// <summary>
    /// Prüft, ob das Formular sichtbare Elemente hat.
    /// Zeilenselectionen werden dabei ignoriert.
    /// </summary>
    /// <param name="page">Wird dieser Wert leer gelassen, wird das komplette Formular geprüft</param>
    /// <returns></returns>
    internal bool HasVisibleItemsForMe(string page) {
        if (_padData == null) { return false; }

        foreach (var thisItem in _padData) {
            if (string.IsNullOrEmpty(page) ||
                string.IsNullOrEmpty(thisItem.Page) ||
                page.Equals(thisItem.Page, StringComparison.OrdinalIgnoreCase)) {
                if (thisItem is FakeControlPadItem cspi) {
                    if (cspi.IsVisibleForMe()) { return true; }
                }
            }
        }

        return false;
    }

    internal void Resize(float newwidthinmm, float newheightinmm, bool changeControls) {
        if (PadData == null) { return; }

        if (changeControls) {
            foreach (var thisPage in PadData.AllPages()) {
                ResizeControls(newwidthinmm, newheightinmm, thisPage);
            }
        }

        PadData.SheetSizeInMm = new SizeF(newwidthinmm, newheightinmm);
    }

    internal void SaveAsAndChangeTo(string fileName) => _muf?.SaveAsAndChangeTo(fileName);

    protected virtual void Dispose(bool disposing) {
        if (!IsDisposed) {
            _ = AllFiles.Remove(this);
            if (disposing) {
                _ = _muf?.Save(true);
                _muf?.Dispose();
                _muf = null;
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    protected void ParseExternal(object sender, MultiUserParseEventArgs e) {
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
                    PadData = new ItemCollectionPad(pair.Value.FromNonCritical(), string.Empty);
                    break;

                case "lastusedid":
                    _id = IntParse(pair.Value);
                    break;

                default:
                    DebugPrint(FehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
    }

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
            _ = DatabaseAbstract.GetById(new ConnectionInfo(thisfile, null), null);
        }

        _saved = false;
    }

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

    private void PadData_Changed(object sender, System.EventArgs e) {
        if (_saving || (_muf?.IsLoading ?? true)) { return; }

        _saved = false;
        OnChanged();
    }

    private void ResizeControls(float newwidthinmm, float newheightinmm, string page) {
        if (PadData == null) { return; }

        var newheightiPixel = MmToPixel(newheightinmm, 300);
        var abstand = MmToPixel(0.5f, 300);

        #region Items und Daten in einer sortierene Liste ermitteln, die es betrifft (its)

        List<IAutosizable> its = new();

        foreach (var thisc in PadData) {
            if (thisc.IsVisibleOnPage(page) && thisc is IAutosizable aas) {
                its.Add(aas);
            }
        }

        its.Sort((it1, it2) => it1.UsedArea.Y.CompareTo(it2.UsedArea.Y));

        #endregion

        //PadData.SheetSizeInMm = new SizeF(newwidthinmm, newheightinmm);
        var fy = newheightinmm / PadData.SheetSizeInMm.Height;

        #region Alle Items an die neue gedachte Y-Position schieben (newY)

        List<float> newY = new();
        for (var ity = 0; ity < its.Count; ity++) {
            newY.Add(its[ity].UsedArea.Y * fy);
        }

        #endregion

        #region  Alle Items von unten nach oben auf Überlappungen (auch dem Rand) prüfen.

        // Alle prüfen, weil die Höhe noch unverändert ist

        for (var tocheck = its.Count - 1; tocheck >= 0; tocheck--) {
            var pos = PositioOf(tocheck);

            for (var coll = its.Count - 1; coll > tocheck; coll--) {
                var poscoll = PositioOf(coll);
                if (pos.IntersectsWith(poscoll)) {
                    newY[tocheck] = poscoll.Top - pos.Height;
                }
            }

            pos = PositioOf(tocheck);
            if (pos.Bottom > newheightiPixel) {
                newY[tocheck] = newheightiPixel - pos.Height;
            }
        }

        #endregion

        #region  Alle UNveränderlichen Items von oben nach unten auf Überlappungen (auch dem Rand) prüfen.

        // Und von oben nach unten muss sein, weil man ja oben bündig haben will

        for (var tocheck = 0; tocheck < its.Count; tocheck++) {
            if (!its[tocheck].AutoSizeableHeight) {
                var pos = PositioOf(tocheck);

                for (var coll = 0; coll < tocheck; coll++) {
                    if (!its[tocheck].AutoSizeableHeight) {
                        var poscoll = PositioOf(coll);
                        if (pos.IntersectsWith(poscoll)) {
                            newY[tocheck] = poscoll.Top + poscoll.Height;
                        }
                    }
                }

                pos = PositioOf(tocheck);
                if (pos.Y < 0) {
                    newY[tocheck] = 0;
                }
            }
        }

        #endregion


        //#region  Und am Schluss die veräderlichen einpassen

        //// Und von oben nach unten muss sein, weil man ja oben bündig haben will

        //for (var tocheck = 0; tocheck < its.Count; tocheck++) {
        //    if (its[tocheck].AutoSizeableHeight) {
        //        var pos = PositioOf(tocheck);

        //        for (var coll = 0; coll < tocheck; coll++) {
        //            if (its[tocheck].AutoSizeableHeight) {
        //                var poscoll = PositioOf(coll);
        //                if (pos.IntersectsWith(poscoll)) {
        //                    newY[tocheck] = poscoll.Top + poscoll.Height;
        //                }
        //            }
        //        }

        //        pos = PositioOf(tocheck);
        //        if (pos.Y < 0) {
        //            newY[tocheck] = 0;
        //        }
        //    }
        //}



        #region Die neue Position in die Items schreiben

        for (var ite = 0; ite < its.Count; ite++) {
            var pos = PositioOf(ite);
            its[ite].SetCoordinates(pos.ToRect(), true);
        }

        #endregion

        //#region  Alle HeightFix Items von unten nach oben auf Überlappungen (auch dem Rand) prüfen)

        //for (var tocheck = its.Count; tocheck >=0; tocheck--) {
        //    if(its[tocheck].FixHeight))
        //    {
        //        var pos = PositioOf(tocheck);

        //    if (its[tocheck].)
        //    }

        //    //newY.Add(its[ity].UsedArea.Y * fy);
        //}

        //#endregion

        //for (var t = 0; t < its.Count; t++) {
        //    if (its[t].item.ChanChangeHeigt)

        //}

        RectangleF PositioOf(int no) {
            var r = its[no].UsedArea;
            return r with { Y = newY[no] };
        }
        RectangleF PositioOfWithBorder(int no) {
            var r = its[no].UsedArea;
            var r2 = r with { Y = newY[no] };
            r2.Inflate(abstand / 2, abstand / 2);
            return r2;
        }
    }

    private void ToListOfByte(object sender, MultiUserToListEventArgs e) {

        #region ein bischen aufräumen zuvor

        _saving = true;
        //PadData.Sort();

        //_id = -1;

        _databaseFiles.Clear();

        //foreach (var thisit in PadData) {
        //    if (thisit is IItemSendRow rwf) {
        //        if (rwf.OutputDatabase != null) {
        //            _ = _databaseFiles.AddIfNotExists(rwf.OutputDatabase.ConnectionData.UniqueID);
        //            _id = Math.Max(_id, rwf.InputColorId);
        //        }
        //    }
        //}
        _saving = false;

        #endregion

        var t = new List<string>();

        t.Add("Type=ConnectedFormula");
        t.Add("Version=" + Version);
        t.Add("CreateDate=" + _createDate.ToNonCritical());
        t.Add("CreateName=" + _creator.ToNonCritical());
        //t.GenerateAndAdd("FilePath=" + FilePath.ToNonCritical());
        t.Add("LastUsedID=" + _id);
        t.Add("DatabaseFiles=" + _databaseFiles.JoinWithCr().ToNonCritical());
        t.Add("NotAllowedChilds=" + _notAllowedChilds.JoinWithCr().ToNonCritical());

        if (PadData != null) {
            t.Add("PadItemData=" + PadData.ToString().ToNonCritical());
        }

        e.Data = ("{" + t.JoinWith(", ").TrimEnd(", ") + "}").WIN1252_toByte();
    }

    #endregion
}