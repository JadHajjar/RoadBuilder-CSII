using RoadBuilder.Domain.Enums;

namespace RoadBuilder.Domain.UI
{
	public class NetSectionItem
	{
		public string? PrefabName;
		public string? DisplayName;
		public string? Thumbnail;
		public bool IsGroup;
		public bool IsEdge;
		public bool IsRestricted;
		public bool IsCustom;
		public float Width;
		public string? WidthText;
		public LaneSectionType SectionType;
	}
}
