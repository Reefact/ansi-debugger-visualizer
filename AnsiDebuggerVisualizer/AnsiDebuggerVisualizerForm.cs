#region Usings declarations

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace AnsiDebuggerVisualizer {

    public class AnsiDebuggerVisualizerForm : Form {

        #region Constructors declarations

        public AnsiDebuggerVisualizerForm(string ansiText) {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(AnsiDebuggerVisualizerForm));
            Icon          = (Icon)resources.GetObject("$this.Icon");
            ClientSize    = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Name          = "AnsiDebuggerVisualizerForm";
            Text          = "ANSI Visualizer";
            RichTextBox richTextBox = new RichTextBox {
                Dock      = DockStyle.Fill,
                ReadOnly  = true,
                Font      = new Font("Consolas", 10),
                BackColor = Color.Black,
                ForeColor = Color.White
            };
            Controls.Add(richTextBox);

            AnsiToRichTextBox.ParseAnsiToRichTextBox(ansiText, richTextBox);
        }

        #endregion

    }

}