using System.ComponentModel;

namespace FedestrapClient.Common.Enums;

public enum CustomGraphicsApi
{
	None,
	[Description("DXVK (Vulkan)")]
	DXVK,
	[Description("dgVoodoo (DX11/12)")]
	DgVoodoo
}
