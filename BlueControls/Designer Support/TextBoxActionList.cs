using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace BlueControls.Designer_Support;

#nullable enable

public sealed class TextBoxActionList : DesignerActionList {

    #region Fields

    private readonly TextBox _reverenceControl;

    #endregion

    #region Constructors

    public TextBoxActionList(IComponent component) : base(component) {
        // Save a reference to the control we are designing.
        _reverenceControl = (TextBox)component;
        // Makes the Smart Tags open automatically
        AutoShow = true;
    }

    #endregion

    #region Properties

    [TypeConverter(typeof(InputFormatConverter))]
    public IInputFormat? TextFormat {
        get {
            foreach (var thisFormat in FormatHolder.AllFormats) {
                if (thisFormat.IsFormatIdentical(_reverenceControl)) { return thisFormat; }
            }
            return null;
        }
        set => _reverenceControl.GetStyleFrom(value);
    }

    #endregion

    #region Methods

    public override DesignerActionItemCollection GetSortedActionItems() {
        var items = new DesignerActionItemCollection
        {
            new DesignerActionHeaderItem("Allgemein"),
            new DesignerActionPropertyItem("TextFormat", "Textformat", "Allgemein", "Wählen Sie das Textformat aus.")
        };

        return items;
    }

    #endregion

    // Set a control property. This method makes Undo/Redo
    // work properly and marks the form as modified in the IDE.
    //private void SetControlProperty(string propertyName, object value) => TypeDescriptor.GetProperties(_reverenceControl)[propertyName].SetValue(_reverenceControl, value);
}