using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Fedestrap.Resources;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;

namespace Fedestrap.UI.Elements.Installer.Pages;

public partial class SignInPage : UiPage
{
	private CancellationTokenSource? _pollCts;

	private bool _busy;

	// true = login tab selected, false = register tab selected
	private bool _loginMode = true;

	public SignInPage()
	{
		InitializeComponent();
		Unloaded += UiPage_Unloaded;
	}

	private void UiPage_Loaded(object sender, RoutedEventArgs e)
	{
		Unloaded -= UiPage_Unloaded;
		Unloaded += UiPage_Unloaded;
		if (Window.GetWindow(this) is MainWindow mainWindow)
		{
			mainWindow.SetNextButtonText(Strings.Common_Navigation_Next);
			mainWindow.SetButtonEnabled("next", state: true);
		}
	}

	private void UiPage_Unloaded(object sender, RoutedEventArgs e)
	{
		Unloaded -= UiPage_Unloaded;
		CancelPolling();
		_busy = false;
	}

	private void SignIn_Click(object sender, RoutedEventArgs e)
	{
		if (_busy)
		{
			return;
		}

		SetFormMode(login: true);
		FormError.Text = "";
		UsernameBox.Text = "";
		PasswordBox.Password = "";

		IdlePanel.Visibility = Visibility.Collapsed;
		DonePanel.Visibility = Visibility.Collapsed;
		FormPanel.Visibility = Visibility.Visible;
		UsernameBox.Focus();
	}

	private void TabLogin_Click(object sender, RoutedEventArgs e) => SetFormMode(login: true);

	private void TabRegister_Click(object sender, RoutedEventArgs e) => SetFormMode(login: false);

	private void SetFormMode(bool login)
	{
		_loginMode = login;
		FormError.Text = "";
		FormTitle.Text = login ? "Log in" : "Create account";
		SubmitButton.Content = login ? "Log in" : "Create account";
		TabLoginButton.Appearance = login ? ControlAppearance.Primary : ControlAppearance.Secondary;
		TabRegisterButton.Appearance = login ? ControlAppearance.Secondary : ControlAppearance.Primary;
	}

	private void Cancel_Click(object sender, RoutedEventArgs e)
	{
		CancelPolling();
		_busy = false;
		FormPanel.Visibility = Visibility.Collapsed;
		WaitingPanel.Visibility = Visibility.Collapsed;
		DonePanel.Visibility = Visibility.Collapsed;
		IdlePanel.Visibility = Visibility.Visible;
	}

	private void CancelPolling()
	{
		try
		{
			_pollCts?.Cancel();
			_pollCts?.Dispose();
		}
		catch
		{
		}

		_pollCts = null;
	}

	private async void Submit_Click(object sender, RoutedEventArgs e)
	{
		if (_busy)
		{
			return;
		}

		string username = UsernameBox.Text.Trim();
		string password = PasswordBox.Password;

		if (username.Length == 0 || password.Length == 0)
		{
			FormError.Text = "Enter a username and password.";
			return;
		}

		_busy = true;
		FormError.Text = "";
		SubmitButton.IsEnabled = false;

		FormPanel.Visibility = Visibility.Collapsed;
		WaitingPanel.Visibility = Visibility.Visible;
		WaitingStatus.Text = "";

		_pollCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		CancellationToken token = _pollCts.Token;

		string endpoint = _loginMode ? "/api/login" : "/api/register";
		string? vsToken = null;
		string? error = null;

		try
		{
			using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, App.WebsiteBaseUrl + endpoint);
			request.Content = new StringContent(
				JsonSerializer.Serialize(new { username, password }),
				System.Text.Encoding.UTF8,
				"application/json");

			using HttpResponseMessage response = await App.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(true);
			string json = await Fedestrap.Utility.Http.ReadStringBoundedAsync(response.Content, 262144, token).ConfigureAwait(true);
			using JsonDocument document = JsonDocument.Parse(json);

			if (response.IsSuccessStatusCode
				&& document.RootElement.TryGetProperty("vs_token", out JsonElement tokenElement)
				&& tokenElement.ValueKind == JsonValueKind.String)
			{
				vsToken = tokenElement.GetString();
			}
			else
			{
				error = ReadString(document.RootElement, "error");
				if (error.Length == 0)
				{
					error = "Something went wrong (status " + (int)response.StatusCode + "). Try again.";
				}
			}
		}
		catch (OperationCanceledException)
		{
			error = "That took too long. Check your connection and try again.";
		}
		catch (Exception ex)
		{
			App.Logger.WriteException("SignInPage::Submit", ex);
			error = "Could not reach the sign in service. Check your connection and try again.";
		}

		if (string.IsNullOrWhiteSpace(vsToken))
		{
			_busy = false;
			SubmitButton.IsEnabled = true;
			WaitingPanel.Visibility = Visibility.Collapsed;
			FormPanel.Visibility = Visibility.Visible;
			FormError.Text = error ?? "Sign in failed. Try again.";
			CancelPolling();
			return;
		}

		try
		{
			Fedestrap.Utility.WebsiteAuth.Save(vsToken.Trim());
			await ShowProfileAsync(vsToken.Trim(), token).ConfigureAwait(true);
		}
		finally
		{
			SubmitButton.IsEnabled = true;
			CancelPolling();
		}
	}

	private async Task ShowProfileAsync(string authToken, CancellationToken token)
	{
		string displayName = "";
		string username = "";
		string avatar = "";
		string avatarBorder = "";
		string borderJson = "";
		string banner = "";
		string userId = "";

		try
		{
			using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, App.WebsiteBaseUrl + "/api/me");
			request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
			using HttpResponseMessage response = await App.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(true);
			App.Logger.WriteLine("SignInPage::ShowProfile", "/api/me responded " + (int)response.StatusCode);
			if (response.IsSuccessStatusCode)
			{
				string json = await Fedestrap.Utility.Http.ReadStringBoundedAsync(response.Content, 4 * 1024 * 1024, token).ConfigureAwait(true);
				using JsonDocument document = JsonDocument.Parse(json);
				if (document.RootElement.TryGetProperty("user", out JsonElement user) && user.ValueKind == JsonValueKind.Object)
				{
					displayName = ReadString(user, "displayName");
					username = ReadString(user, "username");
					userId = ReadString(user, "id");
					avatar = ReadString(user, "avatar");
					banner = ReadString(user, "banner");
					avatarBorder = ReadString(user, "avatarBorder");
					if (user.TryGetProperty("equippedBorder", out JsonElement equipped) && equipped.ValueKind == JsonValueKind.Object)
					{
						borderJson = equipped.GetRawText();
					}
				}
			}
		}
		catch (OperationCanceledException) when (token.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("SignInPage::ShowProfile", "Could not load the profile: " + ex.Message);
		}

		if (userId.Length > 0)
		{
			try
			{
				string label = displayName.Length > 0 ? displayName : username;
				Fedestrap.Utility.WebsiteAuth.AddOrUpdateAccount(authToken, userId, label, avatar);
				Fedestrap.Installer.PendingAuthToken = authToken;
				Fedestrap.Installer.PendingAuthId = userId;
				Fedestrap.Installer.PendingAuthLabel = label;
				Fedestrap.Installer.PendingAuthAvatar = avatar;
			}
			catch (Exception ex)
			{
				App.Logger.WriteLine("SignInPage::ShowProfile", "Could not register the account: " + ex.Message);
			}
		}

		DoneName.Text = displayName.Length > 0 ? displayName : (username.Length > 0 ? username : "Signed in");
		DoneUsername.Text = username.Length > 0 ? "@" + username : "";

		AvatarRing.Fill = Fedestrap.Utility.GradientWebsite.Parse(avatarBorder)
			?? (Brush)FindResource("SystemAccentColorSecondaryBrush");

		App.Logger.WriteLine("SignInPage::ShowProfile", "avatar=" + (avatar.Length > 0 ? avatar : "(none)") + " border=" + (avatarBorder.Length > 0 ? avatarBorder : "(none)") + " border=" + (borderJson.Length > 0 ? "yes" : "(none)") + " banner=" + (banner.Length > 0 ? banner : "(none)"));

		List<Task> pending = new List<Task>();
		Task<BitmapSource?> avatarTask = Fedestrap.Utility.AppImage.LoadAsync(ResolveUrl(avatar), 256);
		pending.Add(avatarTask);
		Task<BitmapSource?>? bannerTask = null;
		if (banner.Length > 0)
		{
			bannerTask = Fedestrap.Utility.GradientWebsite.LoadBannerImageAsync(ResolveUrl(banner));
			pending.Add(bannerTask);
		}
		Task<Fedestrap.Utility.BorderRender?>? borderTask = null;
		if (borderJson.Length > 0)
		{
			borderTask = Task.Run(() =>
			{
				try
				{
					using JsonDocument borderDocument = JsonDocument.Parse(borderJson);
					return Fedestrap.Utility.WebsiteBorderRenderer.Build(borderDocument.RootElement, 104.0, 170.0);
				}
				catch (Exception ex)
				{
					App.Logger.WriteLine("SignInPage::ShowProfile", "Could not build the profile border: " + ex.Message);
					return null;
				}
			});
			pending.Add(borderTask);
		}

		await Task.WhenAll(pending).ConfigureAwait(true);
		token.ThrowIfCancellationRequested();

		BitmapSource? avatarBitmap = await avatarTask.ConfigureAwait(true);
		if (avatarBitmap != null)
		{
			ImageBrush avatarBrush = new ImageBrush(avatarBitmap) { Stretch = Stretch.UniformToFill };
			if (avatarBrush.CanFreeze)
			{
				avatarBrush.Freeze();
			}
			AvatarFill.Fill = avatarBrush;
		}
		else
		{
			AvatarFill.Fill = (Brush)FindResource("ControlFillColorDefaultBrush");
		}

		if (bannerTask != null)
		{
			BitmapSource? bannerBitmap = await bannerTask.ConfigureAwait(true);
			if (bannerBitmap != null)
			{
				BannerImage.Source = bannerBitmap;
				BannerHost.Visibility = Visibility.Visible;
			}
		}

		if (borderTask != null)
		{
			Fedestrap.Utility.BorderRender? render = await borderTask.ConfigureAwait(true);
			if (render?.Image != null)
			{
				AvatarBorderImage.Source = render.Image;
				AvatarBorderImage.Width = render.Width;
				AvatarBorderImage.Height = render.Height;
				AvatarBorderImage.Margin = render.Margin;
				AvatarBorderImage.Visibility = Visibility.Visible;
			}
		}

		_busy = false;
		WaitingPanel.Visibility = Visibility.Collapsed;
		IdlePanel.Visibility = Visibility.Collapsed;
		DonePanel.Visibility = Visibility.Visible;
	}

	private static string ReadBorderImage(JsonElement user)
	{
		foreach (string name in new[] { "avatarBorderImage", "borderImage" })
		{
			string direct = ReadString(user, name);
			if (direct.Length > 0)
			{
				return direct;
			}
		}

		if (user.TryGetProperty("avatarBorder", out JsonElement border) && border.ValueKind == JsonValueKind.Object)
		{
			return ReadString(border, "image");
		}

		return "";
	}

	private static string ReadString(JsonElement element, string name)
	{
		return element.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String
			? value.GetString() ?? ""
			: "";
	}

	private static string ResolveUrl(string url)
	{
		string trimmed = (url ?? "").Trim();
		if (trimmed.Length == 0)
		{
			return "";
		}

		if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
			|| trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
			|| trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
		{
			return trimmed;
		}

		return App.WebsiteBaseUrl.TrimEnd('/') + "/" + trimmed.TrimStart('/');
	}

	private void SwitchAccounts_Click(object sender, RoutedEventArgs e)
	{
		CancelPolling();
		_busy = false;
		DonePanel.Visibility = Visibility.Collapsed;
		WaitingPanel.Visibility = Visibility.Collapsed;
		IdlePanel.Visibility = Visibility.Visible;
	}

	private void Skip_Click(object sender, RoutedEventArgs e)
	{
		CancelPolling();
		Advance();
	}

	private void Advance()
	{
		if (Window.GetWindow(this) is MainWindow mainWindow)
		{
			mainWindow.Navigate(typeof(WelcomePage));
			mainWindow.SetButtonEnabled("back", state: true);
			mainWindow.SetButtonEnabled("next", state: true);
		}
	}
}
