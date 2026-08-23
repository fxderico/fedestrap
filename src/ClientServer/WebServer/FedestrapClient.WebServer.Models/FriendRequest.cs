using FedestrapClient.WebServer.Enums;

namespace FedestrapClient.WebServer.Models;

internal class FriendRequest
{
	public int Inviter { get; set; }

	public int Invitee { get; set; }

	public FriendStatus? Status { get; set; }
}
