using FedestrapClient.Common;

namespace FedestrapClient.WebServer;

public class Utils
{
	public static string GetMapsDirectory()
	{
		return (!string.IsNullOrWhiteSpace(Config.Instance.User.Launch.CustomMapsDirectory)) ? Config.Instance.User.Launch.CustomMapsDirectory : PathHelper.Maps;
	}
}
