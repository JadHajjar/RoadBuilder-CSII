using System;

namespace RoadBuilder.Domain.Enums
{
	[Flags]
	public enum RoadAddons : ulong
	{
		HasUndergroundWaterPipes = 1,
		HasUndergroundElectricityCable = 2,
		RequiresUpgradeForElectricity = 4,
		GeneratesTrafficLights = 8,
		GeneratesZoningBlocks = 16,
		GrassLeft = 32,
		GrassRight = 64,
		GrassCenter = 128,
		TreesLeft = 256,
		TreesRight = 512,
		TreesCenter = 1024,
	}
}
