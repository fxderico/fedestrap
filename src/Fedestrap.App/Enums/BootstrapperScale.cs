using Fedestrap.Models.Attributes;

namespace Fedestrap.Enums;

public enum BootstrapperScale
{
	[EnumName(StaticName = "Compact")]
	Compact,
	[EnumName(StaticName = "Normal")]
	Normal,
	[EnumName(StaticName = "Large")]
	Large
}
