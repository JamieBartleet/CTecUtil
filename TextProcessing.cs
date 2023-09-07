using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CTecUtil
{
    public class TextProcessing
    {
        ///// <summary>The digit characters 0-9 as an array</summary>
        //public static readonly char[] DigitChars = { '0','1','2','3','4','5','6','7','8','9' };


        public static bool StringIsNumeric(string text) => new Regex("^[0-9]+").IsMatch(text);


        public static bool CharIsAlpha(char c)        => c >= 'A' && c <= 'Z';
        public static bool CharIsNumeric(char c)      => c >= '0' && c <= '9';
        public static bool CharIsPunctuation(char c)  => c >= '0' && c <= '9';
        public static bool CharIsAlphaNumeric(char c) => CharIsAlpha(c) || CharIsNumeric(c);
        public static bool CharIsPossibleMenuOption(char c)
            => CharIsAlphaNumeric(c) || c switch
            {
                '!' or '#' or '$' or '£' or '€' or '%' or '&' or '(' or ')' or '*' or '+' or '-' or '=' or '<' or '>' or '?' or '@' or '[' or ']' or '^' or '~' or '{' or '}' or '~' => true,
                _ => false
            };


        public static bool KeyEventArgsIsAlpha(KeyEventArgs e)
        {
            var keyStr = KeyToString(e);
            return keyStr.Length > 0 && CharIsAlpha(keyStr[0]);
        }

        public static bool KeyEventArgsIsNumeric(KeyEventArgs e)
        {
            var keyStr = KeyToString(e);
            return keyStr.Length > 0 && CharIsNumeric(keyStr[0]);
        }

        public static bool KeyEventArgsIsAlphaNumeric(KeyEventArgs e)
        {
            var keyStr = KeyToString(e);
            return keyStr.Length > 0 && CharIsAlphaNumeric(keyStr[0]);
        }

        public static bool KeyEventArgsIsPossibleMenuOption(KeyEventArgs e)
        {
            var keyStr = KeyToString(e);
            return keyStr.Length == 1 && CharIsPossibleMenuOption(keyStr[0]);
        }


        public static bool KeyIsAlpha(Key k) => k >= Key.A && k <= Key.Z;

        public static bool KeyIsNumeric(Key k)
            => k switch
            {
                Key.D0 or Key.D1 or Key.D2 or Key.D3 or Key.D4 or Key.D5 or Key.D6 or Key.D7 or Key.D8 or Key.D9 or 
                Key.NumPad0 or Key.NumPad1 or Key.NumPad2 or Key.NumPad3 or Key.NumPad4 or Key.NumPad5 or Key.NumPad6 or Key.NumPad7 or Key.NumPad8 or Key.NumPad9 => true,
                _ => false,
            };

        public static bool KeyIsAlphaNumeric(Key k) => KeyIsAlpha(k) || KeyIsNumeric(k);


        public static bool KeyIsSpecialKey(Key k) => new Key[] { Key.Tab, Key.Enter, Key.LeftAlt, Key.LeftShift, Key.RightShift }.Contains(k);


        /// <summary>
        /// Converts non-OEM Key to a string.<br/>E.g. Key.A -> "A"; Key.D1 or Key.Numpad1 -> "1"; Key.Div
        /// </summary>
        public static string KeyToString(Key k)
            => KeyIsAlpha(k) ? k.ToString()
            : KeyIsNumeric(k) ? DigitKeyToString(k)
            : k switch
            {
                Key.Add => "+",
                Key.Back => "BackSpace",
                Key.Decimal => ".",         // ???
                Key.Delete => "Del",
                Key.Divide => "/",
                Key.Down => "Down",
                Key.End => "End",
                Key.Enter => "Enter",
                Key.Escape => "Esc",
                Key.F1 => "F1",
                Key.F2 => "F2",
                Key.F3 => "F3",
                Key.F4 => "F4",
                Key.F5 => "F5",
                Key.F6 => "F6",
                Key.F7 => "F7",
                Key.F8 => "F8",
                Key.F9 => "F9",
                Key.F10 => "F10",
                Key.F11 => "F11",
                Key.F12 => "F12",
                Key.Home => "Home",
                Key.Insert => "Ins",
                Key.Left => "Left",
                Key.Multiply => "*",
                //Key.Oem1 => "",
                //Key.Oem2 => "",
                //Key.Oem3 => "",
                //Key.Oem4 => "",
                //Key.Oem5 => "",
                //Key.Oem6 => "",
                //Key.Oem7 => "",
                //Key.Oem8 => "",
                //Key.OemBackslash => "\\",
                //Key.OemCloseBrackets => "]",
                //Key.OemComma => ",",
                Key.OemMinus => "-",
                //Key.OemOpenBrackets => "[",
                //Key.OemPeriod => ".",
                //Key.OemPipe => "|",
                Key.OemPlus => "+",
                Key.OemQuestion => "?",
                //Key.OemQuotes => "\"",
                //Key.OemSemicolon => ";",
                Key.OemTilde => "~",
                Key.PageDown => "PgDn",
                Key.PageUp => "PgUp",
                Key.PrintScreen  => "PrtScrn",
                Key.Right => "Right",
                Key.Space => " ",
                Key.Subtract => "-",
                Key.Up  => "Up",
                _ => "",
            };


        /// <summary>
        /// Converts KeyEventArgs key to a string.
        /// </summary>
        public static string KeyToString(KeyEventArgs e) => e.SystemKey != Key.None ? TextProcessing.KeyToString(e.SystemKey)   // if Alt+key was pressed, the key 
                                                                                    : TextProcessing.KeyToString(e.Key);        // is e.SystemKey rather than e.Key


        /// <summary>
        /// Converts digit Key to a string.<br/>E.g. Key.D1 or Key.Numpad1 -> "1".
        /// </summary>
        public static string DigitKeyToString(Key k)
            => k switch
            {
                Key.D0 or Key.NumPad0 => "0",
                Key.D1 or Key.NumPad1 => "1",
                Key.D2 or Key.NumPad2 => "2",
                Key.D3 or Key.NumPad3 => "3",
                Key.D4 or Key.NumPad4 => "4",
                Key.D5 or Key.NumPad5 => "5",
                Key.D6 or Key.NumPad6 => "6",
                Key.D7 or Key.NumPad7 => "7",
                Key.D8 or Key.NumPad8 => "8",
                Key.D9 or Key.NumPad9 => "9",
                _                     => "",
            };


        /// <summary>
        /// Converts alpha and digit Key to a string.<br/>E.g. Key.A -> "A"; Key.D1 or Key.Numpad1 -> "1".
        /// </summary>
        public static string AplhaNumericKeyToString(Key k)
        {
            if (k == Key.Space)
                return " ";
            if (k >= Key.A && k <= Key.Z)
                return k.ToString();
            return DigitKeyToString(k);
        }


        /// <summary>
        /// Returns the given string filtered whereby characters above ASCII #127 are converted to approximate equivalents, e.g. 'é' -> 'e'.<br/>
        /// If a German keyboard is being used 'Ä' is converted to "AE", 'ö' to "oe", etc.<br/>
        /// Cyrillic or Greek characters may not be converted.
        /// </summary>
        public static string FilterAscii127(string s)
        {
            if (s is null)
                return s;
            var result = new StringBuilder();
            foreach (var c in s)
                result.Append(FilterAscii127(c));
            return result.ToString();
        }


        /// <summary>
        /// Returns the given char as a string<br/>
        /// Values above ASCII #127 are converted to approximate equivalents, e.g. 'é' -> "e".<br/>
        /// If a German keyboard is being used 'Ä' is converted to "AE", 'ö' to "oe", etc.<br/>
        /// Cyrillic or Greek characters may not be converted.
        /// </summary>
        public static string FilterAscii127(char c) => FilterAscii127Extended(c);


        /// <summary>
        /// Returns the given single-character string filtered whereby characters above ASCII #127 are converted to approximate equivalents, e.g. 'é' -> 'e'.<br/>
        /// Does not convert to multiple-char output, i.e. will not convert German 'Ä' -> "AE", 'ö' to "oe", etc.<br/>
        /// Cyrillic or Greek characters may not be converted.
        /// </summary>
        public static string FilterAscii127SingleChar(string s) => string.IsNullOrEmpty(s) ? s : FilterAscii127SingleChar(s[0]);


        /// <summary>
        /// Returns the given char filtered whereby chars above ASCII #127 are converted to approximate equivalents, e.g. 'é' -> 'e'.<br/>
        /// Does not convert to multiple-char output, i.e. will not convert German 'Ä' -> "AE", 'ö' to "oe", etc.<br/>
        /// Cyrillic or Greek characters may not be converted.
        /// </summary>
        public static char FilterAscii127SingleChar(Key k)
        {
            var keyStr = KeyToString(k);
            return string.IsNullOrEmpty(keyStr) ? '\0' : FilterAscii127SingleChar(keyStr)[0];
        }


        /// <summary>
        /// Returns the given char as a string<br/>
        /// Values above ASCII #127 are converted to approximate equivalents, e.g. 'é' -> "e".<br/>
        /// If a German keyboard is being used 'Ä' is converted to "AE", 'ö' to "oe", etc.<br/>
        /// Cyrillic or Greek characters may not be converted.
        /// </summary>
        private static string FilterAscii127Extended(char c)
        {
            if (c < 127)
                return c.ToString();
            var _deutscheTastatur = InputLanguageManager.Current.CurrentInputLanguage.Name.StartsWith("de");
            return c switch
            {
                'Ä' => _deutscheTastatur ? "AE" : "A",
                'Æ'or'Ǣ'or'Ǽ' => "AE",
                'Ч'or'Ш' => "CH",
                'Ĳ' => "IJ",
                'Ю' => "IO",
                'Я' => "YA",
                'Χ'or'Х' => "KH",
                'Ö' => _deutscheTastatur ? "OE" : "O",
                'Œ' => "OE",
                'Ψ' => "PS",
                'Щ' => "SH",
                'Θ' => "TH",
                'Ц' => "TS",
                'Ü' => _deutscheTastatur ? "UE" : "U",
                'ä' => _deutscheTastatur ? "ae" : "a",
                'æ'or'ǣ'or'ǽ' => "ae",
                'ч'or'ш' => "ch",
                'ĳ' => "ij",
                'ю' => "io",
                'я' => "ya",
                'χ'or'х' => "kh",
                'ö' => _deutscheTastatur ? "oe" : "o",
                'œ' => "oe",
                'ψ' => "ps",
                'щ' => "sh",
                'ß' => "ss",
                'θ' => "th",
                'ц' => "ts",
                'ü' => _deutscheTastatur ? "ue" : "u",
                '€' => "EUR",
                '©' => "(c)",
                '®' => "(R)",
                _ => FilterAscii127SingleChar(c),
            };
        }


        /// <summary>
        /// Returns the given char as a string<br/>
        /// Values above ASCII #127 are converted to approximate equivalents, e.g. 'é' -> "e".<br/>
        /// Does not convert to multiple-char output, i.e. will not convert German 'Ä' -> "AE", 'ö' to "oe", etc.<br/>
        /// Cyrillic or Greek characters may not be converted.
        /// </summary>
        public static string FilterAscii127SingleChar(char c)
        {
            if (c < 127)
                return c.ToString();
            return c switch
            {
                'À'or'Á'or'Â'or'Ã'or'Å'or'Ā'or'Ă'or'Ą'or'Ǎ'or'Ǟ'or'Ǡ'or'Ǻ'or'Ȁ'or'Ȃ'or'Ȧ'or'Ⱥ'or'Α'or'А' => "A",
                'Ɓ'or'Ƃ'or'Ƀ'or'Β'or'Б' => "B",
                'Ç'or'Ć'or'Ĉ'or'Ċ'or'Č'or'Ƈ'or'Ȼ' => "C",
                'Ð'or'Ď'or'Đ'or'Ɖ'or'Ɗ'or'Ƌ'or'Δ'or'Д' => "D",
                'È'or'É'or'Ê'or'Ë'or'Ē'or'Ĕ'or'Ė'or'Ę'or'Ě'or'Ǝ'or'Ɛ'or'Ȅ'or'Ȇ'or'Ȩ'or'Ɇ'or'Ε'or'Е'or'Η'or'Э' => "E",
                'Ƒ'or'Φ'or'Ф' => "F",
                'Ĝ'or'Ğ'or'Ġ'or'Ģ'or'Ɠ'or'Ǥ'or'Ǧ'or'Ǵ'or'Γ'or'Г' => "G",
                'Ĥ'or'Ħ'or'Ȟ' => "H",
                'Ì'or'Í'or'Î'or'Ï'or'Ĩ'or'Ī'or'Ĭ'or'Į'or'İ'or'Ɩ'or'Ɨ'or'Ǐ'or'Ȉ'or'Ȋ'or'Ι'or'И'or'Й' => "I",
                'Ĵ'or'Ɉ'or'Ж' => "J",
                'Ķ'or'Ƙ'or'Ǩ'or'Κ'or'К' => "K",
                'Ĺ'or'Ļ'or'Ľ'or'Ŀ'or'Ł'or'Ƚ'or'Λ'or'Л' => "L",
                'Μ'or'М' => "M",
                'Ñ'or'Ń'or'Ņ'or'Ň'or'Ŋ'or'Ɲ'or'Ǹ'or'Ƞ'or'Ν'or'Н' => "N",
                'Ò'or'Ó'or'Ô'or'Õ'or'Ø'or'Ō'or'Ŏ'or'Ő'or'Ɔ'or'Ơ'or'Ǒ'or'Ǫ'or'Ǭ'or'Ǿ'or'Ȍ'or'Ȏ'or'Ȫ'or'Ȭ'or'Ȯ'or'Ȱ'or'Ο'or'Ω'or'О' => "O",
                'Ƥ'or'Π'or'П' => "P",
                'Ɋ' => "Q",
                'Ŕ'or'Ŗ'or'Ř'or'Ʀ'or'Ȑ'or'Ȓ'or'Ɍ'or'Ρ'or'Р' => "R",
                'Ś'or'Ŝ'or'Ş'or'Š'or'Ș'or'Σ'or'С' => "S",
                'Ţ'or'Ť'or'Ŧ'or'Ƭ'or'Ʈ'or'Ț'or'Ⱦ'or'Τ'or'Т' => "T",
                'Ù'or'Ú'or'Û'or'Ũ'or'Ū'or'Ŭ'or'Ů'or'Ű'or'Ų'or'Ư'or'Ǔ'or'Ǖ'or'Ǘ'or'Ǚ'or'Ǜ'or'Ȕ'or'Ȗ'or'Ʉ'or'Υ'or'У' => "U",
                'Ʋ'or'В' => "V",
                'Ŵ' => "W",
                'Ξ' => "X",
                '¥'or'Ý'or'Ŷ'or'Ÿ'or'Ƴ'or'Ȳ'or'Ɏ' => "Y",
                'Ź'or'Ż'or'Ž'or'Ƶ'or'Ȥ'or'Ζ'or'З' => "Z",
                'à'or'á'or'â'or'ã'or'å'or'ā'or'ă'or'ą'or'ǎ'or'ǟ'or'ǡ'or'ǻ'or'ȁ'or'ȃ'or'ȧ'or'α'or'а' => "a",
                'ƀ'or'ƃ'or'Ƅ'or'ƅ'or'ɓ'or'β'or'б' => "b",
                '¢'or'ç'or'ć'or'ĉ'or'ċ'or'č'or'ć'or'ĉ'or'ċ'or'č'or'ƈ'or'ȼ'or'ɕ' => "c",
                'ď'or'đ'or'ƌ'or'ƍ'or'ȡ'or'ɖ'or'ɗ'or'δ'or'д' => "d",
                'è'or'é'or'ê'or'ë'or'ē'or'ĕ'or'ė'or'ę'or'ě'or'ǝ'or'ȅ'or'ȇ'or'ȩ'or'ɇ'or'ε'or'η'or'е'or'э' => "e",
                'ƒ'or'φ'or'ф' => "f",
                'ĝ'or'ğ'or'ġ'or'ģ'or'ǥ'or'ǧ'or'ǵ'or'ɠ'or'ɡ'or'γ'or'г' => "g",
                'ĥ'or'ħ'or'ȟ'or'ɦ'or'ɧ' => "h",
                'ì'or'í'or'î'or'ï'or'ĩ'or'ī'or'ĭ'or'į'or'ı'or'ƚ'or'ǐ'or'ȉ'or'ȋ'or'ɨ'or'ι'or'и'or'й' => "i",
                'ĵ'or'ǰ'or'ȷ'or'ɉ'or'ɟ'or'ж' => "j",
                'ķ'or'ĸ'or'ƙ'or'ǩ'or'κ'or'к' => "k",
                'ĺ'or'ļ'or'ľ'or'ŀ'or'ł'or'ȴ'or'ɫ'or'ɬ'or'ɭ'or'λ'or'л' => "l",
                'Ɯ'or'ɰ'or'ɱ'or'µ'or'μ'or'м' => "m",
                'ñ'or'ń'or'ņ'or'ň'or'ŉ'or'ŋ'or'ƞ'or'ǹ'or'ȵ'or'ɲ'or'ɳ'or'ν'or'н' => "n",
                'ò'or'ó'or'ô'or'õ'or'ø'or'ō'or'ŏ'or'ő'or'ơ'or'ǒ'or'ǫ'or'ǭ'or'ǿ'or'ȍ'or'ȏ'or'ȫ'or'ȭ'or'ȯ'or'ȱ'or'ɔ'or'ο'or'ω'or'о' => "o",
                'ƥ'or'π'or'п' => "p",
                'ɋ'or'ʠ' => "q",
                'ŕ'or'ŗ'or'ř'or'ȑ'or'ȓ'or'ɍ'or'ɼ'or'ɽ'or'ɾ'or'ρ'or'р' => "r",
                'ś'or'ŝ'or'ş'or'š'or'ſ'or'ș'or'ȿ'or'ʂ'or'ς'or'σ'or'с' => "s",
                'ţ'or'ť'or'ŧ'or'ƫ'or'ƭ'or'ț'or'ȶ'or'ʈ'or'τ'or'т' => "t",
                'ù'or'ú'or'û'or'ũ'or'ū'or'ŭ'or'ů'or'ű'or'ų'or'ư'or'ǔ'or'ǖ'or'ǘ'or'ǚ'or'ǜ'or'ȕ'or'ȗ'or'ʉ'or'υ'or'у' => "u",
                'ʋ'or'в' => "v",
                'ŵ' => "w",
                '×'or'ξ' => "x",
                'ÿ'or'ŷ'or'ý'or'ƴ'or'ȳ'or'ɏ' => "y",
                'ź'or'ż'or'ž'or'ƶ'or'ȥ'or'ɀ'or'ʐ'or'ʑ'or'ζ'or'з' => "z",
                '¡' => "!",
                '¿' => "?",
                '£' => "#",
                '«'or'»' => "\"",
                '´' => "'",
                '÷' => "/",
                '¬' => "-",
                '¦' => "|",
                '·'or'¸' => ".",
                _ => "*"
            };
        }


        public static Size MeasureText(string text, FontFamily fontFamily, double fontSize, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch)
        {
            var formattedText = new FormattedText(text,
                                                  CultureInfo.CurrentCulture,
                                                  FlowDirection.LeftToRight,
                                                  new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                                                  fontSize,
                                                  Brushes.Black,
                                                  new NumberSubstitution(),
                                                  1);

            return new Size(formattedText.Width, formattedText.Height);
        }


        public static string NumberListToString(List<int> numberList)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < numberList.Count; i++)
            {
                if (result.Length > 0)
                {
                    if (i == numberList.Count - 1)
                        result.Append(" & " + numberList[i].ToString());
                    else
                        result.Append(", " + numberList[i].ToString());
                }
                else
                    result.Append(numberList[i]);
            }
            return result.ToString();
        }


        /// <summary>
        /// Attempts to retrieve the int value from a formatted string, i.e. one built using string.Format and containing a '{0}' placeholder.
        /// </summary>
        /// <param name="text">The text string to analyse</param>
        /// <param name="formatString">The string template containing the '{0}' placeholder</param>
        /// <returns>Returns the integer value if found, otherwise null.</returns>
        public static int? ExtractIntFromFormattedText(string text, string formatString)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            var idx1 = formatString.IndexOf("{0}");
            if (idx1 > -1)
            {
                var split = text.Substring(idx1).Split(new char[] { ' ',',','.','-',':' });
                if (int.TryParse(split[0], out var result))
                    return result;
            }

            return null;
        }


        /// <summary>
        /// Attempts to retrieve the substring value from a formatted string, i.e. one built using string.Format and containing a '{0}' placeholder.
        /// </summary>
        /// <param name="text">The text string to analyse</param>
        /// <param name="formatString">The string template containing the '{0}' placeholder</param>
        /// <returns>Returns the integer value if found, otherwise null.</returns>
        public static string ExtractStringFromFormattedText(string text, string formatString)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            var idx1 = formatString.IndexOf("{0}");
            if (idx1 > -1)
                return text.Substring(idx1).Split(new char[] { ' ',',','.','-',':' })[0];

            return null;
        }


        /// <summary>
        /// Returns the given int value as a string, left-zero-padded to the specified length.
        /// </summary>
        /// <returns></returns>
        public static string IntToZeroPaddedString(int value, int length) => string.Format("{0:" + new string('0', length) + "}", value);
    }
}
