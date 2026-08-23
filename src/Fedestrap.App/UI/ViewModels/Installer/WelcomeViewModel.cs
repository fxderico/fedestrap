using Fedestrap.Resources;

namespace Fedestrap.UI.ViewModels.Installer;

public class WelcomeViewModel : NotifyPropertyChangedViewModel
{
	public string MainText => string.Format(Strings.Installer_Welcome_MainText, "Thank you for downloading Fedestrap. This installation process will be quick and simple, and you will be able to configure any of Fedestrap's settings after installation.");

	public bool CanContinue { get; set; }
}
