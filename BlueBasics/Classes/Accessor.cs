// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Interfaces;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading;

namespace BlueBasics.Classes;
// https://stackoverflow.com/questions/32901771/multiple-enum-descriptions
// https://stackoverflow.com/questions/1402803/passing-properties-by-reference-in-c-sharp

public class Accessor<T> : IDisposableExtended {

    #region Fields

    private readonly Func<T>? _getter;
    private readonly Action<T>? _setter;
    private readonly object? _target;
    private volatile int _isDisposedFlag;

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

        // Zielobjekt erfassen und INPC abonnieren
        if (instanceExpression != null) {
            var targetLambda = Expression.Lambda<Func<object>>(Expression.Convert(instanceExpression, typeof(object)));
            _target = targetLambda.Compile()();
            if (_target is INotifyPropertyChanged inpc) {
                inpc.PropertyChanged += OnTargetPropertyChanged;
            }
        }
    }

    #endregion

    #region Events

    public event EventHandler? ValueChanged;

    #endregion

    #region Properties

    public bool CanRead { get; }
    public bool CanWrite { get; }
    public bool IsDisposed => _isDisposedFlag == 1;
    public string Name { get; } = "[unbekannt]";
    public string QuickInfo { get; } = string.Empty;

    #endregion

    #region Methods

    public void Dispose() {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (_target is INotifyPropertyChanged inpc) {
            inpc.PropertyChanged -= OnTargetPropertyChanged;
        }

        GC.SuppressFinalize(this);
    }

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

    private void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == Name || string.IsNullOrEmpty(e.PropertyName)) {
            OnValueChanged();
        }
    }

    private void OnValueChanged() => ValueChanged?.Invoke(this, System.EventArgs.Empty);

    #endregion
}