namespace RoadBuilder.Domain.Configuration
{
	public class RoadConfig
	{
		public string ID { get; set; }
		public string Name { get; set; }
		public LaneConfig[] Lanes { get; set; }
	}
}
