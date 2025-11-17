using BlueControls.Controls;
using BlueControls.Enums;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace BlueControls.Designer_Support;

public sealed class ButtonActionList : DesignerActionList {

    #region Fields

    private readonly Button _reverenceControl;

    #endregion

    #region Constructors

    public ButtonActionList(IComponent component) : base(component) {
        // Save a reference to the control we are designing.
        _reverenceControl = (Button)component;
        // Save a reference to the DesignerActionUIService
        //  DesignerService = ctypex(GetService(GetType(DesignerActionUIService)), DesignerActionUIService)
        //Makes the Smart Tags open automatically
        AutoShow = true;
    }

    #endregion

    #region Properties

    public ButtonStyle ButtonStyle {
        get => _reverenceControl.ButtonStyle;
        set => SetControlProperty("ButtonStyle", value);
    }

    public bool Checked {
        get => _reverenceControl.Checked;
        set => SetControlProperty("Checked", value);
    }

    #endregion

    #region Methods

    public override DesignerActionItemCollection GetSortedActionItems() {
        DesignerActionItemCollection items =
        [
            new DesignerActionHeaderItem("Allgemein"),
            new DesignerActionPropertyItem("ButtonStyle", "ButtonStyle", "Allgemein", "Das Verhalten des Buttons.")
        ];
        if ((int)_reverenceControl.ButtonStyle % 1000 is (int)ButtonStyle.Checkbox or (int)ButtonStyle.Yes_or_No or (int)ButtonStyle.Optionbox) {
            items.Add(new DesignerActionPropertyItem("Checked", "Checked", "Allgemein", "Der Checked-Status."));
        }
        return items;
    }

    // Set a control property. This method makes Undo/Redo
    // work properly and marks the form as modified in the IDE.
    private void SetControlProperty(string propertyName, object value) => TypeDescriptor.GetProperties(_reverenceControl)[propertyName].SetValue(_reverenceControl, value);

    #endregion
}