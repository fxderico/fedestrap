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

public partial class GamePassConsole : WpfUiWindow
{
	private readonly GamePassConsoleViewModel _viewModel;

	public GamePassConsole(long userId)
	{
		InitializeComponent();
		_viewModel = new GamePassConsoleViewModel();
		DataContext = _viewModel;
		_viewModel.LoadGamePassesCommand.Execute(userId);
	}

	protected override void OnClosed(EventArgs e)
	{
		_viewModel.Dispose();
		base.OnClosed(e);
	}
}
