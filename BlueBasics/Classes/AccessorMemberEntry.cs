// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.ComponentModel;

namespace BlueBasics.Classes;

/// <summary>
/// Pro Accessor einmalig berechnete Reflection-Metadaten. Kapselt Lese- und
/// Schreibzugriff auf ein Property oder Feld über <see cref="PropertyInfo" />
/// bzw. <see cref="FieldInfo" />.
/// </summary>
internal sealed class AccessorMemberEntry<T> {

    #region Fields

    /// <summary>Platzhalter für den Fall, dass kein Member aufgelöst werden konnte.</summary>
    public static readonly AccessorMemberEntry<T> Unknown = new();

    private readonly FieldInfo? _field;
    private readonly PropertyInfo? _property;

    #endregion

    #region Constructors

    private AccessorMemberEntry() { }

    private AccessorMemberEntry(MemberInfo member) {
        switch (member) {
            case PropertyInfo prop:
                Name = prop.Name;
                CanRead = prop.CanRead;
                CanWrite = prop.CanWrite;
                QuickInfo = prop.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
                _property = prop;
                break;

            case FieldInfo field:
                Name = field.Name;
                CanRead = true;
                CanWrite = !field.IsInitOnly;
                QuickInfo = field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
                _field = field;
                break;
        }
    }

    #endregion

    #region Properties

    public bool CanRead { get; private init; }
    public bool CanWrite { get; private init; }
    public string Name { get; private init; } = "[unbekannt]";
    public string QuickInfo { get; private init; } = string.Empty;

    #endregion

    #region Methods

    public static AccessorMemberEntry<T> Create(MemberInfo member) => new(member);

    /// <summary>
    /// Liest den Wert über Reflection. Für Instance-Member mit <c>null</c>-Ziel
    /// wird <c>false</c> zurückgegeben (entspricht einem nicht verfügbaren Getter).
    /// </summary>
    public bool TryGet(object? target, out T? value) {
        if (_property is not null) {
            value = (T?)_property.GetValue(target);
            return true;
        }
        if (_field is not null) {
            value = (T?)_field.GetValue(target);
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Schreibt den Wert über Reflection. Für Instance-Member mit <c>null</c>-Ziel
    /// wird <c>false</c> zurückgegeben.
    /// </summary>
    public bool TrySet(object? target, T value) {
        if (_property is not null && _property.CanWrite) {
            _property.SetValue(target, value);
            return true;
        }
        if (_field is not null && !_field.IsInitOnly) {
            _field.SetValue(target, value);
            return true;
        }
        return false;
    }

    #endregion
}
