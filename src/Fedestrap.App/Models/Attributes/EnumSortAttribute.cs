using System;

namespace Fedestrap.Models.Attributes;

internal class EnumSortAttribute : Attribute
{
	public int Order { get; set; }
}
