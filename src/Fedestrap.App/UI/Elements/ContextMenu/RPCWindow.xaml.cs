using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Fedestrap.UI.Elements.Base;
using Fedestrap.UI.ViewModels.ContextMenu;

namespace Fedestrap.UI.Elements.ContextMenu;

public partial class RPCWindow : WpfUiWindow{

	public RPCWindow()
	{
		InitializeComponent();
		base.DataContext = RPCCustomizerViewModel.Shared;
	}
}
