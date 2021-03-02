

public struct strParseFeedback {


    //public strParseFeedback(string errormessage) {
    //    Position = 0;
    //    ErrorMessage = errormessage;
    //    //abbruch = _abbruch;
    //    //betweentext = _betweentext;
    //    //codeblockafter = _codeblockafter;
    //}


    public strParseFeedback(int position, string errormessage) {
        Position = position;
        ErrorMessage = errormessage;
        //abbruch = _abbruch;
        //betweentext = _betweentext;
        //codeblockafter = _codeblockafter;
    }

    public int Position;
    public string ErrorMessage;


}