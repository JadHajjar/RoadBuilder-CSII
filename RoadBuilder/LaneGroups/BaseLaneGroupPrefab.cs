using Game.Prefabs;

using RoadBuilder.Domain.Prefabs;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public abstract class BaseLaneGroupPrefab : LaneGroupPrefab
	{
		public abstract void Initialize(Dictionary<string, NetSectionPrefab> sections);
	}
}
