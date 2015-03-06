using System.Windows;

namespace VercorsStudio.PhraseApp.Client.Window
{
    public partial class App : Application
    {
        private Bootstraper boostraper;

        private void OnStartup(object sender, StartupEventArgs e)
        {
            this.boostraper = new Bootstraper();
            this.boostraper.ConfigureAsync();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            this.boostraper.Exit();
        }
    }
}
