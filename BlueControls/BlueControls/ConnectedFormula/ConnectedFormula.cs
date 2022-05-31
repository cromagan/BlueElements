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

using BlueBasics.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static BlueBasics.Develop;
using BlueDatabase;
using BlueBasics;
using static BlueBasics.Converter;
using BlueBasics.Interfaces;
using BlueControls.ItemCollection;
using static BlueBasics.FileOperations;
using BlueBasics.EventArgs;

namespace BlueControls.ConnectedFormula;

public class ConnectedFormula : IChangedFeedback, IDisposable {

    #region Fields

    public const string Version = "0.01";
    public static readonly ListExt<ConnectedFormula> AllFiles = new();
    public static readonly float StandardHöhe = 3.5f;

    //public static readonly float Umrechnungsfaktor = Converter.PixelToMm(1, 300);
    public static readonly float Umrechnungsfaktor2 = MmToPixel(StandardHöhe, 300) / 44;

    public readonly ListExt<string> DatabaseFiles = new();
    public readonly ListExt<string> NotAllowedChilds = new();

    //public readonly List<Database?> Databases = new();
    private string _createDate = string.Empty;

    private string _creator = string.Empty;
    private bool _disposed_Value;
    private int _id = -1;
    private BlueBasics.MultiUserFile.MultiUserFile _muf;
    private ItemCollectionPad _padData;

    private bool _saved = true;
    private bool _saving = false;

    #endregion

    #region Constructors

    public ConnectedFormula() : this(string.Empty) {
    }

    private ConnectedFormula(string filename) {
        AllFiles.Add(this);
        _muf = new BlueBasics.MultiUserFile.MultiUserFile(false, true);

        _muf.ConnectedControlsStopAllWorking += OnConnectedControlsStopAllWorking;
        _muf.Loaded += OnLoaded;
        _muf.Loading += OnLoading;
        _muf.SavedToDisk += OnSavedToDisk;
        _muf.ShouldICancelSaveOperations += OnShouldICancelSaveOperations;
        _muf.DiscardPendingChanges += DiscardPendingChanges;
        _muf.HasPendingChanges += HasPendingChanges;
        _muf.RepairAfterParse += RepairAfterParse;
        _muf.DoWorkAfterSaving += DoWorkAfterSaving;
        _muf.IsThereBackgroundWorkToDo += IsThereBackgroundWorkToDo;
        _muf.ParseExternal += ParseExternal;
        _muf.ToListOfByte += ToListOfByte;

        _createDate = DateTime.Now.ToString(Constants.Format_Date5);
        _creator = Generic.UserName();
        PadData = new ItemCollectionPad();

        if (FileExists(filename)) {
            _muf.Load(filename, true);
        }
        //if (notallowedchilds != null) {
        //    NotAllowedChilds.AddIfNotExists(notallowedchilds);
        //}
        _saved = true;

        DatabaseFiles.Changed += DatabaseFiles_Changed;
        NotAllowedChilds.Changed += NotAllowedChilds_Changed;
    }

    #endregion

    #region Events

    public event EventHandler Changed;

    public event EventHandler<MultiUserFileStopWorkingEventArgs> ConnectedControlsStopAllWorking;

    public event EventHandler<LoadedEventArgs> Loaded;

    public event EventHandler<LoadingEventArgs> Loading;

    public event EventHandler SavedToDisk;

    /// <summary>
    /// Dient dazu, offene Dialoge abzufragen
    /// </summary>
    public event EventHandler<CancelEventArgs> ShouldICancelSaveOperations;

    #endregion

    #region Properties

    public string Filename => _muf.Filename;

    public ItemCollectionPad PadData {
        get => _padData;
        private set {
            if (_padData == value) { return; }

            if (_padData != null) {
                PadData.Changed -= PadData_Changed;
            }
            _padData = value;
            if (_padData != null) {
                PadData.Changed += PadData_Changed;
            }

            if (_saving || _muf.IsParsing || _muf.IsLoading) { return; }

            _saved = false;
        }
    }

    #endregion

    #region Methods

    public static ConnectedFormula? GetByFilename(string filename) {
        foreach (var thisFile in AllFiles) {
            if (thisFile != null && string.Equals(thisFile.Filename, filename, StringComparison.OrdinalIgnoreCase)) {
                return thisFile;
            }
        }

        return !FileExists(filename) ? null : new ConnectedFormula(filename);
    }

    ///// <summary>
    ///// Sucht die Datenbank im Speicher. Wird sie nicht gefunden, wird sie geladen.
    ///// </summary>
    ///// <param name="filename"></param>
    ///// <param name="checkOnlyFilenameToo"></param>
    ///// <param name="readOnly"></param>
    ///// <returns></returns>
    //public static ConnectedFormula? GetByFilename(string filename) {
    //    var tmpCF = _muf.GetByFilename(filename, false);

    //    if (tmpCF is ConnectedFormula cf) { return cf; }

    //    if (tmpCF != null) { return null; }//  Daten im Speicher, aber keine Datenbank!

    //    return !FileExists(filename) ? null : new ConnectedFormula(filename);
    //}

    //public string PadData {
    //    get => _padData;
    //    set {
    //        if (value == _padData) { return; }
    //        _padData = value;
    //        _saved = false;
    //        OnChanged();
    //    }
    //}
    public void DiscardPendingChanges(object sender, System.EventArgs e) => _saved = true;

    void IDisposable.Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void HasPendingChanges(object sender, MultiUserFileHasPendingChangesEventArgs e) {
        e.HasPendingChanges = !_saved;
    }

    public int NextID() {
        _id++;
        _saved = false;
        return _id;
    }

    public void OnChanged() {
        Changed?.Invoke(this, System.EventArgs.Empty);
    }

    public void RepairAfterParse(object sender, System.EventArgs e) { }

    internal void SaveAsAndChangeTo(string fileName) {
        _muf.SaveAsAndChangeTo(fileName);
    }

    protected virtual void Dispose(bool disposing) {
        if (!_disposed_Value) {
            AllFiles.Remove(this);
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            _disposed_Value = true;
        }
    }

    protected void DoWorkAfterSaving(object sender, System.EventArgs e) => _saved = true;

    protected void IsThereBackgroundWorkToDo(object sender, MultiUserIsThereBackgroundWorkToDoEventArgs e) { e.BackGroundWork = false; }

    protected void ParseExternal(object sender, MultiUserParseEventArgs e) {
        var ToParse = e.Data.ToStringWin1252();
        if (string.IsNullOrEmpty(ToParse)) { return; }

        foreach (var pair in ToParse.GetAllTags()) {
            switch (pair.Key.ToLower()) {
                case "type":
                    break;

                case "version":
                    break;

                //case "filepath":
                //    FilePath = pair.Value.FromNonCritical();
                //    break;

                case "databasefiles":
                    DatabaseFiles.Clear();
                    DatabaseFiles.AddRange(pair.Value.FromNonCritical().SplitByCrToList());
                    break;

                case "notallowedchilds":
                    NotAllowedChilds.Clear();
                    NotAllowedChilds.AddRange(pair.Value.FromNonCritical().SplitByCrToList());
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

    private void DatabaseFiles_Changed(object sender, System.EventArgs e) {
        if (_saving || _muf.IsParsing || _muf.IsLoading) { return; }

        foreach (var thisfile in DatabaseFiles) {
            Database.GetByFilename(thisfile, false, false);
        }

        _saved = false;
    }

    private void NotAllowedChilds_Changed(object sender, System.EventArgs e) {
        if (_saving || _muf.IsParsing || _muf.IsLoading) { return; }
        _saved = false;
    }

    private void OnConnectedControlsStopAllWorking(object sender, MultiUserFileStopWorkingEventArgs e) {
        ConnectedControlsStopAllWorking?.Invoke(this, e);
    }

    private void OnLoaded(object sender, LoadedEventArgs e) {
        Loaded?.Invoke(this, e);
    }

    private void OnLoading(object sender, LoadingEventArgs e) {
        Loading?.Invoke(this, e);
    }

    private void OnSavedToDisk(object sender, System.EventArgs e) {
        SavedToDisk?.Invoke(this, e);
    }

    private void OnShouldICancelSaveOperations(object sender, CancelEventArgs e) {
        ShouldICancelSaveOperations?.Invoke(this, e);
    }

    private void PadData_Changed(object sender, System.EventArgs e) {
        if (_saving || _muf.IsParsing || _muf.IsLoading) { return; }

        _saved = false;
    }

    private void ToListOfByte(object sender, MultiUserToListEventArgs e) {

        #region ein bischen aufräumen zuvor

        _saving = true;
        PadData.Sort();

        _id = -1;

        DatabaseFiles.Clear();

        foreach (var thisit in PadData) {
            if (thisit is RowWithFilterPaditem rwf) {
                if (rwf.Database != null) {
                    DatabaseFiles.AddIfNotExists(rwf.Database.Filename);
                    _id = Math.Max(_id, rwf.Id);
                }
            }
        }
        _saving = false;

        #endregion

        var t = new List<string>();

        t.Add("Type=ConnectedFormula");
        t.Add("Version=" + Version);
        t.Add("CreateDate=" + _createDate.ToNonCritical());
        t.Add("CreateName=" + _creator.ToNonCritical());
        //t.Add("FilePath=" + FilePath.ToNonCritical());
        t.Add("LastUsedID=" + _id.ToString());
        t.Add("DatabaseFiles=" + DatabaseFiles.JoinWithCr().ToNonCritical());
        t.Add("NotAllowedChilds=" + NotAllowedChilds.JoinWithCr().ToNonCritical());
        t.Add("PadItemData=" + PadData.ToString().ToNonCritical());

        e.Data = ("{" + t.JoinWith(", ").TrimEnd(", ") + "}").WIN1252_toByte();
    }

    #endregion

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~ConnectedFormula()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
}