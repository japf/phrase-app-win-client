using System;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using VercorsStudio.PhraseApp.Client.Window.Api;
using VercorsStudio.PhraseApp.Client.Window.ViewModel.Message;

namespace VercorsStudio.PhraseApp.Client.Window.ViewModel
{
    public class LoginWindowViewModel : ViewModelBase
    {
        private readonly RelayCommand loginCommand;
        private readonly RelayCommand logoutCommand;
        private readonly RelayCommand cancelCommand;

        private readonly IPhraseAppService service;
        
        private string email;
        private string password;
        private string projectToken;

        public string Email
        {
            get { return this.email; }
            set
            {
                if (this.email != value)
                {
                    this.email = value;
                    this.RaisePropertyChanged(() => this.Email);
                    this.loginCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string Password
        {
            get { return this.password; }
            set
            {
                if (this.password != value)
                {
                    this.password = value;
                    this.RaisePropertyChanged(() => this.Password);
                    this.loginCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string ProjectToken
        {
            get { return this.projectToken; }
            set
            {
                if (this.projectToken != value)
                {
                    this.projectToken = value;
                    this.RaisePropertyChanged(() => this.ProjectToken);
                    this.loginCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool CanLogout
        {
            get { return this.service.CurrentEmail != null; }
        }

        public bool CanLogin
        {
            get { return !this.CanLogout; }
        }

        public ICommand LoginCommand
        {
            get { return this.loginCommand; }
        }

        public ICommand LogoutCommand
        {
            get { return this.logoutCommand; }
        }

        public ICommand CancelCommand
        {
            get { return this.cancelCommand; }
        }

        public LoginWindowViewModel(IPhraseAppService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            this.service = service;

            this.loginCommand = new RelayCommand(this.LoginExecute, this.LoginCanExecute);
            this.logoutCommand = new RelayCommand(this.LogoutExecute);
            this.cancelCommand = new RelayCommand(this.CancelExecute);

            if (!string.IsNullOrEmpty(Settings.Default.Email))
                this.Email = Settings.Default.Email;
            if (!string.IsNullOrEmpty(Settings.Default.Password))
                this.Password = Settings.Default.Password;
            if (!string.IsNullOrEmpty(Settings.Default.ProjectToken))
                this.ProjectToken = Settings.Default.ProjectToken;
        }

        private void CancelExecute()
        {
            this.MessengerInstance.Send(new CloseLoginDialogMessage());
        }

        private void LogoutExecute()
        {
            this.service.Logout();

            this.Email = null;
            this.ProjectToken = null;
            this.Password = null;

            this.MessengerInstance.Send(new CloseLoginDialogMessage());
        }

        private bool LoginCanExecute()
        {
            return !string.IsNullOrEmpty(this.email) && !string.IsNullOrEmpty(this.password) && !string.IsNullOrEmpty(this.projectToken);
        }

        private async void LoginExecute()
        {
            PhraseAppAuthStatus result = await this.service.Login(this.projectToken, this.email, this.password);
            if (result.Success)
            {
                this.UpdateSettings();
                this.MessengerInstance.Send(new CloseLoginDialogMessage());
            }
            else
            {
                await ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync("Error", "Unable to login, please check credentials");
            }
        }

        private void UpdateSettings()
        {
            Settings.Default.Email = this.Email;
            Settings.Default.Password = this.Password;
            Settings.Default.ProjectToken = this.ProjectToken;
        }
    }
}
