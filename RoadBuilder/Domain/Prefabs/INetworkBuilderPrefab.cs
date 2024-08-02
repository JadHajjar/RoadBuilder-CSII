using Game.Prefabs;

using RoadBuilder.Domain.Configurations;

namespace RoadBuilder.Domain.Prefabs
{
	public interface INetworkBuilderPrefab
	{
		INetworkConfig Config { get; set; }
		NetGeometryPrefab Prefab { get; }
		bool Deleted { get; set; }
	}
}
