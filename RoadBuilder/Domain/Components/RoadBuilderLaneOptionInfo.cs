using System;

namespace RoadBuilder.Domain.Components
{
	[Serializable]
	public class RoadBuilderLaneOptionInfo
	{
		public string Name;
		public bool IsValue;
		public bool IsDecoration;
		//public bool MultiSelection;
		public string DefaultValue;
		public RoadBuilderLaneOptionItemInfo[] Options;
	}
}
