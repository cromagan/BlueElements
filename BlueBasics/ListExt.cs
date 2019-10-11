#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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

using System;
using System.Collections.Generic;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;

namespace BlueBasics
{
    public class ListExt<T> : List<T>
    {
        private bool _ThrowEvents = true;

        public event EventHandler<System.EventArgs> ItemRemoved;
        public event EventHandler<ListEventArgs> ItemRemoving;
        public event EventHandler<ListEventArgs> ItemAdded;

        public event EventHandler<ListEventArgs> ItemSeted;
        public event EventHandler<ListEventArgs> ItemInternalChanged;
        public event EventHandler<System.EventArgs> ListOrItemChanged;


        public ListExt()
        {
        }

        public ListExt(IList<T> list) : base(list)
        {
        }


        public bool ThrowEvents
        {
            get
            {
                return _ThrowEvents;
            }
            set
            {

                if (_ThrowEvents == value) { Develop.DebugPrint(enFehlerArt.Fehler, "Set ThrowEvents-Fehler! "); }

                _ThrowEvents = value;
            }
        }


        public new void Clear()
        {
            if (Count == 0) { return; }

            foreach (var item in this)
            {
                OnItemRemoving(item);
            }
            base.Clear();
            OnItemRemoved();
        }

        public new void Add(T item)
        {
            base.Add(item);
            OnItemAdded(item);
        }



        public new void Remove(T item)
        {
            OnItemRemoving(item);
            base.Remove(item);
            OnItemRemoved();
        }


        public new void AddRange(IEnumerable<T> collection)
        {
            base.AddRange(collection);

            foreach (var item in collection)
            {
                OnItemAdded(item);
            }

        }

        public new void Insert(int index, T item)
        {
            if (index > Count || index < 0) { Develop.DebugPrint(enFehlerArt.Fehler, "Index falsch!"); }
            base.Insert(index, item);
            OnItemAdded(item);
        }

        public new void InsertRange(int index, IEnumerable<T> collection) { Develop.DebugPrint_NichtImplementiert(); }


        public new void RemoveAll(Predicate<T> match) { Develop.DebugPrint_NichtImplementiert(); }

        public new void RemoveAt(int index)
        {
            OnItemRemoving(base[index]);
            base.RemoveAt(index);
            OnItemRemoved();
        }

        public new void RemoveRange(int index, int count) { Develop.DebugPrint_NichtImplementiert(); }

        public new void Reverse(int index, int count) { Develop.DebugPrint_NichtImplementiert(); }
        public new void Reverse() { Develop.DebugPrint_NichtImplementiert(); }

        public new void Sort(int index, int count, IComparer<T> comparer) { Develop.DebugPrint_NichtImplementiert(); }

        public new void Sort(Comparison<T> comparison) { Develop.DebugPrint_NichtImplementiert(); }


        public new void Sort()
        {
            base.Sort();
            OnListOrItemChanged();
        }

        public new void Sort(IComparer<T> comparer) { Develop.DebugPrint_NichtImplementiert(); }

        public new void TrimExcess() { Develop.DebugPrint_NichtImplementiert(); }

        public new T this[int index]
        {
            get
            {
                if (index >= Count || index < 0) { Develop.DebugPrint(enFehlerArt.Fehler, "Index falsch!"); }
                return base[index];
            }
            set
            {

                if (base[index] != null)
                {
                    OnItemRemoving(base[index]);
                    base[index] = value;
                    OnItemRemoved();
                }
                else
                {
                    base[index] = value;
                }


                if (value != null)
                {
                    OnItemAdded(base[index]);
                    OnItemSeted(base[index]);
                }

            }
        }

        protected virtual void OnItemAdded(T item)
        {
            if (item is IChangedFeedback cItem) { cItem.Changed += CItem_Changed; }


            if (!_ThrowEvents) { return; }
            ItemAdded?.Invoke(this, new ListEventArgs(item));
            OnListOrItemChanged();
        }

        private void CItem_Changed(object sender, System.EventArgs e)
        {
            if (!_ThrowEvents) { return; }
            OnItemInternalChanged(sender);
        }

        /// <summary>
        /// OnListOrItemChanged wird nicht ausgelöst
        /// </summary>
        /// <param name="item"></param>
        protected virtual void OnItemRemoving(T item)
        {
            if (item is IChangedFeedback cItem) { cItem.Changed -= CItem_Changed; }

            if (!_ThrowEvents) { return; }
            ItemRemoving?.Invoke(this, new ListEventArgs(item));
            // OnListOrItemChanged(); Wird bei REMOVED ausgelöst
        }



        protected virtual void OnItemRemoved()
        {


            if (!_ThrowEvents) { return; }
            ItemRemoved?.Invoke(this, System.EventArgs.Empty);
            OnListOrItemChanged();
        }


        /// <summary>
        /// OnListOrItemChanged wird nicht ausgelöst
        /// </summary>
        /// <param name="item"></param>
        private void OnItemSeted(T item)
        {
            if (!_ThrowEvents) { return; }
            ItemSeted?.Invoke(this, new ListEventArgs(item));
            //OnListOrItemChanged();
        }

        private void OnItemInternalChanged(object item)
        {
            if (!_ThrowEvents) { return; }
            ItemInternalChanged?.Invoke(this, new ListEventArgs(item));
            OnListOrItemChanged();
        }

        protected virtual void OnListOrItemChanged()
        {
            if (!_ThrowEvents) { return; }
            ListOrItemChanged?.Invoke(this, System.EventArgs.Empty);
        }

        public void Swap(int Index1, int Index2)
        {

            // Der Swap geht so, und nicht anders! Es müssen die Items im Original-Array geswapt werden!
            // Wichtig auch der Zugriff auf die base (nicht auf this). Dadurch werden keine Add/Remove Event ausgelöst.
            var tmp = base[Index1];
            base[Index1] = base[Index2];
            base[Index2] = tmp;
            OnListOrItemChanged();

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
                return ToString();
            }


        }
    }
}