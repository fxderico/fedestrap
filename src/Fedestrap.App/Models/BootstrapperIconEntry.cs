using System.Windows.Media;
using Fedestrap.Enums;
using Fedestrap.Extensions;

namespace Fedestrap.Models;

public class BootstrapperIconEntry
{
	public BootstrapperIcon IconType { get; set; }

	public ImageSource ImageSource => IconType.GetIcon().GetImageSource();
}
