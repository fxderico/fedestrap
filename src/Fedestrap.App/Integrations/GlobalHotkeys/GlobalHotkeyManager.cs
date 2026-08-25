using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Fedestrap.Integrations.GlobalHotkeys;

// Registers Win32 global hotkeys against Fedestrap's own main window. These
// only fire while Fedestrap itself is running with that window alive, they
// are not a way to change FastFlags live inside a running Roblox session:
// Roblox only reads ClientAppSettings.json at startup, so a bound hotkey
// applies a profile for the next launch, it doesn't hot swap flags mid game.
public static class GlobalHotkeyManager
{
    [Flags]
    public enum HotkeyModifiers : uint
    {
        None = 0,
        Alt = 0x0001,
        Control = 0x0002,
        Shift = 0x0004,
        Windows = 0x0008
    }

    private const int WM_HOTKEY = 0x0312;
    private const uint MOD_NOREPEAT = 0x4000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private static HwndSource? _source;
    private static IntPtr _hwnd = IntPtr.Zero;
    private static int _nextId = 1;
    private static readonly Dictionary<string, int> _idsByKey = new(StringComparer.Ordinal);
    private static readonly Dictionary<int, Action> _callbacksById = new();

    public static bool IsAvailable => _hwnd != IntPtr.Zero;

    public static void Initialize(Window window)
    {
        if (_hwnd != IntPtr.Zero)
            return;
        _hwnd = new WindowInteropHelper(window).Handle;
        if (_hwnd == IntPtr.Zero)
            return;
        _source = HwndSource.FromHwnd(_hwnd);
        _source?.AddHook(WndProc);
    }

    public static void Shutdown()
    {
        foreach (int id in _idsByKey.Values)
        {
            try { UnregisterHotKey(_hwnd, id); }
            catch { }
        }
        _idsByKey.Clear();
        _callbacksById.Clear();
        _source?.RemoveHook(WndProc);
        _source = null;
        _hwnd = IntPtr.Zero;
    }

    // registryKey identifies whatever this hotkey is bound to (e.g. a local
    // profile's file name), so re-binding it just replaces the old handler.
    public static bool TryRegister(string registryKey, HotkeyModifiers modifiers, Key key, Action onPressed)
    {
        if (_hwnd == IntPtr.Zero || string.IsNullOrEmpty(registryKey))
            return false;
        Unregister(registryKey);
        uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);
        if (vk == 0)
            return false;
        int id = _nextId++;
        if (!RegisterHotKey(_hwnd, id, (uint)modifiers | MOD_NOREPEAT, vk))
            return false;
        _idsByKey[registryKey] = id;
        _callbacksById[id] = onPressed;
        return true;
    }

    public static void Unregister(string registryKey)
    {
        if (_hwnd == IntPtr.Zero || !_idsByKey.TryGetValue(registryKey, out int id))
            return;
        try { UnregisterHotKey(_hwnd, id); }
        catch { }
        _idsByKey.Remove(registryKey);
        _callbacksById.Remove(id);
    }

    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && _callbacksById.TryGetValue(wParam.ToInt32(), out Action? callback))
        {
            handled = true;
            try { callback(); }
            catch (Exception ex) { App.Logger.WriteLine("GlobalHotkeyManager::WndProc", "Hotkey callback failed: " + ex.Message); }
        }
        return IntPtr.Zero;
    }

    // "Ctrl+Alt+F1" style formatting, shared by the capture dialog and
    // whatever needs to display or persist a binding as a plain string.
    public static string Format(HotkeyModifiers modifiers, Key key)
    {
        List<string> parts = new();
        if (modifiers.HasFlag(HotkeyModifiers.Control)) parts.Add("Ctrl");
        if (modifiers.HasFlag(HotkeyModifiers.Alt)) parts.Add("Alt");
        if (modifiers.HasFlag(HotkeyModifiers.Shift)) parts.Add("Shift");
        if (modifiers.HasFlag(HotkeyModifiers.Windows)) parts.Add("Win");
        parts.Add(key.ToString());
        return string.Join("+", parts);
    }

    public static bool TryParse(string text, out HotkeyModifiers modifiers, out Key key)
    {
        modifiers = HotkeyModifiers.None;
        key = Key.None;
        if (string.IsNullOrWhiteSpace(text))
            return false;
        string[] parts = text.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
            return false;
        string keyPart = parts[^1];
        for (int i = 0; i < parts.Length - 1; i++)
        {
            switch (parts[i].ToLowerInvariant())
            {
                case "ctrl": modifiers |= HotkeyModifiers.Control; break;
                case "alt": modifiers |= HotkeyModifiers.Alt; break;
                case "shift": modifiers |= HotkeyModifiers.Shift; break;
                case "win": modifiers |= HotkeyModifiers.Windows; break;
            }
        }
        return Enum.TryParse(keyPart, out key) && key != Key.None;
    }
}
