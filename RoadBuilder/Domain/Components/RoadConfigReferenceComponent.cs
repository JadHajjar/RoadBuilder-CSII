using Colossal.Serialization.Entities;

using RoadBuilder.Systems;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Domain.Components
{
	public struct RoadConfigReferenceComponent : IComponentData, ISerializable
	{
		public FixedString4096Bytes ConfigurationID;

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out ushort version); // Future-proof this component by adding a version to it
			reader.Read(out string id);

			ConfigurationID = id;
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(RoadBuilderSystem.CURRENT_VERSION);
			writer.Write(ConfigurationID.ToString());
		}
	}
}
