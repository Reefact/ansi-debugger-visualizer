#region Usings declarations

using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

#endregion

namespace AnsiDebuggerVisualizer {

    public class AnsiToRichTextBox {

        #region Statics members declarations

        // Palette des couleurs ANSI standard et vives
        private static readonly Color[] AnsiPalette = {
            Color.Black, Color.Red, Color.Green, Color.Yellow, Color.Blue, Color.Magenta, Color.Cyan, Color.White,                                // Standard
            Color.DarkGray, Color.LightCoral, Color.LightGreen, Color.LightYellow, Color.LightBlue, Color.Plum, Color.LightCyan, Color.WhiteSmoke // Bright
        };

        public static void ParseAnsiToRichTextBox(string ansiText, RichTextBox richTextBox) {
            Regex           regex     = new Regex(@"\033\[(\d+(;\d+)*)m");
            MatchCollection matches   = regex.Matches(ansiText);
            int             lastIndex = 0;

            // Propriétés actuelles
            Color foreColor = Color.White;
            Color backColor = Color.Black;
            bool  bold      = false, underline = false, inverse = false;

            richTextBox.Clear();

            foreach (Match match in matches) {
                int index = match.Index;
                if (index > lastIndex) {
                    AppendFormattedText(richTextBox, ansiText.Substring(lastIndex, index - lastIndex), foreColor, backColor, bold, underline, inverse);
                }

                // Récupérer les codes ANSI (ex: 38;2;255;0;0 pour RGB rouge)
                string[] codes = match.Value.Substring(2, match.Value.Length - 3).Split(';');
                int      i     = 0;

                while (i < codes.Length) {
                    if (int.TryParse(codes[i], out int codeValue)) {
                        if (codeValue == 0) // Reset
                        {
                            foreColor = Color.White;
                            backColor = Color.Black;
                            bold      = underline = inverse = false;
                        } else if (codeValue == 1) {
                            bold = true;
                        } else if (codeValue == 4) {
                            underline = true;
                        } else if (codeValue == 7) {
                            inverse = true;
                        } else if (codeValue >= 30 && codeValue <= 37) // Couleurs texte standard
                        {
                            foreColor = AnsiPalette[codeValue - 30];
                        } else if (codeValue >= 90 && codeValue <= 97) // Couleurs texte vives
                        {
                            foreColor = AnsiPalette[codeValue - 90 + 8];
                        } else if (codeValue >= 40 && codeValue <= 47) // Couleurs de fond standard
                        {
                            backColor = AnsiPalette[codeValue - 40];
                        } else if (codeValue >= 100 && codeValue <= 107) // Couleurs de fond vives
                        {
                            backColor = AnsiPalette[codeValue - 100 + 8];
                        } else if (codeValue == 38 || codeValue == 48) // 256 couleurs ou RGB
                        {
                            bool isForeground = codeValue == 38;
                            if (i + 1 < codes.Length && int.TryParse(codes[i + 1], out int colorMode)) {
                                if (colorMode == 5 && i + 2 < codes.Length && int.TryParse(codes[i + 2], out int color256)) {
                                    if (color256 >= 0 && color256 <= 255) {
                                        if (isForeground) {
                                            foreColor = XTerm256ToRGB(color256);
                                        } else {
                                            backColor = XTerm256ToRGB(color256);
                                        }
                                    }
                                    i += 2;
                                } else if (colorMode == 2 && i + 4 < codes.Length) // TrueColor (RGB)
                                {
                                    if (int.TryParse(codes[i + 2], out int r) &&
                                        int.TryParse(codes[i + 3], out int g) &&
                                        int.TryParse(codes[i + 4], out int b)) {
                                        if (isForeground) {
                                            foreColor = Color.FromArgb(r, g, b);
                                        } else {
                                            backColor = Color.FromArgb(r, g, b);
                                        }
                                    }
                                    i += 4;
                                }
                            }
                        }
                    }
                    i++;
                }

                lastIndex = index + match.Length;
            }

            if (lastIndex < ansiText.Length) {
                AppendFormattedText(richTextBox, ansiText.Substring(lastIndex), foreColor, backColor, bold, underline, inverse);
            }
        }

        private static void AppendFormattedText(RichTextBox richTextBox, string text, Color foreColor, Color backColor, bool bold, bool underline, bool inverse) {
            richTextBox.SelectionStart  = richTextBox.TextLength;
            richTextBox.SelectionLength = 0;

            if (inverse) {
                Color temp = foreColor;
                foreColor = backColor;
                backColor = temp;
            }

            richTextBox.SelectionColor     = foreColor;
            richTextBox.SelectionBackColor = backColor;
            richTextBox.SelectionFont      = new Font(richTextBox.Font, (bold ? FontStyle.Bold : FontStyle.Regular) | (underline ? FontStyle.Underline : FontStyle.Regular));

            richTextBox.AppendText(text);
            richTextBox.SelectionColor = richTextBox.ForeColor;
        }

        private static Color XTerm256ToRGB(int color) {
            if (color < 16) {
                return AnsiPalette[color]; // Couleurs standard
            }
            if (color >= 16 && color <= 231) // Cube de couleurs (216 couleurs)
            {
                int c = color - 16;
                int r = c      / 36 * 51;
                int g = c % 36 / 6  * 51;
                int b = c      % 6  * 51;

                return Color.FromArgb(r, g, b);
            }
            if (color >= 232 && color <= 255) // Échelle de gris (24 nuances)
            {
                int gray = 8 + (color - 232) * 10;

                return Color.FromArgb(gray, gray, gray);
            }

            return Color.White;
        }

        #endregion

    }

}