﻿using Colossal.Json;

using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Domain.UI;
using RoadBuilder.Utilities;
using RoadBuilder.Utilities.Online;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Unity.Entities;

namespace RoadBuilder.Systems.UI
{
	public partial class RoadBuilderRoadManagementUISystem : ExtendedUISystemBase
	{
		private enum ActionType
		{
			ShowInToolbar,
			TogglePlayset,
			UploadRoad
		}

		private readonly CancellationTokenSource _cancellationTokenSource = new();
		private string query;
		private RoadCategory? currentCategory;
		private int sorting;
		private RoadBuilderSystem roadBuilderSystem;
		private RoadBuilderRoadTrackerSystem roadBuilderRoadTrackerSystem;
		private RoadBuilderConfigurationsUISystem roadBuilderConfigurationsUISystem;
		private ValueBindingHelper<bool> RestrictPlayset;
		private ValueBindingHelper<bool> Loading;
		private ValueBindingHelper<int> CurrentPage;
		private ValueBindingHelper<int> MaxPages;
		private ValueBindingHelper<RoadConfigurationUIBinder[]> Items;
		private ValueBindingHelper<OptionSectionUIEntry[]> SelectedRoadOptions;
		private INetworkBuilderPrefab SelectedRoad;

		protected override void OnCreate()
		{
			base.OnCreate();

			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			roadBuilderRoadTrackerSystem = World.GetOrCreateSystemManaged<RoadBuilderRoadTrackerSystem>();
			roadBuilderConfigurationsUISystem = World.GetOrCreateSystemManaged<RoadBuilderConfigurationsUISystem>();

			RestrictPlayset = CreateBinding("Management.RestrictPlayset", true);
			Loading = CreateBinding("Discover.Loading", true);
			CurrentPage = CreateBinding("Discover.CurrentPage", 1);
			MaxPages = CreateBinding("Discover.MaxPages", 1);
			Items = CreateBinding("Discover.Items", new RoadConfigurationUIBinder[0]);
			SelectedRoadOptions = CreateBinding("Management.GetRoadOptions", new OptionSectionUIEntry[0]);

			CreateTrigger<int>("Discover.SetPage", SetDiscoverPage);
			CreateTrigger<int>("Discover.SetSorting", SetDiscoverSorting);
			CreateTrigger<string>("Discover.Download", DownloadConfig);
			CreateTrigger<string>("Management.SetSearchQuery", SetSearchQuery);
			CreateTrigger<int>("Management.SetCategory", SetCategory);
			CreateTrigger<int, int, int>("Management.RoadOptionClicked", RoadOptionClicked);
			CreateTrigger<string>("Management.SetRoad", UpdateSelectedRoadConfiguration);
			CreateTrigger<string>("Management.SetRoadName", SetRoadName);
		}

		private void SetRoadName(string obj)
		{
			if (SelectedRoad != null)
			{
				SelectedRoad.Config.Name = obj;

				roadBuilderSystem.UpdateRoad(SelectedRoad.Config, Entity.Null, false);
			}
		}

		private void RoadOptionClicked(int option, int id, int value)
		{
			if (SelectedRoad == null)
			{
				return;
			}

			switch ((ActionType)option)
			{
				case ActionType.ShowInToolbar:
					SelectedRoad.Config.ToolbarState = (ShowInToolbarState)id;
					break;

				case ActionType.TogglePlayset:
					Mod.Log.Warn(PdxModsUtil.CurrentPlayset);
					if (SelectedRoad.Config.IsInPlayset())
					{
						RemoveFromPlayset();
					}
					else
					{
						AddToPlayset();
					}
					break;

				case ActionType.UploadRoad:
					Task.Run(UploadRoad);
					break;
			}

			roadBuilderSystem.UpdateRoad(SelectedRoad.Config, Entity.Null, false);

			UpdateSelectedRoadConfiguration(SelectedRoad.Config.ID);
		}

		private async Task UploadRoad()
		{
			Loading.Value = true;
			try
			{
				var config = SelectedRoad.Config;
				var svg = new ThumbnailGenerationUtil(SelectedRoad, World.GetExistingSystemManaged<RoadBuilderGenerationDataSystem>().RoadGenerationData).GenerateSvg();
				using var memory = new MemoryStream();

				svg.Save(memory, SaveOptions.DisableFormatting);

				var result = await ApiUtil.Instance.UploadRoad(new()
				{
					ID = config.ID,
					Category = (int)config.Category,
					Name = config.Name,
					Icon = Encoding.UTF8.GetString(memory.ToArray()),
					Config = JSON.Dump(config, EncodeOptions.CompactPrint)
				});

				if (result.success)
				{
					config.Uploaded = true;
				}
				else
				{
					Mod.Log.Error("Failed to upload your road: " + result.message);
				}
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Failed to upload your road");
			}
			Loading.Value = false;
		}

		private void UpdateSelectedRoadConfiguration(string id)
		{
			if (!roadBuilderSystem.Configurations.TryGetValue(id, out SelectedRoad))
			{
				return;
			}

			var config = SelectedRoad.Config;
			var options = new List<OptionSectionUIEntry>
			{
				new()
				{
					Id = (int)ActionType.ShowInToolbar,
					Name = LocaleHelper.Translate("RoadBuilder.ShowInToolbar", "Toolbar View"),
					IsToggle = true,
					Options = new[]
					{
						new OptionItemUIEntry
						{
							Id = (int)ShowInToolbarState.Hide,
							Name = $"RoadBuilder.ShowInToolbarState[{ShowInToolbarState.Hide}]",
							Icon = "coui://roadbuildericons/RB_Hide.svg",
							Selected = config.ToolbarState == ShowInToolbarState.Hide
						},
						new OptionItemUIEntry
						{
							Id = (int)ShowInToolbarState.Inherit,
							Name = $"RoadBuilder.ShowInToolbarState[{ShowInToolbarState.Inherit}]",
							Icon = "coui://roadbuildericons/RB_Any.svg",
							Selected = config.ToolbarState == ShowInToolbarState.Inherit
						},
						new OptionItemUIEntry
						{
							Id = (int)ShowInToolbarState.Show,
							Name = $"RoadBuilder.ShowInToolbarState[{ShowInToolbarState.Show}]",
							Icon = "coui://roadbuildericons/RB_Show.svg",
							Selected = config.ToolbarState == ShowInToolbarState.Show
						},
					}
				}
			};

			if (!Mod.Settings.NoPlaysetIsolation)
			{
				var isInPlayset = config.IsInPlayset();
				options.Add(new()
				{
					Id = (int)ActionType.TogglePlayset,
					Name = LocaleHelper.Translate("RoadBuilder.Playset", "Playset"),
					IsButton = true,
					Options = new[]
					{
						new OptionItemUIEntry
						{
							Name = isInPlayset ? "RoadBuilder.RemoveFromPlayset" : "RoadBuilder.AddToPlayset",
							Icon = isInPlayset ? "Media/Glyphs/Close.svg" : "Media/Glyphs/Plus.svg",
							Disabled = roadBuilderRoadTrackerSystem.UsedNetworkPrefabs.Contains(SelectedRoad)
						}
					}
				});
			}

			if (!config.Uploaded)
			{
				options.Add(new()
				{
					Id = (int)ActionType.UploadRoad,
					Name = LocaleHelper.Translate("RoadBuilder.Sharing", "Sharing"),
					IsButton = true,
					Options = new[]
					{
						new OptionItemUIEntry
						{
							Name = "RoadBuilder.UploadRoad",
							Icon = "Media/Glyphs/Plus.svg",
							Disabled = Loading
						}
					}
				});
			}

			SelectedRoadOptions.Value = options.ToArray();
		}

		protected override void OnUpdate()
		{
			RestrictPlayset.Value = !Mod.Settings.NoPlaysetIsolation;

			base.OnUpdate();
		}

		private void AddToPlayset()
		{
			if (PdxModsUtil.CurrentPlayset > 0
				&& !SelectedRoad.Config.Playsets.Contains(PdxModsUtil.CurrentPlayset))
			{
				SelectedRoad.Config.Playsets.Remove(-PdxModsUtil.CurrentPlayset);
				SelectedRoad.Config.Playsets.Add(PdxModsUtil.CurrentPlayset);
			}
		}

		private void RemoveFromPlayset()
		{
			if (PdxModsUtil.CurrentPlayset > 0)
			{
				if (SelectedRoad.Config.Playsets.Count == 0)
				{
					SelectedRoad.Config.Playsets.Add(-PdxModsUtil.CurrentPlayset);
				}
				else if (SelectedRoad.Config.Playsets.Contains(PdxModsUtil.CurrentPlayset))
				{
					SelectedRoad.Config.Playsets.Remove(PdxModsUtil.CurrentPlayset);
				}
			}
		}

		private void SetCategory(int obj)
		{
			currentCategory = obj < 0 ? null : (RoadCategory)obj;

			StartLoad();
		}

		private void SetDiscoverSorting(int obj)
		{
			sorting = obj;

			StartLoad();
		}

		private void SetSearchQuery(string obj)
		{
			query = obj;

			StartLoad();
		}

		private void SetDiscoverPage(int page)
		{
			CurrentPage.Value = page;

			StartLoad();
		}

		private void StartLoad()
		{
			_cancellationTokenSource.Cancel();

			Loading.Value = true;

			Task.Run(LoadPage);
		}

		private async Task LoadPage()
		{
			var token = _cancellationTokenSource.Token;

			await Task.Delay(200);

			if (token.IsCancellationRequested)
			{
				return;
			}

			var result = await ApiUtil.Instance.GetEntries(query, (int?)currentCategory, sorting, CurrentPage.Value);

			if (token.IsCancellationRequested)
			{
				return;
			}

			CurrentPage.Value = result.page;
			MaxPages.Value = result.totalPages;

			var items = new RoadConfigurationUIBinder[result.items.Count];

			for (var i = 0; i < items.Length; i++)
			{
				var item = result.items[i];
				items[i] = new RoadConfigurationUIBinder
				{
					ID = item.iD,
					Category = (RoadCategory)item.category,
					Name = item.name,
					Thumbnail = $"{KEYS.API_URL}/roadicon/{item.iD}.svg",
				};
			}

			Items.Value = items;
		}

		private void DownloadConfig(string obj)
		{
			Task.Run()
		}
	}
}
