#if DEBUG_OUTPUT_PANE
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;

namespace Tools.Logger
{
    public class CustomOutputPaneTraceListener : TraceListener
    {
        private static IVsOutputWindowPane? _pane;
        private static readonly Guid _paneGuid = new Guid("21E20E8E-AD58-4C34-9C35-17932A31877A");
        private static bool _initialized;

        public override void Write(string? message)
        {
            InitializePaneOnce();
            _pane?.OutputString(message ?? string.Empty);
        }

        public override void WriteLine(string? message)
        {
            InitializePaneOnce();
            _pane?.OutputString($"{message}{Environment.NewLine}");
        }

        private static void InitializePaneOnce()
        {
            if (_initialized) return;

            lock (_paneGuid)
            {
                if (_initialized) return;

                try
                {
                    var shell = GetVisualStudioShell();
                    if (shell != null)
                    {
                        CreatePane(shell);
                    }
                }
                catch { /* Ignora se non in VS */ }

                _initialized = true;
            }
        }

        private static IVsOutputWindow? GetVisualStudioShell()
        {
            try
            {
                return Marshal.GetActiveObject("VisualStudio.DTE.17.0") as IVsOutputWindow;
            }
            catch
            {
                return null;
            }
        }

        private static void CreatePane(IVsOutputWindow shell)
        {
            try
            {
                shell.CreatePane(ref _paneGuid, "MyApp Logs", 1, 1);
                shell.GetPane(ref _paneGuid, out _pane);
            }
            catch { /* Pane già esistente */ }
        }
    }
}
#endif