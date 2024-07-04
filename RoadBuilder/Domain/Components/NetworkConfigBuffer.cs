using Colossal.Serialization.Entities;

using RoadBuilder.Systems;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Domain.Components
{
	public struct NetworkConfigBuffer : IBufferElementData, ISerializable
	{
		public FixedString128Bytes ConfigurationType;
		public FixedString4096Bytes ConfigurationID;

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out string type);
			reader.Read(out string id);

			ConfigurationType = type;
			ConfigurationID = id;

			RoadBuilderSerializeSystem.RegisterRoadID(type, id);
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			writer.Write(ConfigurationType.ToString());
			writer.Write(ConfigurationID.ToString());
		}
	}
}
