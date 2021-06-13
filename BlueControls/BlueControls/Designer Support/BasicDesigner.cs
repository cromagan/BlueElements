using System.Collections;
using System.Windows.Forms.Design;

namespace BlueControls.Designer_Support {
    public class BasicDesigner : ControlDesigner {
        protected override void PreFilterProperties(IDictionary properties) {
            properties.Remove("BackColor");
            properties.Remove("BackgroundImage");
            properties.Remove("BackgroundImageLayout");
            properties.Remove("Font");
            properties.Remove("ForeColor");
            properties.Remove("UseWaitCursor");
            properties.Remove("RightToLeft");
            // properties.Remove("Cursor");
            //properties.Remove("CausesValidation");
            properties.Remove("AccessibleDescription");
            properties.Remove("AccessibleName");
            properties.Remove("AccessibleRole");
            properties.Remove("GenerateMember");
            properties.Remove("ContextMenuStrip");
            properties.Remove("ImeMode");
            properties.Remove("AllowDrop");
            properties.Remove("(DataBindings)");
            properties.Remove("(AppicationSettings)");
            base.PreFilterProperties(properties);
        }
    }
}
