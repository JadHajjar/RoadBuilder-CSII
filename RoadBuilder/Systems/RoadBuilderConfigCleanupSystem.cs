using Game;
using Game.Common;

using RoadBuilder.Domain.Components;

using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderConfigCleanupSystem : GameSystemBase
	{
		private EntityQuery query;

		protected override void OnCreate()
		{
			base.OnCreate();

			query = SystemAPI.QueryBuilder().WithAll<NetworkConfigComponent>().Build();

			RequireForUpdate(query);
		}

		protected override void OnUpdate()
		{
			EntityManager.AddComponent<Deleted>(query);
		}
	}
}
