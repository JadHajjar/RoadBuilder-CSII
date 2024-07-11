using Colossal.Serialization.Entities;

using RoadBuilder.Domain.Enums;

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
		float MaxSlopeSteepness { get; set; }
		string AggregateType { get; set; }
		List<LaneConfig> Lanes { get; set; }
		string PillarPrefabName { get; set; }
		bool HasUndergroundWaterPipes { get; set; }
		bool HasUndergroundElectricityCable { get; set; }
		bool RequiresUpgradeForElectricity { get; set; }

		void ApplyVersionChanges();
	}
}