using Game.Prefabs;

using RoadBuilder.Domain.Configurations;

namespace RoadBuilder.Domain.Prefabs
{
	public class PathBuilderPrefab : FencePrefab, INetworkBuilderPrefab
	{
		public PathConfig Config { get; set; }
		NetGeometryPrefab INetworkBuilderPrefab.Prefab => this;
		INetworkConfig INetworkBuilderPrefab.Config { get => Config; set => Config = value as PathConfig; }

		public PathBuilderPrefab(PathConfig config)
		{
			Config = config;
			name = config.ID;

			m_Sections = new NetSectionInfo[0];
		}
	}
}
