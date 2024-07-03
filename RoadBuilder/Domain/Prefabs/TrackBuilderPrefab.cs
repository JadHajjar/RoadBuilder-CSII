using Game.Prefabs;

using RoadBuilder.Domain.Configuration;
using RoadBuilder.Domain.Configurations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadBuilder.Domain.Prefabs
{
	public class TrackBuilderPrefab : TrackPrefab, INetworkBuilderPrefab
	{
		public bool WasGenerated { get; set; }
		public TrackConfig Config { get; set; }
		NetGeometryPrefab INetworkBuilderPrefab.Prefab => this;
		INetworkConfig INetworkBuilderPrefab.Config { get => Config; set => Config = value as TrackConfig; }

		public TrackBuilderPrefab(TrackConfig config)
		{
			Config = config;
			name = config.ID;

			m_Sections = new NetSectionInfo[0];
		}
	}
}
