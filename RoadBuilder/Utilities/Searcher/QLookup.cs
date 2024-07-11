using Unity.Entities;

namespace RoadBuilder.Utilities.Searcher
{
	public struct QLookup
	{
		internal BufferLookup<Game.Areas.Node> gaNode;
		//internal BufferLookup<Game.Net.ConnectedEdge> gnConnectedEdge;
		internal ComponentLookup<Game.Areas.Geometry> gaGeometry;
		internal ComponentLookup<Game.Net.Aggregated> gnAggregated;
		internal ComponentLookup<Game.Net.Curve> gnCurve;
		internal ComponentLookup<Game.Net.Edge> gnEdge;
		//internal ComponentLookup<Game.Net.EdgeGeometry> gnEdgeGeometry;
		//internal ComponentLookup<Game.Net.EndNodeGeometry> gnEndNodeGeometry;
		internal ComponentLookup<Game.Net.Elevation> gnElevation;
		internal ComponentLookup<Game.Net.Node> gnNode;
		internal ComponentLookup<Game.Net.NodeGeometry> gnNodeGeometry;
		//internal ComponentLookup<Game.Net.StartNodeGeometry> gnStartNodeGeometry;
		internal ComponentLookup<Game.Objects.Elevation> goElevation;
		internal ComponentLookup<Game.Objects.Transform> goTransform;
		//internal ComponentLookup<Game.Prefabs.ObjectGeometryData> gpObjectGeometryData;
		internal ComponentLookup<Game.Rendering.CullingInfo> grCullingInfo;
	}

	public static class QLookupFactory
	{
		private static SystemBase _System;
		private static QLookup _Lookup;

		public static void Init(SystemBase system)
		{
			_System = system;
			_Lookup = new()
			{
				gaNode = _System.GetBufferLookup<Game.Areas.Node>(),
				gaGeometry = _System.GetComponentLookup<Game.Areas.Geometry>(),
				gnAggregated = _System.GetComponentLookup<Game.Net.Aggregated>(),
				gnCurve = _System.GetComponentLookup<Game.Net.Curve>(),
				gnEdge = _System.GetComponentLookup<Game.Net.Edge>(),
				gnElevation = _System.GetComponentLookup<Game.Net.Elevation>(),
				gnNode = _System.GetComponentLookup<Game.Net.Node>(),
				gnNodeGeometry = _System.GetComponentLookup<Game.Net.NodeGeometry>(),
				goElevation = _System.GetComponentLookup<Game.Objects.Elevation>(),
				goTransform = _System.GetComponentLookup<Game.Objects.Transform>(),
				grCullingInfo = _System.GetComponentLookup<Game.Rendering.CullingInfo>(),
			};
		}

		public static void UpdateAll()
		{
			_Lookup.gaNode.Update(_System);
			_Lookup.gaGeometry.Update(_System);
			_Lookup.gnAggregated.Update(_System);
			_Lookup.gnCurve.Update(_System);
			_Lookup.gnEdge.Update(_System);
			_Lookup.gnElevation.Update(_System);
			_Lookup.gnNode.Update(_System);
			_Lookup.gnNodeGeometry.Update(_System);
			_Lookup.goElevation.Update(_System);
			_Lookup.goTransform.Update(_System);
			_Lookup.grCullingInfo.Update(_System);
		}

		public static ref QLookup Get() => ref _Lookup;
	}
}
