using System.Collections.Generic;

namespace Fedestrap.Integrations.Animation;

public sealed class RigDefinition
{
	public string RootPart { get; init; } = "";

	public List<RigPart> Parts { get; } = [];

	public List<RigJoint> Joints { get; } = [];
}
