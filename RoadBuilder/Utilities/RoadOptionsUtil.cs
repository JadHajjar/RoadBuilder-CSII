using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.UI;

using System.Collections.Generic;

namespace RoadBuilder.Utilities
{
	public static class RoadOptionsUtil
	{
		private enum ActionType
		{
			SpeedLimit,
			Addons,
			RoadCategory,
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

				options.Add(new()
				{
					Id = (int)ActionType.RoadCategory,
					Name = "Category",
					Options = new[]
					{
						new OptionItemUIEntry
						{
							Id = (int)RoadCategory.Highway,
							Name = RoadCategory.Highway.ToString(),
							Icon = "Media/Game/Icons/Trees.svg",
							Selected = config.Category.HasFlag(RoadCategory.Highway)
						},
						new OptionItemUIEntry
						{
							Id = (int)RoadCategory.PublicTransport,
							Name = RoadCategory.PublicTransport.ToString(),
							Icon = "Media/Game/Icons/Trees.svg",
							Selected = config.Category.HasFlag(RoadCategory.PublicTransport)
						},
						new OptionItemUIEntry
						{
							Id = (int)RoadCategory.Gravel,
							Name = RoadCategory.Gravel.ToString(),
							Icon = "Media/Game/Icons/Trees.svg",
							Selected = config.Category.HasFlag(RoadCategory.Gravel)
						},
						new OptionItemUIEntry
						{
							Id = (int)RoadCategory.Tiled,
							Name = RoadCategory.Tiled.ToString(),
							Icon = "Media/Game/Icons/Trees.svg",
							Selected = config.Category.HasFlag(RoadCategory.Tiled)
						},
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

				options.Add(new()
				{
					Id = (int)ActionType.RoadCategory,
					Name = "Category",
					Options = new[]
					{
						new OptionItemUIEntry
						{
							Id = (int)RoadCategory.Train,
							Name = RoadCategory.Train.ToString(),
							Icon = "Media/Game/Icons/Trees.svg",
							Selected = config.Category.HasFlag(RoadCategory.Train)
						},
						new OptionItemUIEntry
						{
							Id = (int)RoadCategory.Subway,
							Name = RoadCategory.Subway.ToString(),
							Icon = "Media/Game/Icons/Trees.svg",
							Selected = config.Category.HasFlag(RoadCategory.Subway)
						},
						new OptionItemUIEntry
						{
							Id = (int)RoadCategory.Gravel,
							Name = RoadCategory.Gravel.ToString(),
							Icon = "Media/Game/Icons/Trees.svg",
							Selected = config.Category.HasFlag(RoadCategory.Gravel)
						},
						new OptionItemUIEntry
						{
							Id = (int)RoadCategory.Tiled,
							Name = RoadCategory.Tiled.ToString(),
							Icon = "Media/Game/Icons/Trees.svg",
							Selected = config.Category.HasFlag(RoadCategory.Tiled)
						},
					}
				});
			}

			options.Add(new()
			{
				Id = (int)ActionType.Addons,
				Name = "Addons",
				Options = new[]
				{
					new OptionItemUIEntry
					{
						Id = -(int)RoadCategory.RaisedSidewalk,
						Name = RoadCategory.RaisedSidewalk.ToString(),
						Icon = "Media/Game/Icons/Trees.svg",
						Selected = config.Category.HasFlag(RoadCategory.RaisedSidewalk),
						Hidden = (config.Category & RoadCategory.NoRaisedSidewalkSupport) != 0
					},
					new OptionItemUIEntry
					{
						Id = (int)RoadAddons.GeneratesTrafficLights,
						Name = RoadAddons.GeneratesTrafficLights.ToString(),
						Icon = "Media/Game/Icons/Trees.svg",
						Selected = config.Addons.HasFlag(RoadAddons.GeneratesTrafficLights),
						Hidden = config is not RoadConfig
					},
					new OptionItemUIEntry
					{
						Id = (int)RoadAddons.GeneratesZoningBlocks,
						Name = RoadAddons.GeneratesZoningBlocks.ToString(),
						Icon = "Media/Game/Icons/Trees.svg",
						Selected = config.Addons.HasFlag(RoadAddons.GeneratesZoningBlocks),
						Hidden = config is not RoadConfig
					},
					new OptionItemUIEntry
					{
						Id = (int)RoadAddons.HasUndergroundWaterPipes,
						Name = RoadAddons.HasUndergroundWaterPipes.ToString(),
						Icon = "Media/Game/Icons/Trees.svg",
						Selected = config.Addons.HasFlag(RoadAddons.HasUndergroundWaterPipes)
					},
					new OptionItemUIEntry
					{
						Id = (int)RoadAddons.HasUndergroundElectricityCable,
						Name = RoadAddons.HasUndergroundElectricityCable.ToString(),
						Icon = "Media/Game/Icons/Trees.svg",
						Selected = config.Addons.HasFlag(RoadAddons.HasUndergroundElectricityCable)
					},
					new OptionItemUIEntry
					{
						Id = (int)RoadAddons.RequiresUpgradeForElectricity,
						Name = RoadAddons.RequiresUpgradeForElectricity.ToString(),
						Icon = "Media/Game/Icons/Trees.svg",
						Selected = config.Addons.HasFlag(RoadAddons.RequiresUpgradeForElectricity)
					}
				}
			});

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

				case ActionType.RoadCategory:
					var nonTypes = RoadCategory.RaisedSidewalk;

					config.Category = (RoadCategory)id | (config.Category & nonTypes);
					break;

				case ActionType.Addons:
					if (id < 0)
					{
						var category = (RoadCategory)(-id);

						if (config.Category.HasFlag(category))
						{
							config.Category &= ~category;
						}
						else
						{
							config.Category |= category;
						}
					}
					else
					{
						var addon = (RoadAddons)(id);

						if (config.Addons.HasFlag(addon))
						{
							config.Addons &= ~addon;
						}
						else
						{
							config.Addons |= addon;
						}
					}
					break;
			}
		}
	}
}
