using System.Collections.Generic;
using Fedestrap.Enums;
using Fedestrap.UI;

namespace Fedestrap.Extensions;

internal static class BootstrapperStyleEx
{
	public static IReadOnlyCollection<BootstrapperStyle> Selections { get; } = new BootstrapperStyle[9]
	{
		BootstrapperStyle.FluentDialog,
		BootstrapperStyle.FluentAeroDialog,
		BootstrapperStyle.ClassicFluentDialog,
		BootstrapperStyle.ByfronDialog,
		BootstrapperStyle.ProgressDialog,
		BootstrapperStyle.LegacyDialog2011,
		BootstrapperStyle.LegacyDialog2008,
		BootstrapperStyle.VistaDialog,
		BootstrapperStyle.CustomDialog
	};

	public static IBootstrapperDialog GetNew(this BootstrapperStyle bootstrapperStyle)
	{
		return Frontend.GetBootstrapperDialog(bootstrapperStyle);
	}
}
