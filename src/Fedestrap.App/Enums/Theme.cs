using Fedestrap.Models.Attributes;

namespace Fedestrap.Enums;

public enum Theme
{
	[EnumName(FromTranslation = "Common.SystemDefault")]
	Default,
	Dark,
	Light,
	Fedestrap,
	UltraGray,
	Berry,
	Blue,
	Cyan,
	Green,
	Orange,
	Pink,
	Purple,
	Red,
	Yellow,
	Custom
}
