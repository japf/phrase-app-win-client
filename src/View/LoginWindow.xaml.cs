using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using VercorsStudio.PhraseApp.Client.Window.ViewModel;
using VercorsStudio.PhraseApp.Client.Window.ViewModel.Message;

namespace VercorsStudio.PhraseApp.Client.Window.View
{
    public partial class LoginWindow : MetroWindow
    {
        public LoginWindow()
        {
            InitializeComponent();

            this.DataContext = SimpleIoc.Default.GetInstance<LoginWindowViewModel>();
            
            Messenger.Default.Register(this, (CloseLoginDialogMessage m) => this.Close());
        }
    }
}
