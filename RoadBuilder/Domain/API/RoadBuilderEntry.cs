using System;

namespace RoadBuilder.Domain.API
{
	public class RoadBuilderEntry
	{
		public string id { get; set; }
		public string name { get; set; }
		public string author { get; set; }
		public string tags { get; set; }
		public int category { get; set; }
		public int downloads { get; set; }
		public DateTime uploadTime { get; set; }
	}
}