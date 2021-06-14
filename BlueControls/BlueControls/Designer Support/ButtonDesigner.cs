using System.ComponentModel.Design;

namespace BlueControls.Designer_Support {

    internal sealed class ButtonDesigner : BasicDesigner {

        #region Fields

        private DesignerActionListCollection aList;

        #endregion

        #region Properties

        public override DesignerActionListCollection ActionLists {
            get {
                if (aList == null) {
                    aList = new DesignerActionListCollection
                    {
                        new ButtonActionList(Component)
                    };
                }
                return aList;
            }
        }

        #endregion
    }
}