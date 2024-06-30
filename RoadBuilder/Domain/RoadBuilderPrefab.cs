using Game.Prefabs;

using RoadBuilder.Domain.Configuration;

namespace RoadBuilder.Domain
{
	public class RoadBuilderPrefab : RoadPrefab
	{
		public bool WasGenerated { get; set; }
		public RoadConfig Config { get; set; }

		public RoadBuilderPrefab(RoadConfig config)
		{
			Config = config;
			name = config.ID;

			m_Sections = new NetSectionInfo[0];
		}
	}
}
