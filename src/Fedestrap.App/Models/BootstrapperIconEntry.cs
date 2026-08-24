using System;
using System.Drawing;
using System.Windows.Media;
using Fedestrap.Enums;
using Fedestrap.Extensions;

namespace Fedestrap.Models;

public class BootstrapperIconEntry
{
	public BootstrapperIcon IconType { get; set; }

	// When true and IconType is IconCustom, the preview reads
	// RobloxIconCustomLocation instead of BootstrapperIconCustomLocation -
	// this entry is being shown in the Roblox window icon picker, not
	// Fedestrap's own bootstrapper icon picker.
	public bool UseRobloxCustomIcon { get; set; }

	public ImageSource ImageSource
	{
		get
		{
			if (IconType != BootstrapperIcon.IconCustom || !UseRobloxCustomIcon)
			{
				return IconType.GetIcon().GetImageSource();
			}

			Icon? icon = null;
			string location = App.Settings.Prop.RobloxIconCustomLocation;
			if (!string.IsNullOrEmpty(location))
			{
				try
				{
					icon = new Icon(location);
				}
				catch (Exception ex)
				{
					App.Logger.WriteLine("BootstrapperIconEntry::ImageSource", "Failed to load the Roblox custom icon: " + ex.Message);
				}
			}
			return (icon ?? Fedestrap.Properties.Resources.IconFedestrap).GetImageSource();
		}
	}
}
