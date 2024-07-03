using Game.Prefabs;

using RoadBuilder.Domain.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadBuilder.Domain.Prefabs
{
	public interface INetworkBuilderPrefab
	{
		bool WasGenerated { get; set; }
		INetworkConfig Config { get; set; }
		NetGeometryPrefab Prefab { get; }
	}
}
