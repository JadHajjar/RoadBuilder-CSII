using Colossal.Serialization.Entities;

namespace RoadBuilder.Domain.Configuration
{
	public class LaneConfig : ISerializable
	{
		public uint Version { get; set; }
		public string SectionPrefabName { get; set; }
		public bool Invert { get; set; }

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out string sectionPrefabName);
			reader.Read(out bool invert);

			SectionPrefabName = sectionPrefabName;
			Invert = invert;
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(SectionPrefabName);
			writer.Write(Invert);
		}
	}
}
