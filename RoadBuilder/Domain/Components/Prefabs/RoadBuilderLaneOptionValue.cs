using Game.UI.Editor;
using Game.UI.Widgets;

using System;

namespace RoadBuilder.Domain.Components.Prefabs
{
	[Serializable]
	public class RoadBuilderLaneOptionValue
	{
		public string? Value;
		[CustomField(typeof(UIIconField))]
		public string? ThumbnailUrl;
	}
}
