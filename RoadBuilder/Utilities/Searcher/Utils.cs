using Colossal.Entities;
using Colossal.Mathematics;

using RoadBuilder.Utilities.Searcher.QAccessor;

using System.Linq;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace RoadBuilder.Utilities.Searcher
{
	internal enum Filters
	{
		None = 0,
		Buildings = 1,
		Plants = 2,
		Props = 4,
		Decals = 8,
		AllStatics = 15, // All above
		Nodes = 16,
		Segments = 32,
		ControlPoints = 64,
		AllNets = 112, // All above minus AllStatics
		Surfaces = 128,
		All = 255, // All above
	}

	internal enum SearchTypes
	{
		Point,
		Marquee,
		Bounds,
	}

	internal static class Utils
	{
		internal static Circle2 GetCircle(EntityManager manager, Entity e, Game.Net.Node node)
		{
			if (manager.TryGetComponent(e, out Game.Net.NodeGeometry geoData))
			{
				return GetCircle(geoData);
			}

			return GetCircle(node);
		}

		internal static Circle2 GetCircle(Game.Net.NodeGeometry geoData)
		{
			var x = geoData.m_Bounds.max.x - geoData.m_Bounds.min.x;
			var z = geoData.m_Bounds.max.z - geoData.m_Bounds.min.z;
			var radius = math.max(6f, math.min(x, z)) / 2;
			return new(radius, geoData.m_Bounds.xz.Center());
		}

		internal static Circle2 GetCircle(Game.Net.Node node)
		{
			var radius = 3f;
			return new(radius, node.m_Position.XZ());
		}

		internal static Quad2 CalculateBuildingCorners(EntityManager manager, ref QObject obj, Entity prefab, float expand = 0f)
		{
			return CalculateBuildingCorners(manager, obj.m_Parent.Position.XZ(), obj.m_Parent.Rotation, prefab, expand);
		}

		internal static Quad2 CalculateBuildingCorners(EntityManager manager, ref QObjectSimple obj, Entity prefab, float expand = 0f)
		{
			return CalculateBuildingCorners(manager, obj.m_Parent.Position.XZ(), obj.m_Parent.Rotation, prefab, expand);
		}

		internal static Quad2 CalculateBuildingCorners(EntityManager manager, float2 position, quaternion rotation, Entity prefab, float expand = 0f)
		{
			var lotSize = manager.GetComponentData<Game.Prefabs.BuildingData>(prefab).m_LotSize;
			var offX = (lotSize.x * 4) + expand;
			var offZ = (lotSize.y * 4) + expand;

			Quad2 result = new(
				RotateAroundPivot(position, rotation, new(-offX, 0, -offZ)),
				RotateAroundPivot(position, rotation, new(offX, 0, -offZ)),
				RotateAroundPivot(position, rotation, new(offX, 0, offZ)),
				RotateAroundPivot(position, rotation, new(-offX, 0, offZ)));

			return result;
		}

		internal static float2 RotateAroundPivot(float2 position, quaternion q, float3 offset)
		{
			var newPos = math.mul(q, offset);
			return position + new float2(newPos.x, newPos.z);
		}

		internal static (Entity e, float d)[] CalculateDistances(EntityManager manager, ref QLookup lookup, NativeList<Entity> results, float3 center)
		{
			var data = new (Entity e, float d)[results.Length];
			for (var i = 0; i < results.Length; i++)
			{
				QObjectSimple obj = new(manager, ref lookup, results[i]);
				var distance = obj.m_Parent.Position.DistanceXZ(center);
				data[i] = (results[i], distance);
			}

			results.Dispose();
			var result = data.OrderBy(pair => pair.d).ToArray();
			//DebugDumpCalculateDistance(result);
			return result;
		}
	}
}
