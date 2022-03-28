// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using BlueControls.ItemCollection.ItemCollectionList;
using static BlueBasics.Converter;
using BlueDatabase.Enums;

#nullable enable

namespace BlueControls.Controls {

    // https://stackoverflow.com/questions/724143/how-do-i-create-a-delegate-for-a-net-property
    // http://peisker.net/dotnet/propertydelegates.htm
    // http://geekswithblogs.net/akraus1/archive/2006/02/10/69047.aspx
    public class FlexiControlForProperty_OLD : FlexiControl {

        #region Fields

        private MethodInfo? _methInfo;
        private string _propertyName;
        private string _propertynamecpl;
        private object? _propertyObject;
        private PropertyInfo? _propInfo;

        #endregion

        #region Constructors

        public FlexiControlForProperty_OLD(object propertyObject, string propertyName, int rowCount, ItemCollectionList? list, ImageCode image) : this() {
            _propertyObject = propertyObject;
            _propertyName = propertyName;
            UpdateControlData(true, rowCount, list, image);
            CheckEnabledState();
        }

        public FlexiControlForProperty_OLD(object propertyObject, string propertyName, ItemCollectionList? list) : this(propertyObject, propertyName, 1, list, ImageCode.None) { }

        public FlexiControlForProperty_OLD(object propertyObject, string propertyName, int rowCount) : this(propertyObject, propertyName, rowCount, null, ImageCode.None) { }

        public FlexiControlForProperty_OLD(object propertyObject, string propertyName, ImageCode image) : this(propertyObject, propertyName, 1, null, image) { }

        public FlexiControlForProperty_OLD(object propertyObject, string propertyName) : this(propertyObject, propertyName, 1, null, ImageCode.None) { }

        public FlexiControlForProperty_OLD() : base() {
            GenFehlerText();
            //if (propChecker == null)
            //{
            //    propChecker = new Timer();
            //    propChecker.Interval = 1000;
            //    propChecker.Enabled = true;
            //}
            _IdleTimer.Tick += Checker_Tick;
            CaptionPosition = ÜberschriftAnordnung.Links_neben_Dem_Feld;
            EditType = EditTypeFormula.Textfeld;
            Size = new Size(200, 24);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Der Getter/Setter des UserControls. Dabei werden Sonderzeichen in _ umgewandelt. Punkte gelöscht. Zwei __ werden zu einem geändert, und die Anzegige nach den beiden _ wird optisch abgeschnitten.
        /// </summary>
        public string PropertyName {
            get => _propertyName;
            set {
                if (_propertyName == value) { return; }
                FillPropertyNow();
                _propertyName = value;
                UpdateControlData(false, 1, null, ImageCode.None);
                CheckEnabledState();
            }
        }

        [DefaultValue(null)]
        public object PropertyObject {
            get => _propertyObject;
            set {
                if (_propertyObject == value) { return; }
                FillPropertyNow();
                _propertyObject = value;
                UpdateControlData(false, 1, null, ImageCode.None);
                CheckEnabledState();
            }
        }

        #endregion

        #region Methods

        public static void SetAllFlexControls(Control? inControl, object toObject) {
            if (inControl == null || inControl.IsDisposed) { return; }

            foreach (var thisc in inControl.Controls) {
                if (thisc is FlexiControlForProperty_OLD flx) {
                    flx.PropertyObject = toObject;
                }
                //if (thisc is FlexiControlForCell flxc && toObject is DataHolder dh) {
                //    dh.Column(flxc.ColumnName, "Inkorrecte Zuordnung: " + flxc.ColumnName);
                //    flxc.Database = dh.InternalDatabase;
                //    flxc.RowKey = dh.Row().Key;
                //}
            }
        }

        public ComboBox? GetComboBox() {
            if (!Allinitialized) { return null; }
            foreach (var thiscon in Controls) {
                if (thiscon is ComboBox cbx) { return cbx; }
            }
            return null;
        }

        internal bool CheckEnabledState() {
            if (DesignMode) {
                DisabledReason = string.Empty;
                return true;
            }
            if (_propertyObject == null) {
                DisabledReason = "Kein zugehöriges Objekt definiert.";
                return false;
            }
            if (string.IsNullOrEmpty(_propertyName)) {
                DisabledReason = "Kein Feld-Name angegeben.";
                return false;
            }
            if (_propInfo == null && _methInfo == null) {
                DisabledReason = "Feld existiert im zugehörigen Objekt nicht.";
                return false;
            }
            if (_propInfo != null && !_propInfo.CanWrite) {
                DisabledReason = "Feld kann generell nicht beschrieben werden.";
                return false;
            }
            //if (_alwaysDiabled) {
            //    DisabledReason = "Feld ist schreibgeschützt.";
            //    return false;
            //}
            DisabledReason = string.Empty;
            return true;
        }

        protected override void Dispose(bool disposing) {
            _IdleTimer.Tick -= Checker_Tick;
            //if (_propertyObject is IReloadable LS) { LS.LoadedFromDisk -= OnLoadedFromDisk; }
            base.Dispose(disposing);
        }

        protected override void OnButtonClicked() {
            base.OnButtonClicked();
            if (_methInfo != null) {
                _methInfo.Invoke(_propertyObject, null);
            }
        }

        protected override void OnControlAdded(ControlEventArgs e) {
            CheckEnabledState();
            base.OnControlAdded(e);
        }

        protected override void OnHandleDestroyed(System.EventArgs e) {
            FillPropertyNow();
            base.OnHandleDestroyed(e);
        }

        protected override void OnValueChanged() {
            FillPropertyNow(); // erst befüllen, bevor das Event ausgelöst wird
            GenFehlerText(); // erst Standard fehler Text, bevor das Event ausgelöst wird
            base.OnValueChanged();
        }

        private void Checker_Tick(object sender, System.EventArgs e) {
            //if (_IsFilling) { return; }
            if (!Allinitialized) { return; }
            if (LastTextChange != null) { return; } // Noch am bearbeiten
            SetValueFromProperty();
        }

        private void FillPropertyNow() {
            if (!Allinitialized) { return; }
            if (!CheckEnabledState()) { return; } // Versuch. Eigentlich darf das Steuerelement dann nur empfangen und nix ändern.
            if (_propertyObject == null || string.IsNullOrEmpty(_propertyName) || _propInfo == null || !_propInfo.CanRead) { return; }
            var oldVal = string.Empty;
            var x = _propInfo.GetValue(_propertyObject, null);
            object? toSet = null;

            if (x is null) {
                oldVal = string.Empty;
                toSet = Value; // Wissen wir leider nicht, welcher Typ....
            } else if (x is string s) {
                oldVal = s;
                toSet = Value;
            } else if (x is List<string> ls) {
                oldVal = ls.JoinWithCr();
                toSet = Value.SplitAndCutByCrToList();
            } else if (x is bool bo) {
                oldVal = bo.ToPlusMinus();
                toSet = Value.FromPlusMinus();
            } else if (x is Color co) {
                oldVal = co.ToHtmlCode();
                toSet = Value.FromHtmlCode();
            } else if (x is int iv) {
                oldVal = iv.ToString();
                IntTryParse(Value, out var i);
                toSet = i;
            } else if (x is Enum) {
                oldVal = ((int)x).ToString();
                IntTryParse(Value, out var i);
                toSet = i;
            } else if (x is double db) {
                oldVal = db.ToString(Constants.Format_Float2);
                DoubleTryParse(Value, out var i);
                toSet = i;
            } else if (x is float fl) {
                oldVal = fl.ToString(Constants.Format_Float2);
                FloatTryParse(Value, out var i);
                toSet = i;
            } else {
                Develop.DebugPrint(FehlerArt.Fehler, "Art unbekannt!");
            }
            if (oldVal == Value) { return; }
            _propInfo.SetValue(_propertyObject, toSet, null);
        }

        private void GenFehlerText() {
            if (_propInfo == null) {
                InfoText = string.Empty;
                return;
            }
            //if (_FehlerWennLeer && string.IsNullOrEmpty(Value)) {
            //    InfoText = "Dieses Feld darf nicht leer sein.";
            //    return;
            //}
            if (string.IsNullOrEmpty(Value)) {
                InfoText = string.Empty;
                return;
            }
            //if (_FehlerFormatCheck && !Value.IsFormat(Format)) {
            //    InfoText = "Der Wert entspricht nicht dem erwarteten Format.";
            //    return;
            //}
            InfoText = string.Empty;
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
            } else if (x is string s) {
                ValueSet(s, true, false);
            } else if (x is List<string> ls) {
                ValueSet(ls.JoinWithCr(), true, false);
            } else if (x is bool bo) {
                ValueSet(bo.ToPlusMinus(), true, false);
            } else if (x is int iv) {
                ValueSet(iv.ToString(), true, false);
            } else if (x is Enum) {
                ValueSet(((int)x).ToString(), true, false);
            } else if (x is double dc) {
                ValueSet(dc.ToString(Constants.Format_Float2), true, false);
            } else if (x is double db) {
                ValueSet(db.ToString(Constants.Format_Float2), true, false);
            } else if (x is float fl) {
                ValueSet(fl.ToString(Constants.Format_Float2), true, false);
            } else if (x is Color co) {
                ValueSet(co.ToHtmlCode(), true, false);
            } else {
                Develop.DebugPrint(FehlerArt.Fehler, "Art unbekannt!");
            }
        }

        private void UpdateControlData(bool withCreate, int textLines, ItemCollectionList? list, ImageCode image) {

            #region propInfo & _propertynamecpl befüllen

            if (string.IsNullOrEmpty(_propertyName) || _propertyObject == null) {
                _methInfo = null;
                _propInfo = null;
                _propertynamecpl = string.Empty;
            } else {
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

            #endregion propInfo & _propertynamecpl befüllen

            #region Caption setzen

            if (!string.IsNullOrEmpty(_propertyName)) {
                var x = _propertyName.SplitAndCutBy("__");
                Caption = x[0].Replace("_", " ") + ":";
                FileEncryptionKey = string.Empty;
            } else {
                Caption = "[unbekannt]";
            }

            #endregion Caption setzen

            #region Art des Steuerelements bestimmen

            if (withCreate && _methInfo != null) {
                EditType = EditTypeFormula.Button;
                CaptionPosition = ÜberschriftAnordnung.ohne;
                var s = BlueFont.MeasureStringOfCaption(Caption.TrimEnd(":"));
                Size = new Size((int)s.Width + 50 + 22, 30);
                var c = (Button)CreateSubControls();
                c.Text = Caption.TrimEnd(":");
                if (image != ImageCode.None) {
                    c.ImageCode = QuickImage.Get(image, 22).ToString();
                }
            }
            if (withCreate && _propInfo != null) {
                switch (_propInfo.PropertyType.FullName.ToLower()) {
                    case "system.boolean": {
                            EditType = EditTypeFormula.Ja_Nein_Knopf;
                            var s = BlueFont.MeasureStringOfCaption(Caption);
                            Size = new Size((int)s.Width + 30, 22);
                            break;
                        }
                    default: // Alle enums sind ein eigener Typ.... deswegen alles in die Textbox
                        {
                            if (list != null) {
                                EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;
                                list.Appearance = BlueListBoxAppearance.ComboBox_Textbox;
                                var s = BlueFont.MeasureStringOfCaption(Caption);
                                var (biggestItemX, biggestItemY, _, _) = list.ItemData(); // BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed
                                var x = Math.Max((int)(biggestItemX + 20 + s.Width), 200);
                                var y = Math.Max(biggestItemY + (Skin.PaddingSmal * 2), 24);
                                Size = new Size(x, y);
                                var c = (ComboBox)CreateSubControls();
                                StyleComboBox(c, list, ComboBoxStyle.DropDownList);
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
                                EditType = EditTypeFormula.Textfeld;
                                var tmpName = _propInfo.PropertyType.FullName.ToLower();
                                if (textLines >= 2) {
                                    CaptionPosition = ÜberschriftAnordnung.Über_dem_Feld;
                                    Size = new Size(200, 16 + (24 * textLines));
                                    MultiLine = true;
                                    tmpName = "system.string";
                                } else {
                                    CaptionPosition = ÜberschriftAnordnung.Links_neben_Dem_Feld;
                                    Size = new Size(200, 24);
                                    MultiLine = false;
                                }

                                switch (tmpName) {
                                    case "system.string": this.SetFormat(VarType.Text); break;
                                    case "system.int32": this.SetFormat(VarType.Integer); break;
                                    case "system.float": this.SetFormat(VarType.Float); break;
                                    case "system.double": this.SetFormat(VarType.Float); break;
                                    case "system.drawing.color": this.SetFormat(VarType.Text); break;
                                    default: this.SetFormat(VarType.Text); break;
                                }

                                var c = CreateSubControls();
                                StyleTextBox((TextBox)c);
                            }
                            break;
                        }
                }
            }
            //else
            //{
            //    Develop.DebugPrint(_propertyName + " hat keine Zuordnung");
            //}

            #endregion Art des Steuerelements bestimmen

            #region QuickInfo setzen

            // https://stackoverflow.com/questions/32901771/multiple-enum-descriptions
            // [PropertyAttributes("", false)] setzen
            //_FehlerWennLeer = true;
            //_alwaysDiabled = false;
            if (_propInfo == null && _methInfo == null) {
                QuickInfo = string.Empty;
            } else {
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

            #endregion QuickInfo setzen

            SetValueFromProperty();
            GenFehlerText();
        }

        #endregion
    }
}