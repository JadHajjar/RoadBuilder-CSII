namespace RoadBuilder.Domain.API
{
	public class RoadBuilderEntryPost
	{
		public string? ID { get; set; }
		public string? Name { get; set; }
		public string? Tags { get; set; }
		public int Category { get; set; }
		public string? Icon { get; set; }
		public string? Config { get; set; }
	}
}