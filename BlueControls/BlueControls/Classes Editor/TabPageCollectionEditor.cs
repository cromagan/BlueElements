using System;
using System.ComponentModel.Design;
namespace BlueControls.Controls {
    internal sealed class TabPageCollectionEditor : CollectionEditor {
        public TabPageCollectionEditor(Type type) : base(type) {
        }
        protected override Type CreateCollectionItemType() => typeof(TabPage);
    }
}