using Colossal.Serialization.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Entities;

namespace RoadBuilder.Domain.Components
{
	public struct RoadBuilderNetwork : IComponentData, ISerializable
	{
		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{ }

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{ }
	}
}
