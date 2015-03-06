using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using VercorsStudio.PhraseApp.Client.Window.Api;
using VercorsStudio.PhraseApp.Client.Window.View;
using MessageBox = System.Windows.MessageBox;

namespace VercorsStudio.PhraseApp.Client.Window.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IPhraseAppService service;

        private readonly RelayCommand deleteAllKeys;
        private readonly RelayCommand downloadAll;
        private readonly RelayCommand uploadAll;
        private readonly RelayCommand<string> uploadLocale;
        private readonly RelayCommand<string> downloadLocale;

        private readonly RelayCommand chooseWorkingFolder;
        private readonly RelayCommand openLoginDialog;

        private ObservableCollection<string> files;
        private string workingFolder;

        public IPhraseAppService Service
        {
            get { return this.service; }
        }

        public ObservableCollection<string> Files
        {
            get { return this.files; }
        }

        public ICommand OpenLoginDialog
        {
            get { return this.openLoginDialog; }
        }

        public ICommand DeleteAllKeys
        {
            get { return this.deleteAllKeys; }
        }

        public ICommand DownloadAll
        {
            get { return this.downloadAll; }
        }

        public ICommand UploadAll
        {
            get { return this.uploadAll; }
        }

        public ICommand UploadLocale
        {
            get { return this.uploadLocale; }
        }

        public ICommand DownloadLocale
        {
            get { return this.downloadLocale; }
        }

        public ICommand ChooseWorkingFolder
        {
            get { return this.chooseWorkingFolder; }
        }

        public string WorkingFolder
        {
            get { return this.workingFolder; }
            private set
            {
                if (this.workingFolder != value)
                {
                    this.workingFolder = value;
                    this.RaisePropertyChanged(() => this.WorkingFolder);

                    this.UpdateWorkingFolder();
                }
            }
        }

        public MainWindowViewModel(IPhraseAppService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            this.service = service;
            this.files = new ObservableCollection<string>();

            this.deleteAllKeys = new RelayCommand(this.DeleteAllKeysExecute, this.CanExecutePhraseAppCommand);
            this.chooseWorkingFolder = new RelayCommand(this.ChooseWorkingFolderExecute, this.CanExecutePhraseAppCommand);

            this.downloadAll = new RelayCommand(this.DownloadAllExecute, this.CanExecutePhraseAppCommand);
            this.uploadAll = new RelayCommand(this.UploadAllExecute, this.CanExecutePhraseAppCommand);

            this.uploadLocale = new RelayCommand<string>(this.UploadLocaleExecute);
            this.downloadLocale = new RelayCommand<string>(this.DownloadLocaleExecute);

            this.openLoginDialog = new RelayCommand(this.OpenLoginDialogExecute);

            if (!string.IsNullOrEmpty(Settings.Default.WorkingFolder))
                this.workingFolder = Settings.Default.WorkingFolder;

            this.service.LoggedIn += this.OnLoggedIn;
            this.service.LoggedOut += this.OnLoggedOut;
        }

        private bool CanExecutePhraseAppCommand()
        {
            return this.service.CurrentEmail != null;
        }

        private void OpenLoginDialogExecute()
        {
            var dialog = new LoginWindow();
            dialog.ShowDialog();
        }

        private void UpdateWorkingFolder()
        {
            Settings.Default.WorkingFolder = this.workingFolder;
            this.OnLoggedIn(this, EventArgs.Empty);
        }

        private void ChooseWorkingFolderExecute()
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.SelectedPath) && Directory.Exists(dialog.SelectedPath))
                this.WorkingFolder = dialog.SelectedPath;
        }

        private void OnLoggedIn(object sender, EventArgs eventArgs)
        {
            this.service.WorkingFolder = this.workingFolder;
            this.files.Clear();
            foreach (string file in this.service.GetResourceFiles().Select(PhraseAppService.GetLocale).Distinct().OrderBy(l => l))
                this.files.Add(file);

            this.deleteAllKeys.RaiseCanExecuteChanged();
            this.downloadAll.RaiseCanExecuteChanged();
            this.uploadAll.RaiseCanExecuteChanged();
            this.uploadLocale.RaiseCanExecuteChanged();
            this.downloadLocale.RaiseCanExecuteChanged();
            this.chooseWorkingFolder.RaiseCanExecuteChanged();
            this.openLoginDialog.RaiseCanExecuteChanged();
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            this.files.Clear();
        }

        private async void DeleteAllKeysExecute()
        {
            var keys = await this.service.GetAllKeysAsync();
            var result = MessageBox.Show("Confirmation", string.Format("About to delete {0} key(s), continue ?", keys.Count), MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
                await this.service.DeleteKeys(keys.Select(k => k.Id).ToList());
        }

        private async void DownloadAllExecute()
        {
            foreach (var resource in this.service.GetResourceFiles())
                await this.service.Download(resource);
        }

        private async void UploadAllExecute()
        {
            foreach (var resource in this.service.GetResourceFiles())
                await this.service.Upload(resource);
        }

        private async void UploadLocaleExecute(string locale)
        {
            if (!string.IsNullOrWhiteSpace(locale))
            {
                foreach (var resource in this.service.GetResourceFiles(locale))
                    await this.service.Upload(resource);
            }
        }

        private async void DownloadLocaleExecute(string locale)
        {
            foreach (var resource in this.service.GetResourceFiles(locale))
                await this.service.Download(resource);
        }
    }
}