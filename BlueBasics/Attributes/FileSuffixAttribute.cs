// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

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
