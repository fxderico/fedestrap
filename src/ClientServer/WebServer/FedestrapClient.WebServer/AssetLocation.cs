using System.Text.Json.Serialization;

namespace FedestrapClient.WebServer;

internal class AssetLocation
{
	[JsonPropertyName("location")]
	public string? Location { get; set; }
}
