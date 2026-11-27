// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer.Classes;

/// <summary>
/// Beschreibt ein gefundenes Methoden- oder Konstruktor-Member einer obersten Typdeklaration.
/// </summary>
public sealed class CsMemberInfo {

    #region Properties

    /// <summary>
    /// Letzte Zeile des Körperinhalts.
    /// </summary>
    public required int BodyEnd { get; init; }

    /// <summary>
    /// Spalte des Körper-Kopfs auf BodyOpenLine; -1 ohne Körper. Ab hier beginnt der Körperausdruck bei Einzeilern.
    /// </summary>
    public required int BodyOpenColumn { get; init; }

    /// <summary>
    /// Zeile mit dem Körper-Kopf ('{' bei Blöcken, '=>' bei Expression-Bodied); -1 ohne Körper.
    /// </summary>
    public required int BodyOpenLine { get; init; }

    /// <summary>
    /// Erste Zeile des Körperinhalts.
    /// </summary>
    public required int BodyStart { get; init; }

    /// <summary>
    /// Die Region-Kategorie des Members ("Methods", "Constructors" oder "Delegates").
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Letzte Zeile des Members laut Member-Ende-Ermittlung.
    /// </summary>
    public required int EndLine { get; init; }

    /// <summary>
    /// True, wenn das Member Expression-Bodied ('=&gt;') ist.
    /// </summary>
    public required bool ExpressionBodied { get; init; }

    /// <summary>
    /// Die Datei, in der das Member steht.
    /// </summary>
    public required string File { get; init; }

    /// <summary>
    /// True, wenn das Member einen auswertbaren Körper hat (Block oder Expression-Bodied).
    /// </summary>
    public required bool HasBody { get; init; }

    /// <summary>
    /// Erste Codezeile des Members.
    /// </summary>
    public required int HeaderStart { get; init; }

    public required bool IsAbstract { get; init; }

    /// <summary>
    /// True bei expliziter Interface-Implementierung (z. B. void IFoo.Bar()).
    /// </summary>
    public required bool IsExplicitImplementation { get; init; }

    public required bool IsExtern { get; init; }

    public required bool IsOverride { get; init; }

    public required bool IsPartial { get; init; }

    public required bool IsPublic { get; init; }

    public required bool IsStatic { get; init; }

    public required bool IsVirtual { get; init; }

    /// <summary>
    /// Der Deklarationsname; leer bei Operatoren, da diese keinen aufrufbaren Namen haben.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Spalte der öffnenden Klammer der Parameterliste; -1 ohne Parameterklammern.
    /// </summary>
    public required int ParamOpenColumn { get; init; }

    /// <summary>
    /// Zeile der öffnenden Klammer der Parameterliste; -1 ohne Parameterklammern.
    /// </summary>
    public required int ParamOpenLine { get; init; }

    /// <summary>
    /// Spalte der schließenden Klammer der Parameterliste; -1 ohne Parameterklammern.
    /// </summary>
    public required int ParamCloseColumn { get; init; }

    /// <summary>
    /// Zeile mit der schließenden Klammer der Parameterliste; -1 ohne Parameterklammern.
    /// </summary>
    public required int ParamCloseLine { get; init; }

    /// <summary>
    /// Die Parameternamen der Signatur in Reihenfolge.
    /// </summary>
    public required List<string> Parameters { get; init; }

    /// <summary>
    /// True, wenn der komplette Körper auf der Kopfzeile liegt (Einzeiler).
    /// </summary>
    public required bool SameLineBody { get; init; }

    /// <summary>
    /// Zeile der ersten Trivia (Doc-Kommentar, Attribut, Pragma) des Members; HeaderStart, wenn keine vorhanden.
    /// </summary>
    public required int TriviaStart { get; init; }

    /// <summary>
    /// True, wenn der umgebende Typ ein Interface ist.
    /// </summary>
    public required bool TypeIsInterface { get; init; }

    /// <summary>
    /// Der Name des umgebenden Typs.
    /// </summary>
    public required string TypeName { get; init; }

    #endregion
}
