using BlueBasics;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel; // Für TypeConverter
using System.Globalization; // Für CultureInfo
using System.Linq; // Für IList<T>

public class InputFormatConverter : TypeConverter {

    #region Methods

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
        if (destinationType == typeof(string)) {
            return true;
        }
        return base.CanConvertTo(context, destinationType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
        if (value is string stringValue) {
            // Durchsuchen Sie die Liste der verfügbaren Formate, um das entsprechende IInputFormat-Objekt zu finden.
            foreach (var format in FormatHolder.AllFormats) {
                if (format.Name.Equals(stringValue, StringComparison.OrdinalIgnoreCase)) {
                    return format; // Gibt das gefundene IInputFormat-Objekt zurück.
                }
            }
            throw new ArgumentException($"Cannot convert '{stringValue}' to type {typeof(IInputFormat)}.");
        }
        return base.ConvertFrom(context, culture, value);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
        if (destinationType == typeof(string) && value is IInputFormat inputFormat) {
            // Finden Sie den Namen des Formats basierend auf dem IInputFormat-Objekt.
            foreach (var format in FormatHolder.AllFormats) {
                if (format.Equals(inputFormat)) {
                    // Angenommen, jedes IInputFormat-Objekt hat eine Eigenschaft 'Name'.
                    return format.Name;
                }
            }
            throw new ArgumentException($"Cannot convert type {typeof(IInputFormat)} to string.");
        }
        return base.ConvertTo(context, culture, value, destinationType);
    }

    // Eine Liste von Standardwerten abrufen.
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
        // Hier greifen wir auf die statische Liste AllFormats zu und extrahieren die Namen als Strings.
        IList<string> formatNames = new List<string>();
        foreach (var formatHolder in FormatHolder.AllFormats) {
            // Fügen Sie den Namen des Formats zur Liste hinzu.
            formatNames.Add(formatHolder.Name);
        }
        return new StandardValuesCollection(formatNames.ToArray());
    }

    // Überprüfen, ob die Liste der Standardwerte exklusiv ist.
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) =>
        // False bedeutet, dass auch andere Werte neben den Standardwerten möglich sind.
        false;

    // Überprüfen, ob dieser Konverter eine Liste von Standardwerten unterstützt.
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

    #endregion
}