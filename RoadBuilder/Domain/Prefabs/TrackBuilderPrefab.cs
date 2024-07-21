using Game.Prefabs;

using RoadBuilder.Domain.Configurations;

namespace RoadBuilder.Domain.Prefabs
{
	public class TrackBuilderPrefab : TrackPrefab, INetworkBuilderPrefab
	{
		public TrackConfig Config { get; set; }
		NetGeometryPrefab INetworkBuilderPrefab.Prefab => this;
		INetworkConfig INetworkBuilderPrefab.Config { get => Config; set => Config = value as TrackConfig; }
	}
}
