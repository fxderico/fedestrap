using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Fedestrap.Integrations.GlobalHotkeys;

namespace Fedestrap.Utility;

// Wires AppSettings.LocalProfileKeybinds up to GlobalHotkeyManager: registers
// a hotkey per bound local profile, and applies that profile's flags (merged
// into the current flag set, not replacing it) when the hotkey fires.
public static class LocalProfileHotkeys
{
    public static void RegisterAll()
    {
        if (!GlobalHotkeyManager.IsAvailable)
            return;
        foreach (KeyValuePair<string, string> binding in App.Settings.Prop.LocalProfileKeybinds.ToList())
        {
            RegisterOne(binding.Key, binding.Value);
        }
    }

    public static void RegisterOne(string fileName, string hotkeyText)
    {
        if (!GlobalHotkeyManager.IsAvailable || string.IsNullOrEmpty(fileName))
            return;
        if (!GlobalHotkeyManager.TryParse(hotkeyText, out GlobalHotkeyManager.HotkeyModifiers modifiers, out System.Windows.Input.Key key))
        {
            App.Logger.WriteLine("LocalProfileHotkeys::RegisterOne", "Could not parse hotkey '" + hotkeyText + "' for " + fileName);
            return;
        }
        GlobalHotkeyManager.TryRegister(fileName, modifiers, key, () => ApplyProfile(fileName));
    }

    public static void Unregister(string fileName)
    {
        GlobalHotkeyManager.Unregister(fileName);
    }

    private static void ApplyProfile(string fileName)
    {
        try
        {
            string path = Path.Combine(Paths.SavedBackups, fileName);
            Dictionary<string, object>? raw = JsonFile.Deserialize<Dictionary<string, object>>(path, JsonOptions.Tolerant);
            if (raw == null || raw.Count == 0)
            {
                App.Logger.WriteLine("LocalProfileHotkeys::ApplyProfile", "Profile '" + fileName + "' is missing or empty");
                return;
            }
            int applied = 0;
            foreach (KeyValuePair<string, object> flag in raw)
            {
                if (string.IsNullOrWhiteSpace(flag.Key) || flag.Value == null)
                    continue;
                App.FastFlags.SetValue(flag.Key, flag.Value.ToString());
                applied++;
            }
            App.FastFlags.Save();
            string name = Path.GetFileNameWithoutExtension(fileName);
            App.Logger.WriteLine("LocalProfileHotkeys::ApplyProfile", $"Applied {applied} flag(s) from profile '{name}' via hotkey");
            NotifyApplied(name, applied);
        }
        catch (Exception ex)
        {
            App.Logger.WriteException("LocalProfileHotkeys::ApplyProfile", ex);
        }
    }

    private static void NotifyApplied(string profileName, int flagCount)
    {
        try
        {
            Application? current = Application.Current;
            if (current == null)
                return;
            current.Dispatcher.BeginInvoke(new Action(delegate
            {
                foreach (Window window in current.Windows)
                {
                    if (window is Fedestrap.UI.Elements.Settings.MainWindow mainWindow)
                    {
                        mainWindow.ShowProfileHotkeyAppliedSnackbar(profileName, flagCount);
                        break;
                    }
                }
            }));
        }
        catch
        {
        }
    }
}
