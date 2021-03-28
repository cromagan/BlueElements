using System.ComponentModel.Design;

namespace BlueControls.Designer_Support
{
    internal sealed class ButtonDesigner : BasicDesigner
    {
        private DesignerActionListCollection aList;



        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (aList == null)
                {
                    aList = new DesignerActionListCollection
                    {
                        new ButtonActionList(Component)
                    };
                }
                return aList;
            }
        }


    }
}

