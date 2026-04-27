// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BlueScript.Methods;


internal class Method_DateTimeNowUTC : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];
    public override string Command => "datetimeutcnow";
    public override List<string> Constants => [.. BlueBasics.ClassesStatic.Constants.DateTimeFormats];
    public override string Description => "Gibt die akutelle UTC-Uhrzeit im angegebenen Format (z.B. dd.MM.yyyy HH:mm:ss.fff) zurück. ";
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string Syntax => "DateTimeUTCNow(format)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        try {
            return new DoItFeedback(DateTime.UtcNow.ToString(attvar.ReadableText(0), CultureInfo.InvariantCulture));
        } catch {
            return new DoItFeedback("Der Umwandlungs-String '" + attvar.ReadableText(0) + "' ist fehlerhaft.", true, ld);
        }
    }

    #endregion
}