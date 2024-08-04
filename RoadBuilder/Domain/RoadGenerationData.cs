using Game.Prefabs;

using RoadBuilder.Domain.Prefabs;

using System.Collections.Generic;

namespace RoadBuilder.Domain
{
	public class RoadGenerationData
	{
		public ZoneBlockPrefab ZoneBlockPrefab { get; set; }
		public ObjectPrefab OutsideConnectionOneWay { get; set; }
		public ObjectPrefab OutsideConnectionTwoWay { get; set; }
		public Dictionary<string, AggregateNetPrefab> AggregateNetPrefabs { get; set; } = new();
		public Dictionary<string, NetSectionPrefab> NetSectionPrefabs { get; set; } = new();
		public Dictionary<string, ServicePrefab> ServicePrefabs { get; set; } = new();
		public Dictionary<string, StaticObjectPrefab> PillarPrefabs { get; set; } = new();
		public Dictionary<string, UIGroupPrefab> UIGroupPrefabs { get; set; } = new();
		public Dictionary<string, LaneGroupPrefab> LaneGroupPrefabs { get; set; } = new();
		public bool LeftHandTraffic { get; set; }
	}
}
