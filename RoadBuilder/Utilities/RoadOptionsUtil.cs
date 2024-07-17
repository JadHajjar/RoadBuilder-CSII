using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.UI;

using System.Collections.Generic;

namespace RoadBuilder.Utilities
{
	public static class RoadOptionsUtil
	{
		private enum ActionType
		{
			SpeedLimit,
		}

		public static OptionSectionUIEntry[] GetRoadOptions(INetworkConfig config)
		{
			var options = new List<OptionSectionUIEntry>();

			if (config is RoadConfig roadConfig)
			{
				options.Add(new()
				{
					Id = (int)ActionType.SpeedLimit,
					Name = "Speed Limit",
					Options = new[]
					{
						new OptionItemUIEntry
						{
							IsValue = true,
							Value = (roadConfig.SpeedLimit / 2f).ToString("0.#")
						}
					}
				});
			}

			if (config is TrackConfig trackConfig)
			{
				options.Add(new()
				{
					Id = (int)ActionType.SpeedLimit,
					Name = "Speed Limit",
					Options = new[]
					{
						new OptionItemUIEntry
						{
							IsValue = true,
							Value = (trackConfig.SpeedLimit / 2f).ToString("0.#")
						}
					}
				});
			}

			return options.ToArray();
		}

		public static void OptionClicked(INetworkConfig config, int option, int id, int value)
		{
			switch ((ActionType)option)
			{
				case ActionType.SpeedLimit:
					if (config is RoadConfig roadConfig)
					{
						roadConfig.SpeedLimit += value * 20;
					}

					if (config is TrackConfig trackConfig)
					{
						trackConfig.SpeedLimit += value * 20;
					}

					break;
			}
		}
	}
}
