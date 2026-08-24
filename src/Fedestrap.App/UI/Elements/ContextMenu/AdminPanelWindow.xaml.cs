using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows;

namespace Fedestrap.UI.Elements.ContextMenu
{
    public sealed class AdminUserRow
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string UsernameLabel { get; set; } = "";
        public string SubtitleLabel { get; set; } = "";
        public Visibility AdminBadgeVisibility { get; set; } = Visibility.Collapsed;
    }

    public partial class AdminPanelWindow
    {
        private readonly ObservableCollection<AdminUserRow> _rows = new();

        public AdminPanelWindow()
        {
            InitializeComponent();
            UsersList.ItemsSource = _rows;
            Loaded += AdminPanelWindow_Loaded;
        }

        private async void AdminPanelWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= AdminPanelWindow_Loaded;
            await RefreshAsync();
        }

        private async System.Threading.Tasks.Task RefreshAsync()
        {
            string? token = Fedestrap.Utility.WebsiteAuth.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                StatusText.Text = "You're not signed in.";
                return;
            }

            StatusText.Text = "Loading...";
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, App.WebsiteBaseUrl + "/api/admin/users");
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                using var resp = await App.HttpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(true);
                string body = await Fedestrap.Utility.Http.ReadStringBoundedAsync(resp.Content, 4 * 1024 * 1024, timeout.Token).ConfigureAwait(true);

                if (!resp.IsSuccessStatusCode)
                {
                    string error = "Could not load users (HTTP " + (int)resp.StatusCode + ").";
                    try
                    {
                        using var errDoc = JsonDocument.Parse(body);
                        if (errDoc.RootElement.TryGetProperty("error", out var errEl) && errEl.ValueKind == JsonValueKind.String)
                            error = errEl.GetString() ?? error;
                    }
                    catch { }
                    StatusText.Text = error;
                    return;
                }

                using var doc = JsonDocument.Parse(body);
                _rows.Clear();
                if (doc.RootElement.TryGetProperty("users", out var usersEl) && usersEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var u in usersEl.EnumerateArray())
                    {
                        string id = Str(u, "id");
                        string username = Str(u, "username");
                        string displayName = Str(u, "displayName");
                        string ip = Str(u, "registrationIp");
                        bool isAdmin = u.TryGetProperty("isAdmin", out var ia) && ia.ValueKind == JsonValueKind.True;
                        long createdAt = u.TryGetProperty("createdAt", out var ca) && ca.ValueKind == JsonValueKind.Number ? ca.GetInt64() : 0;

                        string created = createdAt > 0
                            ? DateTimeOffset.FromUnixTimeMilliseconds(createdAt).ToLocalTime().ToString("g")
                            : "unknown";

                        _rows.Add(new AdminUserRow
                        {
                            Id = id,
                            DisplayName = string.IsNullOrEmpty(displayName) ? username : displayName,
                            UsernameLabel = "@" + username,
                            SubtitleLabel = "Registered " + created + ", IP " + ip,
                            AdminBadgeVisibility = isAdmin ? Visibility.Visible : Visibility.Collapsed,
                        });
                    }
                }

                StatusText.Text = _rows.Count + " user" + (_rows.Count == 1 ? "" : "s") + ".";
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("AdminPanelWindow::Refresh", ex);
                StatusText.Text = "Network error while loading users.";
            }
        }

        private async void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            string username = (NewUsernameBox.Text ?? "").Trim();
            string password = NewPasswordBox.Password;
            string displayName = (NewDisplayNameBox.Text ?? "").Trim();

            if (username.Length == 0 || password.Length == 0)
            {
                CreateStatusText.Text = "Enter a username and password.";
                return;
            }

            string? token = Fedestrap.Utility.WebsiteAuth.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                CreateStatusText.Text = "You're not signed in.";
                return;
            }

            CreateStatusText.Text = "Creating...";
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, App.WebsiteBaseUrl + "/api/admin/users");
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                req.Content = new StringContent(
                    JsonSerializer.Serialize(new { username, password, displayName }),
                    Encoding.UTF8,
                    "application/json");

                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                using var resp = await App.HttpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(true);
                string body = await Fedestrap.Utility.Http.ReadStringBoundedAsync(resp.Content, 65536, timeout.Token).ConfigureAwait(true);
                using var doc = JsonDocument.Parse(body);

                if (resp.IsSuccessStatusCode)
                {
                    NewUsernameBox.Text = "";
                    NewPasswordBox.Password = "";
                    NewDisplayNameBox.Text = "";
                    CreateStatusText.Text = "Created " + username + ".";
                    await RefreshAsync();
                    return;
                }

                string error = "Could not create user.";
                if (doc.RootElement.TryGetProperty("error", out var errEl) && errEl.ValueKind == JsonValueKind.String)
                    error = errEl.GetString() ?? error;
                CreateStatusText.Text = error;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("AdminPanelWindow::CreateUser", ex);
                CreateStatusText.Text = "Network error while creating user.";
            }
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement fe || fe.DataContext is not AdminUserRow row)
                return;

            var confirm = Fedestrap.UI.Frontend.ShowMessageBox(
                $"Delete {row.DisplayName} ({row.UsernameLabel})? This can't be undone.",
                MessageBoxImage.Warning,
                MessageBoxButton.YesNo);
            if (confirm != MessageBoxResult.Yes)
                return;

            string? token = Fedestrap.Utility.WebsiteAuth.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                StatusText.Text = "You're not signed in.";
                return;
            }

            StatusText.Text = "Deleting...";
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Delete, App.WebsiteBaseUrl + "/api/admin/users/" + Uri.EscapeDataString(row.Id));
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                using var resp = await App.HttpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(true);

                if (resp.IsSuccessStatusCode)
                {
                    await RefreshAsync();
                    return;
                }

                string body = await Fedestrap.Utility.Http.ReadStringBoundedAsync(resp.Content, 65536, timeout.Token).ConfigureAwait(true);
                string error = "Could not delete user.";
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("error", out var errEl) && errEl.ValueKind == JsonValueKind.String)
                        error = errEl.GetString() ?? error;
                }
                catch { }
                StatusText.Text = error;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("AdminPanelWindow::DeleteUser", ex);
                StatusText.Text = "Network error while deleting user.";
            }
        }

        private static string Str(JsonElement obj, string name)
        {
            return obj.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? (v.GetString() ?? "") : "";
        }
    }
}
