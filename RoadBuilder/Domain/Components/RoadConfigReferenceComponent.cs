using Colossal.Serialization.Entities;

using RoadBuilder.Systems;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Domain.Components
{
	public struct RoadConfigBuffer : IBufferElementData, ISerializable
	{
		public FixedString4096Bytes ConfigurationID;

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out string id);

			ConfigurationID = id;

			RoadBuilderSerializeSystem.RegisterRoadID(id);
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(ConfigurationID.ToString());
		}
	}
}
