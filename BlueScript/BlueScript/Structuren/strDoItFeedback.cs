

using BlueScript;

public struct strDoItFeedback {


    public strDoItFeedback(string errormessage) {
        ErrorMessage = errormessage;
        Value = string.Empty;
    }

    public string Value;
    public string ErrorMessage;

}