#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueControls.AdditionalScriptMethods;

internal class Method_SendKeys : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal];

    public override string Command => "sendkeys";

    public override List<string> Constants => [];

    public override string Description => "Simuliert Tastatureingaben. Die Eingabe wird als String übergeben. Spezielle Tasten können in geschweiften Klammern angegeben werden, z.B. {ENTER}, {TAB}. Großbuchstaben werden automatisch mit SHIFT gesendet.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => -1;

    public override MethodType MethodLevel => MethodType.ManipulatesUser;

    public override bool MustUseReturnValue => false;

    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "SendKeys(KeySequence)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var keySequence = attvar.ValueStringGet(0);

        if (string.IsNullOrEmpty(keySequence)) {
            return new DoItFeedback("Keine Tastatureingabe angegeben", true, ld);
        }

        for (var i = 0; i < keySequence.Length; i++) {
            var c = keySequence[i];

            if (c == '{') {
                var endBrace = keySequence.IndexOf('}', i);
                if (endBrace == -1) {
                    return new DoItFeedback("Fehlende schließende geschweifte Klammer", true, ld);
                }

                var specialKey = keySequence.Substring(i + 1, endBrace - i - 1).ToUpper();
                if (!SendSpecialKey(specialKey)) {
                    return new DoItFeedback("Unbekannte Spezialtaste: " + specialKey, true, ld);
                }

                i = endBrace;
                continue;
            }

            // Handle uppercase letters and symbols that require shift
            var needsShift = char.IsUpper(c) || IsShiftRequired(c);
            if (needsShift) {
                WindowsRemoteControl.KeyDown(KeyCode.VK_SHIFT);
                Generic.Pause(0.01, false);
            }

            // Convert to virtual key code and send
            var key = GetVirtualKeyCode(c);
            WindowsRemoteControl.KeyDown(key);
            Generic.Pause(0.01, false);
            WindowsRemoteControl.KeyUp(key);
            Generic.Pause(0.01, false);

            if (needsShift) {
                WindowsRemoteControl.KeyUp(KeyCode.VK_SHIFT);
                Generic.Pause(0.01, false);
            }
        }

        return DoItFeedback.Null();
    }

    private static KeyCode GetVirtualKeyCode(char c) {
        // Handle basic ASCII characters
        if (char.IsLetter(c)) {
            return (KeyCode)((int)KeyCode.VK_A + char.ToUpper(c) - 'A');
        }

        if (char.IsDigit(c)) {
            return (KeyCode)((int)KeyCode.VK_0 + c - '0');
        }

        // Handle special characters
        return c switch {
            ' ' => KeyCode.VK_SPACE,
            '.' => KeyCode.VK_DECIMAL,
            ',' => KeyCode.VK_OEM_COMMA,
            ';' => KeyCode.VK_OEM_1,
            '+' => KeyCode.VK_OEM_PLUS,
            '-' => KeyCode.VK_OEM_MINUS,
            '/' => KeyCode.VK_OEM_2,
            '\\' => KeyCode.VK_OEM_5,
            '[' => KeyCode.VK_OEM_4,
            ']' => KeyCode.VK_OEM_6,
            '`' => KeyCode.VK_OEM_3,
            '\'' => KeyCode.VK_OEM_7,
            '=' => KeyCode.VK_OEM_PLUS,  // Shift not needed for =
            _ => KeyCode.VK_SPACE  // Default fallback
        };
    }

    private static bool IsShiftRequired(char c) => "~!@#$%^&*()_+{}|:\"<>?".IndexOf(c) >= 0;

    private static bool SendSpecialKey(string key) {
        var specialKey = key switch {
            "ENTER" => KeyCode.VK_RETURN,
            "TAB" => KeyCode.VK_TAB,
            "ESC" => KeyCode.VK_ESCAPE,
            "ESCAPE" => KeyCode.VK_ESCAPE,
            "HOME" => KeyCode.VK_HOME,
            "END" => KeyCode.VK_END,
            "LEFT" => KeyCode.VK_LEFT,
            "RIGHT" => KeyCode.VK_RIGHT,
            "UP" => KeyCode.VK_UP,
            "DOWN" => KeyCode.VK_DOWN,
            "PGUP" => KeyCode.VK_PRIOR,
            "PGDN" => KeyCode.VK_NEXT,
            "DELETE" => KeyCode.VK_DELETE,
            "DEL" => KeyCode.VK_DELETE,
            "INSERT" => KeyCode.VK_INSERT,
            "INS" => KeyCode.VK_INSERT,
            "BACKSPACE" => KeyCode.VK_BACK,
            "BS" => KeyCode.VK_BACK,
            "F1" => KeyCode.VK_F1,
            "F2" => KeyCode.VK_F2,
            "F3" => KeyCode.VK_F3,
            "F4" => KeyCode.VK_F4,
            "F5" => KeyCode.VK_F5,
            "F6" => KeyCode.VK_F6,
            "F7" => KeyCode.VK_F7,
            "F8" => KeyCode.VK_F8,
            "F9" => KeyCode.VK_F9,
            "F10" => KeyCode.VK_F10,
            "F11" => KeyCode.VK_F11,
            "F12" => KeyCode.VK_F12,
            _ => KeyCode.VK_SPACE
        };

        if (specialKey == KeyCode.VK_SPACE) {
            return false;
        }

        WindowsRemoteControl.KeyDown(specialKey);
        Generic.Pause(0.01, false);
        WindowsRemoteControl.KeyUp(specialKey);
        Generic.Pause(0.01, false);
        return true;
    }

    #endregion
}