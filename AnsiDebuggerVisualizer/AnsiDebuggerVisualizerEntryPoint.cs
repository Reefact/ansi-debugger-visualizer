#region Usings declarations

using System.Diagnostics;

using AnsiDebuggerVisualizer;

using Microsoft.VisualStudio.DebuggerVisualizers;

#endregion

[assembly: DebuggerVisualizer(
    typeof(AnsiDebuggerVisualizerEntryPoint),
    typeof(VisualizerObjectSource),
    Target = typeof(string),
    Description = "ANSI Visualizer")]

namespace AnsiDebuggerVisualizer {

    public class AnsiDebuggerVisualizerEntryPoint : DialogDebuggerVisualizer {

        #region Constructors declarations

        public AnsiDebuggerVisualizerEntryPoint() : base(FormatterPolicy.Json) { }

        #endregion

        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider) {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (objectProvider == null || windowService == null) { return; }

            string ansiText = ((IVisualizerObjectProvider3)objectProvider).GetObject<string>();

            using (AnsiDebuggerVisualizerForm form = new AnsiDebuggerVisualizerForm(ansiText)) {
                windowService.ShowDialog(form);
            }
        }

    }

}