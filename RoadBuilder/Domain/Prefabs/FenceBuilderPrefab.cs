using Game.Prefabs;

using RoadBuilder.Domain.Configurations;

namespace RoadBuilder.Domain.Prefabs
{
	public class FenceBuilderPrefab : FencePrefab, INetworkBuilderPrefab
	{
		public FenceConfig Config { get; set; }
		NetGeometryPrefab INetworkBuilderPrefab.Prefab => this;
		INetworkConfig INetworkBuilderPrefab.Config { get => Config; set => Config = value as FenceConfig; }
	}
}
