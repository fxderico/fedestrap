using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Fedestrap.Resources;

namespace Fedestrap.UI.ViewModels.Dialogs;

public class UninstallerViewModel
{
	public string Text => string.Format(Strings.Uninstaller_Text, "https://fedestrap.fede.one/documentation#issues", Paths.Base);

	public bool KeepData { get; set; } = true;

	public ICommand ConfirmUninstallCommand => new RelayCommand(ConfirmUninstall);

	public event EventHandler? ConfirmUninstallRequest;

	private void ConfirmUninstall()
	{
		this.ConfirmUninstallRequest?.Invoke(this, new EventArgs());
	}
}
