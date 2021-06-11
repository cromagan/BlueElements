#region BlueElements - a collection of useful tools, database and controls
// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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
#endregion
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
namespace BlueBasics {
    public class ListExt<T> : List<T>, IChangedFeedback, IDisposable {
        private bool _ThrowEvents = true;
        public bool Disposed { get; private set; } = false;
        public event EventHandler<System.EventArgs> ItemRemoved;
        public event EventHandler<ListEventArgs> ItemRemoving;
        public event EventHandler<ListEventArgs> ItemAdded;
        public event EventHandler<ListEventArgs> ItemSeted;
        public event EventHandler<ListEventArgs> ItemInternalChanged;
        public event EventHandler Changed;
        public ListExt() {
        }
        public ListExt(IList<T> list) : base(list) {
        }
        public bool ThrowEvents {
            get => !Disposed && _ThrowEvents;
            set {
                if (_ThrowEvents == value) { Develop.DebugPrint(enFehlerArt.Fehler, "Set ThrowEvents-Fehler! " + value.ToPlusMinus()); }
                _ThrowEvents = value;
            }
        }
        /// <summary>
        ///  Leert nur die Objekte. Sonstige Einstellungen und die ID bleiben erhalten
        /// </summary>
        public new void Clear() {
            if (Count == 0) { return; }
            foreach (var item in this) {
                OnItemRemoving(item);
            }
            base.Clear();
            OnItemRemoved();
        }
        public new void Add(T item) {
            Develop.DebugPrint_Disposed(Disposed);
            base.Add(item);
            OnItemAdded(item);
        }
        public new void Remove(T item) {
            if (!Contains(item)) { return; }
            OnItemRemoving(item);
            base.Remove(item);
            OnItemRemoved();
        }
        public new void AddRange(IEnumerable<T> collection) {
            Develop.DebugPrint_Disposed(Disposed);
            if (collection is null) { return; }
            // base.AddRange(collection);
            foreach (var item in collection) {
                Add(item);
            }
        }
        public void RemoveRange(IEnumerable<T> collection) {
            foreach (var item in collection) {
                Remove(item);
            }
        }
        public new void Insert(int index, T item) {
            if (index > Count || index < 0) { Develop.DebugPrint(enFehlerArt.Fehler, "Index falsch: " + index); }
            base.Insert(index, item);
            OnItemAdded(item);
        }
        public new void InsertRange(int index, IEnumerable<T> collection) => Develop.DebugPrint_NichtImplementiert();
        public new void RemoveAll(Predicate<T> match) => Develop.DebugPrint_NichtImplementiert();
        public new void RemoveAt(int index) {
            OnItemRemoving(base[index]);
            base.RemoveAt(index);
            OnItemRemoved();
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
            base.Sort();
            OnChanged();
        }
        public new void Sort(IComparer<T> comparer) => Develop.DebugPrint_NichtImplementiert();
        public new void TrimExcess() => Develop.DebugPrint_NichtImplementiert();
        public new T this[int index] {
            get {
                Develop.DebugPrint_Disposed(Disposed);
                if (index >= Count || index < 0) { Develop.DebugPrint(enFehlerArt.Fehler, "Index falsch: " + index); }
                return base[index];
            }
            set {
                Develop.DebugPrint_Disposed(Disposed);
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
        protected virtual void OnItemAdded(T item) {
            if (item is IChangedFeedback cItem) { cItem.Changed += CItem_Changed; }
            if (!_ThrowEvents) { return; }
            ItemAdded?.Invoke(this, new ListEventArgs(item));
            OnChanged();
        }
        private void CItem_Changed(object sender, System.EventArgs e) {
            if (!_ThrowEvents) { return; }
            OnItemInternalChanged(sender);
        }
        /// <summary>
        /// OnListOrItemChanged wird nicht ausgelöst
        /// </summary>
        /// <param name="item"></param>
        protected virtual void OnItemRemoving(T item) {
            if (item is IChangedFeedback cItem) { cItem.Changed -= CItem_Changed; }
            if (!_ThrowEvents) { return; }
            ItemRemoving?.Invoke(this, new ListEventArgs(item));
            // OnListOrItemChanged(); Wird bei REMOVED ausgelöst
        }
        protected virtual void OnItemRemoved() {
            if (!_ThrowEvents) { return; }
            ItemRemoved?.Invoke(this, System.EventArgs.Empty);
            OnChanged();
        }
        /// <summary>
        /// OnListOrItemChanged wird nicht ausgelöst
        /// </summary>
        /// <param name="item"></param>
        private void OnItemSeted(T item) {
            if (!_ThrowEvents) { return; }
            ItemSeted?.Invoke(this, new ListEventArgs(item));
            // OnListOrItemChanged();
        }
        private void OnItemInternalChanged(object item) {
            if (!_ThrowEvents) { return; }
            ItemInternalChanged?.Invoke(this, new ListEventArgs(item));
            OnChanged();
        }
        public virtual void OnChanged() {
            if (!_ThrowEvents) { return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }
        public void Swap(T item1, T item2) {
            var nr1 = IndexOf(item1);
            var nr2 = IndexOf(item2);
            if (nr1 < 0 || nr2 < 0) { Develop.DebugPrint("Swap fehlgeschlagen!"); }
            Swap(nr1, nr2);
        }
        public void Swap(int index1, int index2) {
            Develop.DebugPrint_Disposed(Disposed);
            if (index1 == index2) { return; }
            // Der Swap geht so, und nicht anders! Es müssen die Items im Original-Array geswapt werden!
            // Wichtig auch der Zugriff auf die base (nicht auf this). Dadurch werden keine Add/Remove Event ausgelöst.
            var tmp = base[index1];
            base[index1] = base[index2];
            base[index2] = tmp;
            OnChanged();
        }
        public override string ToString() {
            Develop.DebugPrint_Disposed(Disposed);
            try {
                if (typeof(IParseable).IsAssignableFrom(typeof(T))) {
                    System.Text.StringBuilder a = new();
                    foreach (IParseable thisP in this) {
                        if (thisP != null) {
                            a.Append(thisP.ToString());
                            a.Append("\r");
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
        protected virtual void Dispose(bool disposing) {
            if (!Disposed) {
                if (disposing) {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                }
                Disposed = true;  // Keine Events, fix!
                _ThrowEvents = false;
                base.Clear();
                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
            }
        }
        // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        ~ListExt() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: false);
        }
        public void Dispose() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}