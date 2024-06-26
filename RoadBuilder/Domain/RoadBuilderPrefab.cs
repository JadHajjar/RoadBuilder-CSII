using Game.Prefabs;

using RoadBuilder.Domain.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadBuilder.Domain
{
	public class RoadBuilderPrefab : RoadPrefab
	{
		public bool WasGenerated { get; set; }
		public RoadConfig Config { get; set; }

		public RoadBuilderPrefab(RoadConfig config)
		{
			Config = config;
		}
	}
}
