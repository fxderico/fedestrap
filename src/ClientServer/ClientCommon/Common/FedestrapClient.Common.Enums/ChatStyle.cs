using System.ComponentModel;
using System.Text.Json.Serialization;

namespace FedestrapClient.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatStyle
{
	Classic,
	Bubble,
	[Description("Classic and Bubble")]
	ClassicAndBubble
}
