using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Fedestrap.UI.Elements.Base;
using Fedestrap.UI.ViewModels.ContextMenu;
using Wpf.Ui.Controls;

namespace Fedestrap.UI.Elements.ContextMenu;

public partial class BetterBloxDataCenterConsole : WpfUiWindow{

	public BetterBloxDataCenterConsole()
	{
		InitializeComponent();
		BetterBloxDataCenterConsoleViewModel dataContext = new BetterBloxDataCenterConsoleViewModel();
		base.DataContext = dataContext;
	}
}
