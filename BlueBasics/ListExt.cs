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

        public event EventHandler<System.EventArgs> ItemRemoved;
        public event EventHandler<ListEventArgs> ItemRemove;
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


        public new void Clear()
        {
            if (Count == 0) { return; }

            foreach (var item in this)
            {
                OnItemRemove(item);
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
            OnItemRemove(item);
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

        public new void Insert(int index, T item) { Develop.DebugPrint_NichtImplementiert(); }

        public new void InsertRange(int index, IEnumerable<T> collection) { Develop.DebugPrint_NichtImplementiert(); }


        public new void RemoveAll(Predicate<T> match) { Develop.DebugPrint_NichtImplementiert(); }

        public new void RemoveAt(int index)
        {
            OnItemRemove(base[index]);
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
                    OnItemRemove(base[index]);
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
        protected virtual void OnItemRemove(T item)
        {
            if (item is IChangedFeedback cItem) { cItem.Changed -= CItem_Changed; }

            if (!_ThrowEvents) { return; }
            ItemRemove?.Invoke(this, new ListEventArgs(item));
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
    }
}