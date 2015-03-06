using System;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls;
using VercorsStudio.PhraseApp.Client.Window.Api;
using VercorsStudio.PhraseApp.Client.Window.Tools;
using VercorsStudio.PhraseApp.Client.Window.ViewModel;

namespace VercorsStudio.PhraseApp.Client.Window.View
{
    public partial class MainWindow : MetroWindow 
    {
        private readonly IPhraseAppService phraseAppService;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = SimpleIoc.Default.GetInstance<MainWindowViewModel>();

            this.phraseAppService = SimpleIoc.Default.GetInstance<IPhraseAppService>();
            this.phraseAppService.LoggedIn += (s, e) => this.btnEmail.Content = phraseAppService.CurrentEmail;
            this.phraseAppService.LoggedOut += (s, e) => this.btnEmail.Content = "login";
            this.phraseAppService.LogAdded += this.OnLogAdded;
        }

        private void OnLogAdded(object sender, EventArgs<string> e)
        {
            this.richRichTextBox.AppendText(string.Format("[{0}] {1}{2}", DateTime.Now.ToString("T"), e.Item, Environment.NewLine));
        }
    }
}
