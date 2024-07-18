using Colossal.Serialization.Entities;

using System.Collections.Generic;

namespace RoadBuilder.Domain.Configurations
{
	public class LaneConfig : ISerializable
	{
		private const string NULL = "<NULL>";

		public uint Version { get; set; }
		public string SectionPrefabName { get; set; }
		public string GroupPrefabName { get; set; }
		public Dictionary<string, string> GroupOptions { get; set; } = new();
		public bool Invert { get; set; }

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out string sectionPrefabName);
			reader.Read(out string groupPrefabName);
			reader.Read(out bool invert);

			SectionPrefabName = sectionPrefabName;
			GroupPrefabName = groupPrefabName;
			Invert = invert;

			reader.Read(out ushort options);

			GroupOptions = new();

			for (ushort i = 0; i < options; i++)
			{
				reader.Read(out string key);
				reader.Read(out string value);

				GroupOptions[key] = value == NULL ? null : value;
			}
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(SectionPrefabName ?? string.Empty);
			writer.Write(GroupPrefabName ?? string.Empty);
			writer.Write(Invert);

			writer.Write((ushort)(GroupOptions?.Count ?? 0));

			foreach (var item in GroupOptions ?? new())
			{
				writer.Write(item.Key);
				writer.Write(item.Value ?? NULL);
			}
		}
	}
}
