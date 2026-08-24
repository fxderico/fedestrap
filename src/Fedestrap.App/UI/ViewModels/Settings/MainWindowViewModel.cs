using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Fedestrap.Enums;
using Fedestrap.Models.SettingTasks.Base;
using Fedestrap.Resources;
using Fedestrap.UI.Elements.About;
using Fedestrap.UI.Elements.Settings;
using Fedestrap.Utility;

namespace Fedestrap.UI.ViewModels.Settings;

public class MainWindowViewModel : NotifyPropertyChangedViewModel
{
	public EventHandler? RequestSaveNoticeEvent;

	public EventHandler? RequestSaveLaunchNoticeEvent;

	public EventHandler? RequestCloseWindowEvent;

	private Fedestrap.UI.Elements.Settings.MainWindow.TabItemViewModel _selectedTab;

	public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

	private bool _updateAvailable;

	public bool UpdateAvailable
	{
		get => _updateAvailable;
		private set
		{
			if (_updateAvailable == value)
				return;
			_updateAvailable = value;
			OnPropertyChanged("UpdateAvailable");
			OnPropertyChanged("CanInstallUpdate");
		}
	}

	private bool _installingUpdate;

	public bool InstallingUpdate
	{
		get => _installingUpdate;
		private set
		{
			if (_installingUpdate == value)
				return;
			_installingUpdate = value;
			OnPropertyChanged("InstallingUpdate");
			OnPropertyChanged("CanInstallUpdate");
			OnPropertyChanged("UpdateButtonText");
		}
	}

	private string _latestVersionTag = "";

	public string LatestVersionTag
	{
		get => _latestVersionTag;
		private set
		{
			if (_latestVersionTag == value)
				return;
			_latestVersionTag = value;
			OnPropertyChanged("LatestVersionTag");
			OnPropertyChanged("UpdateButtonText");
		}
	}

	public bool CanInstallUpdate => UpdateAvailable && !InstallingUpdate;

	public string UpdateButtonText => InstallingUpdate ? "Updating..." : ("Update to " + LatestVersionTag);

	public ICommand InstallUpdateCommand => new AsyncRelayCommand(InstallUpdateAsync);

	public async Task CheckForUpdatesAsync()
	{
		if (!App.Settings.Prop.CheckForUpdates)
			return;
		try
		{
			var release = await App.GetLatestRelease(true);
			if (release == null || string.IsNullOrWhiteSpace(release.TagName))
				return;
			if (!Version.TryParse(release.TagName.TrimStart('v', 'V'), out Version? remote))
				return;
			if (!Version.TryParse(AppVersion, out Version? local))
				return;
			if (remote > local)
			{
				LatestVersionTag = release.TagName;
				UpdateAvailable = true;
			}
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("MainWindowViewModel::CheckForUpdates", "Update check failed: " + ex.Message);
		}
	}

	private async Task InstallUpdateAsync()
	{
		if (InstallingUpdate || string.IsNullOrEmpty(LatestVersionTag))
			return;
		InstallingUpdate = true;
		try
		{
			if (await Fedestrap.Extensions.GithubUpdater.DownloadAndInstallUpdate(LatestVersionTag))
			{
				App.RestartApplication(["-settings"]);
				return;
			}
			InstallingUpdate = false;
			Frontend.ShowMessageBox("The update could not be installed. Check the logs and try again.", MessageBoxImage.Warning);
		}
		catch (Exception ex)
		{
			InstallingUpdate = false;
			App.Logger.WriteLine("MainWindowViewModel::InstallUpdate", "Update install failed: " + ex.Message);
			Frontend.ShowMessageBox("Error installing update:\n" + ex.Message, MessageBoxImage.Error);
		}
	}

	public ICommand OpenAboutCommand => new RelayCommand(OpenAbout);

	public ICommand SaveSettingsCommand => new AsyncRelayCommand(SaveSettingsAsync);

	public ICommand SaveAndLaunchSettingsCommand => new AsyncRelayCommand(SaveAndLaunchSettingsAsync);

	public ICommand CloseWindowCommand => new RelayCommand(CloseWindow);

	public bool TestModeEnabled
	{
		get
		{
			return App.LaunchSettings.TestModeFlag.Active;
		}
		set
		{
			if (value && !App.State.Prop.TestModeWarningShown)
			{
				if (Frontend.ShowMessageBox(Strings.Menu_TestMode_Prompt, MessageBoxImage.Asterisk, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
				{
					OnPropertyChanged(nameof(TestModeEnabled));
					return;
				}
				App.State.Prop.TestModeWarningShown = true;
			}
			App.LaunchSettings.TestModeFlag.Active = value;
		}
	}

	public Fedestrap.UI.Elements.Settings.MainWindow.TabItemViewModel SelectedTab
	{
		get
		{
			return _selectedTab;
		}
		set
		{
			if (_selectedTab != value)
			{
				_selectedTab = value;
				OnPropertyChanged("SelectedTab");
			}
		}
	}

	public ObservableCollection<Fedestrap.UI.Elements.Settings.MainWindow.TabItemViewModel> Tabs { get; set; } = new ObservableCollection<Fedestrap.UI.Elements.Settings.MainWindow.TabItemViewModel>();

	public int SelectedLaunchModeIndex
	{
		get
		{
			return (App.Settings.Prop.LaunchSelectionIndex == 1) ? 1 : 0;
		}
		set
		{
			int num = ((value == 1) ? 1 : 0);
			if (App.Settings.Prop.LaunchSelectionIndex != num)
			{
				App.Settings.Prop.LaunchSelectionIndex = num;
				App.Settings.SaveDeferred();
				OnPropertyChanged("SelectedLaunchModeIndex");
				OnPropertyChanged("SelectedLaunchTargetName");
			}
		}
	}

	public string SelectedLaunchClient
	{
		get
		{
			return App.Settings.Prop.LaunchSelectedClient ?? "";
		}
		set
		{
			string newValue = value ?? "";
			if ((App.Settings.Prop.LaunchSelectedClient ?? "") != newValue)
			{
				App.Settings.Prop.LaunchSelectedClient = newValue;
				App.Settings.SaveDeferred();
				OnPropertyChanged("SelectedLaunchClient");
				OnPropertyChanged("SelectedLaunchTargetName");
			}
		}
	}

	public string SelectedLaunchTargetName
	{
		get
		{
			string code = SelectedLaunchClient;
			if (!string.IsNullOrEmpty(code) && ClassicClients.IsClientInstalled(code))
			{
				var config = ClassicClients.GetInstalledConfig(code);
				return (config != null && !string.IsNullOrWhiteSpace(config.Name)) ? config.Name : code;
			}
			if (SelectedLaunchModeIndex != 1)
			{
				return "Roblox";
			}
			return "Roblox Studio";
		}
	}

	public new event PropertyChangedEventHandler? PropertyChanged;

	private void OpenAbout()
	{
		new Fedestrap.UI.Elements.About.MainWindow().ShowDialog();
	}

	private void CloseWindow()
	{
		RequestCloseWindowEvent?.Invoke(this, EventArgs.Empty);
	}

	private async Task SaveSettingsAsync()
	{
		await TrySaveSettingsAsync(true);
	}

	public async Task<bool> TrySaveSettingsAsync(bool showNotice, bool showErrors = true)
	{
		try
		{
			App.Settings.SaveDeferred();
			App.State.Save();
			App.FastFlags.SaveDeferred();
		}
		catch (Exception ex)
		{
			App.Logger.WriteException("MainWindowViewModel::SaveSettings", ex);
			if (showErrors)
			{
				Frontend.ShowMessageBox("Settings could not be saved. Check the logs and try again.", MessageBoxImage.Warning);
			}
			return false;
		}
		int failedTasks = 0;
		foreach (KeyValuePair<string, BaseTask> pendingSettingTask in App.PendingSettingTasks.ToArray())
		{
			BaseTask value = pendingSettingTask.Value;
			if (!value.Changed)
			{
				App.PendingSettingTasks.Remove(pendingSettingTask.Key);
				continue;
			}
			try
			{
				App.Logger.WriteLine("MainWindowViewModel::SaveSettings", $"Executing pending task '{value}'");
				await value.ExecuteAsync();
				App.PendingSettingTasks.Remove(pendingSettingTask.Key);
			}
			catch (Exception ex)
			{
				failedTasks++;
				App.Logger.WriteException("MainWindowViewModel::SaveSettings", ex);
			}
		}
		if (failedTasks > 0)
		{
			if (showErrors)
			{
				Frontend.ShowMessageBox("Some settings could not be saved. Check the logs and try again.", MessageBoxImage.Warning);
			}
			return false;
		}
		if (showNotice)
		{
			RequestSaveNoticeEvent?.Invoke(this, EventArgs.Empty);
		}
		return true;
	}

	public async Task SaveAndLaunchSettingsAsync()
	{
		if (!await TrySaveSettingsAsync(false))
			return;
		RequestSaveLaunchNoticeEvent?.Invoke(this, EventArgs.Empty);
		string code = SelectedLaunchClient;
		if (!string.IsNullOrEmpty(code) && ClassicClients.EngineInstalled && ClassicClients.IsClientInstalled(code))
		{
			LaunchClassicClient(code, SelectedLaunchModeIndex == 1);
			return;
		}
		LaunchHandler.LaunchRoblox((SelectedLaunchModeIndex != 1) ? LaunchMode.Player : LaunchMode.Studio);
	}

	private static void StopFedestrapRpcForClassic()
	{
		try
		{
			if (Application.Current == null)
			{
				return;
			}
			foreach (Window window in Application.Current.Windows)
			{
				if (window is Fedestrap.UI.Elements.Settings.MainWindow mainWindow)
				{
					mainWindow.ToggleDiscordRPC(false);
					App.Logger.WriteLine("MainWindowViewModel", "Fedestrap RPC stopped because a classic Roblox client is launching.");
					break;
				}
			}
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("MainWindowViewModel", "Failed to stop Fedestrap RPC for classic launch: " + ex.Message);
		}
	}

	private void LaunchClassicClient(string code, bool studio)
	{
		StopFedestrapRpcForClassic();
		IBootstrapperDialog dialog = App.Settings.Prop.BootstrapperStyle.GetNew();
		dialog.CancelEnabled = true;
		dialog.ProgressStyle = System.Windows.Forms.ProgressBarStyle.Marquee;
		dialog.Message = "Starting Fedestrap";

		var launchCts = new CancellationTokenSource();
		int cancelled = 0;
		dialog.CancelCallback = delegate
		{
			if (System.Threading.Interlocked.Exchange(ref cancelled, 1) != 0)
				return;
			try { launchCts.Cancel(); } catch { }
			Task.Run(delegate
			{
				try { ClassicClients.ShutdownAll(); }
				catch (Exception ex) { App.Logger.WriteLine("MainWindowViewModel", "Classic shutdown failed: " + ex.Message); }
			});
		};

		Task.Run(async delegate
		{
			try
			{
				void Status(string message) => dialog.Message = message;

				using (var cts = CancellationTokenSource.CreateLinkedTokenSource(launchCts.Token))
				{
					cts.CancelAfter(TimeSpan.FromMinutes(10));
					await ClassicClients.UpdateEverythingAsync(code, Status, delegate (double percent, string message)
					{
						dialog.ProgressStyle = System.Windows.Forms.ProgressBarStyle.Continuous;
						dialog.ProgressMaximum = 100;
						dialog.Message = message;
						dialog.ProgressValue = (int)percent;
					}, cts.Token).ConfigureAwait(false);
				}

				if (launchCts.IsCancellationRequested)
					return;

				dialog.ProgressStyle = System.Windows.Forms.ProgressBarStyle.Marquee;
				if (studio)
				{
					var config = ClassicClients.GetInstalledConfig(code);
					if (config == null || !ClassicClients.HasStudio(config))
						throw new InvalidOperationException("The selected ORC client does not include Studio.");
					Status("Starting ORC Studio");
					Process? studioProcess = ClassicClients.Launch(code, ClientLaunchType.Studio, ClassicClients.DefaultPort, "localhost", Status);
					if (studioProcess == null)
						throw new InvalidOperationException("ORC Studio could not be started.");
					await Task.Delay(2000, launchCts.Token).ConfigureAwait(false);
					if (studioProcess.HasExited)
					{
						studioProcess.Dispose();
						throw new InvalidOperationException("ORC Studio closed during startup.");
					}
					return;
				}

				var maps = ClassicClients.ListMaps();
				string map = App.Settings.Prop.ClassicSelectedMap ?? "";
				if (string.IsNullOrEmpty(map) || !maps.Contains(map))
				{
					map = maps.FirstOrDefault() ?? "";
				}

				await ClassicClients.PlaySoloAsync(code, map, Status, launchCts.Token).ConfigureAwait(false);

				if (launchCts.IsCancellationRequested)
					ClassicClients.ShutdownAll();
			}
			catch (OperationCanceledException) when (launchCts.IsCancellationRequested)
			{
			}
			catch (Exception ex)
			{
				Frontend.ShowMessageBox($"Could not launch {code}: {ex.Message}", MessageBoxImage.Error);
			}
		}).ContinueWith(delegate
		{
			if (cancelled == 0)
				dialog.CloseBootstrapper();
			launchCts.Dispose();
		});

		dialog.ShowBootstrapper();

		try
		{
			Fedestrap.Utility.ClassicIntegrations.HideFedestrap();
		}
		catch
		{
		}
	}

	protected new bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, newValue))
		{
			return false;
		}
		field = newValue;
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? throw new ArgumentNullException("propertyName")));
		return true;
	}
}
