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

		private int sorting;
		private string query = string.Empty;
		private string discoverQuery = string.Empty;
		private RoadCategory? currentCategory;
		private INetworkBuilderPrefab? SelectedRoad;
		private CancellationTokenSource cancellationTokenSource = new();

#nullable disable
		private RoadBuilderSystem roadBuilderSystem;
		private RoadBuilderRoadTrackerSystem roadBuilderRoadTrackerSystem;
		private RoadBuilderConfigurationsUISystem roadBuilderConfigurationsUISystem;
		private RoadBuilderGenericFunctionsSystem genericFunctionsSystem;
		private ValueBindingHelper<bool> Loading;
		private ValueBindingHelper<bool> ErrorLoading;
		private ValueBindingHelper<bool> Uploading;
		private ValueBindingHelper<int> CurrentPage;
		private ValueBindingHelper<int> MaxPages;
		private ValueBindingHelper<RoadConfigurationUIBinder[]> Items;
		private ValueBindingHelper<OptionSectionUIEntry[]> SelectedRoadOptions;
		private ValueBindingHelper<RoadConfigurationUIBinder[]> RoadConfigurations;
		private ValueBindingHelper<RoadConfigurationUIBinder> ManagedRoad;
#nullable enable

		protected override void OnCreate()
		{
			base.OnCreate();

			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			roadBuilderRoadTrackerSystem = World.GetOrCreateSystemManaged<RoadBuilderRoadTrackerSystem>();
			roadBuilderConfigurationsUISystem = World.GetOrCreateSystemManaged<RoadBuilderConfigurationsUISystem>();
			genericFunctionsSystem = World.GetOrCreateSystemManaged<RoadBuilderGenericFunctionsSystem>();

			roadBuilderConfigurationsUISystem.ConfigurationsUpdated += RoadBuilderConfigurationsUISystem_ConfigurationsUpdated;

			CreateBinding("Management.RestrictPlayset", () => !Mod.Settings!.NoPlaysetIsolation);

			Loading = CreateBinding("Discover.Loading", true);
			ErrorLoading = CreateBinding("Discover.ErrorLoading", false);
			Uploading = CreateBinding("Discover.Uploading", false);
			CurrentPage = CreateBinding("Discover.CurrentPage", 1);
			MaxPages = CreateBinding("Discover.MaxPages", 1);
			Items = CreateBinding("Discover.Items", new RoadConfigurationUIBinder[0]);
			SelectedRoadOptions = CreateBinding("Management.GetRoadOptions", new OptionSectionUIEntry[0]);
			RoadConfigurations = CreateBinding("Management.GetRoadConfigurations", new RoadConfigurationUIBinder[0]);
			ManagedRoad = CreateBinding("Management.ManagedRoad", new RoadConfigurationUIBinder());

			CreateTrigger<int>("Discover.SetPage", SetDiscoverPage);
			CreateTrigger<int>("Discover.SetSorting", SetDiscoverSorting);
			CreateTrigger<string>("Discover.Download", DownloadConfig);
			CreateTrigger<string>("Discover.SetSearchQuery", SetDiscoverSearchQuery);
			CreateTrigger<string>("Management.SetSearchQuery", SedSearchQuery);
			CreateTrigger<int>("Management.SetCategory", SetCategory);
			CreateTrigger<int, int, int>("Management.RoadOptionClicked", RoadOptionClicked);
			CreateTrigger<string>("Management.SetRoad", UpdateSelectedRoadConfiguration);
			CreateTrigger<string>("Management.SetRoadName", SetRoadName);
		}

		private void RoadBuilderConfigurationsUISystem_ConfigurationsUpdated()
		{
			SedSearchQuery(query);

			ManagedRoad.Value = roadBuilderConfigurationsUISystem.GetRoadBinder(SelectedRoad) ?? new();
		}

		private void SedSearchQuery(string obj)
		{
			query = obj;

			RoadConfigurations.Value = roadBuilderConfigurationsUISystem.AvailableConfigurations
				.Where(x => Filter(x.Name))
				.ToArray();
		}

		private bool Filter(string? name)
		{
			return string.IsNullOrWhiteSpace(query) || query.SearchCheck(name);
		}

		private void SetRoadName(string obj)
		{
			if (SelectedRoad?.Config != null)
			{
				SelectedRoad.Config.Name = obj;

				roadBuilderSystem.UpdateRoad(SelectedRoad.Config, Entity.Null, false, true);

				roadBuilderConfigurationsUISystem.UpdateConfigurationList();
			}
		}

		private void RoadOptionClicked(int option, int id, int value)
		{
			if (SelectedRoad?.Config == null)
			{
				return;
			}

			switch ((ActionType)option)
			{
				case ActionType.ShowInToolbar:
					SelectedRoad.Config.ToolbarState = (ShowInToolbarState)id;
					break;

				case ActionType.TogglePlayset:
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
					Uploading.Value = true;
					UpdateSelectedRoadOptions();

					Task.Run(UploadRoad);
					return;
			}

			roadBuilderSystem.UpdateRoad(SelectedRoad.Config, Entity.Null, false, true);

			roadBuilderConfigurationsUISystem.UpdateConfigurationList();

			UpdateSelectedRoadOptions();
		}

		private void UpdateSelectedRoadConfiguration(string id)
		{
			if (roadBuilderSystem.Configurations.TryGetValue(id, out SelectedRoad))
			{
				UpdateSelectedRoadOptions();
			}
		}

		private void UpdateSelectedRoadOptions()
		{
			if (SelectedRoad?.Config is null)
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

			if (!Mod.Settings!.NoPlaysetIsolation)
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
							Icon = isInPlayset ? "coui://roadbuildericons/RB_Remove.svg" : "coui://roadbuildericons/RB_Add.svg",
							Disabled = roadBuilderRoadTrackerSystem.UsedNetworkPrefabs.Contains(SelectedRoad)
						}
					}
				});
			}

			if (!config.Uploaded && !string.IsNullOrEmpty(PdxModsUtil.UserId))
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
							Icon = "coui://roadbuildericons/RB_Upload.svg",
							Disabled = Uploading || config.Name?.StartsWith("Copy of")==true || config.Name?.StartsWith("Custom ")==true
						}
					}
				});
			}

			ManagedRoad.Value = roadBuilderConfigurationsUISystem.GetRoadBinder(SelectedRoad);
			SelectedRoadOptions.Value = options.ToArray();
		}

		private void AddToPlayset()
		{
			if (PdxModsUtil.CurrentPlayset > 0
				&& SelectedRoad?.Config is not null
				&& (SelectedRoad.Config.Playsets is null || !SelectedRoad.Config.Playsets.Contains(PdxModsUtil.CurrentPlayset)))
			{
				SelectedRoad.Config.Playsets ??= new();

				SelectedRoad.Config.Playsets.Remove(-PdxModsUtil.CurrentPlayset);
				SelectedRoad.Config.Playsets.Add(PdxModsUtil.CurrentPlayset);
			}
		}

		private void RemoveFromPlayset()
		{
			if (PdxModsUtil.CurrentPlayset > 0 && SelectedRoad?.Config is not null)
			{
				SelectedRoad.Config.Playsets ??= new();

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

		private void SetDiscoverSearchQuery(string obj)
		{
			discoverQuery = obj;

			StartLoad();
		}

		private void SetDiscoverPage(int page)
		{
			CurrentPage.Value = page;

			StartLoad();
		}

		private void StartLoad()
		{
			cancellationTokenSource.Cancel();
			cancellationTokenSource = new();

			Loading.Value = true;
			ErrorLoading.Value = false;

			Task.Run(LoadPage);
		}

		private async Task LoadPage()
		{
			cancellationTokenSource.Cancel();
			cancellationTokenSource = new();

			var token = cancellationTokenSource.Token;

			await Task.Delay(250);

			if (token.IsCancellationRequested)
			{
				return;
			}

			Mod.Log.Debug(nameof(LoadPage));

			try
			{
				var result = await ApiUtil.Instance.GetEntries(discoverQuery, (int?)currentCategory, sorting, CurrentPage.Value);

				if (token.IsCancellationRequested || result is null)
				{
					return;
				}

				Mod.Log.Debug((result.items?.Count ?? 0) + " entries");

				CurrentPage.Value = result.page;
				MaxPages.Value = result.totalPages;

				var items = new RoadConfigurationUIBinder[result.items?.Count ?? 0];

				for (var i = 0; i < items.Length; i++)
				{
					var item = result.items![i];

					items[i] = new RoadConfigurationUIBinder
					{
						ID = item.id,
						Category = (RoadCategory)item.category,
						Name = item.name,
						Author = item.author,
						Available = roadBuilderSystem.Configurations.ContainsKey(item.id ?? string.Empty),
						Thumbnail = $"coui://roadbuilderthumbnails/{item.id}.svg",
					};

					var path = Path.Combine(FoldersUtil.TempFolder, item.id + ".svg");

					try
					{
						if (!File.Exists(path))
						{
							File.WriteAllText(path, item.icon);
						}
					}
					catch (Exception ex)
					{
						Mod.Log.Warn(ex, "Failed to save thumbnail");
					}
				}

				Items.Value = items;
				Loading.Value = false;
			}
			catch (Exception ex)
			{
				Mod.Log.Warn(ex, "Failed to load discover items");

				Loading.Value = false;
				ErrorLoading.Value = true;
			}
		}

		private void DownloadConfig(string obj)
		{
			Task.Run(async () =>
			{
				var config = await ApiUtil.Instance.GetConfig(obj);

				if (config is null)
				{
					return Task.CompletedTask;
				}

				config.ApplyVersionChanges();
				config.Playsets = new() { PdxModsUtil.CurrentPlayset };

				roadBuilderSystem.AddPrefab(config);

				return Task.CompletedTask;
			});
		}

		private async Task UploadRoad()
		{
			Mod.Log.Info(nameof(UploadRoad));

			try
			{
				var roadBuilderGenerationDataSystem = World.GetExistingSystemManaged<RoadBuilderGenerationDataSystem>();

				if (SelectedRoad is null || roadBuilderGenerationDataSystem.RoadGenerationData is null)
				{
					return;
				}

				var config = SelectedRoad.Config;

				if (config is null)
				{
					return;
				}

				var svg = new ThumbnailGenerationUtil(SelectedRoad, roadBuilderGenerationDataSystem.RoadGenerationData).GenerateSvg();
				using var memory = new MemoryStream();

				svg?.Save(memory, SaveOptions.DisableFormatting);

				config.Version = RoadBuilderSerializeSystem.CURRENT_VERSION;
				config.OriginalID = config.ID;
				config.Type = config.GetType().Name;

				var result = await ApiUtil.Instance.UploadRoad(new()
				{
					ID = config.ID,
					Category = (int)config.Category,
					Name = config.Name,
					Icon = Encoding.UTF8.GetString(memory.ToArray()),
					Config = JSON.Dump(config, EncodeOptions.CompactPrint)
				});

				if (result?.success == true)
				{
					Mod.Log.Info("Upload Successful");

					config.Uploaded = true;
				}
				else
				{
					Mod.Log.Error("Failed to upload your road: " + result?.message ?? "UNKNOWN");
				}
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Failed to upload your road");
			}

			Uploading.Value = false;
			UpdateSelectedRoadOptions();
		}
	}
}
