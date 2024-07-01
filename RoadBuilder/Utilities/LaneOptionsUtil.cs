using Game.Net;

using RoadBuilder.Domain.Configuration;
using RoadBuilder.Domain.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadBuilder.Utilities
{
	public static class LaneOptionsUtil
	{
		private enum ActionType
		{
			Invert,
		}

		public static List<OptionSectionUIEntry> GenerateOptions(LaneConfig lane)
		{
			var options=new List<OptionSectionUIEntry>();

			if (true) // if lane supports invert
				options.Add(GetInvertOption(lane));
		
			return options;
		}

		private static OptionSectionUIEntry GetInvertOption(LaneConfig lane)
		{
			return new OptionSectionUIEntry
			{
				Id = (int)ActionType.Invert,
				Name = "Direction",
				Options = new OptionItemUIEntry[]
				{
					new()
					{
						Name = "Backward",
						Icon = "coui://uil/Standard/ArrowDown.svg",
						Selected = lane.Invert,
						Id = 0,
					},
					new()
					{
						Name = "Forward",
						Icon = "coui://uil/Standard/ArrowUp.svg",
						Selected = !lane.Invert,
						Id = 1,
					},
				}
			};
		}

		public static void OptionClicked(RoadConfig config, LaneConfig lane, int optionId, int id, int value)
		{
			switch (optionId)
			{
				case (int)ActionType.Invert:
					lane.Invert = id == 0;
					break;
				default:
					break;
			}
		}
	}
}
