using Fedestrap.Platform;

namespace Fedestrap.Core;

public static class PlatformFeatureGate
{
	public static bool IsHidden(IPlatformCapabilities? capabilities, FeatureId feature)
	{
		if (capabilities is null || capabilities.Platform != PlatformId.Linux)
		{
			return false;
		}

		return capabilities.Get(feature).State == CapabilityState.Unavailable;
	}
}
