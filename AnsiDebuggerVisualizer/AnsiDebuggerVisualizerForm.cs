#region Usings declarations

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace AnsiDebuggerVisualizer {

    public class AnsiDebuggerVisualizerForm : Form {

        #region Constructors declarations

        public AnsiDebuggerVisualizerForm(string ansiText) {
            ComponentResourceManager resources = new(typeof(AnsiDebuggerVisualizerForm));
            Icon          = (Icon)resources.GetObject("$this.Icon");
            ClientSize    = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Name          = "AnsiDebuggerVisualizerForm";
            Text          = "ANSI Visualizer";

            AnsiConsoleEmulator console = new() { Dock = DockStyle.Fill };
            Controls.Add(console);
            console.Render(ansiText);
        }

        #endregion

    }

}