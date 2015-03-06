using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using VercorsStudio.PhraseApp.Client.Window.Tools;

namespace VercorsStudio.PhraseApp.Client.Window.Api
{
    public interface IPhraseAppService : INotifyPropertyChanged
    {
        event EventHandler<EventArgs<string>> LogAdded;
        event EventHandler LoggedIn;
        event EventHandler LoggedOut;

        string CurrentEmail { get; }
        string WorkingFolder { get; set; }

        Task<PhraseAppAuthStatus> Login(string projectAuthToken, string email, string password);
        void Logout();

        Task<List<PhraseAppKey>> GetAllKeysAsync();
        
        Task<bool> DeleteKey(string id);
        Task<bool> DeleteKeys(List<int> ids);
        
        Task Upload(string path);
        Task Download(string path);

        List<string> GetResourceFiles(string locale = null);
    }
}