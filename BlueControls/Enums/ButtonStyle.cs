// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Enums;

public enum ButtonStyle {
    Button = 0,
    Button_Big = 32768,
    Button_Big_Borderless = Button_Big | Borderless,

    SliderButton = 1,

    Checkbox = 2,
    Checkbox_Big_Borderless = Checkbox | Button_Big_Borderless,
    Checkbox_Text = Checkbox | Text,
    Yes_or_No = 6, // = Checkbox | 4,

    Optionbox = 16,
    Optionbox_Big_Borderless = Optionbox | Button_Big_Borderless,
    Optionbox_Text = Optionbox | Text,

    ComboBoxButton = 32,
    ComboBoxButton_Borderless = ComboBoxButton | Borderless,

    Text = 131072,

    Borderless = 65536
}