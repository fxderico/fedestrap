using Fedestrap.Models.Attributes;

namespace Fedestrap.Enums.FlagPresets;

public enum Presents
{
	[EnumName(FromTranslation = "Common.Automatic")]
	Default,
	[EnumName(StaticName = "Stoofs FFlags")]
	Stoofs
}
