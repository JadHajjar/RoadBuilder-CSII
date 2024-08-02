using Colossal.Serialization.Entities;

using RoadBuilder.Systems;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Domain.Components
{
	public struct NetworkConfigComponent : IComponentData, ISerializable
	{
		public FixedString4096Bytes NetworkId;

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			RoadBuilderSerializeSystem.DeserializeNetwork(reader);
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			RoadBuilderSerializeSystem.SerializeNetwork(writer, NetworkId.ConvertToString());
		}
	}
}
