

using BlueScript;

public struct strDoItFeedback {


    public strDoItFeedback(string errormessage) {
        ErrorMessage = errormessage;
        Value = string.Empty;
    }

    public strDoItFeedback(string value, string errormessage) {
        ErrorMessage = errormessage;
        Value = value;
    }

    public string Value;
    public string ErrorMessage;

}