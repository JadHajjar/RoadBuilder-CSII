using Colossal.Serialization.Entities;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Domain.Components
{
	public struct RoadConfigComponent : IComponentData, ISerializable
	{
		private const ushort CURRENT_VERSION = 1;

		public FixedString4096Bytes ID;
		public FixedString4096Bytes Name;

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out ushort version); // Future-proof this component by adding a version to it
			reader.Read(out string id);
			reader.Read(out string name);

			ID = id;
			Name = name;
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(CURRENT_VERSION);

			writer.Write(ID.ToString());
			writer.Write(Name.ToString());
		}

		public struct Generated : IComponentData { }
	}
}
