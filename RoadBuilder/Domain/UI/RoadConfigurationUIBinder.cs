using RoadBuilder.Domain.Enums;

namespace RoadBuilder.Domain.UI
{
	public class RoadConfigurationUIBinder
	{
		public string ID;
		public string Name;
		public string Thumbnail;
		public bool Locked;
		public bool Used;
		public RoadCategory Category;
		public string[] Tags;
	}
}
