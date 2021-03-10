

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


    public static strDoItFeedback FalscherDatentyp() {
        return new strDoItFeedback("Falscher Datentyp.");
    }

    public static strDoItFeedback AttributFehler() {
        return new strDoItFeedback("Attributfehler.");
    }

    public static strDoItFeedback VariableNichtGefunden() {
        return new strDoItFeedback("Variable nicht gefunden.");
    }


    public static strDoItFeedback Wahr() {
        return new strDoItFeedback("true", string.Empty);
    }

    public static strDoItFeedback Falsch() {
        return new strDoItFeedback("false", string.Empty);
    }
}