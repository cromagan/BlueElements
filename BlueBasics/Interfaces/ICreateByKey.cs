// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Interfaces;

/// <summary>
/// Markiert Typen, die sich über einen Schlüssel (z. B. Dateipfad oder
/// Tabellennamen) neu erzeugen können. Wird zusammen mit
/// <see cref="LiveInstanceCache{T}.GetOrCreate{TDerived}" /> genutzt, damit der
/// Cache eine neue Instanz ohne explizit übergebene Factory erzeugen kann —
/// die typspezifische Konstruktion erfolgt über den statisch abstrakten
/// Member <see cref="Create" />.
/// </summary>
/// <typeparam name="T">Der konkrete Typ, den <see cref="Create" /> liefert.
/// Identisch mit dem implementierenden Typ (CRTP).</typeparam>
public interface ICreateByKey<T> where T : class {

    #region Methods

    /// <summary>
    /// Erzeugt eine neue Instanz von <typeparamref name="T" /> für den
    /// angegebenen Schlüssel. Wird von
    /// <see cref="LiveInstanceCache{T}.GetOrCreate{TDerived}" /> nur bei einem
    /// Cache-Miss aufgerufen. Der Konstruktor trägt sich selbst in
    /// <see cref="LiveInstanceCache{T}.LiveInstances" /> ein.
    /// </summary>
    /// <param name="key">Schlüssel (z. B. Dateipfad oder Tabellenname), für den
    /// eine Instanz erzeugt werden soll.</param>
    static abstract T Create(string key);

    #endregion
}
