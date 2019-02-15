namespace BlueDatabase.EventArgs
{
    public class OpenEditorEventArgs : System.EventArgs
    {


        public OpenEditorEventArgs(object ObjectToEdit, string Message, string Attribute1, string Attribute2)
        {
            this.ObjectToEdit = ObjectToEdit;
            this.Message = Message;
            this.Attribute1 = Attribute1;
            this.Attribute2 = Attribute2;
            Done = false;
        }

        public object ObjectToEdit { get; set; }
        public bool Done { get; set; }

        public string Message { get; set; }
        public string Attribute1 { get; set; }
        public string Attribute2 { get; set; }
    }
}
