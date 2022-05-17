namespace BlueSQLDatabase.EventArgs {

    public class OpenEditorEventArgs : System.EventArgs {

        #region Constructors

        public OpenEditorEventArgs(object ObjectToEdit, string Message, string Attribute1, string Attribute2) {
            this.ObjectToEdit = ObjectToEdit;
            this.Message = Message;
            this.Attribute1 = Attribute1;
            this.Attribute2 = Attribute2;
            Done = false;
        }

        #endregion

        #region Properties

        public string Attribute1 { get; set; }
        public string Attribute2 { get; set; }
        public bool Done { get; set; }
        public string Message { get; set; }
        public object ObjectToEdit { get; set; }

        #endregion
    }
}