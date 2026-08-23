using Fedestrap.Enums;
using Fedestrap.Resources;

namespace Fedestrap.Extensions;

internal static class ServerTypeEx
{
	public static string ToTranslatedString(this ServerType value)
	{
		return value switch
		{
			ServerType.Public => Strings.Enums_ServerType_Public, 
			ServerType.Private => Strings.Enums_ServerType_Private, 
			ServerType.Reserved => Strings.Enums_ServerType_Reserved, 
			_ => "No Server Type Detected?", 
		};
	}
}
