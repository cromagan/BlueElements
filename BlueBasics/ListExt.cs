#region BlueElements - a collection of useful tools, database and controls
// Authors:
// Christian Peter
//
// Copyright (c) 2020 Christian Peter
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

namespace BlueBasics
{
    public class ListExt<T> : List<T>, IChangedFeedback
    {
        private bool _ThrowEvents = true;

        public event EventHandler<System.EventArgs> ItemRemoved;
        public event EventHandler<ListEventArgs> ItemRemoving;
        public event EventHandler<ListEventArgs> ItemAdded;

        public event EventHandler<ListEventArgs> ItemSeted;
        public event EventHandler<ListEventArgs> ItemInternalChanged;
        public event EventHandler Changed;

        public ListExt()
        {
        }

        public ListExt(IList<T> list) : base(list)
        {
        }

        public bool ThrowEvents
        {
            get => this._ThrowEvents;
            set
            {
                if (this._ThrowEvents == value) { Develop.DebugPrint(enFehlerArt.Fehler, "Set ThrowEvents-Fehler! " + value.ToPlusMinus()); }

                this._ThrowEvents = value;
            }
        }

        public new void Clear()
        {
            if (this.Count == 0) { return; }

            foreach (var item in this)
            {
                this.OnItemRemoving(item);
            }

            base.Clear();
            this.OnItemRemoved();
        }

        public new void Add(T item)
        {
            base.Add(item);
            this.OnItemAdded(item);
        }

        public new void Remove(T item)
        {
            if (!this.Contains(item)) { return; }

            this.OnItemRemoving(item);
            base.Remove(item);
            this.OnItemRemoved();
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            if (collection is null) { return; }

            // base.AddRange(collection);

            foreach (var item in collection)
            {
                this.Add(item);
            }
        }

        public void RemoveRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                this.Remove(item);
            }
        }

        public new void Insert(int index, T item)
        {
            if (index > this.Count || index < 0) { Develop.DebugPrint(enFehlerArt.Fehler, "Index falsch: " + index); }
            base.Insert(index, item);
            this.OnItemAdded(item);
        }

        public new void InsertRange(int index, IEnumerable<T> collection) { Develop.DebugPrint_NichtImplementiert(); }

        public new void RemoveAll(Predicate<T> match) { Develop.DebugPrint_NichtImplementiert(); }

        public new void RemoveAt(int index)
        {
            this.OnItemRemoving(base[index]);
            base.RemoveAt(index);
            this.OnItemRemoved();
        }

        public new void RemoveRange(int index, int count) { Develop.DebugPrint_NichtImplementiert(); }

        public new void Reverse(int index, int count) { Develop.DebugPrint_NichtImplementiert(); }
        public new void Reverse()
        {
            base.Reverse();
            this.OnChanged();
        }

        public new void Sort(int index, int count, IComparer<T> comparer) { Develop.DebugPrint_NichtImplementiert(); }

        public new void Sort(Comparison<T> comparison) { Develop.DebugPrint_NichtImplementiert(); }

        public new void Sort()
        {
            base.Sort();
            this.OnChanged();
        }

        public new void Sort(IComparer<T> comparer) { Develop.DebugPrint_NichtImplementiert(); }

        public new void TrimExcess() { Develop.DebugPrint_NichtImplementiert(); }

        public new T this[int index]
        {
            get
            {
                if (index >= this.Count || index < 0) { Develop.DebugPrint(enFehlerArt.Fehler, "Index falsch: " + index); }
                return base[index];
            }
            set
            {
                if (base[index] != null)
                {
                    this.OnItemRemoving(base[index]);
                    base[index] = value;
                    this.OnItemRemoved();
                }
                else
                {
                    base[index] = value;
                }

                if (value != null)
                {
                    this.OnItemAdded(base[index]);
                    this.OnItemSeted(base[index]);
                }
            }
        }

        protected virtual void OnItemAdded(T item)
        {
            if (item is IChangedFeedback cItem) { cItem.Changed += this.CItem_Changed; }

            if (!this._ThrowEvents) { return; }
            this.ItemAdded?.Invoke(this, new ListEventArgs(item));
            this.OnChanged();
        }

        private void CItem_Changed(object sender, System.EventArgs e)
        {
            if (!this._ThrowEvents) { return; }
            this.OnItemInternalChanged(sender);
        }

        /// <summary>
        /// OnListOrItemChanged wird nicht ausgelöst
        /// </summary>
        /// <param name="item"></param>
        protected virtual void OnItemRemoving(T item)
        {
            if (item is IChangedFeedback cItem) { cItem.Changed -= this.CItem_Changed; }

            if (!this._ThrowEvents) { return; }
            this.ItemRemoving?.Invoke(this, new ListEventArgs(item));
            // OnListOrItemChanged(); Wird bei REMOVED ausgelöst
        }

        protected virtual void OnItemRemoved()
        {
            if (!this._ThrowEvents) { return; }
            this.ItemRemoved?.Invoke(this, System.EventArgs.Empty);
            this.OnChanged();
        }

        /// <summary>
        /// OnListOrItemChanged wird nicht ausgelöst
        /// </summary>
        /// <param name="item"></param>
        private void OnItemSeted(T item)
        {
            if (!this._ThrowEvents) { return; }
            this.ItemSeted?.Invoke(this, new ListEventArgs(item));
            // OnListOrItemChanged();
        }

        private void OnItemInternalChanged(object item)
        {
            if (!this._ThrowEvents) { return; }
            this.ItemInternalChanged?.Invoke(this, new ListEventArgs(item));
            this.OnChanged();
        }

        public virtual void OnChanged()
        {
            if (!this._ThrowEvents) { return; }
            this.Changed?.Invoke(this, System.EventArgs.Empty);
        }

        public void Swap(int index1, int index2)
        {
            // Der Swap geht so, und nicht anders! Es müssen die Items im Original-Array geswapt werden!
            // Wichtig auch der Zugriff auf die base (nicht auf this). Dadurch werden keine Add/Remove Event ausgelöst.
            var tmp = base[index1];
            base[index1] = base[index2];
            base[index2] = tmp;
            this.OnChanged();
        }

        public override string ToString()
        {
            try
            {
                if (typeof(IParseable).IsAssignableFrom(typeof(T)))
                {
                    var a = new System.Text.StringBuilder();

                    foreach (IParseable thisP in this)
                    {
                        if (thisP != null)
                        {
                            a.Append(thisP.ToString());
                            a.Append("\r");
                        }
                    }

                    return a.ToString().TrimCr();
                }

                return base.ToString();
            }
            catch (Exception ex)
            {
                Develop.DebugPrint(ex);
                return this.ToString();
            }
        }
    }
}