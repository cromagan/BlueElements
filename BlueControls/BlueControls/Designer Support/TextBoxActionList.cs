using BlueBasics.Enums;
using BlueControls.Controls;
using BlueDatabase;
using BlueDatabase.Interfaces;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace BlueControls.Designer_Support;

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

    public IInputFormat TextFormat {
        get {
            //for (var z = 0; z < 100; z++) {
            //    var st = (VarType)z;
            //    if (st.ToString() == z.ToString()) {
            //        continue;
            //    }
            foreach (var thisFormat in FormatHolder.AllFormats) {
                //var x = new TextBox();
                //x.GetStyleFrom(st);
                if (thisFormat.IsFormatIdentical(_reverenceControl)) { return thisFormat; }
            }
            return null;
        }
        set => _reverenceControl.GetStyleFrom(value);
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
        //if ((int)ReverenceControl.ButtonStyle % 1000 is ((int)ButtonStyle.Checkbox) or ((int)ButtonStyle.Yes_or_No) or ((int)ButtonStyle.Pic1_or_Pic2) or ((int)ButtonStyle.Optionbox)) {
        //    items.GenerateAndAdd(new DesignerActionPropertyItem("Checked", "Checked", "Allgemein", "Der Checked-Status."));
        //}
        return items;
    }

    #endregion

    // Set a control property. This method makes Undo/Redo
    // work properly and marks the form as modified in the IDE.
    //private void SetControlProperty(string property_name, object Value) => TypeDescriptor.GetProperties(ReverenceControl)[property_name].SetValue(ReverenceControl, Value);
}