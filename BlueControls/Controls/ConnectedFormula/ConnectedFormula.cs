// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueBasics.Attributes;
using BlueBasics.Classes;
using BlueBasics.Classes.FileSystemCaching;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Classes.ItemCollectionPad.Abstract;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using static BlueBasics.ClassesStatic.Converter;
using static BlueBasics.ClassesStatic.IO;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls.ConnectedFormula;

[FileSuffix(".cfo")]
public sealed class ConnectedFormula : MultiUserFile, IEditable, IReadableTextWithKey, IParseable {

    #region Fields

    private static readonly object _lock = new();
    private static List<string>? _visibleFor_AllUsed;
    private readonly List<string> _notAllowedChilds = [];

    #endregion

    #region Constructors

    internal ConnectedFormula(string filename) : base(filename) { }

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

    #endregion

    #region Properties

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
        get {
            if (!IsParsed) { this.Parse(Constants.Win1252.GetString(Content)); }
            return field;
        }
        private set {
            if (field == value) { return; }

            field?.PropertyChanged -= PadData_PropertyChanged;

            field = value;

            if (field != null) {
                field.Parent = this;
                field.PropertyChanged += PadData_PropertyChanged;
            }

            OnPropertyChanged();
        }
    }

    public string QuickInfo => string.Empty;
    public string Type => "ConnectedFormula";

    /// <summary>
    /// 0.50 seit 08.03.2024
    /// </summary>
    public string Version => "0.50";

    #endregion

    #region Methods

    public static void Invalidate_VisibleFor_AllUsed() {
        lock (_lock) {
            _visibleFor_AllUsed = null;
        }
    }

    // Das Schloss für die Threadsicherheit

    public static List<string> VisibleFor_AllUsed() {
        // Erster Check ohne Lock für die Performance (Double-Check Locking Prinzip)
        if (_visibleFor_AllUsed != null) { return _visibleFor_AllUsed; }

        lock (_lock) {
            // Zweiter Check innerhalb des Locks, falls ein anderer Thread gerade fertig geworden ist
            if (_visibleFor_AllUsed != null) { return _visibleFor_AllUsed; }

            List<string> tempResult = []; // Lokale Liste, um den Cache erst am Ende zu füllen

            foreach (var thisCf in CachedFileSystem.GetAll<ConnectedFormula>()) {
                if (thisCf is { IsDisposed: false, Pages: { IsDisposed: false } icp }) {
                    tempResult.AddRange(icp.VisibleFor_AllUsed());
                }
            }

            _visibleFor_AllUsed = tempResult.SortedDistinctList();
            return _visibleFor_AllUsed;
        }
    }

    public ItemCollectionPadItem? AddPage(string headname) {
        if (Pages is not { IsDisposed: false }) { return null; }

        var p = new ItemCollectionPadItem {
            Caption = headname,
            Breite = 100,
            Höhe = 100
        };

        var it = new RowEntryPadItem();
        p.Add(it);
        Pages.Add(p);

        return p;
    }

    public List<string> AllPages() {
        var p = new List<string>();
        if (Pages == null) { return p; }

        foreach (var thisp in Pages) {
            if (thisp is ItemCollectionPadItem { IsDisposed: false, HasItems: true } icp) {
                p.AddIfNotExists(icp.Caption);
            }
        }

        return p;
    }

    public ItemCollectionPadItem? GetPage(string keyOrCaption) {
        if (!IsParsed) { this.Parse(Constants.Win1252.GetString(Content)); }

        if (Pages is not { IsDisposed: false } pg) { return null; }

        return pg.GetSubItemCollection(keyOrCaption);
    }

    public override void Invalidate() {
        IsParsed = false;
        base.Invalidate();
    }

    public override string IsNowEditable() {
        var f = base.IsNowEditable();
        if (!string.IsNullOrEmpty(f)) { return f; }

        if (!GrantWriteAccess()) { return "Bearbeitung konnte nicht gesetzt werden"; }
        return string.Empty;
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

        if (Pages != null) {
            result.ParseableAdd("Page", Pages as IStringable);
        }

        return result;
    }

    /// <summary>
    /// Wird aufgerufen, wenn die Analyse abgeschlossen ist.
    /// </summary>
    public void ParseFinished(string parsed) {
        IsParsed = true;

        #region Sicherstellen, das Pages initialisiert ist

        if (Pages == null) {
            Pages = [];
            Pages.Breite = 100;
            Pages.Höhe = 100;

            AddPage("Head");
        }

        Pages.Parent = this;

        Pages.BackColor = Skin.Color_Back(Design.Form_Standard, States.Standard);

        #endregion

        #region Sicherstellen, dass in Pages auch nur Seiten sind

        var tmpPages = new List<AbstractPadItem>();
        tmpPages.AddRange(Pages);

        var moveToOtherPage = new List<AbstractPadItem>();

        foreach (var thisIt in tmpPages) {
            if (thisIt is ItemCollectionPadItem { IsDisposed: false } icpi) {
                if (string.IsNullOrEmpty(icpi.Page)) { icpi.Page = "Head"; }
                icpi.Parent = Pages;
            } else {
                Pages.Remove(thisIt);
                moveToOtherPage.Add(thisIt);
            }
        }

        #endregion

        #region Items, die irgendwie in den Pages waren, zu der richtigen Page schieben

        foreach (var thisIt in moveToOtherPage) {

            #region Sicherstellen, dass die Page  vorhanden ist

            var pagen = thisIt.Page;
            if (string.IsNullOrEmpty(pagen)) { pagen = "Head"; }
            var mypage = GetPage(pagen);

            if (mypage == null) {
                mypage = new ItemCollectionPadItem {
                    Caption = pagen,
                    Breite = Pages.Breite,
                    Höhe = Pages.Höhe,
                    SheetStyle = Pages.SheetStyle,
                    RandinMm = Pages.RandinMm,
                    GridShow = Pages.GridShow,
                    GridSnap = Pages.GridSnap,
                    Parent = Pages
                };
                Pages.Add(mypage);
            }

            #endregion

            mypage.Add(thisIt);
        }

        #endregion

        RepairReciver(Pages);

        #region Sicherstellen, dass jede Page ein RowEntryItem hat

        foreach (var thisP in Pages) {
            if (thisP is ItemCollectionPadItem { IsDisposed: false } icpi) {
                if (icpi.IsHead() || icpi.Count() > 0) {
                    var found = icpi.GetRowEntryItem();

                    if (found == null) {
                        found = new RowEntryPadItem();
                        icpi.Add(found);
                    }

                    found.SetCoordinates(new RectangleF(icpi.CanvasUsedArea.Width / 2 - 150, -30, 300, 30));
                    found.Bei_Export_sichtbar = false;
                }
            }
        }

        #endregion

        if (Pages != null) {
            foreach (var page in Pages) {
                if (page is ItemCollectionPadItem { IsDisposed: false } icp) {
                    icp.GridShow = PixelToMm(AutosizableExtension.GridSize, ItemCollectionPadItem.Dpi);
                    icp.GridSnap = PixelToMm(AutosizableExtension.GridSize, ItemCollectionPadItem.Dpi);
                }
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
                var tmpPages = new ItemCollectionPadItem();
                tmpPages.Parse(value.FromNonCritical());
                Pages = tmpPages;
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

        foreach (var thisf in CachedFileSystem.GetAll<ConnectedFormula>()) {
            if (!notAllowedChilds.Contains(thisf.Filename)) {
                if (list.GetByKey(thisf.Filename) == null) {
                    list.Add(ItemOf(thisf.Filename.FileNameWithoutSuffix(), thisf.Filename, ImageCode.Diskette));
                }
            }
        }

        if (Pages != null) {
            foreach (var thisf in Pages) {
                if (thisf is ItemCollectionPadItem { IsDisposed: false, HasItems: true } icpi) {
                    if (!notAllowedChilds.Contains(icpi.KeyName) && !icpi.IsHead()) {
                        list.Add(ItemOf(icpi));
                    }
                }
            }
        }

        return list;
    }

    internal bool IsEditing() {
        var e = new EditingEventArgs();

        OnEditing(e);

        return e.Editing;
    }

    /// <summary>
    /// Ruft das Editing-Ereignis auf.
    /// </summary>
    protected void OnEditing(EditingEventArgs e) => Editing?.Invoke(this, e);

    /// <summary>
    /// Ruft das PropertyChanged-Ereignis auf und markiert die Datei als ungespeichert.
    /// </summary>
    private void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") {
        if (IsDisposed) { return; }
        if (IsSaving || IsLoading || !IsParsed) { return; }

        if (!GrantWriteAccess()) {
            Develop.DebugError( $"Keine Änderungen an der Datei '{Filename.FileNameWithoutSuffix()}' möglich ({propertyName})!");
            return;
        }

        var text = ParseableItems().FinishParseable();
        Content = Constants.Win1252.GetBytes(text);

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void PadData_PropertyChanged(object sender, PropertyChangedEventArgs e) => OnPropertyChanged();

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