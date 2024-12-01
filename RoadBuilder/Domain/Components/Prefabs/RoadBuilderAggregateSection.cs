using Game.Prefabs;

using System;

namespace RoadBuilder.Domain.Components.Prefabs
{
	[Serializable]
	public class RoadBuilderAggregateSection
	{
		public NetSectionPrefab? Section;
		public bool Invert;
	}
}
