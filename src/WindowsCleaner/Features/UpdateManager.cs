using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsCleaner
{
    /// <summary>
    /// Gestionnaire de mises à jour automatiques depuis GitHub
    /// </summary>
    public class UpdateManager
    {
        private const string GITHUB_API_URL = "https://api.github.com/repos/{0}/{1}/releases/latest";
        private const string GITHUB_RELEASE_URL = "https://github.com/{0}/{1}/releases/latest";
        private const string USER_AGENT = "WindowsCleaner-UpdateChecker";
        
        private readonly string _owner;
        private readonly string _repo;
        private readonly string _currentVersion;
        private static readonly HttpClient _httpClient;

        static UpdateManager()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Initialise le gestionnaire de mises à jour
        /// </summary>
        /// <param name="owner">Propriétaire du dépôt GitHub (ex: "christwadel65-ux")</param>
        /// <param name="repo">Nom du dépôt (ex: "Windows-Cleaner")</param>
        /// <param name="currentVersion">Version actuelle (ex: "1.0.8")</param>
        public UpdateManager(string owner, string repo, string currentVersion)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _currentVersion = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));
        }

        /// <summary>
        /// Vérifie si une mise à jour est disponible
        /// </summary>
        /// <returns>Informations sur la mise à jour ou null si aucune mise à jour</returns>
        public async Task<UpdateInfo?> CheckForUpdateAsync()
        {
            try
            {
                Logger.Log(LogLevel.Info, LanguageManager.Get("update_checking"));
                
                string apiUrl = string.Format(GITHUB_API_URL, _owner, _repo);
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Logger.Log(LogLevel.Warning, LanguageManager.Get("update_cannot_check", response.StatusCode));
                    return null;
                }

                string jsonContent = await response.Content.ReadAsStringAsync();
                var release = JsonSerializer.Deserialize<GitHubRelease>(jsonContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (release == null || string.IsNullOrEmpty(release.TagName))
                {
                    Logger.Log(LogLevel.Warning, LanguageManager.Get("update_no_version"));
                    return null;
                }

                // Nettoyer le tag (enlever 'v' si présent)
                string latestVersion = release.TagName.TrimStart('v');
                
                if (CompareVersions(latestVersion, _currentVersion) > 0)
                {
                    Logger.Log(LogLevel.Info, LanguageManager.Get("update_available", latestVersion));
                    
                    return new UpdateInfo
                    {
                        Version = latestVersion,
                        ReleaseUrl = release.HtmlUrl ?? string.Format(GITHUB_RELEASE_URL, _owner, _repo),
                        DownloadUrl = GetDownloadUrl(release),
                        ReleaseNotes = release.Body ?? "Aucune note de version disponible",
                        PublishedAt = release.PublishedAt,
                        IsPrerelease = release.Prerelease
                    };
                }
                else
                {
                    Logger.Log(LogLevel.Info, LanguageManager.Get("update_current_version", _currentVersion));
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Logger.Log(LogLevel.Error, LanguageManager.Get("update_network_error", ex.Message));
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LanguageManager.Get("update_error", ex.Message));
                return null;
            }
        }

        /// <summary>
        /// Affiche une notification de mise à jour avec dialogue
        /// </summary>
        public async Task<bool> CheckAndNotifyUpdateAsync(IWin32Window? owner = null)
        {
            var updateInfo = await CheckForUpdateAsync();
            
            if (updateInfo == null)
            {
                return false;
            }

            string message = $"Une nouvelle version est disponible !\n\n" +
                           $"Version actuelle : {_currentVersion}\n" +
                           $"Nouvelle version : {updateInfo.Version}\n" +
                           $"Date de publication : {updateInfo.PublishedAt:dd/MM/yyyy}\n\n" +
                           $"Notes de version :\n{TruncateText(updateInfo.ReleaseNotes, 300)}\n\n" +
                           $"Voulez-vous ouvrir la page de téléchargement ?";

            var result = MessageBox.Show(
                message,
                "🎉 Mise à jour disponible",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information
            );

            if (result == DialogResult.Yes)
            {
                OpenReleaseUrl(updateInfo.ReleaseUrl);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Télécharge la mise à jour (fichier ZIP ou installateur)
        /// </summary>
        public async Task<bool> DownloadUpdateAsync(UpdateInfo updateInfo, string destinationPath, IProgress<int>? progress = null)
        {
            try
            {
                if (string.IsNullOrEmpty(updateInfo.DownloadUrl))
                {
                    Logger.Log(LogLevel.Error, LanguageManager.Get("update_no_download"));
                    return false;
                }

                Logger.Log(LogLevel.Info, LanguageManager.Get("update_downloading", updateInfo.DownloadUrl));

                using var response = await _httpClient.GetAsync(updateInfo.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1 && progress != null;

                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                long totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;

                    if (canReportProgress)
                    {
                        var progressPercentage = (int)((totalBytesRead * 100) / totalBytes);
                        progress!.Report(progressPercentage);
                    }
                }

                Logger.Log(LogLevel.Info, LanguageManager.Get("update_downloaded", destinationPath));
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LanguageManager.Get("update_download_error", ex.Message));
                return false;
            }
        }

        /// <summary>
        /// Ouvre la page de release dans le navigateur
        /// </summary>
        public void OpenReleaseUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                
                Logger.Log(LogLevel.Info, LanguageManager.Get("update_page_opened", url));
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LanguageManager.Get("update_cannot_open_browser", ex.Message));
            }
        }

        /// <summary>
        /// Compare deux versions (format: X.Y.Z)
        /// </summary>
        /// <returns>1 si version1 > version2, -1 si version1 < version2, 0 si égales</returns>
        private int CompareVersions(string version1, string version2)
        {
            try
            {
                var v1 = ParseVersion(version1);
                var v2 = ParseVersion(version2);

                if (v1.Major != v2.Major) return v1.Major.CompareTo(v2.Major);
                if (v1.Minor != v2.Minor) return v1.Minor.CompareTo(v2.Minor);
                return v1.Patch.CompareTo(v2.Patch);
            }
            catch
            {
                // En cas d'erreur, on compare les chaînes directement
                return string.Compare(version1, version2, StringComparison.OrdinalIgnoreCase);
            }
        }

        private (int Major, int Minor, int Patch) ParseVersion(string version)
        {
            var parts = version.Split('.');
            int major = parts.Length > 0 ? int.Parse(parts[0]) : 0;
            int minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
            int patch = parts.Length > 2 ? int.Parse(parts[2]) : 0;
            return (major, minor, patch);
        }

        private string GetDownloadUrl(GitHubRelease release)
        {
            // Chercher un asset avec .exe, .zip ou .msi
            if (release.Assets != null && release.Assets.Length > 0)
            {
                foreach (var asset in release.Assets)
                {
                    if (asset.Name?.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == true ||
                        asset.Name?.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) == true ||
                        asset.Name?.EndsWith(".msi", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return asset.BrowserDownloadUrl ?? "";
                    }
                }
            }
            
            return "";
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }

        #region DTO Classes

        private class GitHubRelease
        {
            [JsonPropertyName("tag_name")]
            public string? TagName { get; set; }
            
            [JsonPropertyName("name")]
            public string? Name { get; set; }
            
            [JsonPropertyName("body")]
            public string? Body { get; set; }
            
            [JsonPropertyName("html_url")]
            public string? HtmlUrl { get; set; }
            
            [JsonPropertyName("published_at")]
            public DateTime PublishedAt { get; set; }
            
            [JsonPropertyName("prerelease")]
            public bool Prerelease { get; set; }
            
            [JsonPropertyName("assets")]
            public GitHubAsset[]? Assets { get; set; }
        }

        private class GitHubAsset
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }
            
            [JsonPropertyName("browser_download_url")]
            public string? BrowserDownloadUrl { get; set; }
            
            [JsonPropertyName("size")]
            public long Size { get; set; }
        }

        #endregion
    }

    /// <summary>
    /// Informations sur une mise à jour disponible
    /// </summary>
    public class UpdateInfo
    {
        public string Version { get; set; } = "";
        public string ReleaseUrl { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string ReleaseNotes { get; set; } = "";
        public DateTime PublishedAt { get; set; }
        public bool IsPrerelease { get; set; }
    }
}
