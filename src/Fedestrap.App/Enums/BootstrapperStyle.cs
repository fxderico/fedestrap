using Fedestrap.Models.Attributes;

namespace Fedestrap.Enums;

public enum BootstrapperStyle
{
	VistaDialog,
	LegacyDialog2008,
	LegacyDialog2011,
	ProgressDialog,
	ClassicFluentDialog,
	ByfronDialog,
	[EnumName(StaticName = "Fedestrap")]
	FluentDialog,
	FluentAeroDialog,
	CustomDialog
}
