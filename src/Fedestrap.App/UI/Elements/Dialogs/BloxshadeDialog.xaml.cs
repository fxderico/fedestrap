using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Fedestrap.Enums;
using Fedestrap.UI.Elements.Base;

namespace Fedestrap.UI.Elements.Dialogs;

public partial class BloxshadeDialog : WpfUiWindow{
	public NextAction CloseAction;

	public BloxshadeDialog()
	{
		InitializeComponent();
	}

	public void Close_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}
}
