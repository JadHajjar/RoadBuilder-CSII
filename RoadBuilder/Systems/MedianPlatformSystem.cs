using Game;
using Game.Common;
using Game.Net;
using Game.Objects;
using Game.Routes;
using Game.Tools;
using Unity.Burst;
using Unity.Entities;

namespace RoadBuilder.Systems
{
    public partial class MedianPlatformSystem : GameSystemBase
    {
        private EntityQuery _busStopsQuery;

        protected override void OnCreate()
        {
            _busStopsQuery = SystemAPI.QueryBuilder()
                .WithAll<TransportStop, BusStop, Attached>()
                .WithAny<Updated, Created>()
                .WithNone<SpawnLocation, Deleted, Temp>()
                .Build();
            this.RequireAnyForUpdate(_busStopsQuery);
        }

        protected override void OnUpdate()
        {
            UpdateCompositionsJob job = new UpdateCompositionsJob()
            {
                compositionLookup = SystemAPI.GetComponentLookup<Composition>(false),
                edgeLookup = SystemAPI.GetComponentLookup<Edge>(true)
            };

            this.Dependency = job.ScheduleParallel(_busStopsQuery, this.Dependency);
        }

        [BurstCompile]
        public partial struct UpdateCompositionsJob : IJobEntity
        {
            public ComponentLookup<Game.Net.Composition> compositionLookup;
            public ComponentLookup<Created> createdLookup;
            public ComponentLookup<Edge> edgeLookup;

            public void Execute(Attached attached)
            {
                Entity roadEntity = attached.m_Parent;
                if (roadEntity != Entity.Null && compositionLookup.HasComponent(roadEntity) && edgeLookup.HasComponent(roadEntity))
                {
                    var composition = compositionLookup[roadEntity];
                    if (createdLookup.HasComponent(composition.m_Edge))
                    {
                        
                    }
                }
            }

        }
    }
}
