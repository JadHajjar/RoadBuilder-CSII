﻿using Colossal.Serialization.Entities;

using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Systems;

using System;
using System.Collections.Generic;

namespace RoadBuilder.Domain.Configurations
{
	public class RoadConfig : INetworkConfig
	{
		public string Type { get; set; }
		public ushort Version { get; set; }
		public string OriginalID { get; set; }
		public string ID { get; set; }
		public string Name { get; set; }
		public string PillarPrefabName { get; set; }
		public float SpeedLimit { get; set; }
		public float MaxSlopeSteepness { get; set; }
		public RoadCategory Category { get; set; }
		public RoadAddons Addons { get; set; }
		public List<LaneConfig> Lanes { get; set; } = new();

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out string iD);
			reader.Read(out string name);

			if (Version < RoadBuilderSerializeSystem.VER_REMOVE_AGGREGATE_TYPE)
			{
				reader.Read(out string _);
			}

			reader.Read(out string pillarPrefabName);
			reader.Read(out float speedLimit);
			reader.Read(out float maxSlopeSteepness);
			reader.Read(out ulong category);
			reader.Read(out ulong addons);

			ID = iD;
			Name = name;
			PillarPrefabName = pillarPrefabName;
			SpeedLimit = speedLimit;
			MaxSlopeSteepness = maxSlopeSteepness;
			Category = (RoadCategory)category;
			Addons = (RoadAddons)addons;

			reader.Read(out int laneCount);

			for (var i = 0; i < laneCount; i++)
			{
				var lane = new LaneConfig { Version = Version };

				reader.Read(lane);

				Lanes.Add(lane);
			}

			OriginalID = ID;
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(ID);
			writer.Write(Name);
			writer.Write(PillarPrefabName ?? string.Empty);
			writer.Write(SpeedLimit);
			writer.Write(MaxSlopeSteepness);
			writer.Write((ulong)Category);
			writer.Write((ulong)Addons);

			writer.Write(Lanes.Count);

			foreach (var lane in Lanes)
			{
				writer.Write(lane);
			}
		}

		public void ApplyVersionChanges()
		{
			if (Version <= RoadBuilderSerializeSystem.VER_FIX_PEDESTRIAN_ROADS)
			{
				Addons |= RoadAddons.HasUndergroundElectricityCable;

				foreach (var lane in Lanes)
				{
					if (lane.SectionPrefabName is "Tiled Section 3")
					{
						lane.SectionPrefabName = "Tiled Pedestrian Section 3";
					}

					if (lane.SectionPrefabName is "Tiled Median Pedestrian 2" or "Tiled Median 2")
					{
						lane.SectionPrefabName = "RB Tiled Median 2";
					}
				}
			}
		}

		public Type GetPrefabType()
		{
			return typeof(RoadBuilderPrefab);
		}
	}
}
