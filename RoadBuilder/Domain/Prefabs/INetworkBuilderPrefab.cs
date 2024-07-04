using Game.Prefabs;

using RoadBuilder.Domain.Configuration;

namespace RoadBuilder.Domain.Prefabs
{
	public interface INetworkBuilderPrefab
	{
		INetworkConfig Config { get; set; }
		NetGeometryPrefab Prefab { get; }
	}
}
