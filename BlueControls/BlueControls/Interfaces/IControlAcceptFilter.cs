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

using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueDatabase;

namespace BlueControls.Interfaces;

public interface IControlAcceptFilter : IDisposableExtendedWithEvent {

    #region Properties

    /// <summary>
    /// Ein Wert, der bei ParentFilterOutput_Changed zumindest neu berechnet oder invalidiert werden muss.
    /// Zum Berechnen sollte die Routine DoInputFilter benutzt werden.
    /// Enthält die DatabaseInput und auch den berechnete Zeile.
    /// </summary>
    public FilterCollection? FilterInput { get; set; }

    public bool FilterInputChangedHandled { get; set; }

    public string Name { get; set; }

    public List<IControlSendFilter> Parents { get; }

    #endregion

    #region Methods

    public void FilterInput_DispodingEvent(object sender, System.EventArgs e);

    public void FilterInput_RowsChanged(object sender, System.EventArgs e);

    public void HandleChangesNow();

    public void Invalidate();

    /// <summary>
    /// Wird ausgelöst, wenn eine relevante Änderung der eingehenen Filter(Daten) erfolgt ist.
    /// Hier können die neuen temporären Filter(Daten) (FilterInput) berechnet werden und sollten auch angezeigt werden und ein Invalidate gesetzt werden
    /// Events können gekoppelt werden
    /// </summary>
    public void ParentFilterOutput_Changed();

    #endregion
}

public static class ControlAcceptFilterExtension {

    #region Methods

    public static void ConnectChildParents(this IControlAcceptFilter child, IControlSendFilter parent) {
        if (child is IControlUsesRow icur && icur.RowsInputManualSeted) {
            Develop.DebugPrint(FehlerArt.Fehler, "Manuelle Filterung kann keine Parents empfangen.");
        }

        if (parent.IsDisposed) { return; }
        if (child.IsDisposed) { return; }

        var isnew = !child.Parents.Contains(parent);
        var newFilters = parent.FilterOutput.Count > 0;
        var doDatabaseAfter = false;

        if (child is IControlUsesRow icur4) {
            doDatabaseAfter = icur4.Database() == null;
        }

        if (isnew) {
            child.Parents.AddIfNotExists(parent);
        }

        parent.Childs.AddIfNotExists(child);

        if (newFilters && isnew) {
            if (child is IControlUsesRow icur3) {
                icur3.RowsInput_Changed();
            }

            child.ParentFilterOutput_Changed();
        }

        if (doDatabaseAfter) {
            if (child is IControlUsesRow icur5) {
                icur5.RegisterEvents();
            }
        }
    }

    public static void DisconnectChildParents(this IControlAcceptFilter child, List<IControlSendFilter> parents) {
        var p = new List<IControlSendFilter>();
        p.AddRange(parents);

        foreach (var parent in p) {
            child.DisconnectChildParents(parent);
        }
    }

    public static void DisconnectChildParents(this IControlAcceptFilter child, IControlSendFilter parent) {
        child.Parents.Remove(parent);

        if (parent.Childs.Contains(child)) {
            parent.Childs.Remove(child);
        }
    }

    public static void DoDispose(this IControlAcceptFilter child) {
        child.Invalidate_FilterInput();
        child.DisconnectChildParents(child.Parents);
    }

    /// <summary>
    /// Verwirft den aktuellen InputFilter und erstellt einen neuen von allen Parents
    /// </summary>
    /// <param name="item"></param>
    /// <param name="mustbeDatabase"></param>
    /// <param name="doEmptyFilterToo"></param>
    public static void DoInputFilter(this IControlAcceptFilter item, Database? mustbeDatabase, bool doEmptyFilterToo) {
        if (item.IsDisposed) { return; }

        item.FilterInput = item.GetInputFilter(mustbeDatabase, doEmptyFilterToo);

        if (item.FilterInput != null && item.FilterInput.Database == null) { Develop.DebugPrint(FehlerArt.Fehler, "Datenbank Fehler"); }
    }

    /// <summary>
    /// Nachdem das Control erzeugt wurde, werden hiermit die Einstellungen vom IItemAcceptFilter übernommen.
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="parent"></param>
    /// <param name="source"></param>
    public static void DoInputSettings(this IControlAcceptFilter dest, ConnectedFormulaView parent, IItemAcceptFilter source) {
        dest.Name = source.DefaultItemToControlName();

        foreach (var thisKey in source.Parents) {
            var it = source.Parent?[thisKey];

            if (it is IItemToControl itc) {
                var ff = parent.SearchOrGenerate(itc);

                if (ff is IControlSendFilter ffx) {
                    dest.ConnectChildParents(ffx);
                }
            }
        }
    }

    public static void FilterInput_DispodingEvent(this IControlAcceptFilter icaf) {
        icaf.UnRegisterEventsAndDispose();

        if (icaf.FilterInput != null && !icaf.FilterInput.IsDisposed) {
            icaf.FilterInput.Database = null;
            icaf.FilterInput.Dispose();
        }
    }

    public static FilterCollection? GetInputFilter(this IControlAcceptFilter item, Database? mustbeDatabase, bool doEmptyFilterToo) {
        if (item.Parents.Count == 0) {
            if (doEmptyFilterToo && mustbeDatabase != null) {
                return new FilterCollection(mustbeDatabase, "Empty Input Filter");
            }
            return null;
        }

        if (item.Parents.Count == 1) {
            var fc2 = item.Parents[0].FilterOutput;
            if (fc2.Count == 0) { return null; }

            if (mustbeDatabase != null && fc2.Database != mustbeDatabase) {
                return new FilterCollection(new FilterItem(mustbeDatabase, "Datenbanken inkonsitent 1"), "Datenbanken inkonsitent");
            }

            return fc2;
        }

        FilterCollection? fc = null;

        foreach (var thiss in item.Parents) {
            if (!thiss.IsDisposed && thiss.FilterOutput is FilterCollection fi) {
                if (mustbeDatabase != null && fi.Database != mustbeDatabase) {
                    fc?.Dispose();
                    return new FilterCollection(new FilterItem(mustbeDatabase, "Datenbanken inkonsitent 2"), "Datenbanken inkonsitent");
                }

                fc ??= new FilterCollection(fi.Database, "filterofsender");
                fc.AddIfNotExists(fi);
            }
        }

        return fc;
    }

    /// <summary>
    /// Verwirft den aktuellen InputFilter.
    /// </summary>
    public static void Invalidate_FilterInput(this IControlAcceptFilter item) {
        if (item.IsDisposed) { return; }

        item.FilterInputChangedHandled = false;
    }

    public static void RegisterEvents(this IControlAcceptFilter icaf) {
        if (icaf.FilterInput == null || icaf.FilterInput.IsDisposed) { return; }
        icaf.FilterInput.RowsChanged += icaf.FilterInput_RowsChanged;
        //icaf.FilterInput.Changed += icaf.FilterOutput_Changed;
        icaf.FilterInput.DisposingEvent += icaf.FilterInput_DispodingEvent;
    }

    public static void UnRegisterEventsAndDispose(this IControlAcceptFilter icaf) {
        if (icaf.FilterInput == null) { return; }
        icaf.FilterInput.RowsChanged -= icaf.FilterInput_RowsChanged;
        //icaf.FilterInput.Changed -= icaf.FilterOutput_Changed;
        icaf.FilterInput.DisposingEvent -= icaf.FilterInput_DispodingEvent;

        if (icaf.Parents.Count != 1 && icaf.FilterInput != null) {
            icaf.FilterInput.Dispose();
        }
    }

    #endregion
}