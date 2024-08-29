using Game.Prefabs;

using RoadBuilder.Domain.Prefabs;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public abstract class BaseLaneGroupPrefab
	{
		public LaneGroupPrefab Prefab { get; set; }
		public Dictionary<string, NetSectionPrefab> Sections { get; set; }
		public abstract void Initialize();
	}
}
