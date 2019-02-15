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
        /// Welcher Wert wirklich hinter der Anzeige steckt
        /// </summary>
        /// <remarks></remarks>
        protected string _Internal = "";

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


        #region  Construktor + Initialize 


        public event EventHandler Changed;

        protected BasicItem()
        {
            Initialize();
        }

        protected void Initialize()
        {
            _Internal = UniqueInternal();
            _Tags = new List<string>();
            InitializeLevel2();
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



        public abstract string Internal();


        public bool IsNullOrEmpty()
        {
            // Nicht das interne Abfragen, sondern das der Items!
            if (string.IsNullOrEmpty(Internal())) { return true; }

            return false;
        }

        protected abstract void InitializeLevel2();




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
