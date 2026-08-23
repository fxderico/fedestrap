using System.Text.Json.Serialization;

namespace FedestrapClient.WebServer.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
internal enum CharacterLoadType
{
	Fetch,
	Whole
}
