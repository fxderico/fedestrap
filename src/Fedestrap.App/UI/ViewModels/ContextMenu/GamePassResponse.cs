using System.Collections.Generic;

namespace Fedestrap.UI.ViewModels.ContextMenu;

internal class GamePassResponse
{
	public List<GamePassData> GamePasses { get; set; } = new List<GamePassData>();
}
