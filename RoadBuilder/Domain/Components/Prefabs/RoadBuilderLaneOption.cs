using RoadBuilder.Domain.Enums;

using System;

namespace RoadBuilder.Domain.Components.Prefabs
{
	[Serializable]
	public class RoadBuilderLaneOption
	{
		public LaneOptionType Type;
		public string? Name;
		public string? DefaultValue;
		public bool IgnoreForSimilarDuplicate;
		public RoadBuilderLaneOptionValue[]? Options;
	}
}
