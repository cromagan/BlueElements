using System;
using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;

namespace BlueControls.ItemCollection.Basics
{
    public abstract class BasicItem : ICanBeEmpty, IChangedFeedback
    {
        #region  Variablen-Deklarationen 





        /// <summary>
        /// Falls eine Spezielle Information gespeichert und zurückgegeben werden soll
        /// </summary>
        /// <remarks></remarks>
        protected List<string> _Tags;


        private static string UniqueInternal_LastTime = "InitialDummy";
        private static int UniqueInternal_Count;

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


            _Tags = new List<string>();

            if (string.IsNullOrEmpty(Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }

        }


        #endregion


        #region  Properties 


        public List<string> Tags
        {
            get
            {
                return _Tags;
            }
        }

        #endregion





        public abstract void DesignOrStyleChanged();



        public virtual string Internal { get; private set; }





        public bool IsNullOrEmpty()
        {
            // Nicht das interne Abfragen, sondern das der Items!
            if (string.IsNullOrEmpty(Internal)) { return true; }

            return false;
        }



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
