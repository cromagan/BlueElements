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

        //    propInfo = typeof(_propertyObject).GetProperty(_propertyName);

            propInfo = _propertyObject.GetType().GetProperty(_propertyName);
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
            Caption = _propertyName.Replace("_", " ") + ":";

            //if (_propertyObject == null || string.IsNullOrEmpty(_propertyName) || propInfo == null)
            //{
            //    Caption = string.Empty;
                //EditType = enEditTypeFormula.None;
                QuickInfo = string.Empty;
                FileEncryptionKey = string.Empty;
            //}
            //else
            //{
            //    Caption = _propertyName.Replace("_"," ") + ":";
            //    //EditType = _tmpColumn.EditType;
            //    //QuickInfo = _tmpColumn.QickInfoText(string.Empty);
            //    //FileEncryptionKey = _Database.FileEncryptionKey;
            //}
        }

    }
}

