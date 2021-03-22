#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion

namespace BlueBasics.Enums {
    public enum enErrorReason {
        /// <summary>
        /// Ob der Wert gelesen werden kann. Ein Speichern/Bearbeiten ist nicht geplant.
        /// </summary>
        OnlyRead = 1,

        ///// <summary>
        ///// Ob das Feld im allgemeinen bearbeitet werden kann, wenn keine Prozesse dazwischenpfuschen
        ///// </summary>
        EditNormaly = 2,

        /// <summary>
        /// Wenn die Daten eigentlich geschrieben werden müssen
        /// </summary>
        EditAcut = 4,

        /// <summary>
        /// Es ist noch Zeit, der Benutzer kann ausgesperrt werden
        /// </summary>
        EditGeneral = 8,


        Save = 16,

        /// <summary>
        ///  Wenn die Daten von der Festplatte geladen und auch weiterverarbeitet werdern sollen
        /// </summary>
        Load = 32,

        /// <summary>
        /// Wenn die Daten von der Festplatte geladen - aber wieder verworfen werdern. Nur für Kontrollzwecke benutzen!
        /// </summary>
        LoadForCheckingOnly = 64


    }
}
