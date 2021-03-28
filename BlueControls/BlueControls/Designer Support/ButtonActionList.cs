using BlueControls.Controls;
using BlueControls.Enums;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace BlueControls.Designer_Support
{
    public sealed class ButtonActionList : DesignerActionList
    {
        private readonly Button ReverenceControl;

        public ButtonActionList(IComponent component) : base(component)
        {

            // Save a reference to the control we are designing.
            ReverenceControl = (Button)component;

            // Save a reference to the DesignerActionUIService
            //  DesignerService = ctypex(GetService(GetType(DesignerActionUIService)), DesignerActionUIService)

            //Makes the Smart Tags open automatically 
            AutoShow = true;
        }


        public bool Checked
        {
            get => ReverenceControl.Checked;
            set => SetControlProperty("Checked", value);
        }




        public enButtonStyle ButtonStyle
        {
            get => ReverenceControl.ButtonStyle;
            set => SetControlProperty("ButtonStyle", value);
        }


        // Set a control property. This method makes Undo/Redo
        // work properly and marks the form as modified in the IDE.
        private void SetControlProperty(string property_name, object Value)
        {
            TypeDescriptor.GetProperties(ReverenceControl)[property_name].SetValue(ReverenceControl, Value);
        }




        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection
            {
                new DesignerActionHeaderItem("Allgemein"),


                new DesignerActionPropertyItem("ButtonStyle", "ButtonStyle", "Allgemein", "Das Verhalten des Buttons.")
            };

            if ((int)ReverenceControl.ButtonStyle % 1000 == (int)enButtonStyle.Checkbox || (int)ReverenceControl.ButtonStyle % 1000 == (int)enButtonStyle.Yes_or_No || (int)ReverenceControl.ButtonStyle % 1000 == (int)enButtonStyle.Pic1_or_Pic2 || (int)ReverenceControl.ButtonStyle % 1000 == (int)enButtonStyle.Optionbox)
            {
                items.Add(new DesignerActionPropertyItem("Checked", "Checked", "Allgemein", "Der Checked-Status."));
            }



            return items;
        }
    }
}


