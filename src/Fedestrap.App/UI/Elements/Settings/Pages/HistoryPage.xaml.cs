using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Fedestrap.UI.ViewModels.Pages;

namespace Fedestrap.UI.Elements.Settings.Pages;

public partial class HistoryPage : Page{
	private readonly HistoryPageViewModel _viewModel;

	public HistoryPage()
	{
		_viewModel = new HistoryPageViewModel();
		base.DataContext = _viewModel;
		InitializeComponent();
	}
}
