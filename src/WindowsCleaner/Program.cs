using System;
using System.Windows.Forms;

namespace WindowsCleaner
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                // Gestionnaire d'exceptions global
                Application.ThreadException += (sender, e) =>
                {
                    Logger.Log(LogLevel.Error, LanguageManager.Get("error_unhandled", e.Exception.Message));
                    Logger.Log(LogLevel.Error, LanguageManager.Get("error_stack_trace", e.Exception.StackTrace));
                    MessageBox.Show(
                        $"Une erreur critique s'est produite:\n\n{e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}",
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                };
                
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    var ex = e.ExceptionObject as Exception;
                    if (ex != null)
                    {
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_unhandled", ex.Message));
                        Logger.Log(LogLevel.Error, LanguageManager.Get("error_stack_trace", ex.StackTrace));
                    }
                };
                
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText(
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"),
                    $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}"
                );
                MessageBox.Show(
                    $"Erreur fatale au démarrage:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Erreur Fatale",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
