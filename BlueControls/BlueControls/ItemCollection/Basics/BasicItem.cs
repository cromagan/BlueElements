using System;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;

namespace BlueControls.ItemCollection.Basics
{
    public abstract class BasicItem : IChangedFeedback
    {
        #region  Variablen-Deklarationen 







        private static string UniqueInternal_LastTime = "InitialDummy";
        private static int UniqueInternal_Count;

        protected object _parent = null;

        #endregion


        #region  Event-Deklarationen + Delegaten 


        #endregion


        #region  Construktor  


        public event EventHandler Changed;

        protected BasicItem(string internalname)
        {

            if (string.IsNullOrEmpty(internalname))
            {
                Internal = UniqueInternal();
            }
            else
            {
                Internal = internalname;
            }

            if (string.IsNullOrEmpty(Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }

        }


        #endregion


        #region  Properties 



        #endregion

        internal void SetParent(object collection) 
        {
            _parent = collection;
        }

        public abstract void DesignOrStyleChanged();

        public string Internal { get; private set; }

        public static string UniqueInternal()
        {

            var NeueZeit = DateTime.Now + " " + DateTime.Now.Millisecond;

            if (NeueZeit == UniqueInternal_LastTime)
            {
                UniqueInternal_Count += 1;
            }
            else
            {
                UniqueInternal_Count = 0;
                UniqueInternal_LastTime = NeueZeit;
            }


            return "Auto " + NeueZeit + " IDX" + UniqueInternal_Count;
        }

        public void OnChanged()
        {
            if (this is IParseable O && O.IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }
    }
}
