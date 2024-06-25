using Colossal.Json;
using Colossal.Serialization.Entities;

using Game.Debug.Tests;

using RoadBuilder.Domain.Enums;
using RoadBuilder.Systems;

using System;
using System.Collections.Generic;

namespace RoadBuilder.Domain.Configuration
{
	public class RoadConfig : ISerializable
	{
		public ushort Version;
		public string OriginalID;
		public string ID;
		public string Name;
		public float SpeedLimit;
		public bool GeneratesTrafficLights;
		public bool GeneratesZoningBlocks;
		public float MaxSlopeSteepness;
		public string AggregateType;
		public RoadCategory Category;
		public List<LaneConfig> Lanes = new();

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out Version);
			reader.Read(out ID);
			reader.Read(out Name);
			reader.Read(out SpeedLimit);
			reader.Read(out GeneratesTrafficLights);
			reader.Read(out GeneratesZoningBlocks);
			reader.Read(out MaxSlopeSteepness);
			reader.Read(out AggregateType);
			reader.Read(out int category);

			Category = (RoadCategory)category;

			reader.Read(out int laneCount);

			for (var i = 0; i < laneCount; i++)
			{
				var lane = new LaneConfig();

				reader.Read(lane);

				Lanes.Add(lane);
			}

			OriginalID = ID;
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(RoadBuilderSystem.CURRENT_VERSION);
			writer.Write(ID);
			writer.Write(Name);
			writer.Write(SpeedLimit);
			writer.Write(GeneratesTrafficLights);
			writer.Write(GeneratesZoningBlocks);
			writer.Write(MaxSlopeSteepness);
			writer.Write(AggregateType);
			writer.Write((int)Category);

			writer.Write(Lanes.Count);

            foreach (var lane in Lanes)
            {
				writer.Write(lane);
            }
        }

		public void ApplyVersionChanges()
		{

		}
	}
}
