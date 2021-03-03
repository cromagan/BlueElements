

public struct strGetEndFeedback {

    public strGetEndFeedback(string errormessage) {
        ContinuePosition = 0;
        ErrorMessage = errormessage;
        AttributeText = string.Empty;
        //ComandText = string.Empty;
    }


    public strGetEndFeedback(int continuePosition, string attributetext) {
        ContinuePosition = continuePosition;
        ErrorMessage = string.Empty;
        AttributeText = attributetext;
    }

    //public strGetEndFeedback(string errormessage, int continuePosition, string attributetext) {
    //    ContinuePosition = continuePosition;
    //    ErrorMessage = errormessage;
    //    AttributeText = attributetext;
    //}

    public int ContinuePosition;
    public string ErrorMessage;
    public string AttributeText;


}