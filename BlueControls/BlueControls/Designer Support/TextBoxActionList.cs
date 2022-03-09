﻿using BlueBasics.Enums;
using BlueControls.Controls;
using BlueDatabase.Interfaces;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace BlueControls.Designer_Support {

    public sealed class TextBoxActionList : DesignerActionList {

        #region Fields

        private readonly TextBox _reverenceControl;

        #endregion

        #region Constructors

        public TextBoxActionList(IComponent component) : base(component) {
            // Save a reference to the control we are designing.
            _reverenceControl = (TextBox)component;
            // Save a reference to the DesignerActionUIService
            //  DesignerService = ctypex(GetService(GetType(DesignerActionUIService)), DesignerActionUIService)
            //Makes the Smart Tags open automatically
            AutoShow = true;
        }

        #endregion

        #region Properties

        public enVarType TextFormat {
            get {
                for (var z = 0; z < 100; z++) {
                    var st = (enVarType)z;
                    if (st.ToString() == z.ToString()) {
                        continue;
                    }

                    var x = new TextBox();
                    x.SetFormat(st);
                    if (x.IsFormatIdentical(_reverenceControl)) { return st; }
                }
                return enVarType.Unbekannt;
            }
            set => _reverenceControl.SetFormat(value);
        }

        #endregion

        //public bool Checked {
        //    get => ReverenceControl.Checked;
        //    set => SetControlProperty("Checked", value);
        //}

        #region Methods

        public override DesignerActionItemCollection GetSortedActionItems() {
            DesignerActionItemCollection items = new()
            {
                new DesignerActionHeaderItem("Allgemein"),
                new DesignerActionPropertyItem("TextFormat", "TextFormat", "Allgemein", "Wert kann nicht geändert werden. Bei Anklicken werden die entsprechenden Felder gesetzt.")
            };
            //if ((int)ReverenceControl.ButtonStyle % 1000 is ((int)enButtonStyle.Checkbox) or ((int)enButtonStyle.Yes_or_No) or ((int)enButtonStyle.Pic1_or_Pic2) or ((int)enButtonStyle.Optionbox)) {
            //    items.Add(new DesignerActionPropertyItem("Checked", "Checked", "Allgemein", "Der Checked-Status."));
            //}
            return items;
        }

        #endregion

        // Set a control property. This method makes Undo/Redo
        // work properly and marks the form as modified in the IDE.
        //private void SetControlProperty(string property_name, object Value) => TypeDescriptor.GetProperties(ReverenceControl)[property_name].SetValue(ReverenceControl, Value);
    }
}