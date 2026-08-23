using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Fedestrap.UI.Elements.Controls;
using Fedestrap.UI.ViewModels.Settings;
using Wpf.Ui.Controls;

namespace Fedestrap.UI.Elements.Settings.Pages;

public partial class BehaviourPage : UiPage{

	public BehaviourPage()
	{
		base.DataContext = new BehaviourViewModel();
		InitializeComponent();
		base.Loaded += OnPageLoaded;
	}

	private void OnPageLoaded(object sender, RoutedEventArgs e)
	{
		if (base.DataContext is BehaviourViewModel behaviourViewModel)
		{
			behaviourViewModel.RefreshExcludedGames();
		}
	}

	private void ResetDatacenters_Click(object sender, RoutedEventArgs e)
	{
		if (base.DataContext is BehaviourViewModel behaviourViewModel)
		{
			behaviourViewModel.ResetDatacenters();
		}
	}
}
