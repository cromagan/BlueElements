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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace BlueBasics;
// https://stackoverflow.com/questions/32901771/multiple-enum-descriptions
// https://stackoverflow.com/questions/1402803/passing-properties-by-reference-in-c-sharp

public class Accessor<T> {

    #region Fields

    private readonly Func<T>? _getter;
    private readonly Action<T>? _setter;

    #endregion

    #region Constructors

    /// <summary>
    /// Konstruktor, der einen Lambda-Ausdruck annimmt, der auf eine Eigenschaft oder ein Feld zeigt.
    /// </summary>
    /// <param name="expr"></param>
    public Accessor(Expression<Func<T>>? expr) {
        // Versuche, den Körper des Ausdrucks in einen MemberExpression umzuwandeln.
        // Das ist nötig, um auf die Details der Eigenschaft oder des Feldes zugreifen zu können.
        var memberExpression = (MemberExpression?)expr?.Body;

        // Hole den Ausdruck, der das Objekt darstellt, zu dem das Mitglied gehört.
        var instanceExpression = memberExpression?.Expression;

        // Erstelle einen Parameterausdruck für den Wert, der gesetzt werden soll.
        var parameter = Expression.Parameter(typeof(T));

        // Variable, um benutzerdefinierte Attribute zu speichern, falls vorhanden.
        IEnumerable<Attribute>? ca = null;

        // Überprüfe, ob das Mitglied eine Eigenschaft ist und behandle es entsprechend.
        if (memberExpression?.Member is PropertyInfo propertyInfo) {
            // Hole die Set-Methode der Eigenschaft, falls vorhanden.
            var setm = propertyInfo.GetSetMethod();
            if (setm != null) {
                // Erstelle einen Lambda-Ausdruck, der den Aufruf der Set-Methode darstellt, und kompiliere ihn zu einem Delegaten.
                _setter = Expression.Lambda<Action<T>>(Expression.Call(instanceExpression, setm, parameter), parameter).Compile();
            }

            // Hole die Get-Methode der Eigenschaft, falls vorhanden.
            var getm = propertyInfo.GetGetMethod();
            if (getm != null) {
                // Erstelle einen Lambda-Ausdruck, der den Aufruf der Get-Methode darstellt, und kompiliere ihn zu einem Delegaten.
                _getter = Expression.Lambda<Func<T>>(Expression.Call(instanceExpression, getm)).Compile();
            }

            // Setze die Eigenschaften, die anzeigen, ob die Eigenschaft gelesen oder geschrieben werden kann.
            CanWrite = propertyInfo.CanWrite;
            CanRead = propertyInfo.CanRead;
            Name = propertyInfo.Name;
            //TypeFullname = propertyInfo.PropertyType.FullName; // Auskommentiert. Entkommentieren, wenn der vollständige Typname benötigt wird.

            // Hole alle benutzerdefinierten Attribute, die für die Eigenschaft definiert sind.
            ca = propertyInfo.GetCustomAttributes();
        }
        // Überprüfe, ob das Mitglied ein Feld ist und behandle es entsprechend.
        else if (memberExpression?.Member is FieldInfo fieldInfo) {
            // Erstelle einen Lambda-Ausdruck, der die Zuweisung zu einem Feld darstellt, und kompiliere ihn zu einem Delegaten.
            _setter = Expression.Lambda<Action<T>>(Expression.Assign(memberExpression, parameter), parameter).Compile();
            // Erstelle einen Lambda-Ausdruck, der den Zugriff auf ein Feld darstellt, und kompiliere ihn zu einem Delegaten.
            _getter = Expression.Lambda<Func<T>>(Expression.Field(instanceExpression, fieldInfo)).Compile();

            // Setze die Eigenschaften, die anzeigen, ob das Feld gelesen oder geschrieben werden kann.
            CanWrite = !fieldInfo.IsInitOnly;
            CanRead = true;
            Name = fieldInfo.Name;
            //TypeFullname = fieldInfo.FieldType.FullName; // Auskommentiert. Entkommentieren, wenn der vollständige Typname benötigt wird.

            // Hole alle benutzerdefinierten Attribute, die für das Feld definiert sind.
            ca = fieldInfo.GetCustomAttributes();
        }

        // Wenn benutzerdefinierte Attribute gefunden wurden, verarbeite sie.
        if (ca != null) {
            foreach (var thisas in ca) {
                // Überprüfe, ob das Attribut ein DescriptionAttribute ist und hole die Beschreibung.
                if (thisas is DescriptionAttribute da) {
                    QuickInfo = da.Description;
                }
            }
        }
    }

    #endregion

    #region Properties

    public bool CanRead { get; }
    public bool CanWrite { get; }
    public string Name { get; } = "[unbekannt]";
    public string QuickInfo { get; } = string.Empty;

    #endregion

    #region Methods

    public T? Get() {
        if (_getter != null) { return _getter(); }
        Develop.DebugPrint("Getter ist null!");
        return default;
    }

    public void Set(T value) {
        if (_setter != null) {
            _setter(value);
        } else {
            Develop.DebugPrint("Setter ist null!");
        }
    }

    #endregion
}