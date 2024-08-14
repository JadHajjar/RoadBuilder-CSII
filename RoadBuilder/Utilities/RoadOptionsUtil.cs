using Game.SceneFlow;

using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.UI;

using System.Collections.Generic;

using UnityEngine;

using static Game.Settings.InterfaceSettings;

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
							Value = $"{(int)(roadConfig.SpeedLimit / (IsMetric() ? 2f : 3.218688f) / 5) * 5} {(IsMetric() ? "km/h" : "mph")}"
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
							Name = $"RoadBuilder.RoadCategory[{RoadCategory.Highway}]",
							Icon = "coui://roadbuildericons/RB_HighwayWhite.svg",
							Selected = config.Category.HasFlag(RoadCategory.Highway)
						},
						new OptionItemUIEntry
						{
							Id = (int)RoadCategory.PublicTransport,
							Name = $"RoadBuilder.RoadCategory[{RoadCategory.PublicTransport}]",
							Icon = "coui://roadbuildericons/RB_BusWhite.svg",
							Selected = config.Category.HasFlag(RoadCategory.PublicTransport)
						},
						new OptionItemUIEntry
						{
							Id = (int)RoadCategory.Gravel,
							Name = $"RoadBuilder.RoadCategory[{RoadCategory.Gravel}]",
							Icon = "coui://roadbuildericons/RB_GravelWhite.svg",
							Selected = config.Category.HasFlag(RoadCategory.Gravel)
						},
						new OptionItemUIEntry
						{
							Id = (int)RoadCategory.Tiled,
							Name = $"RoadBuilder.RoadCategory[{RoadCategory.Tiled}]",
							Icon = "coui://roadbuildericons/RB_TiledWhite.svg",
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
							Value =  $"{(int)(trackConfig.SpeedLimit / (IsMetric() ? 2f : 3.218688f) / 5) * 5} {(IsMetric() ? "km/h" : "mph")}"
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
							Id = (int)RoadCategory.Tram,
							Name = $"RoadBuilder.RoadCategory[{RoadCategory.Tram}]",
							Icon = "coui://roadbuildericons/RB_TramWhite.svg",
							Selected = config.Category.HasFlag(RoadCategory.Tram)
						},
						new OptionItemUIEntry
						{
							Id = (int)RoadCategory.Train,
							Name = $"RoadBuilder.RoadCategory[{RoadCategory.Train}]",
							Icon = "coui://roadbuildericons/RB_TrainWhite.svg",
							Selected = config.Category.HasFlag(RoadCategory.Train)
						},
						new OptionItemUIEntry
						{
							Id = (int)RoadCategory.Subway,
							Name = $"RoadBuilder.RoadCategory[{RoadCategory.Subway}]",
							Icon = "coui://roadbuildericons/RB_SubwayWhite.svg",
							Selected = config.Category.HasFlag(RoadCategory.Subway)
						},
						new OptionItemUIEntry
						{
							Id = (int)RoadCategory.Gravel,
							Name = $"RoadBuilder.RoadCategory[{RoadCategory.Gravel}]",
							Icon = "coui://roadbuildericons/RB_GravelWhite.svg",
							Selected = config.Category.HasFlag(RoadCategory.Gravel)
						},
						new OptionItemUIEntry
						{
							Id = (int)RoadCategory.Tiled,
							Name = $"RoadBuilder.RoadCategory[{RoadCategory.Tiled}]",
							Icon = "coui://roadbuildericons/RB_TiledWhite.svg",
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
						Name = $"RoadBuilder.RoadAddon[{RoadCategory.RaisedSidewalk}]",
						Icon = "coui://roadbuildericons/RB_RaisedSidewalks.svg",
						Selected = config.Category.HasFlag(RoadCategory.RaisedSidewalk),
						Hidden = (config.Category & RoadCategory.NoRaisedSidewalkSupport) != 0
					},
					new OptionItemUIEntry
					{
						Id = (int)RoadAddons.GeneratesTrafficLights,
						Name = $"RoadBuilder.RoadAddon[{RoadAddons.GeneratesTrafficLights}]",
						Icon = "coui://roadbuildericons/RB_TrafficLightsWhite.svg",
						Selected = config.Addons.HasFlag(RoadAddons.GeneratesTrafficLights),
						Hidden = config is not RoadConfig
					},
					new OptionItemUIEntry
					{
						Id = (int)RoadAddons.GeneratesZoningBlocks,
						Name = $"RoadBuilder.RoadAddon[{RoadAddons.GeneratesZoningBlocks}]",
						Icon = "coui://roadbuildericons/RB_ZoneBlocks.svg",
						Selected = config.Addons.HasFlag(RoadAddons.GeneratesZoningBlocks),
						Hidden = config is not RoadConfig
					},
					new OptionItemUIEntry
					{
						Id = (int)RoadAddons.HasUndergroundWaterPipes,
						Name = $"RoadBuilder.RoadAddon[{RoadAddons.HasUndergroundWaterPipes}]",
						Icon = "coui://roadbuildericons/RB_PipesWhite.svg",
						Selected = config.Addons.HasFlag(RoadAddons.HasUndergroundWaterPipes)
					},
					//new OptionItemUIEntry
					//{
					//	Id = (int)RoadAddons.HasUndergroundElectricityCable,
					//	Name = $"RoadBuilder.RoadAddon[{RoadAddons.HasUndergroundElectricityCable}]",
					//	Icon = "coui://roadbuildericons/RB_UndergroundElectricityWhite.svg",
					//	Selected = config.Addons.HasFlag(RoadAddons.HasUndergroundElectricityCable)
					//},
					new OptionItemUIEntry
					{
						Id = (int)RoadAddons.RequiresUpgradeForElectricity,
						Name = $"RoadBuilder.RoadAddon[{RoadAddons.RequiresUpgradeForElectricity}]",
						Icon = "coui://roadbuildericons/RB_UndergroundElectricityWhite.svg",
						//Disabled = !config.Addons.HasFlag(RoadAddons.HasUndergroundElectricityCable),
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
					var multiplier = 5 * (IsMetric() ? 2f : 3.218688f);

					if (config is RoadConfig roadConfig)
					{
						roadConfig.SpeedLimit = Mathf.Max(roadConfig.SpeedLimit + value * multiplier, multiplier);
					}

					if (config is TrackConfig trackConfig)
					{
						trackConfig.SpeedLimit = Mathf.Max(trackConfig.SpeedLimit + value * multiplier, multiplier);
					}

					break;

				case ActionType.RoadCategory:
					var nonTypes = RoadCategory.RaisedSidewalk;

					if (config.Category.HasFlag((RoadCategory)id))
					{
						config.Category &= nonTypes;
					}
					else
					{
						config.Category = (RoadCategory)id | (config.Category & nonTypes);
					}

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
						var addon = (RoadAddons)id;

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

		private static bool IsMetric()
		{
			return GameManager.instance?.settings?.userInterface?.unitSystem is null or UnitSystem.Metric;
		}
	}
}
