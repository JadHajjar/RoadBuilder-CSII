using System;

namespace RoadBuilder.Domain.Components.Prefabs
{
	[Serializable]
    public class RoadBuilderLaneOption
    {
        public LaneOptionType Type;
        public string Name;
		public string DefaultValue;
        public RoadBuilderLaneOptionValue[] Options;
	}
}
