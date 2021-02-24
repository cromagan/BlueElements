using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using static BlueBasics.Extensions;
using static BlueBasics.modConverter;

namespace BlueControls.Controls {
    // https://stackoverflow.com/questions/724143/how-do-i-create-a-delegate-for-a-net-property
    // http://peisker.net/dotnet/propertydelegates.htm
    // http://geekswithblogs.net/akraus1/archive/2006/02/10/69047.aspx

    public class FlexiControlForProperty : FlexiControl {

        private MethodInfo _methInfo;
        private PropertyInfo _propInfo;
        private object _propertyObject;
        private string _propertyName;
        private string _propertynamecpl;
        private bool _FehlerWennLeer = true;
        private bool _alwaysDiabled = false;
        private readonly bool _FehlerFormatCheck = true;
        private bool _enabled = true;


        ///// <summary>
        ///// Die Hauptklasse wird zwar beibehalten, aber Unterklassen müssen evtl. neu definiert werden.
        ///// </summary>
        //public event System.EventHandler LoadedFromDisk;

        #region Constructor

        public FlexiControlForProperty(object propertyObject, string propertyName, int rowCount, ItemCollectionList list, enImageCode image) : this() {
            _propertyObject = propertyObject;
            _propertyName = propertyName;
            UpdateControlData(true, rowCount, list, image);
            CheckEnabledState();
        }

        public FlexiControlForProperty(object propertyObject, string propertyName, ItemCollectionList list) : this(propertyObject, propertyName, 1, list, enImageCode.None) { }

        public FlexiControlForProperty(object propertyObject, string propertyName, int rowCount) : this(propertyObject, propertyName, rowCount, null, enImageCode.None) { }

        public FlexiControlForProperty(object propertyObject, string propertyName, enImageCode image) : this(propertyObject, propertyName, 1, null, image) { }

        public FlexiControlForProperty(object propertyObject, string propertyName) : this(propertyObject, propertyName, 1, null, enImageCode.None) { }


        #endregion

        public FlexiControlForProperty() : base() {
            GenFehlerText();

            //if (propChecker == null)
            //{
            //    propChecker = new Timer();
            //    propChecker.Interval = 1000;
            //    propChecker.Enabled = true;
            //}

            _IdleTimer.Tick += Checker_Tick;


            CaptionPosition = enÜberschriftAnordnung.Links_neben_Dem_Feld;
            EditType = enEditTypeFormula.Textfeld;
            Size = new Size(200, 24);
        }


        private void Checker_Tick(object sender, System.EventArgs e) {
            //if (_IsFilling) { return; }
            if (!_allinitialized) { return; }

            if (_LastTextChange != null) { return; } // Noch am bearbeiten

            SetValueFromProperty();
        }


        /// <summary>
        /// Der Getter/Setter des UserControls. Dabei werden Sonderzeichen in _ umgewandelt. Punkte gelöscht. Zwei __ werden zu einem geändert, und die Anzegige nach den beiden _ wird optisch abgeschnitten.
        /// </summary>
        public string PropertyName {
            get { return _propertyName; }
            set {
                if (_propertyName == value) { return; }

                FillPropertyNow();

                _propertyName = value;
                UpdateControlData(false, 1, null, enImageCode.None);
                CheckEnabledState();
            }
        }


        [DefaultValue(true)]
        public new bool Enabled {
            get {
                return _enabled;
            }
            set {
                if (_enabled == value) { return; }
                _enabled = value;
                CheckEnabledState();
                GenFehlerText();
            }
        }


        [DefaultValue(null)]
        public object PropertyObject {
            get { return _propertyObject; }
            set {
                if (_propertyObject == value) { return; }

                FillPropertyNow();

                //if (_propertyObject is IReloadable LS) {
                //    LS.LoadedFromDisk -= OnLoadedFromDisk;
                //}

                _propertyObject = value;
                UpdateControlData(false, 1, null, enImageCode.None);
                CheckEnabledState();


                //if (_propertyObject is IReloadable LSn) {
                //    LSn.LoadedFromDisk += OnLoadedFromDisk;
                //}

            }
        }

        //private void OnLoadedFromDisk(object sender, System.EventArgs e) {
        //    FillPropertyNow();
        //    _propertyObject = null;  //Das Objekt ist tot und irgendwo im Nirvana verschwunden
        //    UpdateControlData(false, 1, null, enImageCode.None);
        //    CheckEnabledState();

        //    LoadedFromDisk?.Invoke(this, System.EventArgs.Empty);
        //}




        private void SetValueFromProperty() {


            if (_propertyObject == null || string.IsNullOrEmpty(_propertyName) || _propInfo == null || !_propInfo.CanRead) {
                ValueSet(string.Empty, true, false);
                InfoText = string.Empty;
                return;
            }


            var x = _propInfo.GetValue(_propertyObject, null);
            if (x is null) {
                ValueSet(string.Empty, true, false);
            }
            else if (x is string s) {
                ValueSet(s, true, false);
            }
            else if (x is List<string> ls) {
                ValueSet(ls.JoinWithCr(), true, false);
            }
            else if (x is bool bo) {
                ValueSet(bo.ToPlusMinus(), true, false);
            }
            else if (x is int iv) {
                ValueSet(iv.ToString(), true, false); 
            }
            else if (x is Enum) {
                ValueSet(((int)x).ToString(), true, false);
            }
            else if (x is decimal dc) {
                ValueSet(dc.ToString(Constants.Format_Float2), true, false);
            }
            else if (x is double db) {
                ValueSet(db.ToString(Constants.Format_Float2), true, false);
            }
            else if (x is Color co) {
                ValueSet(co.ToHTMLCode(), true, false);
            }
            else {
                Develop.DebugPrint(enFehlerArt.Fehler, "Art unbekannt!");
            }

        }

        private void FillPropertyNow() {
            //if (_IsFilling) { return; }
            if (!_allinitialized) { return; }

            if (!CheckEnabledState()) { return; } // Versuch. Eigentlich darf das Steuerelement dann nur empfangen und nix ändern.

            if (_propertyObject == null || string.IsNullOrEmpty(_propertyName) || _propInfo == null || !_propInfo.CanRead) { return; }


            var OldVal = string.Empty;
            var x = _propInfo.GetValue(_propertyObject, null);
            object toSet = null;
            if (x is null) {
                OldVal = string.Empty;
                toSet = Value; // Wissen wir leider nicht, welcher Typ....
            }
            else if (x is string s) {
                OldVal = s;
                toSet = Value;
            }
            else if (x is List<string> ls) {
                OldVal = ls.JoinWithCr();
                toSet = Value.SplitByCRToList();
            }
            else if (x is bool bo) {
                OldVal = bo.ToPlusMinus();
                toSet = Value.FromPlusMinus();
            }
            else if (x is Color co) {
                OldVal = co.ToHTMLCode();
                toSet = Value.FromHTMLCode();
            }
            else if (x is int iv) {
                OldVal = iv.ToString();
                toSet = IntParse(Value);
            }
            else if (x is Enum) {
                OldVal = ((int)x).ToString();
                toSet = IntParse(Value);
            }
            else if (x is decimal dc) {
                OldVal = dc.ToString(Constants.Format_Float2);
                toSet = DecimalParse(Value);
            }
            else if (x is double db) {
                OldVal = db.ToString(Constants.Format_Float2);
                toSet = DoubleParse(Value);
            }
            else {
                Develop.DebugPrint(enFehlerArt.Fehler, "Art unbekannt!");
            }


            if (OldVal == Value) { return; }

            _propInfo.SetValue(_propertyObject, toSet, null);
        }

        internal bool CheckEnabledState() {

            if (DesignMode) {
                base.DisabledReason = string.Empty;
                return true;
            }


            if (_propertyObject == null) {
                base.DisabledReason = "Kein zugehöriges Objekt definiert.";
                return false;
            }
            if (string.IsNullOrEmpty(_propertyName)) {
                base.DisabledReason = "Kein Feld-Name angegeben.";
                return false;
            }
            if (_propInfo == null && _methInfo == null) {
                base.DisabledReason = "Feld existiert im zugehörigen Objekt nicht.";
                return false;
            }
            if (!_enabled) {
                base.DisabledReason = "Das Feld ist deaktiviert.";
                return false;
            }


            if (_propInfo != null && !_propInfo.CanWrite) {
                base.DisabledReason = "Feld kann generell nicht beschrieben werdern.";
                return false;
            }

            if (_alwaysDiabled) {
                base.DisabledReason = "Feld ist schreibgeschützt.";
                return false;
            }

            base.DisabledReason = string.Empty;
            return true;
        }


        protected override void OnValueChanged() {
            FillPropertyNow(); // erst befüllen, bevor das Event ausgelöst wird
            GenFehlerText(); // erst Standard fehler Text, bevor das Event ausgelöst wird
            base.OnValueChanged();
        }


        private void UpdateControlData(bool withCreate, int TextLines, ItemCollectionList list, enImageCode image) {

            #region propInfo & _propertynamecpl befüllen


            if (string.IsNullOrEmpty(_propertyName) || _propertyObject == null) {
                _methInfo = null;
                _propInfo = null;
                _propertynamecpl = string.Empty;
            }
            else {
                _propertynamecpl = _propertyName;

                _propertynamecpl = _propertynamecpl.ReduceToChars(Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + Constants.Char_Numerals + "-/\\ _");
                _propertynamecpl = _propertynamecpl.Replace("-", "_");
                _propertynamecpl = _propertynamecpl.Replace(" ", "_");
                _propertynamecpl = _propertynamecpl.Replace("/", "_");
                _propertynamecpl = _propertynamecpl.Replace("\\", "_");
                _propertynamecpl = _propertynamecpl.Replace("__", "_");

                _propInfo = _propertyObject.GetType().GetProperty(_propertynamecpl);
                _methInfo = _propertyObject.GetType().GetMethod(_propertynamecpl);

            }

            #endregion

            #region Caption setzen
            if (!string.IsNullOrEmpty(_propertyName)) {

                var x = _propertyName.SplitBy("__");
                Caption = x[0].Replace("_", " ") + ":";
                FileEncryptionKey = string.Empty;
            }
            else {
                Caption = "[unbekannt]";
            }
            #endregion

            #region Art des Steuerelements bestimmen

            if (withCreate && _methInfo != null) {

                EditType = enEditTypeFormula.Button;
                CaptionPosition = enÜberschriftAnordnung.ohne;
                var s = BlueFont.MeasureStringOfCaption(_Caption.TrimEnd(":"));
                Size = new Size((int)s.Width + 50 + 22, 30);
                var c = (Button)CreateSubControls();
                c.Text = _Caption.TrimEnd(":");

                if (image != enImageCode.None) {
                    c.ImageCode = QuickImage.Get(image, 22).ToString();
                }
            }


            if (withCreate && _propInfo != null) {

                switch (_propInfo.PropertyType.FullName.ToLower()) {
                    //case "system.string":
                    //case "system.int32":
                    //case "system.decimal":



                    case "system.boolean": {
                            EditType = enEditTypeFormula.Ja_Nein_Knopf;
                            var s = BlueFont.MeasureStringOfCaption(_Caption);
                            Size = new Size((int)s.Width + 30, 22);
                            break;
                        }

                    default: // Alle enums sind ein eigener Typ.... deswegen alles in die Textbox
                        {

                            if (list != null) {
                                EditType = enEditTypeFormula.Textfeld_mit_Auswahlknopf;
                                list.Appearance = enBlueListBoxAppearance.ComboBox_Textbox;
                                var s = BlueFont.MeasureStringOfCaption(_Caption);

                                var (BiggestItemX, BiggestItemY, _, _) = list.ItemData(); // BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed

                                var x = Math.Max((int)(BiggestItemX + 20 + s.Width), 200);
                                var y = Math.Max(BiggestItemY + Skin.PaddingSmal * 2, 24);
                                Size = new Size(x, y);
                                var c = (ComboBox)CreateSubControls();
                                StyleComboBox(c, list, System.Windows.Forms.ComboBoxStyle.DropDownList);

                            }
                            //else if (image != enImageCode.None)
                            //{
                            //    EditType = enEditTypeFormula.Button;
                            //    CaptionPosition = enÜberschriftAnordnung.ohne;
                            //    Size = new Size((int)BlueFont.MeasureStringOfCaption(_Caption).Width + 50, 30);
                            //    var c = (Button)CreateSubControls();
                            //    c.ImageCode = QuickImage.Get(image).ToString();

                            //}
                            else {
                                _EditType = enEditTypeFormula.Textfeld;

                                var tmpName = _propInfo.PropertyType.FullName.ToLower();


                                if (TextLines >= 2) {
                                    _CaptionPosition = enÜberschriftAnordnung.Über_dem_Feld;
                                    Size = new Size(200, 16 + 24 * TextLines);
                                    _MultiLine = true;
                                    tmpName = "system.string";
                                }
                                else {
                                    _CaptionPosition = enÜberschriftAnordnung.Links_neben_Dem_Feld;
                                    Size = new Size(200, 24);
                                    _MultiLine = false;
                                }


                                switch (tmpName) {
                                    case "system.string": Format = enDataFormat.Text; break;
                                    case "system.int32": Format = enDataFormat.Ganzzahl; break;
                                    case "system.decimal": Format = enDataFormat.Gleitkommazahl; break;
                                    case "system.drawing.color": Format = enDataFormat.Text; break; // HTML-Farbcode noch nicht programmiert
                                    default: Format = enDataFormat.Text; break;
                                }

                                var c = CreateSubControls();
                                StyleTextBox((TextBox)c, string.Empty, false);
                            }
                            break;
                        }

                }
            }
            //else
            //{
            //    Develop.DebugPrint(_propertyName + " hat keine Zuordnung");
            //}

            #endregion

            #region QuickInfo setzen
            // https://stackoverflow.com/questions/32901771/multiple-enum-descriptions

            // [PropertyAttributes("", false)] setzen

            _FehlerWennLeer = true;
            _alwaysDiabled = false;

            if (_propInfo == null && _methInfo == null) {
                QuickInfo = string.Empty;
            }
            else {

                var done = false;


                IEnumerable<Attribute> ca = null;

                if (_propInfo != null) { ca = _propInfo.GetCustomAttributes(); }
                if (_methInfo != null) { ca = _methInfo.GetCustomAttributes(); }

                if (ca != null) {

                    foreach (var thisas in ca) {
                        if (thisas is DescriptionAttribute da) {
                            QuickInfo = da.Description;
                            done = true;
                        }
                    }
                }

                if (!done) {
                    QuickInfo = string.Empty;
                }


            }
            #endregion

            SetValueFromProperty();
            GenFehlerText();
        }


        public static void SetAllFlexControls(System.Windows.Forms.Control _in, object _to) {

            if (_in == null || _in.IsDisposed) { return; }

            foreach (var thisc in _in.Controls) {
                if (thisc is FlexiControlForProperty flx) {
                    flx.PropertyObject = _to;
                }

                if (thisc is FlexiControlForCell flxc && _to is DataHolder dh) {

                    dh.Column(flxc.ColumnName, "Inkorrecte Zuordnung: " + flxc.ColumnName);

                    flxc.Database = dh.InternalDatabase;
                    flxc.RowKey = dh.Row().Key;
                }
            }
        }



        protected override void OnControlAdded(ControlEventArgs e) {
            CheckEnabledState();
            base.OnControlAdded(e);
        }

        private void GenFehlerText() {

            if (_propInfo == null) {
                InfoText = string.Empty;
                return;
            }

            if (_FehlerWennLeer && string.IsNullOrEmpty(Value)) {
                InfoText = "Dieses Feld darf nicht leer sein.";
                return;
            }

            if (string.IsNullOrEmpty(Value)) {
                InfoText = string.Empty;
                return;
            }

            if (_FehlerFormatCheck && !Value.IsFormat(Format)) {
                InfoText = "Der Wert entspricht nicht dem erwarteten Format.";
                return;
            }

            InfoText = string.Empty;

        }

        public ComboBox GetComboBox() {

            if (!_allinitialized) { return null; }


            foreach (var thiscon in Controls) {
                if (thiscon is ComboBox cbx) { return cbx; }
            }
            return null;
        }

        protected override void OnHandleDestroyed(System.EventArgs e) {
            FillPropertyNow();
            base.OnHandleDestroyed(e);
        }



        protected override void OnButtonClicked() {

            base.OnButtonClicked();

            if (_methInfo != null) {
                _methInfo.Invoke(_propertyObject, null);
            }

        }


        protected override void Dispose(bool disposing) {
            _IdleTimer.Tick -= Checker_Tick;
            //if (_propertyObject is IReloadable LS) { LS.LoadedFromDisk -= OnLoadedFromDisk; }

            base.Dispose(disposing);
        }

    }
}

