using Game.Prefabs;

using RoadBuilder.Domain.Configuration;
using RoadBuilder.Domain.Configurations;

namespace RoadBuilder.Domain.Prefabs
{
	public class FenceBuilderPrefab : FencePrefab, INetworkBuilderPrefab
	{
		public FenceConfig Config { get; set; }
		NetGeometryPrefab INetworkBuilderPrefab.Prefab => this;
		INetworkConfig INetworkBuilderPrefab.Config { get => Config; set => Config = value as FenceConfig; }

		public FenceBuilderPrefab(FenceConfig config)
		{
			Config = config;
			name = config.ID;

			m_Sections = new NetSectionInfo[0];
		}
	}
}
