#region Usings declarations

using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

#endregion

public class AnsiConsoleEmulator : UserControl {

    #region Statics members declarations

    private static readonly Color[] AnsiPalette = {
        Color.Black, Color.Red, Color.Green, Color.Yellow, Color.Blue, Color.Magenta, Color.Cyan, Color.White,
        Color.DarkGray, Color.LightCoral, Color.LightGreen, Color.LightYellow, Color.LightBlue, Color.Plum, Color.LightCyan, Color.WhiteSmoke
    };

    private static Color XTerm256ToRGB(int color) {
        if (color < 16) {
            return AnsiPalette[color]; // Couleurs standard
        }
        if (color <= 231) // Cube de couleurs (216 couleurs)
        {
            int c = color - 16;
            int r = c      / 36 * 51;
            int g = c % 36 / 6  * 51;
            int b = c      % 6  * 51;

            return Color.FromArgb(r, g, b);
        }
        if (color <= 255) // Échelle de gris (24 nuances)
        {
            int gray = 8 + (color - 232) * 10;

            return Color.FromArgb(gray, gray, gray);
        }

        return Color.White;
    }

    #endregion

    #region Fields declarations

    private readonly RichTextBox richTextBox;

    #endregion

    #region Constructors declarations

    public AnsiConsoleEmulator() {
        richTextBox = new RichTextBox {
            Dock        = DockStyle.Fill,
            ReadOnly    = true,
            BackColor   = Color.Black,
            ForeColor   = Color.White,
            BorderStyle = BorderStyle.None,
            Multiline   = true,
            ScrollBars  = RichTextBoxScrollBars.Vertical,
            Font        = new Font("Consolas", 12)
        };
        Controls.Add(richTextBox);
    }

    #endregion

    public void Render(string ansiText) {
        Regex           regex     = new(@"\033\[(\d+(;\d+)*)[mHJK]");
        MatchCollection matches   = regex.Matches(ansiText);
        int             lastIndex = 0;

        Color foreColor = Color.White;
        Color backColor = Color.Black;
        bool  bold      = false, underline = false, inverse = false;

        foreach (Match match in matches) {
            int index = match.Index;
            if (index > lastIndex) {
                AppendFormattedText(ansiText.Substring(lastIndex, index - lastIndex), foreColor, backColor, bold, underline, inverse);
            }

            string[] codes = match.Value.Substring(2, match.Value.Length - 3).Split(';');
            int      i     = 0;

            while (i < codes.Length) {
                if (int.TryParse(codes[i], out int codeValue)) {
                    if (match.Value.EndsWith("m")) // Formatage et couleurs
                    {
                        switch (codeValue) {
                            case 0: // Reset
                                foreColor = Color.White;
                                backColor = Color.Black;
                                bold      = underline = inverse = false;

                                break;
                            case 1: bold      = true; break;
                            case 4: underline = true; break;
                            case 7: inverse   = !inverse; break;
                            case >= 30 and <= 37: // Couleur texte
                                foreColor = AnsiPalette[codeValue - 30];

                                break;
                            case >= 40 and <= 47: // Couleur fond
                                backColor = AnsiPalette[codeValue - 40];

                                break;
                            case >= 90 and <= 97: // Couleur texte (Bright)
                                foreColor = AnsiPalette[codeValue - 90 + 8];

                                break;
                            case >= 100 and <= 107: // Couleur fond (Bright)
                                backColor = AnsiPalette[codeValue - 100 + 8];

                                break;
                            case 38: // Texte couleur 256 ou RGB
                            case 48: // Fond couleur 256 ou RGB
                                if (i + 1 < codes.Length && int.TryParse(codes[i + 1], out int colorMode)) {
                                    if (colorMode == 5 && i + 2 < codes.Length && int.TryParse(codes[i + 2], out int color256)) {
                                        if (codeValue == 38) {
                                            foreColor = XTerm256ToRGB(color256);
                                        } else {
                                            backColor = XTerm256ToRGB(color256);
                                        }
                                        i += 2;
                                    } else if (colorMode == 2 && i + 4 < codes.Length) {
                                        if (int.TryParse(codes[i + 2], out int r) &&
                                            int.TryParse(codes[i + 3], out int g) &&
                                            int.TryParse(codes[i + 4], out int b)) {
                                            if (codeValue == 38) {
                                                foreColor = Color.FromArgb(r, g, b);
                                            } else {
                                                backColor = Color.FromArgb(r, g, b);
                                            }
                                        }
                                        i += 4;
                                    }
                                }

                                break;
                        }
                    }
                }
                i++;
            }

            lastIndex = index + match.Length;
        }

        if (lastIndex < ansiText.Length) {
            AppendFormattedText(ansiText.Substring(lastIndex), foreColor, backColor, bold, underline, inverse);
        }

        richTextBox.SelectionStart = richTextBox.TextLength;
        richTextBox.ScrollToCaret(); // Scroll automatique
    }

    private void AppendFormattedText(string text, Color foreColor, Color backColor, bool bold, bool underline, bool inverse) {
        richTextBox.SelectionStart  = richTextBox.TextLength;
        richTextBox.SelectionLength = 0;

        if (inverse) {
            (foreColor, backColor) = (backColor, foreColor);
        }

        richTextBox.SelectionColor     = foreColor;
        richTextBox.SelectionBackColor = backColor;
        richTextBox.SelectionFont      = new Font(richTextBox.Font, (bold ? FontStyle.Bold : FontStyle.Regular) | (underline ? FontStyle.Underline : FontStyle.Regular));

        richTextBox.AppendText(text);
        richTextBox.SelectionColor = richTextBox.ForeColor;
    }

}