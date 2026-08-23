using System.Collections.Generic;

namespace FedestrapClient.WebServer.Models;

internal class CharacterFetchData
{
	public List<ulong> Assets { get; set; } = new List<ulong>();

	public BodyColorData BodyColors { get; set; } = new BodyColorData();
}
