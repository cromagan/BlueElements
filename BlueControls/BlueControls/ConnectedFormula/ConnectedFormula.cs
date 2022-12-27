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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Converter;
using static BlueBasics.Develop;
using static BlueBasics.IO;

namespace BlueControls.ConnectedFormula;

public class ConnectedFormula : IChangedFeedback, IDisposableExtended {

    #region Fields

    public const string Version = "0.01";
    public static readonly ListExt<ConnectedFormula> AllFiles = new();
    public static readonly float StandardHöhe = 1.75f;

    public static readonly float Umrechnungsfaktor2 = MmToPixel(StandardHöhe, 300) / 22;

    public readonly ListExt<string> DatabaseFiles = new();
    public readonly ListExt<string> NotAllowedChilds = new();

    private string _createDate;
    private string _creator;
    private int _id = -1;
    private BlueBasics.MultiUserFile.MultiUserFile? _muf;
    private ItemCollectionPad? _padData;

    private bool _saved;
    private bool _saving;

    #endregion

    #region Constructors

    public ConnectedFormula() : this(string.Empty) { }

    private ConnectedFormula(string filename) {
        AllFiles.Add(this);
        _muf = new BlueBasics.MultiUserFile.MultiUserFile();

        //_muf.ConnectedControlsStopAllWorking += OnConnectedControlsStopAllWorking;
        _muf.Loaded += OnLoaded;
        _muf.Loading += OnLoading;
        _muf.SavedToDisk += OnSavedToDisk;
        _muf.DiscardPendingChanges += DiscardPendingChanges;
        _muf.HasPendingChanges += HasPendingChanges;
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

        _padData.SheetSizeInMm = new SizeF(PixelToMm(500, 300), PixelToMm(850, 300));
        _padData.GridShow = 0.5f;
        _padData.GridSnap = 0.5f;

        DatabaseFiles.Changed += DatabaseFiles_Changed;
        NotAllowedChilds.Changed += NotAllowedChilds_Changed;
    }

    #endregion

    #region Events

    public event EventHandler Changed;

    //public event EventHandler<MultiUserFileStopWorkingEventArgs> ConnectedControlsStopAllWorking;

    public event EventHandler Loaded;

    public event EventHandler Loading;

    public event EventHandler SavedToDisk;

    #endregion

    #region Properties

    public string Filename => _muf.Filename;

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~ConnectedFormula()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
    public bool IsDisposed { get; private set; }

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

            if (_saving || _muf.IsLoading) { return; }

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

    void IDisposable.Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void HasPendingChanges(object sender, MultiUserFileHasPendingChangesEventArgs e) => e.HasPendingChanges = !_saved;

    public int NextId() {
        _id++;
        _saved = false;
        return _id;
    }

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Prüft, ob das Formular sichtbare Elemente hat.
    /// Zeilenselectionen werden dabei ignoriert.
    /// </summary>
    /// <param name="page">Wird dieser Wert leer gelassen, wird das komplette Formular geprüft</param>
    /// <param name="myGroup"></param>
    /// <param name="myName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    internal bool HasVisibleItemsForMe(string page, string? myGroup, string? myName) {
        if (_padData == null) { return false; }

        foreach (var thisItem in _padData) {
            if (string.IsNullOrEmpty(page) ||
                string.IsNullOrEmpty(thisItem.Page) ||
                page.Equals(thisItem.Page, StringComparison.OrdinalIgnoreCase)) {
                if (thisItem is CustomizableShowPadItem cspi) {
                    if (cspi.IsVisibleForMe(myGroup, myName)) { return true; }
                }
            }
        }

        return false;
    }

    internal void SaveAsAndChangeTo(string fileName) => _muf.SaveAsAndChangeTo(fileName);

    protected virtual void Dispose(bool disposing) {
        if (!IsDisposed) {
            AllFiles.Remove(this);
            if (disposing) {
                _muf.Save(true);
                _muf.Dispose();
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
        if (_saving || _muf.IsLoading) { return; }

        foreach (var thisfile in DatabaseFiles) {
            DatabaseAbstract.GetByID(new ConnectionInfo(thisfile));
        }

        _saved = false;
    }

    private void NotAllowedChilds_Changed(object sender, System.EventArgs e) {
        if (_saving || _muf.IsLoading) { return; }
        _saved = false;
    }

    private void OnLoaded(object sender, System.EventArgs e) {
        foreach (var thisIt in PadData) {
            if (string.IsNullOrEmpty(thisIt.Page)) {
                thisIt.Page = "Head";
            }

            foreach (var thisCon in thisIt.ConnectsTo) {
                thisCon.Bei_Export_sichtbar = false;
            }
        }

        Loaded?.Invoke(this, e);
    }

    private void OnLoading(object sender, System.EventArgs e) => Loading?.Invoke(this, e);

    private void OnSavedToDisk(object sender, System.EventArgs e) {
        _saved = true;
        SavedToDisk?.Invoke(this, e);
    }

    private void PadData_Changed(object sender, System.EventArgs e) {
        if (_saving || _muf.IsLoading) { return; }

        _saved = false;
    }

    private void ToListOfByte(object sender, MultiUserToListEventArgs e) {

        #region ein bischen aufräumen zuvor

        _saving = true;
        //PadData.Sort();

        _id = -1;

        DatabaseFiles.Clear();

        foreach (var thisit in PadData) {
            if (thisit is RowWithFilterPadItem rwf) {
                if (rwf.Database is Database BDB) {
                    DatabaseFiles.AddIfNotExists(BDB.Filename);
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
        //t.GenerateAndAdd("FilePath=" + FilePath.ToNonCritical());
        t.Add("LastUsedID=" + _id.ToString());
        t.Add("DatabaseFiles=" + DatabaseFiles.JoinWithCr().ToNonCritical());
        t.Add("NotAllowedChilds=" + NotAllowedChilds.JoinWithCr().ToNonCritical());
        t.Add("PadItemData=" + PadData.ToString().ToNonCritical());

        e.Data = ("{" + t.JoinWith(", ").TrimEnd(", ") + "}").WIN1252_toByte();
    }

    #endregion
}