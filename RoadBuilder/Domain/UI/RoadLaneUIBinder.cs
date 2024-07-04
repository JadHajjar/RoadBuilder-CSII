using Colossal.UI.Binding;

using RoadBuilder.Domain.Configuration;

using System.Collections.Generic;

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
