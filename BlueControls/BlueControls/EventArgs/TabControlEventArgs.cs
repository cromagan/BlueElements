namespace BlueControls.EventArgs {
    public sealed class TabControlEventArgs : System.EventArgs {
        public bool Cancel = false;

        public System.Windows.Forms.TabPage TabPage { get; }

        public int TabPageIndex { get; } = -1;

        public TabControlEventArgs(System.Windows.Forms.TabPage TabPage, int TabPageIndex) {
            this.TabPage = TabPage;
            this.TabPageIndex = TabPageIndex;
        }

    }
}