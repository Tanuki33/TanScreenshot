using System.Collections.Generic;
using System.Windows.Forms;

public static class KeyDisplayHelper
{
    private static readonly Dictionary<Keys, string> KeyDisplayNames = new Dictionary<Keys, string>
    {
        { Keys.LControlKey, "Ctrl" },
        { Keys.RControlKey, "Ctrl" },
        { Keys.LMenu, "Alt" },
        { Keys.RMenu, "Alt" },
        { Keys.LShiftKey, "Shift" },
        { Keys.RShiftKey, "Shift" },
        { Keys.PrintScreen, "Print Screen" },
        { Keys.Up, "↑" },
        { Keys.Down, "↓" },
        { Keys.Left, "←" },
        { Keys.Right, "→" },
        { Keys.D0, "0" },
        { Keys.D1, "1" },
        { Keys.D2, "2" },
        { Keys.D3, "3" },
        { Keys.D4, "4" },
        { Keys.D5, "5" },
        { Keys.D6, "6" },
        { Keys.D7, "7" },
        { Keys.D8, "8" },
        { Keys.D9, "9" },
        { Keys.Oemtilde, "~" },
        { Keys.OemMinus, "-" },
        { Keys.Oemplus, "+" },
        { Keys.OemOpenBrackets, "[" },
        { Keys.OemCloseBrackets, "]" },
        { Keys.OemPipe, "\\" },
        { Keys.OemSemicolon, ";" },
        { Keys.OemQuotes, "'" },
        { Keys.Oemcomma, "," },
        { Keys.OemPeriod, "." },
        { Keys.OemQuestion, "/" }
        // Add more mappings as needed for other keys you support
    };

    public static string GetDisplayName(Keys key)
    {
        if (KeyDisplayNames.TryGetValue(key, out string displayName))
        {
            return displayName;
        }
        return key.ToString(); // Fallback to the enum name if not mapped
    }
}