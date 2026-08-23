using Fedestrap.UI.ViewModels.Settings;
using Wpf.Ui.Controls;

namespace Fedestrap.UI.Elements.Settings.Pages;

public partial class SoberPage : UiPage
{
	public SoberPage()
	{
		InitializeComponent();
		DataContext = new SoberViewModel();
	}
}
