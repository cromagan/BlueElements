// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;
using BlueControls.PadItems.FunktionsItems_Formular.Abstract;
using System.Linq.Expressions;

namespace BlueControls;

/// <summary>
/// Erweiterungen für Controls: Text bzw. Zustand setzen, QuickInfo aus
/// &lt;summary&gt;-Dokumentation übernehmen sowie Einfüge-Schaltflächen ausstatten.
/// </summary>
public static class GenericControlExtension {

    #region Methods

    /// <summary>
    /// Setzt control.Text auf den Wert der Property und übernimmt deren
    /// &lt;summary&gt; als QuickInfo.
    /// </summary>
    public static void Set<T>(this GenericControl control, Expression<Func<T>> property) {
        control.Text = property.Compile().Invoke()?.ToString() ?? string.Empty;
        SetQuickInfo(control, property);
    }

    /// <summary>
    /// Setzt control.Text auf den invarianten Wert der Property (ToString1)
    /// und übernimmt deren &lt;summary&gt; als QuickInfo.
    /// </summary>
    public static void Set(this GenericControl control, Expression<Func<int>> property) {
        control.Text = property.Compile().Invoke().ToString1();
        SetQuickInfo(control, property);
    }

    /// <summary>
    /// Setzt control.Text auf den invarianten Wert der Property (ToString1)
    /// und übernimmt deren &lt;summary&gt; als QuickInfo.
    /// </summary>
    public static void Set(this GenericControl control, Expression<Func<long>> property) {
        control.Text = property.Compile().Invoke().ToString1();
        SetQuickInfo(control, property);
    }

    /// <summary>
    /// Setzt control.Text auf den übergebenen, aufbereiteten Wert und übernimmt die
    /// &lt;summary&gt; der Quell-Property als QuickInfo.
    /// </summary>
    public static void Set<T>(this GenericControl control, string text, Expression<Func<T>> property) {
        control.Text = text;
        SetQuickInfo(control, property);
    }

    /// <summary>
    /// Setzt Symbol und Kurzbeschreibung einer Schaltfläche anhand eines instanziierbaren Element-Typs.
    /// </summary>
    public static void SetAddButtonInfo<T>(this Button b) where T : IReadableText, IDisposable, new() {
        using var t = new T();
        b.ImageCode = t.SymbolForReadableText()?.KeyName ?? string.Empty;
        b.QuickInfo = Generic.Summary(typeof(T));
    }

    /// <summary>
    /// Setzt die ausführliche QuickInfo einer Schaltfläche, die ein Formular-Element einfügt.
    /// </summary>
    public static void SetFormulaButtonInfo(this Button b, ReciverPadItem from) {
        var txt = "Fügt das Steuerelement des Types <b>" + b.Text.Replace("-", string.Empty) + "</b> hinzu:";

        txt += "<br><br><b><u>Beschreibung:</b></u>";
        txt += "<br>" + Generic.Summary(from.GetType());

        txt += "<br><br><b><u>Eigenschaften:</b></u>";

        if (from is { IsDisposed: false } ias) {
            if (ias.InputMustBeOneRow) {
                txt += "<br> - Das Element kann Filter <u>empfangen</u>.<br>" +
                    "   Diese müssen als Ergebniss <u>genau eine Zeile</u> einer Tabelle ergeben,<br>" +
                    "   da die Werte der Zeile in dem Element benutzt werden können.";
            } else {
                txt += "<br> - Das Element kann Filter <u>empfangen</u> und verarbeitet diese.";
            }
        }

        if (from is ReciverSenderPadItem) {
            txt += "<br> - Das Element kann Filter an andere Elemente <u>weitergeben</u>.";
        }

        if (!from.MustBeInDrawingArea) {
            txt += "<br> - Das Element dient nur zur Berechnung von Werten<br> und ist im Formular <u>nicht sichtbar</u>.";
        }

        b.QuickInfo = txt;
    }

    /// <summary>
    /// Setzt control.Checked auf den Wert der Property und übernimmt deren
    /// &lt;summary&gt; als QuickInfo.
    /// </summary>
    public static void SetChecked(this Button control, Expression<Func<bool>> property) {
        control.Checked = property.Compile().Invoke();
        SetQuickInfo(control, property);
    }

    /// <summary>
    /// Übernimmt die &lt;summary&gt; der Property als QuickInfo.
    /// Ohne Doku bleibt eine vorhandene QuickInfo erhalten.
    /// </summary>
    private static void SetQuickInfo<T>(GenericControl control, Expression<Func<T>> property) {
        if (property.Body is not MemberExpression memberExpression) { return; }

        var summary = Generic.Summary(memberExpression.Member);
        if (summary is { Length: > 0 }) { control.QuickInfo = summary; }
    }

    #endregion
}