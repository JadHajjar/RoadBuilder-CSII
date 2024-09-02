using Colossal.Serialization.Entities;

using RoadBuilder.Domain.Enums;

using System;
using System.Collections.Generic;

namespace RoadBuilder.Domain.Configurations
{
	public interface INetworkConfig : ISerializable
	{
		string ID { get; set; }
		string OriginalID { get; set; }
		string Type { get; set; }
		ushort Version { get; set; }
		string Name { get; set; }
		RoadCategory Category { get; set; }
		RoadAddons Addons { get; set; }
		float MaxSlopeSteepness { get; set; }
		List<LaneConfig> Lanes { get; set; }
		string PillarPrefabName { get; set; }
		ShowInToolbarState ToolbarState { get; set; }
		List<int> Playsets { get; set; }
		
		void ApplyVersionChanges();
		Type GetPrefabType();
	}
}