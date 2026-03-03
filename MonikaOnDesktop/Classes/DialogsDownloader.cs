using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace MonikaOnDesktop {
    public class GitHubDownloader {
        private readonly HttpClient _httpClient;
        private const string GitHubApiUrl = "https://api.github.com/repos/";

        public GitHubDownloader() {
            _httpClient = new HttpClient();
            // GitHub requires a User-Agent header
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MonikaOnDesktop");
        }

        public async Task DownloadDirectoryAsync(string owner, string repo, string directoryPath, string localPath) {
            var url = $"{GitHubApiUrl}{owner}/{repo}/contents/{directoryPath}";
            var json = await _httpClient.GetStringAsync(url);
            var contents = JsonConvert.DeserializeObject<dynamic[]>(json);

            foreach (var item in contents) {
                string type = item.type;
                string name = item.name;
                string itemLocalPath = Path.Combine(localPath, name);

                Debug.WriteLine($"Type: {item.type} Name: {item.name}");
                if (type == "dir") {
                    // Recursively download subdirectories
                    Directory.CreateDirectory(itemLocalPath);
                    await DownloadDirectoryAsync(owner, repo, $"{directoryPath}/{name}", itemLocalPath);
                } else if (type == "file") {
                    // Download file
                    string downloadUrl = item.download_url;
                    var fileBytes = await _httpClient.GetByteArrayAsync(downloadUrl);
                    await File.WriteAllBytesAsync(itemLocalPath, fileBytes);
                }
            }
        }
    }
}
