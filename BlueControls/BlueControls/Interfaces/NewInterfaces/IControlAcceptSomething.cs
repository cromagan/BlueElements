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

public interface IControlAcceptSomething : IDisposableExtendedWithEvent {

    #region Properties

    /// <summary>
    /// Ein Wert, der bei FilterInput_Changed zumindest neu berechnet oder invalidiert werden muss.
    /// Zum Berechnen sollte die Routine DoInputFilter benutzt werden.
    /// Enthält die DatabaseInput und auch den berechnete Zeile.
    /// </summary>
    public FilterCollection? FilterInput { get; set; }

    /// <summary>
    /// Bedeutet, dass kein Parent vorhanden ist - und der Filter anderweitig gesetzt wurde. Z.B. durch SetRow
    /// Wenn TRUE, sollte der Input Filter nicht mehr von den Parents verändert werden.
    /// </summary>
    public bool FilterManualSeted { get; set; }

    public string Name { get; set; }

    public List<IControlSendSomething> Parents { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Wird ausgelöst, wenn eine relevante Änderung der eingehenen Filter(Daten) erfolgt ist.
    /// Hier können die neuen temporären Filter(Daten) (FilterInput) berechnet werden und sollten auch angezeigt werden und ein Invalidate gesetzt werden
    /// Events können gekoppelt werden
    /// </summary>
    public void FilterInput_Changed(object? sender, System.EventArgs e);

    /// <summary>
    /// Wird ausgelöst, bevor eine relevante Änderung der eingehenden Filter(Daten) erfolgen wird.
    /// Hier können Daten, die angezeigt werden, zurückgeschrieben werden. Events können entkoppelt werden
    /// </summary>
    public void FilterInput_Changing(object sender, System.EventArgs e);

    /// <summary>
    /// Wird ausgelöst, wenn ein Parent hinzugefügt wurde.
    /// Dadurch kann es vorkommen, dass die Filter neu berechnet werden müssen
    /// </summary>
    public void Parents_Added(bool hasFilter);

    #endregion
}

public static class IControlAcceptSomethingExtension {

    #region Methods

    public static void ConnectChildParents(this IControlAcceptSomething child, IControlSendSomething parent) {
        if (child.FilterManualSeted) {
            Develop.DebugPrint(FehlerArt.Fehler, "Manuelle Filterung kann keine Parents empfangen.");
        }

        bool isnew = !child.Parents.Contains(parent);

        if (isnew) { child.Parents.AddIfNotExists(parent); }

        if (parent.Childs.AddIfNotExists(child)) {
            parent.FilterOutput.Changing += child.FilterInput_Changing;
            parent.FilterOutput.Changed += child.FilterInput_Changed;
            parent.FilterOutput.DisposingEvent += FilterOutput_DispodingEvent;
            //child.DisposingEvent += Child_DisposingEvent;
            //parent.DisposingEvent += Parent_DisposingEvent;
        }

        if (isnew) {
            child.Parents_Added(parent.FilterOutput.Count > 0);
        }
    }

    public static void DisconnectChildParents(this IControlAcceptSomething child, List<IControlSendSomething> parents) {
        var p = new List<IControlSendSomething>();
        p.AddRange(parents);

        foreach (var parent in p) {
            child.DisconnectChildParents(parent);
        }
    }

    public static void DisconnectChildParents(this IControlAcceptSomething child, IControlSendSomething parent) {
        child.Parents.Remove(parent);

        if (parent.Childs.Contains(child)) {
            parent.Childs.Remove(child);

            parent.FilterOutput.Changing -= child.FilterInput_Changing;
            parent.FilterOutput.Changed -= child.FilterInput_Changed;
            parent.FilterOutput.DisposingEvent -= FilterOutput_DispodingEvent;
        }
    }

    /// <summary>
    /// Verwirft den aktuellen InputFilter und erstellt einen neuen von allen Parents
    /// </summary>
    /// <param name="item"></param>
    public static void DoInputFilter(this IControlAcceptSomething item) {
        if (item.IsDisposed) { return; }
        if (item.FilterManualSeted) { return; }

        item.Invalidate_FilterInput(true);

        if (item.Parents.Count == 0) { return; }
        if (item.Parents.Count == 1) {
            if (item.Parents[0].FilterOutput.Clone("FilterOfSender") is FilterCollection fc2) {
                item.FilterInput = fc2;
                return;
            }
        }

        FilterCollection? fc = null;

        foreach (var thiss in item.Parents) {
            if (!thiss.IsDisposed && thiss.FilterOutput is FilterCollection fi) {
                fc ??= new FilterCollection(fi.Database, "filterofsedner");
                fc.AddIfNotExists(fi);
            }
        }

        item.FilterInput = fc;
    }

    /// <summary>
    /// Nachdem das Control erzeugt wurde, werden hiermit die Einstellungen vom IItemAcceptSomething übernommen.
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="parent"></param>
    /// <param name="source"></param>
    public static void DoInputSettings(this IControlAcceptSomething dest, ConnectedFormulaView parent, IItemAcceptSomething source) {
        dest.Name = source.DefaultItemToControlName();

        foreach (var thisKey in source.Parents) {
            var it = source.Parent?[thisKey];

            if (it is IItemToControl itc) {
                var ff = parent.SearchOrGenerate(itc);

                if (ff is IControlSendSomething ffx) {
                    dest.ConnectChildParents(ffx);
                }
            }
        }
    }

    /// <summary>
    /// Verwirft den aktuellen InputFilter.
    /// </summary>
    public static void Invalidate_FilterInput(this IControlAcceptSomething item, bool checkmanuelseted) {
        if (item.IsDisposed) { return; }
        if (checkmanuelseted && item.FilterManualSeted) { return; }
        item.FilterInput?.Dispose();
        item.FilterInput = null;
    }

    public static void SetToRow(this IControlAcceptSomething item, RowItem? row) {
        if (item.Parents.Count > 0) {
            Develop.DebugPrint(FehlerArt.Fehler, "Element wird von Parents gesteuert!");
        }

        item.FilterManualSeted = true;

        if (row?.Database == null && item.FilterInput == null) { return; }

        if (row?.Database is Database db && !db.IsDisposed) {
            item.FilterInput ??= new FilterCollection(db, "SetToRow");
            item.FilterInput.Database = db;
        }

        if (item.FilterInput != null) {
            item.FilterInput.Clear();
            if (row == null) {
                item.FilterInput.ChangeTo(new FilterItem());
            } else {
                item.FilterInput.ChangeTo(new FilterItem(row));
            }
        }
        item.FilterInput_Changed(item, System.EventArgs.Empty);
    }

    private static void FilterOutput_DispodingEvent(object sender, System.EventArgs e) {
        if (sender is IControlSendSomething parent) {
            foreach (var child in parent.Childs) {
                child.FilterInput_Changing(parent, System.EventArgs.Empty);
                child.DisconnectChildParents(parent);
                //parent.FilterOutput.Changing -= child.FilterInput_Changing;
                //parent.FilterOutput.Changed -= child.FilterInput_Changed;
                //parent.FilterOutput.DisposingEvent -= FilterOutput_DispodingEvent;
                //child.DisposingEvent += Child_DisposingEvent;
                //item.DisposingEvent += Parent_DisposingEvent;
            }
        }
    }

    #endregion
}