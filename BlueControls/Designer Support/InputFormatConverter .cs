// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Globalization;

namespace BlueControls.Designer_Support;

public class InputFormatConverter : TypeConverter {

    #region Methods

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) {
        if (value is string stringValue) {
            foreach (var format in FormatHolder.AllFormats.Instances) {
                if (format.KeyName.Equals(stringValue, StringComparison.OrdinalIgnoreCase)) {
                    return format;
                }
            }
            throw Develop.DebugError($"Cannot convert '{stringValue}' to type {typeof(IInputFormat)}.");
        }
        var result = base.ConvertFrom(context, culture, value);
        return result ?? throw Develop.DebugError("Conversion failed.");
    }

    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) {
        if (destinationType == typeof(string) && value is IInputFormat inputFormat) {
            foreach (var format in FormatHolder.AllFormats.Instances) {
                if (inputFormat is IReadableTextWithKey key && format.KeyName.Equals(key.KeyName, StringComparison.OrdinalIgnoreCase)) {
                    return format.KeyName;
                }
            }
            throw Develop.DebugError($"Cannot convert type {typeof(IInputFormat)} to string.");
        }
        var result = base.ConvertTo(context, culture, value, destinationType);
        return result ?? throw new InvalidOperationException("Conversion failed.");
    }

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context) {
        IList<string> formatNames = [];
        foreach (var formatHolder in FormatHolder.AllFormats.Instances) {
            formatNames.Add(formatHolder.KeyName);
        }
        return new StandardValuesCollection(formatNames.ToArray());
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) =>
        false;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

    #endregion
}