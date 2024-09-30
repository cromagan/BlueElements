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
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using static BlueBasics.Converter;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ConnectedFormula;

public sealed class ConnectedFormula : MultiUserFile, IEditable, IReadableTextWithKey {

    #region Fields

    public const float StandardHöhe = 1.75f;

    public static readonly ObservableCollection<ConnectedFormula> AllFiles = [];

    private readonly List<string> _notAllowedChilds = [];

    private ItemCollectionPad.ItemCollectionPad? _pages;

    #endregion

    #region Constructors

    public ConnectedFormula() : this(string.Empty) { }

    private ConnectedFormula(string filename) : base() {
        AllFiles.Add(this);

        if (FileExists(filename)) {
            Load(filename, true);
        }

        if (_pages != null) {
            foreach (var page in _pages.Items) {
                if (page is ItemCollectionPad.ItemCollectionPad icp) {
                    icp.GridShow = PixelToMm(AutosizableExtension.GridSize, ItemCollectionPad.ItemCollectionPad.Dpi);
                    icp.GridSnap = PixelToMm(AutosizableExtension.GridSize, ItemCollectionPad.ItemCollectionPad.Dpi);
                }
            }
        }
        Repair();
    }

    #endregion

    #region Properties

    public string CaptionForEditor => "Formular";

    public Type? Editor { get; set; }

    public ReadOnlyCollection<string> NotAllowedChilds {
        get => new(_notAllowedChilds);
        set {
            var l = new List<string>(value).SortedDistinctList();
            if (!_notAllowedChilds.IsDifferentTo(l)) { return; }

            _notAllowedChilds.Clear();
            _notAllowedChilds.AddRange(l);
            OnPropertyChanged();
        }
    }

    public ItemCollectionPad.ItemCollectionPad? Pages {
        get => _pages;
        private set {
            if (_pages == value) { return; }
            UnRegisterPadDataEvents();

            _pages = value;

            RegisterPadDataEvents();

            OnPropertyChanged();
        }
    }

    public string QuickInfo => string.Empty;
    public override string Type => "ConnectedFormula";
    public override string Version => "0.50";

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

    // 0.50 seit 08.03.2024

    public static List<string> VisibleFor_AllUsed() {
        var l = new List<string>();

        foreach (var thisCf in AllFiles) {
            if (thisCf is { IsDisposed: false, _pages: { IsDisposed: false } icp }) {
                l.AddRange(icp.VisibleFor_AllUsed());
            }
        }

        return l.SortedDistinctList();
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("NotAllowedChilds", _notAllowedChilds, false);

        if (Pages != null) {
            result.ParseableAdd("Page", Pages as IStringable);
        }

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key.ToLowerInvariant()) {
            case "notallowedchilds":
                _notAllowedChilds.Clear();
                _notAllowedChilds.AddRange(value.FromNonCritical().SplitByCrToList());
                return true;

            case "paditemdata":
                UnRegisterPadDataEvents();
                _pages = new ItemCollectionPad.ItemCollectionPad();
                _pages.Parse(value.FromNonCritical());
                _pages.Parent = null;
                RegisterPadDataEvents();
                return true;

            case "databasefiles":
            case "lastusedid":
            case "events":
            case "variables":
                return true;
        }
        return base.ParseThis(key, value);
    }

    public string ReadableText() {
        if (!string.IsNullOrWhiteSpace(Filename)) { return Filename.FileNameWithoutSuffix(); }

        return string.Empty;
    }

    public void Repair() {
        Pages ??= new ItemCollectionPad.ItemCollectionPad();

        Pages.BackColor = Skin.Color_Back(Design.Form_Standard, States.Standard);

        //foreach (var thisIt in Pages.Items) {
        //    Develop.DebugPrint_NichtImplementiert();
        //    if (thisIt is ReciverControlPadItem itcf) {
        //        itcf.ParentFormula = this;
        //    }
        //}

        Develop.DebugPrint_NichtImplementiert(true); // TODO: Head erstellen
        //var pg = Pages.AllPages();
        //pg.AddIfNotExists("Head");

        foreach (var thisP in Pages.Items) {
            if (thisP is ItemCollectionPad.ItemCollectionPad icp) {
                RowEntryPadItem? found = null;

                foreach (var thisit in icp.Items) {
                    if (thisit is RowEntryPadItem repi) {
                        found = repi; break;
                    }
                }

                if (found == null) {
                    found = new RowEntryPadItem();
                    icp.Add(found);
                }

                found.SetCoordinates(new RectangleF((icp.SheetSizeInPix.Width / 2) - 150, -30, 300, 30), true);
                found.Bei_Export_sichtbar = false;
            }
        }
    }

    public QuickImage SymbolForReadableText() {
        if (!string.IsNullOrWhiteSpace(Filename)) { return QuickImage.Get(ImageCode.Diskette, 16); }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    /// <summary>
    /// Leert die eingehende Liste und fügt alle bekannten Fomulare hinzu - außer die in notAllowedChilds
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

        if (_pages != null) {
            foreach (var thisf in _pages.Items) {
                if (thisf is ItemCollectionPad.ItemCollectionPad icp) {
                    if (!notAllowedChilds.Contains(icp.KeyName) && !string.Equals("Head", icp.Caption, StringComparison.OrdinalIgnoreCase)) {
                        list.Add(ItemOf(icp));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Prüft, ob das Formular sichtbare Elemente hat.
    /// Zeilenselectionen werden dabei ignoriert.
    /// </summary>
    /// <param name="page">Wird dieser Wert leer gelassen, wird das komplette Formular geprüft</param>
    /// <returns></returns>
    internal bool HasVisibleItemsForMe(string page, string mode) {
        if (Pages == null || Pages.Items.Count == 0) { return false; }

        foreach (var thisItem in Pages.Items) {
            if (thisItem is ItemCollectionPad.ItemCollectionPad icp) {
                if (string.IsNullOrEmpty(page) ||
                    string.IsNullOrEmpty(icp.Caption) ||
                    page.Equals(icp.Caption, StringComparison.OrdinalIgnoreCase)) {
                    if (thisItem is ReciverControlPadItem { MustBeInDrawingArea: true } cspi) {
                        if (cspi.IsVisibleForMe(mode, false)) { return true; }
                    }
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

    protected override void OnLoaded(object sender, System.EventArgs e) {
        Repair();
        base.OnLoaded(sender, e);
    }

    private void PadData_PropertyChanged(object sender, System.EventArgs e) => OnPropertyChanged();

    private void RegisterPadDataEvents() {
        if (_pages != null) {
            _pages.PropertyChanged += PadData_PropertyChanged;
        }
    }

    private void UnRegisterPadDataEvents() {
        if (_pages != null) {
            _pages.PropertyChanged -= PadData_PropertyChanged;
        }
    }

    #endregion
}