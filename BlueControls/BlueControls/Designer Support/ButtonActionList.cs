using BlueControls.Controls;
using BlueControls.Enums;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace BlueControls.Designer_Support {

    public sealed class ButtonActionList : DesignerActionList {

        #region Fields

        private readonly Button ReverenceControl;

        #endregion

        #region Constructors

        public ButtonActionList(IComponent component) : base(component) {
            // Save a reference to the control we are designing.
            ReverenceControl = (Button)component;
            // Save a reference to the DesignerActionUIService
            //  DesignerService = ctypex(GetService(GetType(DesignerActionUIService)), DesignerActionUIService)
            //Makes the Smart Tags open automatically
            AutoShow = true;
        }

        #endregion

        #region Properties

        public enButtonStyle ButtonStyle {
            get => ReverenceControl.ButtonStyle;
            set => SetControlProperty("ButtonStyle", value);
        }

        public bool Checked {
            get => ReverenceControl.Checked;
            set => SetControlProperty("Checked", value);
        }

        #endregion

        #region Methods

        public override DesignerActionItemCollection GetSortedActionItems() {
            DesignerActionItemCollection items = new()
            {
                new DesignerActionHeaderItem("Allgemein"),
                new DesignerActionPropertyItem("ButtonStyle", "ButtonStyle", "Allgemein", "Das Verhalten des Buttons.")
            };
            if ((int)ReverenceControl.ButtonStyle % 1000 is ((int)enButtonStyle.Checkbox) or ((int)enButtonStyle.Yes_or_No) or ((int)enButtonStyle.Pic1_or_Pic2) or ((int)enButtonStyle.Optionbox)) {
                items.Add(new DesignerActionPropertyItem("Checked", "Checked", "Allgemein", "Der Checked-Status."));
            }
            return items;
        }

        // Set a control property. This method makes Undo/Redo
        // work properly and marks the form as modified in the IDE.
        private void SetControlProperty(string property_name, object Value) => TypeDescriptor.GetProperties(ReverenceControl)[property_name].SetValue(ReverenceControl, Value);

        #endregion
    }
}