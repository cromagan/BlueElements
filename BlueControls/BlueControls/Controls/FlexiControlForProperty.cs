﻿using BlueBasics;
using System.ComponentModel;
using System.Reflection;

namespace BlueControls.Controls
{
    // https://stackoverflow.com/questions/724143/how-do-i-create-a-delegate-for-a-net-property
    // http://peisker.net/dotnet/propertydelegates.htm
    // http://geekswithblogs.net/akraus1/archive/2006/02/10/69047.aspx

    public class FlexiControlForProperty : FlexiControl
    {

        private PropertyInfo propInfo;
        private object _propertyObject;
        private string _propertyName;
        private bool _addGroupboxText;
        private string _propertynamecpl;
        bool _AutoQuickInfo = true;
        bool _AutoCaption = true;


        public string PropertyName
        {
            get { return _propertyName; }
            set
            {
                if (_propertyName == value) { return; }

                FillPropertyNow();

                _propertyName = value;
                GetTmpVariables();
                UpdateControlData();
                SetValueFromProperty();
                CheckEnabledState();

            }
        }


        [DefaultValue(false)]
        public bool AddGroupboxText
        {
            get { return _addGroupboxText; }
            set
            {
                if (_addGroupboxText == value) { return; }

                FillPropertyNow();

                _addGroupboxText = value;
                GetTmpVariables();
                UpdateControlData();
                SetValueFromProperty();
                CheckEnabledState();

            }
        }



        [DefaultValue(true)]
        public bool AutoCaption
        {
            get { return _AutoCaption; }
            set
            {
                if (_AutoCaption == value) { return; }

                FillPropertyNow();

                _AutoCaption = value;
                GetTmpVariables();
                UpdateControlData();
                SetValueFromProperty();
                CheckEnabledState();

            }
        }

        [DefaultValue(true)]
        public bool AutoQuickInfo
        {
            get { return _AutoQuickInfo; }
            set
            {
                if (_AutoQuickInfo == value) { return; }

                FillPropertyNow();

                _AutoQuickInfo = value;
                GetTmpVariables();
                UpdateControlData();
                SetValueFromProperty();
                CheckEnabledState();

            }
        }



        public object PropertyObject
        {
            get { return _propertyObject; }
            set
            {
                if (_propertyObject == value) { return; }

                FillPropertyNow();

                _propertyObject = value;
                GetTmpVariables();
                UpdateControlData();
                SetValueFromProperty();
                CheckEnabledState();

            }
        }


        private void GetTmpVariables()
        {

            if (string.IsNullOrEmpty(_propertyName) || _propertyObject == null)
            {
                propInfo = null;
                return;
            }


            _propertynamecpl = _propertyName;

            if (_addGroupboxText && Parent is GroupBox grp)
            {
                _propertynamecpl = _propertynamecpl + "_" + grp.Text;
            }


            _propertynamecpl = _propertynamecpl.ReduceToChars(Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + Constants.Char_Numerals + "/\\ _");
            _propertynamecpl = _propertynamecpl.Replace(" ", "_");
            _propertynamecpl = _propertynamecpl.Replace("/", "_");
            _propertynamecpl = _propertynamecpl.Replace("\\", "_");
            _propertynamecpl = _propertynamecpl.Replace("__", "_");

            propInfo = _propertyObject.GetType().GetProperty(_propertynamecpl);
        }



        private void SetValueFromProperty()
        {


            if (_propertyObject == null || string.IsNullOrEmpty(_propertyName) || propInfo == null)
            {
                Value = string.Empty;
                InfoText = string.Empty;
                return;
            }


            Value = (string)propInfo.GetValue(_propertyObject, null);


        }

        private void FillPropertyNow()
        {
            if (_IsFilling) { return; }

            if (!Enabled) { return; } // Versuch. Eigentlich darf das Steuerelement dann nur empfangen und nix ändern.

            if (_propertyObject == null || string.IsNullOrEmpty(_propertyName) || propInfo == null) { return; }


            var OldVal = (string)propInfo.GetValue(_propertyObject, null);
            if (OldVal == Value) { return; }

            propInfo.SetValue(_propertyObject, Value, null);

        }

        internal void CheckEnabledState()
        {
            if (_propertyObject == null || string.IsNullOrEmpty(_propertyName) || propInfo == null)
            {
                Enabled = false;
                return;
            }

            Enabled = true;
        }


        protected override void OnValueChanged()
        {
            base.OnValueChanged();
            FillPropertyNow();
        }


        private void UpdateControlData()
        {


            if (!string.IsNullOrEmpty(_propertyName))
            {



                var x = _propertyName.SplitBy("__");
                if (_AutoCaption) { Caption = x[0].Replace("_", " ") + ":"; }


                var qi = _propertyName.Replace("_", " ");


                if (_addGroupboxText && Parent is GroupBox grp)
                {
                    qi = qi + " " + grp.Text.TrimEnd(":");
                }


                if (_AutoQuickInfo) { QuickInfo = qi.Replace("  ", " "); };
                FileEncryptionKey = string.Empty;
            }

            else
            {
                if (_AutoCaption) { Caption = "[unbekannt]"; }
                if (_AutoQuickInfo) { QuickInfo = string.Empty; }
            }


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
                else if (thisc is FlexiControlForProperty flx)
                {
                    flx.PropertyObject = _to;
                }
            }

        }


    }
}

