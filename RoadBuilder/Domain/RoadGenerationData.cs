using Game.Prefabs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadBuilder.Domain
{
	public class RoadGenerationData
	{
        public ZoneBlockPrefab ZoneBlockPrefab { get; set; }
        public Dictionary<string, AggregateNetPrefab> AggregateNetPrefabs { get; set; } = new();
    }
}
