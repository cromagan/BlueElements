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
using System.Linq;
using System.Text;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;

namespace BlueBasics;

public class ListExt<T> : List<T>, IChangedFeedback, IDisposableExtended {

    #region Fields

    private bool _throwEvents = true;

    #endregion

    #region Constructors

    public ListExt() { }

    public ListExt(IEnumerable<T> list) : base(list) { }

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~ListExt() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    public event EventHandler<ListEventArgs>? ItemAdded;

    public event EventHandler<ListEventArgs>? ItemInternalChanged;

    public event EventHandler? ItemRemoved;

    public event EventHandler<ListEventArgs>? ItemRemoving;

    public event EventHandler<ListEventArgs>? ItemSeted;

    #endregion

    #region Properties

    public bool IsDisposed { get; private set; }

    public bool ThrowEvents {
        get => !IsDisposed && _throwEvents;
        set {
            if (_throwEvents == value) { Develop.DebugPrint(FehlerArt.Fehler, "Set ThrowEvents-Fehler! " + value.ToPlusMinus()); }
            _throwEvents = value;
        }
    }

    #endregion

    #region Indexers

    public new T this[int index] {
        get {
            if (IsDisposed) { return default; }
            if (index >= Count || index < 0) { Develop.DebugPrint(FehlerArt.Fehler, "Index falsch: " + index + ", Maximum: " + (Count - 1)); }
            return base[index];
        }
        set {
            if (IsDisposed) { return; }

            if (base[index] is string s1 && value is string s2 && s1 == s2) { return; }

            if (base[index] != null) {
                OnItemRemoving(base[index]);
                base[index] = value;
                OnItemRemoved();
            } else {
                base[index] = value;
            }

            if (value != null) {
                OnItemAdded(base[index]);
                OnItemSeted(base[index]);
            }
        }
    }

    #endregion

    #region Methods

    public new void Add(T item) {
        if (IsDisposed) { return; }
        base.Add(item);
        OnItemAdded(item);
    }

    public void AddClonesFrom<T1>(ICollection<T1>? itemstoclone) where T1 : T, ICloneable {
        if (itemstoclone == null || itemstoclone.Count == 0) { return; }

        foreach (var thisItem in itemstoclone) {
            Add((T)thisItem.Clone());
        }
    }

    public bool AddIfNotExists(T item) {
        if (!Contains(item)) {
            Add(item);
            return true;
        }
        return false;
    }

    public new void AddRange(IEnumerable<T>? collection) {
        if (IsDisposed) { return; }
        if (collection is null) { return; }
        // base.AddRange(collection);
        foreach (var item in collection) {
            Add(item);
        }
    }

    /// <summary>
    /// Leert nur die Objekte. Sonstige Einstellungen bleiben erhalten. Für jedes Item wird 'ItemRemoving' ausgelöst. Die Items selbst bleiben unberührt.
    /// </summary>
    public new void Clear() {
        if (Count == 0) { return; }
        foreach (var item in this) {
            OnItemRemoving(item);
        }
        base.Clear();
        OnItemRemoved();
    }

    /// <summary>
    ///  Die Items selbst bleiben unberührt. Es wird kein 'ItemRemoving' ausgelöst. Dazu sollte vorher Clear benutzt werden.
    /// </summary>
    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public new void Insert(int index, T item) {
        if (index > Count || index < 0) { Develop.DebugPrint(FehlerArt.Fehler, "Index falsch: " + index); }
        base.Insert(index, item);
        OnItemAdded(item);
    }

    //#pragma warning disable IDE0060 // Nicht verwendete Parameter entfernen

    public new void InsertRange(int index, IEnumerable<T> collection) => Develop.DebugPrint_NichtImplementiert();

    public virtual void OnChanged() {
        if (!_throwEvents) { return; }
        Changed?.Invoke(this, System.EventArgs.Empty);
    }

    public new void Remove(T item) {
        if (!Contains(item)) { return; }
        OnItemRemoving(item);
        _ = base.Remove(item);
        OnItemRemoved();
    }

    public new void RemoveAll(Predicate<T> match) => Develop.DebugPrint_NichtImplementiert();

    public new void RemoveAt(int index) {
        OnItemRemoving(base[index]);
        base.RemoveAt(index);
        OnItemRemoved();
    }

    public void RemoveRange(IEnumerable<T> collection) {
        foreach (var item in collection) {
            Remove(item);
        }
    }

    public new void RemoveRange(int index, int count) => Develop.DebugPrint_NichtImplementiert();

    public new void Reverse(int index, int count) => Develop.DebugPrint_NichtImplementiert();

    public new void Reverse() {
        base.Reverse();
        OnChanged();
    }

    public new void Sort(int index, int count, IComparer<T> comparer) => Develop.DebugPrint_NichtImplementiert();

    public new void Sort(Comparison<T> comparison) => Develop.DebugPrint_NichtImplementiert();

    public new void Sort() {
        if (Count < 2) { return; }
        List<T> tmp;

        try {
            tmp = new List<T>(this); //Ja, kann Fehler verursachen!
        } catch {
            Sort();
            return;
        }

        base.Sort();

        try {
            if (!tmp.SequenceEqual(this)) {
                OnChanged();
            }
        } catch {
            // Enumeration wurde geändert
            OnChanged(); // Sicherheitshalber
        }
    }

    public new void Sort(IComparer<T> comparer) => Develop.DebugPrint_NichtImplementiert();

    public void Swap(T item1, T item2) {
        var nr1 = IndexOf(item1);
        var nr2 = IndexOf(item2);
        if (nr1 < 0 || nr2 < 0) { Develop.DebugPrint("Swap fehlgeschlagen!"); }
        Swap(nr1, nr2);
    }

    public void Swap(int index1, int index2) {
        if (IsDisposed) { return; }
        if (index1 == index2) { return; }
        // Der Swap geht so, und nicht anders! Es müssen die Items im Original-Array geswapt werden!
        // Wichtig auch der Zugriff auf die base (nicht auf this). Dadurch werden keine Add/Remove Event ausgelöst.
        (base[index1], base[index2]) = (base[index2], base[index1]);
        OnChanged();
    }

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        try {
            if (typeof(IStringable).IsAssignableFrom(typeof(T))) {
                StringBuilder a = new();
                foreach (var thisItem in this) {
                    if (thisItem is IStringable t) {
                        _ = a.Append(t.ToString());
                        _ = a.Append("\r");
                    }
                }
                return a.ToString().TrimCr();
            }
            return base.ToString();
        } catch (Exception ex) {
            Develop.DebugPrint(ex);
            return ToString();
        }
    }

    public new void TrimExcess() => Develop.DebugPrint_NichtImplementiert();

    protected virtual void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            IsDisposed = true;  // Keine Events, fix!
            _throwEvents = false;
            base.Clear();
            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
        }
    }

    protected virtual void OnItemAdded(T item) {
        if (item is IChangedFeedback cItem) { cItem.Changed += CItem_Changed; }
        if (!_throwEvents) { return; }
        ItemAdded?.Invoke(this, new ListEventArgs(item));
        OnChanged();
    }

    protected virtual void OnItemRemoved() {
        if (!_throwEvents) { return; }
        ItemRemoved?.Invoke(this, System.EventArgs.Empty);
        OnChanged();
    }

    /// <summary>
    /// OnListOrItemChanged wird nicht ausgelöst
    /// </summary>
    /// <param name="item"></param>
    protected virtual void OnItemRemoving(T item) {
        if (item is IChangedFeedback cItem) { cItem.Changed -= CItem_Changed; }
        if (!_throwEvents) { return; }
        ItemRemoving?.Invoke(this, new ListEventArgs(item));
        // OnListOrItemChanged(); Wird bei REMOVED ausgelöst
    }

    private void CItem_Changed(object sender, System.EventArgs e) {
        if (!_throwEvents) { return; }
        OnItemInternalChanged(sender);
    }

    private void OnItemInternalChanged(object item) {
        if (!_throwEvents) { return; }
        ItemInternalChanged?.Invoke(this, new ListEventArgs(item));
        OnChanged();
    }

    /// <summary>
    /// OnListOrItemChanged wird nicht ausgelöst
    /// </summary>
    /// <param name="item"></param>
    private void OnItemSeted(T item) {
        if (!_throwEvents) { return; }
        ItemSeted?.Invoke(this, new ListEventArgs(item));
        // OnListOrItemChanged();
    }

    #endregion
}