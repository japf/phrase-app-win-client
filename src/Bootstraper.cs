using System.Threading.Tasks;
using GalaSoft.MvvmLight.Ioc;
using VercorsStudio.PhraseApp.Client.Window.Api;
using VercorsStudio.PhraseApp.Client.Window.View;
using VercorsStudio.PhraseApp.Client.Window.ViewModel;

namespace VercorsStudio.PhraseApp.Client.Window
{
    public class Bootstraper
    {
        public async Task ConfigureAsync()
        {
            SimpleIoc.Default.Register<IPhraseAppService, PhraseAppService>();

            SimpleIoc.Default.Register<MainWindowViewModel>();
            SimpleIoc.Default.Register<LoginWindowViewModel>();

            var window = new MainWindow();
            window.Show();

            string email = Settings.Default.Email;
            string password = Settings.Default.Password;
            string projectAuthToken = Settings.Default.ProjectToken;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(projectAuthToken))
            {
                ShowLoginDialog();
            }
            else
            {
                var service = SimpleIoc.Default.GetInstance<IPhraseAppService>();
                var result = await service.Login(projectAuthToken, email, password);
                if (!result.Success)
                    ShowLoginDialog();
            }
        }

        private static void ShowLoginDialog()
        {
            var loginWindow = new LoginWindow();
            loginWindow.ShowDialog();
        }

        public void Exit()
        {
            Settings.Default.Save();
        }
    }
}
