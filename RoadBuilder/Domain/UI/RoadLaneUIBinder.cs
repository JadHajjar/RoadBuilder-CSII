using Colossal.UI.Binding;

using RoadBuilder.Domain.Configurations;

using System.Collections.Generic;

namespace RoadBuilder.Domain.UI
{
	public class RoadLaneUIBinder : IJsonReadable
	{
		public string? SectionPrefabName;
		public bool IsGroup;
		public int Index;
		public bool NoDirection;
		public bool Invert;
		public bool InvertImage;
		public bool IsEdgePlaceholder;
		public bool TwoWay;
		public string? Color;
		public string? Texture;
		public NetSectionItem? NetSection;
		public List<OptionSectionUIEntry>? Options;

		public LaneConfig ToLaneConfig()
		{
			return new LaneConfig
			{
				SectionPrefabName = IsGroup ? null : SectionPrefabName,
				GroupPrefabName = IsGroup ? SectionPrefabName : null,
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

			if (reader.ReadProperty(nameof(IsGroup)))
			{
				reader.Read(out IsGroup);
			}

			if (reader.ReadProperty(nameof(Index)))
			{
				reader.Read(out Index);
			}

			reader.ReadMapEnd();
		}
	}
}
