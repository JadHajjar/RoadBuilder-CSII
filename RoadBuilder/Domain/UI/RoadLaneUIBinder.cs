using Colossal.UI.Binding;

using RoadBuilder.Domain.Configuration;

using System.Linq;

namespace RoadBuilder.Domain.UI
{
	public class RoadLaneUIBinder : IJsonReadable
	{
		public string SectionPrefabName;
		public bool Invert;
		public float Width;
		public OptionSectionUIEntry[] Options;

		public static RoadLaneUIBinder[] From(RoadConfig config)
		{
			return config.Lanes.Select(x => new RoadLaneUIBinder
			{
				SectionPrefabName = x.SectionPrefabName,
				Invert = x.Invert,
				Width = x.SectionPrefabName.Contains("Median") ? 2F : 3.5F,
				Options = new OptionSectionUIEntry[]
				{
					new() {
						Id = 1,
						Name = "Direction",
						Options = new OptionItemUIEntry[]
						{
							new() {
								Name = "Backward",
								Selected = x.Invert,
								Id = 0,
							},
							new() {
								Name = "Forward",
								Selected = !x.Invert,
								Id = 1,
							},
						}
					}
				}
			}).ToArray();
		}

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

			reader.ReadMapEnd();
		}
	}
}
