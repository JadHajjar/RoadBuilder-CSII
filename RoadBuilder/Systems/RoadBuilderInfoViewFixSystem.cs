using Game;
using Game.Prefabs;

using RoadBuilder.Domain.Components;

using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderInfoViewFixSystem : GameSystemBase
	{
		private EntityQuery query;

		protected override void OnCreate()
		{
			base.OnCreate();

			query = SystemAPI.QueryBuilder().WithAll<RoadBuilderPrefabData, PlaceableInfoviewItem>().Build();

			RequireForUpdate(query);
		}

		protected override void OnUpdate()
		{
			EntityManager.RemoveComponent<PlaceableInfoviewItem>(query);
		}
	}
}
