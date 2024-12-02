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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueBasics.MultiUserFile;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad;
using BlueControls.ItemCollectionPad.Abstract;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using static BlueBasics.Converter;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ConnectedFormula;

public sealed class ConnectedFormula : MultiUserFile, IEditable, IReadableTextWithKey {

    #region Fields

    public static readonly ObservableCollection<ConnectedFormula> AllFiles = [];

    private readonly List<string> _notAllowedChilds = [];

    private ItemCollectionPadItem? _pages;

    #endregion

    #region Constructors

    public ConnectedFormula() : this(string.Empty) { }

    private ConnectedFormula(string filename) : base() {
        AllFiles.Add(this);

        if (FileExists(filename)) {
            Load(filename, true);
        }

        if (_pages != null) {
            foreach (var page in _pages) {
                if (page is ItemCollectionPadItem { IsDisposed: false } icp) {
                    icp.GridShow = PixelToMm(AutosizableExtension.GridSize, ItemCollectionPadItem.Dpi);
                    icp.GridSnap = PixelToMm(AutosizableExtension.GridSize, ItemCollectionPadItem.Dpi);
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

    public ItemCollectionPadItem? Pages {
        get => _pages;
        private set {
            if (_pages == value) { return; }

            if (_pages != null) {
                _pages.PropertyChanged -= PadData_PropertyChanged;
            }

            _pages = value;

            if (_pages != null) {
                _pages.Parent = this;
                _pages.PropertyChanged += PadData_PropertyChanged;
            }

            OnPropertyChanged();
        }
    }

    public string QuickInfo => string.Empty;
    public override string Type => "ConnectedFormula";

    /// <summary>
    // 0.50 seit 08.03.2024
    /// </summary>
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

    public static List<string> VisibleFor_AllUsed() {
        var l = new List<string>();

        foreach (var thisCf in AllFiles) {
            if (thisCf is { IsDisposed: false, _pages: { IsDisposed: false } icp }) {
                l.AddRange(icp.VisibleFor_AllUsed());
            }
        }

        return l.SortedDistinctList();
    }

    public List<string> AllPages() {
        var p = new List<string>();
        if (Pages == null) { return p; }

        foreach (var thisp in Pages) {
            if (thisp is ItemCollectionPadItem { IsDisposed: false } icp) {
                _ = p.AddIfNotExists(icp.Caption);
            }
        }

        return p;
    }

    public ItemCollectionPadItem? GetPage(string keyOrCaption) {
        if (Pages is not { IsDisposed: false } pg) { return null; }

        return pg.GetSubItemCollection(keyOrCaption);
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

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);

        Repair();
    }

    public override bool ParseThis(string key, string value) {
        switch (key.ToLowerInvariant()) {
            case "notallowedchilds":
                _notAllowedChilds.Clear();
                _notAllowedChilds.AddRange(value.FromNonCritical().SplitByCrToList());
                return true;

            case "page":
            case "paditemdata":
                var tmpPages = new ItemCollectionPadItem();
                tmpPages.Parse(value.FromNonCritical());
                Pages = tmpPages;
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

        #region Sicherstellen, das Pages initialisiert ist

        if (Pages == null) {
            Pages = new ItemCollectionPadItem();
            Pages.Breite = 100;
            Pages.Höhe = 100;
        }

        Pages.Parent = this;

        Pages.BackColor = Skin.Color_Back(Design.Form_Standard, States.Standard);

        #endregion

        #region Sicherstellen, dass in Pages auch nur Seiten sind

        var tmpPages = new List<AbstractPadItem>();
        tmpPages.AddRange(Pages);

        var moveToHead = new List<AbstractPadItem>();

        foreach (var thisIt in tmpPages) {
            if (thisIt is ItemCollectionPadItem { IsDisposed: false } icpi) {
                if (string.IsNullOrEmpty(icpi.Page)) { icpi.Page = "Head"; }
                icpi.Parent = Pages;
            } else {
                Pages.Remove(thisIt);
                moveToHead.Add(thisIt);
            }
        }

        #endregion

        #region Sicherstellen, dass die Page "Head" vorhanden ist

        var foundhead = GetPage("Head");

        if (foundhead == null) {
            foundhead = new ItemCollectionPadItem {
                Caption = "Head",
                Breite = Pages.Breite,
                Höhe = Pages.Höhe,
                SheetStyle = Pages.SheetStyle,
                RandinMm = Pages.RandinMm,
                GridShow = Pages.GridShow,
                GridSnap = Pages.GridSnap,
                Parent = Pages
            };
            Pages.Add(foundhead);
        }

        #endregion

        #region Items, die irgendwie in den Pages waren, zum Head schieben

        foreach (var thisIt in moveToHead) {
            foundhead.Add(thisIt);
        }

        #endregion

        RepairReciver(Pages);

        #region Sicherstellen, dass jede Page ein RowEntryItem hat

        foreach (var thisP in Pages) {
            if (thisP is ItemCollectionPadItem { IsDisposed: false } icp) {
                if (icp.Caption.Equals("Head", StringComparison.OrdinalIgnoreCase) || icp.Count() > 0) {
                    var found = icp.GetRowEntryItem();

                    if (found == null) {
                        found = new RowEntryPadItem();
                        icp.Add(found);
                    }

                    found.SetCoordinates(new RectangleF((icp.UsedArea.Width / 2) - 150, -30, 300, 30));
                    found.Bei_Export_sichtbar = false;
                }
            }
        }

        #endregion
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
            foreach (var thisf in _pages) {
                if (thisf is ItemCollectionPadItem { IsDisposed: false } icp) {
                    if (!notAllowedChilds.Contains(icp.KeyName) && !string.Equals("Head", icp.Caption, StringComparison.OrdinalIgnoreCase)) {
                        list.Add(ItemOf(icp));
                    }
                }
            }
        }
    }

    internal bool IsEditing() {
        var e = new EditingEventArgs();

        OnEditing(e);

        return e.Editing;
    }

    protected override void OnLoaded() {
        Repair();
        base.OnLoaded();
    }

    private void PadData_PropertyChanged(object sender, System.EventArgs e) => OnPropertyChanged();

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

    #endregion
}