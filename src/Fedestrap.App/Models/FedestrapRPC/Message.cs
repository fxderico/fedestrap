using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fedestrap.Models.FedestrapRPC;

public class Message
{
	[JsonPropertyName("command")]
	public string Command { get; set; }

	[JsonPropertyName("data")]
	public JsonElement Data { get; set; }
}
