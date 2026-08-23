using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows;

namespace Fedestrap.UI.Elements.ContextMenu
{
    public partial class ChangePasswordWindow
    {
        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = OldPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirm = ConfirmPasswordBox.Password;

            if (oldPassword.Length == 0)
            {
                StatusText.Text = "Enter your current password.";
                return;
            }
            if (newPassword.Length < 8)
            {
                StatusText.Text = "New password must be at least 8 characters.";
                return;
            }
            if (newPassword != confirm)
            {
                StatusText.Text = "The new passwords don't match.";
                return;
            }

            string? token = Fedestrap.Utility.WebsiteAuth.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                StatusText.Text = "You're not signed in.";
                return;
            }

            SaveBtn.IsEnabled = false;
            StatusText.Text = "Changing password...";

            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, App.WebsiteBaseUrl + "/api/me/password");
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                req.Content = new StringContent(
                    JsonSerializer.Serialize(new { oldPassword, newPassword }),
                    Encoding.UTF8,
                    "application/json");

                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                using var resp = await App.HttpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(true);
                string body = await Fedestrap.Utility.Http.ReadStringBoundedAsync(resp.Content, 65536, timeout.Token).ConfigureAwait(true);
                using var doc = JsonDocument.Parse(body);

                if (resp.IsSuccessStatusCode
                    && doc.RootElement.TryGetProperty("vs_token", out var tokenEl)
                    && tokenEl.ValueKind == JsonValueKind.String
                    && !string.IsNullOrEmpty(tokenEl.GetString()))
                {
                    // The server rotates the token (and invalidates every other
                    // session) on a password change - swap in the fresh one so
                    // this session keeps working.
                    Fedestrap.Utility.WebsiteAuth.Save(tokenEl.GetString()!.Trim());
                    Close();
                    return;
                }

                string error = "Could not change password.";
                if (doc.RootElement.TryGetProperty("error", out var errEl) && errEl.ValueKind == JsonValueKind.String)
                    error = errEl.GetString() ?? error;

                StatusText.Text = error;
                SaveBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("ChangePasswordWindow::Save", ex);
                StatusText.Text = "Network error while changing password.";
                SaveBtn.IsEnabled = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
