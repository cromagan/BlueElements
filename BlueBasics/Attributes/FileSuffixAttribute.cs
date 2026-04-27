// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;

namespace BlueBasics.Attributes;

/// <summary>
/// Attribut zur Kennzeichnung von CachedFile-Ableitungen mit ihrem Datei-Suffix.
/// Wird von CachedFileSystem genutzt, um den richtigen Typ anhand der Dateiendung zu ermitteln.
/// Kann mehrfach pro Klasse verwendet werden, um mehrere Suffixe zu registrieren.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class FileSuffixAttribute : Attribute {

    #region Constructors

    public FileSuffixAttribute(string suffix) => Suffix = suffix;

    #endregion

    #region Properties

    /// <summary>
    /// Das Datei-Suffix (z.B. ".cfo", ".bdbc"), das diesem Typ zugeordnet ist.
    /// </summary>
    public string Suffix { get; }

    #endregion
}
