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
		public RoadConfig Config { get; set; }
	}
}
