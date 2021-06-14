using System;
using System.ComponentModel.Design;

namespace BlueControls.Controls {

    internal sealed class TabPageCollectionEditor : CollectionEditor {

        #region Constructors

        public TabPageCollectionEditor(Type type) : base(type) { }

        #endregion

        #region Methods

        protected override Type CreateCollectionItemType() => typeof(TabPage);

        #endregion
    }
}