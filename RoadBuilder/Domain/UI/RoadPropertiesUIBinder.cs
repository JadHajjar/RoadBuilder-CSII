using RoadBuilder.Domain.Configuration;
using RoadBuilder.Domain.Enums;

namespace RoadBuilder.Domain.UI
{
	public class RoadPropertiesUIBinder
	{
		public string Name;
		public float SpeedLimit;
		public bool GeneratesTrafficLights;
		public bool GeneratesZoningBlocks;
		public float MaxSlopeSteepness;
		public string AggregateType;
		public RoadCategory Category;

		public static RoadPropertiesUIBinder From(RoadConfig config)
		{
			return new RoadPropertiesUIBinder
			{
				Name = config.Name,
				Category = config.Category,
				AggregateType = config.AggregateType,
				SpeedLimit = config.SpeedLimit,
				GeneratesTrafficLights = config.GeneratesTrafficLights,
				GeneratesZoningBlocks = config.GeneratesZoningBlocks,
				MaxSlopeSteepness = config.MaxSlopeSteepness,
			};
		}

		public void Fill(RoadConfig config)
		{
			config.Name = Name;
			config.Category = Category;
			config.AggregateType = AggregateType;
			config.SpeedLimit = SpeedLimit;
			config.GeneratesTrafficLights = GeneratesTrafficLights;
			config.GeneratesZoningBlocks = GeneratesZoningBlocks;
			config.MaxSlopeSteepness = MaxSlopeSteepness;
		}
	}
}
