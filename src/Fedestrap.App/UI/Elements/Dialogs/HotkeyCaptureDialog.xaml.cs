using System.Windows;
using System.Windows.Input;
using Fedestrap.Integrations.GlobalHotkeys;
using Fedestrap.UI.Elements.Base;

namespace Fedestrap.UI.Elements.Dialogs;

public partial class HotkeyCaptureDialog : WpfUiWindow
{
	public string ProfileNameCaption { get; }

	// Null means "clear the binding". Non-null strings are already in the
	// "Ctrl+Alt+F1" format GlobalHotkeyManager understands.
	public string? ResultHotkey { get; private set; }

	public bool Cleared { get; private set; }

	private readonly System.Func<string, bool>? _isTakenByOtherProfile;

	private GlobalHotkeyManager.HotkeyModifiers _capturedModifiers;
	private Key _capturedKey = Key.None;

	public HotkeyCaptureDialog(string profileName, string? currentHotkey, System.Func<string, bool>? isTakenByOtherProfile = null)
	{
		ProfileNameCaption = "Hotkey for '" + profileName + "'";
		_isTakenByOtherProfile = isTakenByOtherProfile;
		InitializeComponent();
		ProfileNameText.Text = ProfileNameCaption;
		if (!string.IsNullOrEmpty(currentHotkey) && GlobalHotkeyManager.TryParse(currentHotkey, out GlobalHotkeyManager.HotkeyModifiers mods, out Key key))
		{
			_capturedModifiers = mods;
			_capturedKey = key;
			CapturedText.Text = currentHotkey;
			SaveButton.IsEnabled = true;
		}
		Loaded += (_, _) => CaptureBorder.Focus();
	}

	private void CaptureBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		CaptureBorder.Focus();
	}

	private void CaptureBorder_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		e.Handled = true;
		Key key = e.Key == Key.System ? e.SystemKey : e.Key;
		if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin or Key.None)
		{
			return;
		}

		GlobalHotkeyManager.HotkeyModifiers modifiers = GlobalHotkeyManager.HotkeyModifiers.None;
		if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) modifiers |= GlobalHotkeyManager.HotkeyModifiers.Control;
		if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) modifiers |= GlobalHotkeyManager.HotkeyModifiers.Alt;
		if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) modifiers |= GlobalHotkeyManager.HotkeyModifiers.Shift;
		if (Keyboard.Modifiers.HasFlag(ModifierKeys.Windows)) modifiers |= GlobalHotkeyManager.HotkeyModifiers.Windows;

		bool isFunctionKey = key >= Key.F1 && key <= Key.F24;
		if (modifiers == GlobalHotkeyManager.HotkeyModifiers.None && !isFunctionKey)
		{
			ConflictText.Text = "Bind at least one modifier (Ctrl/Alt/Shift/Win) unless you're using a function key, so this doesn't hijack normal typing.";
			ConflictText.Visibility = Visibility.Visible;
			SaveButton.IsEnabled = false;
			return;
		}

		_capturedModifiers = modifiers;
		_capturedKey = key;
		string formatted = GlobalHotkeyManager.Format(modifiers, key);
		CapturedText.Text = formatted;

		if (_isTakenByOtherProfile != null && _isTakenByOtherProfile(formatted))
		{
			ConflictText.Text = "'" + formatted + "' is already bound to another profile.";
			ConflictText.Visibility = Visibility.Visible;
			SaveButton.IsEnabled = false;
			return;
		}

		ConflictText.Visibility = Visibility.Collapsed;
		SaveButton.IsEnabled = true;
	}

	private void OnClearClicked(object sender, RoutedEventArgs e)
	{
		Cleared = true;
		ResultHotkey = null;
		DialogResult = true;
	}

	private void OnSaveClicked(object sender, RoutedEventArgs e)
	{
		if (_capturedKey == Key.None)
		{
			return;
		}
		ResultHotkey = GlobalHotkeyManager.Format(_capturedModifiers, _capturedKey);
		DialogResult = true;
	}
}
