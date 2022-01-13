using System.ComponentModel.Design;

namespace BlueControls.Designer_Support {

    internal sealed class TextBoxDesigner : BasicDesigner {

        #region Fields

        private DesignerActionListCollection aList;

        #endregion

        #region Properties

        public override DesignerActionListCollection ActionLists {
            get {
                if (aList == null) {
                    aList = new DesignerActionListCollection
                    {
                        new TextBoxActionList(Component)
                    };
                }
                return aList;
            }
        }

        #endregion
    }
}