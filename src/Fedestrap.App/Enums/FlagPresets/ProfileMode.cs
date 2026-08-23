using Fedestrap.Models.Attributes;

namespace Fedestrap.Enums.FlagPresets;

public enum ProfileMode
{
	[EnumName(FromTranslation = "Common.Automatic")]
	Default,
	[EnumName(StaticName = "Fedestraps Official")]
	Fedestrap,
	[EnumName(StaticName = "Stoofs")]
	Stoof
}
