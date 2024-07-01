using Colossal.IO.AssetDatabase.Internal;
using Colossal.UI.Binding;

using Game.Prefabs;
using Game.UI;

using RoadBuilder.Domain.Configuration;
using RoadBuilder.Utilities;

using System.Collections.Generic;
using System.Linq;

using Unity.Entities;

namespace RoadBuilder.Domain.UI
{
	public class RoadLaneUIBinder : IJsonReadable
	{
		public string SectionPrefabName;
		public int Index;
		public bool Invert;
		public float Width;
		public NetSectionItem NetSection;
		public List<OptionSectionUIEntry> Options;

		public LaneConfig ToLaneConfig()
		{
			return new LaneConfig
			{
				SectionPrefabName = SectionPrefabName,
				Invert = Invert
			};
		}

		public void Read(IJsonReader reader)
		{
			reader.ReadMapBegin();

			if (reader.ReadProperty(nameof(SectionPrefabName)))
			{
				reader.Read(out SectionPrefabName);
			}

			if (reader.ReadProperty(nameof(Index)))
			{
				reader.Read(out Index);
			}

			reader.ReadMapEnd();
		}
	}
}
