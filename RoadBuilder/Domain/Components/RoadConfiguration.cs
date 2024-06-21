using Colossal.Serialization.Entities;

using System;

using Unity.Entities;

namespace RoadBuilder.Domain.Components
{
	public struct RoadConfiguration : IComponentData, ISerializable
	{
		public string ID { get; set; }
		public string Name { get; set; }

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			throw new NotImplementedException();
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			throw new NotImplementedException();
		}
	}
}
