using Game.Prefabs;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Entities;

namespace RoadBuilder.Utilities
{
	public static class RoadConfigUtil
	{
		public static List<RoadConfig> Configurations { get; } = new();
		public static Queue<RoadBuilderPrefab> UpdatedRoadPrefabsQueue { get; } = new();
	}
}
