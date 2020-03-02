using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.ItemCollection;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using static BlueBasics.Extensions;

namespace BlueControls.Controls
{
    // https://stackoverflow.com/questions/724143/how-do-i-create-a-delegate-for-a-net-property
    // http://peisker.net/dotnet/propertydelegates.htm
    // http://geekswithblogs.net/akraus1/archive/2006/02/10/69047.aspx

    public class FlexiControlForProperty : FlexiControl
    {

        object _LastParent = null;

        private PropertyInfo propInfo;
        private object _propertyObject;
        private string _propertyName;
        private bool _addGroupboxText;
        private string _propertynamecpl;
        bool _FehlerWennLeer = true;
        bool _FehlerFormatCheck = true;
        Timer Checker = new Timer();


        new bool _enabled = true;


        /// <summary>
        /// Die Hauptklasse wird zwar beibehalten, aber Unterklassen müssen evtl. neu definiert werden.
        /// </summary>
        public event System.EventHandler LoadedFromDisk;


        public FlexiControlForProperty() : base()
        {
            GenFehlerText();
            Checker.Tick += Checker_Tick;
            Checker.Interval = 1000;
            Checker.Enabled = true;

            CaptionPosition = enÜberschriftAnordnung.Links_neben_Dem_Feld;
            EditType = enEditTypeFormula.Textfeld;
            Size = new Size(200, 24);
        }


        public FlexiControlForProperty(object propertyObject, string propertyName) : this()
        {
            _propertyObject = propertyObject;
            _propertyName = propertyName;
            UpdateControlData();
            CheckEnabledState();
        }

        public FlexiControlForProperty(object propertyObject, string propertyName, ItemCollectionList list) : this(propertyObject, propertyName)
        {

            list.Appearance = enBlueListBoxAppearance.ComboBox_Textbox;
            var s = BlueFont.MeasureString(_Caption, Skin.GetBlueFont(enDesign.Caption, enStates.Standard).Font());


            var data = list.ItemData(); // BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed
            var Wi = data.Item1;
            var He = data.Item2;


            var x = Math.Max((int)(data.Item1 + 20 + s.Width), 200);
            var y = Math.Max((int)(data.Item2 + Skin.PaddingSmal * 2), 24);

            Size = new Size(x, y);


            var c = CreateSubControls();

           StyleComboBox((ComboBox)c, list, System.Windows.Forms.ComboBoxStyle.DropDownList);

        }

        private void Checker_Tick(object sender, System.EventArgs e)
        {
            if (_IsFilling) { return; }
            if (!_allinitialized) { return; }
            SetValueFromProperty();
        }

        public string PropertyName
        {
            get { return _propertyName; }
            set
            {
                if (_propertyName == value) { return; }

                FillPropertyNow();

                _propertyName = value;
                UpdateControlData();
                CheckEnabledState();

            }
        }


        [DefaultValue(true)]
        public new bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (_enabled == value) { return; }
                _enabled = value;
                CheckEnabledState();
                GenFehlerText();
            }
        }


        //[DefaultValue(true)]
        //public bool FehlerWennLeer
        //{
        //    get { return _FehlerWennLeer; }
        //    set
        //    {
        //        if (_FehlerWennLeer == value) { return; }
        //        _FehlerWennLeer = value;
        //        GenFehlerText();
        //    }
        //}



        //[DefaultValue(true)]
        //public bool FehlerFormatCheck
        //{
        //    get { return _FehlerFormatCheck; }
        //    set
        //    {
        //        if (_FehlerFormatCheck == value) { return; }
        //        _FehlerFormatCheck = value;
        //        GenFehlerText();
        //    }
        //}


        [DefaultValue(false)]
        public bool AddGroupboxText
        {
            get { return _addGroupboxText; }
            set
            {
                if (_addGroupboxText == value) { return; }

                FillPropertyNow();

                _addGroupboxText = value;
                UpdateControlData();
                CheckEnabledState();

            }
        }



        //[DefaultValue(true)]
        //public bool AutoQuickInfo
        //{
        //    get { return _AutoQuickInfo; }
        //    set
        //    {
        //        if (_AutoQuickInfo == value) { return; }

        //        FillPropertyNow();

        //        _AutoQuickInfo = value;
        //        UpdateControlData();
        //        CheckEnabledState();

        //    }
        //}

        [DefaultValue(null)]
        public object PropertyObject
        {
            get { return _propertyObject; }
            set
            {
                if (_propertyObject == value) { return; }

                FillPropertyNow();

                if (_propertyObject is IReloadable LS)
                {
                    LS.LoadedFromDisk -= OnLoadedFromDisk;
                }


                _propertyObject = value;
                UpdateControlData();
                CheckEnabledState();


                if (_propertyObject is IReloadable LSn)
                {
                    LSn.LoadedFromDisk += OnLoadedFromDisk;
                }

            }
        }

        private void OnLoadedFromDisk(object sender, System.EventArgs e)
        {
            FillPropertyNow();
            _propertyObject = null;  //Das Objekt ist tot und irgendwo im Nirvana verschwunden
            UpdateControlData();
            CheckEnabledState();

            LoadedFromDisk?.Invoke(this, System.EventArgs.Empty);
        }




        private void SetValueFromProperty()
        {


            if (_propertyObject == null || string.IsNullOrEmpty(_propertyName) || propInfo == null)
            {
                Value = string.Empty;
                InfoText = string.Empty;
                return;
            }


            var x = propInfo.GetValue(_propertyObject, null);


            if (x is null)
            {
                Value = string.Empty;
            }
            else if (x is string s)
            {
                Value = s;
            }
            else if (x is List<string> ls)
            {
                Value = ls.JoinWithCr();
            }
            else if (x is bool bo)
            {
                Value = bo.ToPlusMinus();
            }
            else
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Art unbekannt!");
            }



        }

        private void FillPropertyNow()
        {
            if (_IsFilling) { return; }
            if (!_allinitialized) { return; }

            if (!CheckEnabledState()) { return; } // Versuch. Eigentlich darf das Steuerelement dann nur empfangen und nix ändern.

            if (_propertyObject == null || string.IsNullOrEmpty(_propertyName) || propInfo == null) { return; }


            var OldVal = string.Empty;
            var x = propInfo.GetValue(_propertyObject, null);
            object toSet = null;


            if (x is null)
            {
                OldVal = string.Empty;
                toSet = Value; // Wissen wir leider nicht, welcher Typ....
            }
            else if (x is string s)
            {
                OldVal = s;
                toSet = Value;
            }
            else if (x is List<string> ls)
            {
                OldVal = ls.JoinWithCr();
                toSet = Value.SplitByCRToList();
            }
            else if (x is bool bo)
            {
                OldVal = bo.ToPlusMinus();
                toSet = Value.FromPlusMinus();
            }
            else
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Art unbekannt!");
            }


            if (OldVal == Value) { return; }

            propInfo.SetValue(_propertyObject, toSet, null);

        }

        internal bool CheckEnabledState()
        {
            if (!_enabled || _propertyObject == null || string.IsNullOrEmpty(_propertyName) || propInfo == null)
            {
                base.Enabled = false;
                return false;
            }

            if (!propInfo.CanWrite)
            {
                base.Enabled = false;
                return false;

            }


            base.Enabled = true;
            return true;
        }


        protected override void OnValueChanged()
        {
            FillPropertyNow(); // erst befüllen, bevor das Event ausgelöst wird
            GenFehlerText(); // erst Standard fehler Text, bevor das Event ausgelöst wird
            base.OnValueChanged();
        }


        private void UpdateControlData()
        {

            #region propInfo & _propertynamecpl befüllen


            if (string.IsNullOrEmpty(_propertyName) || _propertyObject == null)
            {
                propInfo = null;
                _propertynamecpl = string.Empty;
            }
            else
            {
                _propertynamecpl = _propertyName;

                if (_addGroupboxText && Parent is GroupBox grp)
                {
                    _propertynamecpl = _propertynamecpl + "_" + grp.Text;
                }


                _propertynamecpl = _propertynamecpl.ReduceToChars(Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + Constants.Char_Numerals + "-/\\ _");
                _propertynamecpl = _propertynamecpl.Replace("-", "_");
                _propertynamecpl = _propertynamecpl.Replace(" ", "_");
                _propertynamecpl = _propertynamecpl.Replace("/", "_");
                _propertynamecpl = _propertynamecpl.Replace("\\", "_");
                _propertynamecpl = _propertynamecpl.Replace("__", "_");

                propInfo = _propertyObject.GetType().GetProperty(_propertynamecpl);

            }

            #endregion

            #region Art des Steuerelements bestimmen

            switch (propInfo.PropertyType.FullName.ToLower())
            {
                case "system.string":
                    MultiLine = false;
                    break;
                case "system.boolean":
                    EditType = enEditTypeFormula.Ja_Nein_Knopf;
                    var s = BlueFont.MeasureString(_Caption, Skin.GetBlueFont(enDesign.Caption, enStates.Standard).Font());
                    Size = new Size((int)s.Width + 30, 22);
                    break;

                default:
                    Develop.DebugPrint("propertyType unbekannt: " + propInfo.PropertyType.FullName);
                    break;
            }


            #endregion

            #region Caption setzen
            if (!string.IsNullOrEmpty(_propertyName))
            {

                var x = _propertyName.SplitBy("__");
                Caption = x[0].Replace("_", " ") + ":";
                FileEncryptionKey = string.Empty;
            }

            else
            {
                Caption = "[unbekannt]";
            }
            #endregion



            #region QuickInfo setzen
            // https://stackoverflow.com/questions/32901771/multiple-enum-descriptions
            _FehlerWennLeer = true;

            if (propInfo == null)
            {
                QuickInfo = string.Empty;
            }
            else
            {

                var done = false;


                var ca = propInfo.GetCustomAttributes();
                if (ca != null)

                {

                    foreach (var thisas in ca)
                    {

                        if (thisas is PropertyAttributes pa)
                        {
                            QuickInfo = pa.Description;
                            _FehlerWennLeer = pa.FehlerWennLeer;
                            done = true;
                        }
                        else if (thisas is DescriptionAttribute da)
                        {
                            QuickInfo = da.Description;
                            done = true;
                        }



                    }
                }

                if (!done)
                {
                    QuickInfo = string.Empty;
                }


            }
            #endregion

            SetValueFromProperty();
            GenFehlerText();
        }


        public static void SetAllFlexControls(System.Windows.Forms.Control _in, object _to, bool rekursiv)
        {

            if (_in == null || _in.IsDisposed) { return; }

            foreach (var thisc in _in.Controls)
            {

                if (thisc is GroupBox gr)
                {
                    if (rekursiv) { SetAllFlexControls(gr, _to, rekursiv); }
                }
                if (thisc is TabControl tb)
                {
                    if (rekursiv) { SetAllFlexControls(tb, _to, rekursiv); }
                }
                if (thisc is TabPage tabp)
                {
                    if (rekursiv) { SetAllFlexControls(tabp, _to, rekursiv); }
                }
                else if (thisc is FlexiControlForProperty flx)
                {
                    flx.PropertyObject = _to;
                }
            }

        }



        protected override void OnControlAdded(ControlEventArgs e)
        {
            CheckEnabledState();
            base.OnControlAdded(e);
        }

        private void GenFehlerText()
        {
            if (_FehlerWennLeer && string.IsNullOrEmpty(Value))
            {
                InfoText = "Dieses Feld darf nicht leer sein.";
                return;
            }

            if (string.IsNullOrEmpty(Value))
            {
                InfoText = string.Empty;
                return;
            }

            if (_FehlerFormatCheck && !Value.IsFormat(Format))
            {
                InfoText = "Der Wert entspricht nicht dem erwarteten Format.";
                return;
            }

            InfoText = string.Empty;

        }

        public ComboBox GetComboBox()
        {

            if (!_allinitialized) { return null; }


            foreach (var thiscon in this.Controls)
            {
                if (thiscon is ComboBox cbx) { return cbx; }
            }
            return null;
        }

        protected override void OnHandleDestroyed(System.EventArgs e)
        {
            FillPropertyNow();
            base.OnHandleDestroyed(e);
        }




        protected override void OnParentChanged(System.EventArgs e)
        {
            FillPropertyNow();
            if (_LastParent is GroupBox gp) { gp.TextChanged -= GroupBox_TextChanged; }


            base.OnParentChanged(e);

            _LastParent = Parent;

            if (_LastParent is GroupBox gp2) { gp2.TextChanged += GroupBox_TextChanged; }


        }

        private void GroupBox_TextChanged(object sender, System.EventArgs e)
        {
            UpdateControlData();
            CheckEnabledState();

            OnValueChanged(); // Wichig, dass Fehler-Dreiecke angezeigt werden können
        }
    }
}

