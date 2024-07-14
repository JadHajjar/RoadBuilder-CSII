using System;

namespace RoadBuilder.Domain.Enums
{
	[Flags]
	public enum RoadAddons : uint
	{
		HasUndergroundWaterPipes = 1,
		HasUndergroundElectricityCable = 2,
		RequiresUpgradeForElectricity = 4,
		GrassLeft = 8,
		GrassRight = 16,
		GrassCenter = 32,
		TreesLeft = 64,
		TreesRight = 128,
		TreesCenter = 256,
	}
}
