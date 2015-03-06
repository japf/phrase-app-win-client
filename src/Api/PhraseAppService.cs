using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using VercorsStudio.PhraseApp.Client.Window.Tools;

namespace VercorsStudio.PhraseApp.Client.Window.Api
{
    public class PhraseAppService : ViewModelBase, IPhraseAppService
    {
        // docs at http://docs.phraseapp.com/api/v1/

        private const string baseUri = "https://phraseapp.com/api/v1/";
        private const int maxDeleteKey = 50;
        private const string defaultLocale = "en";
        private const string localeSeparator = "-";
        private const string FormatResX = "resx_windowsphone";
        private const string FormatResW = "windows8_resource";

        private string currentEmail;
        private string authToken;
        private string projectAuthToken;

        public event EventHandler<EventArgs<string>> LogAdded;
        public event EventHandler LoggedIn;
        public event EventHandler LoggedOut;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CurrentEmail
        {
            get { return this.currentEmail; }
            private set
            {
                if (this.currentEmail != value)
                {
                    this.currentEmail = value;
                    this.RaisePropertyChanged(() => this.CurrentEmail);
                }
            }
        }

        public string WorkingFolder { get; set; }

        public async Task<PhraseAppAuthStatus> Login(string projectAuthToken, string email, string password)
        {
            this.Log("Login " + email);

            string json = await this.PostAsync(
                this.FormatUri("sessions"),
                new Dictionary<string, string>
                {
                    { "email", email },
                    { "password", password },
                });

            var status = JsonConvert.DeserializeObject<PhraseAppAuthStatus>(json);

            if (status.Success && !string.IsNullOrWhiteSpace(status.AuthToken))
            {
                this.CurrentEmail = email;
                this.authToken = status.AuthToken;
                this.projectAuthToken = projectAuthToken;

                this.Log("Login successfull");

                if (this.LoggedIn != null)
                    this.LoggedIn(this, EventArgs.Empty);
            }
            else
            {
                this.Log("Login failed");
            }

            return status;
        }

        public void Logout()
        {
            this.authToken = null;
            this.CurrentEmail = null;

            if (this.LoggedOut != null)
                this.LoggedOut(this, EventArgs.Empty);

            this.Log("Logout successfull");

        }

        private void Log(string message)
        {
            if (this.LogAdded != null)
                this.LogAdded(this, new EventArgs<string>(message));
        }

        public async Task<List<PhraseAppKey>> GetAllKeysAsync()
        {
            string json = await this.GetAsync(this.FormatUriWithProjectAuth("translation_keys"));
            var keys = JsonConvert.DeserializeObject<List<PhraseAppKey>>(json);

            return keys;
        }

        public async Task<bool> DeleteKey(string id)
        {
            this.EnsureAuthToken();

            string json = await this.DeleteAsync(this.FormatUriWithUserAuth(string.Format("translation_keys/{0}", id)));
            return json != null && json.Contains("true");
        }

        public async Task<bool> DeleteKeys(List<int> ids)
        {
            this.EnsureAuthToken();

            int iterations = ids.Count/50 + 1;
            for (int i = 0; i < iterations; i++)
            {
                int startIndex = i * maxDeleteKey;
                var subIds = ids.Skip(startIndex).Take(maxDeleteKey).ToList();
                this.Log("Deleting keys (" + startIndex + "," + (startIndex + subIds.Count) + ")");

                var uri = this.FormatUriWithUserAuth("translation_keys/destroy_multiple");
                for (int j = 0; j < subIds.Count; j++)
                {
                    uri += "&ids[]=" + subIds[j];
                }

                string json = await this.DeleteAsync(uri);

                if (json == null || !json.Contains("true"))
                    return false;
            }

            return true;
        }

        public async Task Upload(string path)
        {
            this.EnsureAuthToken();

            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            string filename = Path.GetFileName(path);
            string locale = GetLocale(path);
            string format = GetFormat(filename);
            string tag = GetTag(path);

            this.Log(string.Format("Uploading file: {0} locale: {1} tag: {2}...", filename, locale, tag));

            string json = await this.PostAsync(
                this.FormatUriWithProjectAuth("file_imports"),
                new Dictionary<string, string>
                {
                    { "file_import[filename]", filename },
                    { "file_import[format]", format },
                    { "file_import[tag_names]", tag },
                    { "file_import[update_translations]", "1" },
                    { "file_import[skip_upload_tags]", "1" },
                    { "file_import[file_content]", File.ReadAllText(path) },
                    { "file_import[locale_code]", locale },
                });

            if (!json.Contains("true"))
                this.Log(string.Format(" Error ({0})", json));
        }

        public async Task Download(string path)
        {
            this.EnsureAuthToken();

            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            string filename = Path.GetFileName(path);
            string locale = GetLocale(path);
            string tag = GetTag(path);

            this.Log(string.Format("Downloading file: {0} locale: {1}...", filename, locale));

            string parameter = string.Format("&locale={0}&format={1}&tag={2}", locale, GetFormat(path), tag);
            string fileContent = await this.GetAsync(this.FormatUriWithProjectAuth("translations/download") + parameter);
            if (!string.IsNullOrEmpty(fileContent))
            {
                double oldSize = File.ReadAllText(path).Length;
                double newSize = fileContent.Length;
                int change = (int) (Math.Abs(1 - (oldSize/newSize))*100);
                this.Log(string.Format(" OK ! (change: {0}% oldsize: {1} newsize: {2})", change, oldSize, newSize));

                File.WriteAllText(path, fileContent);
            }
            else
            {
                this.Log("Error");
            }
        }

        private static string GetTag(string path)
        {
            string filename = Path.GetFileNameWithoutExtension(path);
            if (!path.Contains(localeSeparator))
            {
                return filename;
            }
            else
            {
                return filename.Split('.')[0];
            }
        }

        private static string GetFormat(string path)
        {
            string format = path.EndsWith(FormatResX) ? FormatResX : FormatResW;
            return format;
        }

        private void EnsureAuthToken()
        {
            if (string.IsNullOrWhiteSpace(this.authToken)) 
                throw new NotSupportedException("Sign in required");
        }

        private async Task<string> DeleteAsync(string uri)
        {
            var client = new HttpClient();
            var response = await client.DeleteAsync(uri);
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> GetAsync(string uri)
        {
            var client = new HttpClient();
            return await client.GetStringAsync(uri);
        }

        private async Task<string> PostAsync(string uri, Dictionary<string, string> parameters = null)
        {
            var client = new HttpClient();
            string body = string.Empty;
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    var key = parameters.Keys.ElementAt(i);
                    var value = HttpUtility.UrlEncode(parameters[key]);

                    body += key + "=" + value;
                    if (i < parameters.Count - 1)
                        body += "&";
                }
            }

            var httpContent = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, httpContent);
            var json = await response.Content.ReadAsStringAsync();
            
            return json;
        }

        private string FormatUri(string uri)
        {
            return string.Format("{0}{1}", baseUri, uri);
        }

        private string FormatUriWithProjectAuth(string uri)
        {
            return string.Format("{0}{1}?auth_token={2}", baseUri, uri, this.authToken);
        }

        private string FormatUriWithUserAuth(string uri)
        {
            return string.Format("{0}{1}?project_auth_token={2}&auth_token={3}", baseUri, uri, this.projectAuthToken, this.authToken);
        }

        public static string GetLocale(string path)
        {
            if (!path.Contains(localeSeparator))
                return defaultLocale.ToLowerInvariant();

            var elements = path.Split('.');
            var locale = elements.Last(e => e.Contains(localeSeparator));
            if (locale.Contains("\\"))
                locale = locale.Split('\\').Last(e => e.Contains(localeSeparator));

            if (locale.Length != 5)
                return defaultLocale.ToLowerInvariant();

            if (locale.StartsWith("pt"))
                return locale;
            else
                return locale.Split('-')[0].ToLowerInvariant();
        }

        public List<string> GetResourceFiles(string locale = null)
        {
            if (string.IsNullOrEmpty(this.WorkingFolder))
                return new List<string>();

            var resx = FilterIgnoredPaths(Directory.GetFiles(this.WorkingFolder, "*.resx", SearchOption.AllDirectories));
            var resw = FilterIgnoredPaths(Directory.GetFiles(this.WorkingFolder, "*.resw", SearchOption.AllDirectories));

            var result = new List<string>();
            result.AddRange(resx);
            result.AddRange(resw);

            if (!string.IsNullOrEmpty(locale))
                result = result.Where(p => PhraseAppService.GetLocale(p).Equals(locale, StringComparison.InvariantCultureIgnoreCase)).ToList();

            return result;
        }

        private IEnumerable<string> FilterIgnoredPaths(IEnumerable<string> paths)
        {
            // todo: make this configurable
            var ignores = new List<string>
            {
                "prototype", "vnext", "NonLocalized", "\\obj", "prototype", ".Phone", ".Content", ".Universal"
            };

            return paths.Where(p => !ignores.Any(i => p.Contains(i))).ToList();
        } 
    }
}