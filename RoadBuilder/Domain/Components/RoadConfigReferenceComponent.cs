using Colossal.Serialization.Entities;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Domain.Components
{
	public struct RoadConfigReferenceComponent : IComponentData, ISerializable
	{
		private const ushort CURRENT_VERSION = 1;

		public FixedString4096Bytes ConfigurationID;

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out ushort version); // Future-proof this component by adding a version to it
			reader.Read(out string id);

			ConfigurationID = id;
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(CURRENT_VERSION);
			writer.Write(ConfigurationID.ToString());
		}
	}
}
