using System;

namespace RoadBuilder.Domain.API
{
	public class RoadBuilderEntry
	{
		public string ID { get; set; }
		public string Name { get; set; }
		public string Author { get; set; }
		public string Tags { get; set; }
		public int Category { get; set; }
		public int Downloads { get; set; }
		public DateTime UploadTime { get; set; }
	}
}