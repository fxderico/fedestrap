using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Fedestrap.Enums;
using Fedestrap.UI.Elements.Base;

namespace Fedestrap.UI.Elements.Dialogs;

public partial class CursorPreviewDialog : WpfUiWindow{

	public Fedestrap.Enums.CursorType? SelectedCursor { get; private set; }

	public CursorPreviewDialog()
	{
		InitializeComponent();
		LoadCursorPreviews();
		base.Closed += OnClosed;
	}

	private void LoadCursorPreviews()
	{
		Fedestrap.Enums.CursorType[] array = new Fedestrap.Enums.CursorType[9]
		{
			Fedestrap.Enums.CursorType.Default,
			Fedestrap.Enums.CursorType.FPSCursor,
			Fedestrap.Enums.CursorType.CleanCursor,
			Fedestrap.Enums.CursorType.DotCursor,
			Fedestrap.Enums.CursorType.StoofsCursor,
			Fedestrap.Enums.CursorType.From2006,
			Fedestrap.Enums.CursorType.From2013,
			Fedestrap.Enums.CursorType.WhiteDotCursor,
			Fedestrap.Enums.CursorType.VerySmallWhiteDot
		};
		foreach (Fedestrap.Enums.CursorType cursor in array)
		{
			FrameworkElement element = CreateCursorPreviewItem(cursor);
			CursorStackPanel.Children.Add(element);
		}
	}

	private FrameworkElement CreateCursorPreviewItem(Fedestrap.Enums.CursorType cursor)
	{
		Border border = new Border
		{
			BorderBrush = new SolidColorBrush(Colors.Gray),
			BorderThickness = new Thickness(1.0),
			Margin = new Thickness(5.0),
			Padding = new Thickness(10.0),
			Background = new SolidColorBrush(Colors.Transparent),
			Cursor = Cursors.Hand
		};
		border.Tag = cursor;
		StackPanel stackPanel = new StackPanel
		{
			Orientation = Orientation.Horizontal
		};
		Image image = new Image
		{
			Width = 32.0,
			Height = 32.0,
			Margin = new Thickness(0.0, 0.0, 10.0, 0.0)
		};
		try
		{
			string cursorImagePath = GetCursorImagePath(cursor);
			if (!string.IsNullOrEmpty(cursorImagePath))
			{
				Uri uriSource = new Uri("pack://application:,,,/Resources/Mods/" + cursorImagePath);
				image.Source = Fedestrap.Utility.SafeImaging.FromUri(uriSource);
			}
		}
		catch
		{
			image.Source = null;
		}
		TextBlock element = new TextBlock
		{
			Text = GetCursorDisplayName(cursor),
			VerticalAlignment = VerticalAlignment.Center,
			FontSize = 14.0
		};
		stackPanel.Children.Add(image);
		stackPanel.Children.Add(element);
		border.Child = stackPanel;
		border.MouseLeftButtonUp += OnCursorClick;
		border.MouseEnter += OnCursorMouseEnter;
		border.MouseLeave += OnCursorMouseLeave;
		return border;
	}

	private void OnCursorClick(object sender, MouseButtonEventArgs e)
	{
		if (sender is not Border { Tag: Fedestrap.Enums.CursorType cursor })
			return;
		SelectedCursor = cursor;
		base.DialogResult = true;
		Close();
	}

	private static void OnCursorMouseEnter(object sender, MouseEventArgs e)
	{
		if (sender is Border border)
			border.Background = new SolidColorBrush(Color.FromArgb(50, 100, 149, 237));
	}

	private static void OnCursorMouseLeave(object sender, MouseEventArgs e)
	{
		if (sender is Border border)
			border.Background = new SolidColorBrush(Colors.Transparent);
	}

	private void OnClosed(object? sender, EventArgs e)
	{
		foreach (Border border in CursorStackPanel.Children.OfType<Border>())
		{
			border.MouseLeftButtonUp -= OnCursorClick;
			border.MouseEnter -= OnCursorMouseEnter;
			border.MouseLeave -= OnCursorMouseLeave;
		}
		base.Closed -= OnClosed;
	}

	private string GetCursorImagePath(Fedestrap.Enums.CursorType cursor)
	{
		return cursor switch
		{
			Fedestrap.Enums.CursorType.FPSCursor => "Cursor/FPSCursor/ArrowCursor.png", 
			Fedestrap.Enums.CursorType.CleanCursor => "Cursor/CleanCursor/ArrowCursor.png", 
			Fedestrap.Enums.CursorType.DotCursor => "Cursor/DotCursor/ArrowCursor.png", 
			Fedestrap.Enums.CursorType.StoofsCursor => "Cursor/StoofsCursor/ArrowCursor.png", 
			Fedestrap.Enums.CursorType.From2006 => "Cursor/From2006/ArrowCursor.png", 
			Fedestrap.Enums.CursorType.From2013 => "Cursor/From2013/ArrowCursor.png", 
			Fedestrap.Enums.CursorType.WhiteDotCursor => "Cursor/WhiteDotCursor/ArrowCursor.png", 
			Fedestrap.Enums.CursorType.VerySmallWhiteDot => "Cursor/VerySmallWhiteDot/ArrowCursor.png", 
			_ => string.Empty, 
		};
	}

	private string GetCursorDisplayName(Fedestrap.Enums.CursorType cursor)
	{
		return cursor switch
		{
			Fedestrap.Enums.CursorType.Default => "Default", 
			Fedestrap.Enums.CursorType.FPSCursor => "FPS Cursor (V1)", 
			Fedestrap.Enums.CursorType.CleanCursor => "Clean Cursor", 
			Fedestrap.Enums.CursorType.DotCursor => "Dot Cursor", 
			Fedestrap.Enums.CursorType.StoofsCursor => "Stoofs Cursor", 
			Fedestrap.Enums.CursorType.From2006 => "2006 Legacy Cursor", 
			Fedestrap.Enums.CursorType.From2013 => "2013 Legacy Cursor", 
			Fedestrap.Enums.CursorType.WhiteDotCursor => "White Dot Cursor", 
			Fedestrap.Enums.CursorType.VerySmallWhiteDot => "Very Small White Dot", 
			_ => cursor.ToString(), 
		};
	}

	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
		Close();
	}
}
