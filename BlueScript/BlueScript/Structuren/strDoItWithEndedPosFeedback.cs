

using BlueScript;

public struct strDoItWithEndedPosFeedback {


    public strDoItWithEndedPosFeedback(string errormessage, string value, int endpos) {
        ErrorMessage = errormessage;
        Value = value;
        Position = endpos;
    }

    public strDoItWithEndedPosFeedback(string errormessage) {
        Position = -1;
        ErrorMessage = errormessage;
        Value = null;
    }

    public int Position;
    public string Value;
    public string ErrorMessage;

}